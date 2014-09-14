namespace BeatDetection.SampleMonoGame
{
  using Microsoft.Xna.Framework;
  using Microsoft.Xna.Framework.Extensions;
  using Microsoft.Xna.Framework.Graphics;
  using Microsoft.Xna.Framework.Input;

  /// <summary>
  /// This is the main type for your game
  /// </summary>
  public class Game1 : Game
  {
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    ISoundEngine soundEngine;

    public Game1()
      : base()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
    }

    /// <summary>
    /// Allows the game to perform any initialization it needs to before starting to run.
    /// This is where it can query for any required services and load any non-graphic
    /// related content.  Calling base.Initialize will enumerate through any components
    /// and initialize them as well.
    /// </summary>
    protected override void Initialize()
    {
      // TODO: Add your initialization logic here
      this.IsMouseVisible = true;

      base.Initialize();
    }

    private int viewportWidth;
    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice);
      soundEngine = new SoundEngine().RegisterOnBeatCallback(this.OnBeat);
      Texture2DManager.Instance.Initialize(this.GraphicsDevice)
        .Load("test", @"D:\Music\test.jpg")
        .CreateSquare("square");

      InputManager.Instance.Bind("start", Keys.Space)
        .Bind("fire", Keys.Space)
        .Bind("up", Keys.W)
        .Bind("down", Keys.S)
        .Bind("left", Keys.A)
        .Bind("right", Keys.D);

      viewportWidth = GraphicsDevice.Viewport.Width;
    }

    private uint lastBeatPos;
    private int drawSize;
    private void OnBeat(uint pos)
    {
      this.lastBeatPos = pos;
    }

    /// <summary>
    /// UnloadContent will be called once per game and is the place to unload
    /// all content.
    /// </summary>
    protected override void UnloadContent()
    {
      // TODO: Unload any non ContentManager content here
      soundEngine.Dispose();
    }

    bool hit = false;
    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        Exit();

      InputManager.Instance.Update();

      var elapsed = gameTime.ElapsedGameTime.Milliseconds;
      if (!soundEngine.IsMusicPlaying && InputManager.Instance.IsKeyPressed("start"))
      {
        soundEngine.LoadMusic(@"D:\Music\test.mp3")
          .SetBeatDetectionFrequency(100.0f, 150.0f)
          .StartBeatDetection()
          .PlayMusic(3);
      }

      soundEngine.Update(elapsed);

      if(soundEngine.IsMusicPlaying)
      {
        var musicPosition = soundEngine.MusicPosition;
        if (musicPosition - 20 < lastBeatPos)
        {
          drawSize = 1000;
        }

        if (drawSize > 600 && InputManager.Instance.IsKeyDown("fire"))
        {
          hit = true;
        }
        else
        {
          hit = false;
        }

        if (drawSize > 0)
        {
          drawSize = drawSize - gameTime.ElapsedGameTime.Milliseconds * 2;

          if (drawSize < 0)
          {
            drawSize = 0;
          }
        }
      }

      base.Update(gameTime);
    }

    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

      // Draw loaded texture
      spriteBatch.Draw(Texture2DManager.Instance["test"], new Vector2(), Color.White);

      Vector2 coor = new Vector2(viewportWidth/2, 400);
      var square = Texture2DManager.Instance["square"];
      var musicPosition = soundEngine.MusicPosition;
      foreach(var beatPosition in soundEngine.BeatPositions)
      {
        if (beatPosition > musicPosition
          && beatPosition < musicPosition + 2000)
        {
          uint diff = (beatPosition - musicPosition) / 4;

          var color = Color.White * 0.5f;

          var pos = new Vector2(coor.X + 5 + diff, coor.Y);
          var scale = new Vector2(2, 20);
          var center = new Vector2(1, 1);
          spriteBatch.Draw(square, pos, null, color, 0.0f, center, scale, SpriteEffects.None, 1.0f);

          var pos2 = new Vector2(coor.X - 5 - diff, coor.Y);
          spriteBatch.Draw(square, pos2, null, color, 0.0f, center, scale, SpriteEffects.None, 1.0f);
        }
      }

      var barColor = Color.White * 0.5f;
      if (hit)
      {
        barColor = Color.Red;
      }

      var size = new Vector2(10, 40);
      spriteBatch.Draw(square, coor, null, barColor, 0.0f, new Vector2(1, 1), size, SpriteEffects.None, 1.0f);
      spriteBatch.Draw(square, coor, null, barColor, 0.0f, new Vector2(1, 1), new Vector2(10, 0.08f * drawSize), SpriteEffects.None, 1.0f);

      spriteBatch.End();

      base.Draw(gameTime);
    }
  }
}

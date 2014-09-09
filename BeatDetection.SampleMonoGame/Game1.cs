#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
#endregion

namespace BeatDetection.SampleMonoGame
{
  /// <summary>
  /// This is the main type for your game
  /// </summary>
  public class Game1 : Game
  {
    GraphicsDeviceManager graphics;
    SpriteBatch spriteBatch;
    SoundEngine soundEngine;
    bool playing = false;

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

      base.Initialize();
    }

    int ViewportWidth;
    Texture2D rect1;
    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice);
      soundEngine = new SoundEngine().Subscribe(this.OnBeat);

      ViewportWidth = GraphicsDevice.Viewport.Width;

      // TODO: use this.Content to load your game content here
      rect1 = new Texture2D(graphics.GraphicsDevice, 2, 2);

      Color[] data = new Color[4];
      for (int i = 0; i < data.Length; ++i) data[i] = Color.White;
      rect1.SetData(data);
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

    /// <summary>
    /// Allows the game to run logic such as updating the world,
    /// checking for collisions, gathering input, and playing audio.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Update(GameTime gameTime)
    {
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        Exit();

      if(playing == false)
      {
        var task = soundEngine.Load(@"D:\Music\test.mp3")
                    .SetAnalyzeFrequency(100.0f, 150.0f)
                    .Play(2);

        playing = true;
      }

      soundEngine.Update();

      var musicPosition = soundEngine.Position;
      if ( musicPosition - 20 < lastBeatPos &&  lastBeatPos < musicPosition + 20)
      {
        drawSize = 500;
      }

      if (drawSize > 0)
      {
        drawSize = drawSize - gameTime.ElapsedGameTime.Milliseconds;

        if (drawSize < 0)
        {
          drawSize = 0;
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

      spriteBatch.Begin();
      Vector2 coor = new Vector2(ViewportWidth/2, 400);

      spriteBatch.Draw(rect1, coor, null, Color.White, 0.0f, new Vector2(1, 1), new Vector2(10, 0.15f * drawSize), SpriteEffects.None, 1.0f);

      var musicPosition = soundEngine.Position;
      foreach(var beatPosition in soundEngine.BeatPositions)
      {
        if (beatPosition > musicPosition
          && beatPosition < musicPosition + 2000)
        {
          uint diff = (beatPosition - musicPosition) / 4;

          var color = Color.White;

          var pos = new Vector2(coor.X + 5 + diff, coor.Y);
          var scale = new Vector2(2, 20);
          var center = new Vector2(1, 1);
          spriteBatch.Draw(rect1, pos, null, color, 0.0f, center, scale, SpriteEffects.None, 1.0f);

          var pos2 = new Vector2(coor.X - 5 - diff, coor.Y);
          spriteBatch.Draw(rect1, pos2, null, color, 0.0f, center, scale, SpriteEffects.None, 1.0f);
        }
      }

      var kbstate = Keyboard.GetState();
      var size = new Vector2(10, 40);
      if(kbstate.IsKeyDown(Keys.Space))
      {
        size = new Vector2(12, 42);
      }

      spriteBatch.Draw(rect1, coor, null, Color.White, 0.0f, new Vector2(1,1), size, SpriteEffects.None, 1.0f);

      spriteBatch.End();

      base.Draw(gameTime);
    }
  }
}

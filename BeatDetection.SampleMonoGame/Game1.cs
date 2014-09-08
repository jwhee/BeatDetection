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

    Texture2D rect1;
    Texture2D rect2;
    /// <summary>
    /// LoadContent will be called once per game and is the place to load
    /// all of your content.
    /// </summary>
    protected override void LoadContent()
    {
      // Create a new SpriteBatch, which can be used to draw textures.
      spriteBatch = new SpriteBatch(GraphicsDevice);
      soundEngine = new SoundEngine();

      // TODO: use this.Content to load your game content here
      rect1 = new Texture2D(graphics.GraphicsDevice, 80, 30);
      rect2 = new Texture2D(graphics.GraphicsDevice, 80, 30);

      Color[] data = new Color[80 * 30];
      for (int i = 0; i < data.Length; ++i) data[i] = Color.White;
      rect1.SetData(data);
      for (int i = 0; i < data.Length; ++i) data[i] = Color.Black;
      rect2.SetData(data);
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
        var task = soundEngine.Load(@"D:\Music\test2.mp3")
                    .SetAnalyzeFrequency(50.0f, 150.0f)
                    .Play(1);

        playing = true;
      }

      soundEngine.Update();
      if (soundEngine.IsBeat())
      {
        if (lastDrawIsBeat == false)
        {
          if (drawTexture1)
          {
            drawTexture1 = false;
          }
          else
          {
            drawTexture1 = true;
          }
        }

        lastDrawIsBeat = true;
      }
      else
      {
        lastDrawIsBeat = false;
      }

      base.Update(gameTime);
    }

    Color testColor = Color.White;
    bool lastDrawIsBeat = false;
    bool drawTexture1 = true;
    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      Vector2 coor = new Vector2(10, 20);

      spriteBatch.Begin();
      if(drawTexture1)
      {
        spriteBatch.Draw(rect1, coor, Color.White);
      }
      else
      {
        spriteBatch.Draw(rect2, coor, Color.White);
      }

      spriteBatch.End();

      base.Draw(gameTime);
    }
  }
}

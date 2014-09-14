namespace Microsoft.Xna.Framework.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.IO;

  using Microsoft.Xna.Framework.Graphics;
  
  public class Texture2DManager
  {
    private static readonly Lazy<Texture2DManager> instance
      = new Lazy<Texture2DManager>(() => new Texture2DManager());

    private GraphicsDevice graphics;
    private Dictionary<string, Texture2D> textures;
    private Dictionary<string, List<Texture2D>> slicedTextures;
    private BlendState blendColor = null;
    private BlendState blendAlpha = null;
    private SpriteBatch spriteBatch = null;

    private Texture2DManager()
    {
      this.textures = new Dictionary<string, Texture2D>();
      this.slicedTextures = new Dictionary<string, List<Texture2D>>();
    }

    public Texture2DManager Initialize(GraphicsDevice graphics)
    {
      this.graphics = graphics;

      this.blendColor = new BlendState()
      {
        ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue,
        AlphaDestinationBlend = Blend.Zero,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.SourceAlpha,
        ColorSourceBlend = Blend.SourceAlpha
      };

      this.blendAlpha = new BlendState()
      {
        ColorWriteChannels = ColorWriteChannels.Alpha,
        AlphaDestinationBlend = Blend.Zero,
        ColorDestinationBlend = Blend.Zero,
        AlphaSourceBlend = Blend.One,
        ColorSourceBlend = Blend.One,
      };

      this.spriteBatch = new SpriteBatch(this.graphics);

      return this;
    }

    public Texture2DManager Slice(string key, int row, int col)
    {
      var original = this.textures[key];

      if (row == 0)
      {
        row = 1;
      }

      if (col == 0)
      {
        col = 1;
      }

      var width = original.Width / row;
      var height = original.Height / col;
      
      using (var renderTarget = new RenderTarget2D(this.graphics, width, height))
      {
        var sliced = new List<Texture2D>();
        for (int i = 0; i < col; i++ )
        {
          for (int j = 0; j < row; j++ )
          {
            var texture = new Texture2D(this.graphics, width, height);
            var sourceRectangle = new Rectangle(j * width, i * height, width, height);

            //Setup a render target to hold our final texture which will have premulitplied alpha values
            this.graphics.SetRenderTarget(renderTarget);
            this.graphics.Clear(Color.Black);

            // Multiply each color by the source alpha, and write in just the color values into the final texture
            this.spriteBatch.Begin(SpriteSortMode.Immediate, this.blendColor);
            this.spriteBatch.Draw(original, Vector2.Zero, sourceRectangle, Color.White);
            this.spriteBatch.End();

            // Now copy over the alpha values from the PNG source texture to the final one, without multiplying them
            this.spriteBatch.Begin(SpriteSortMode.Immediate, this.blendAlpha);
            this.spriteBatch.Draw(original, Vector2.Zero, sourceRectangle, Color.White);
            this.spriteBatch.End();

            //Release the GPU back to drawing to the screen
            graphics.SetRenderTarget(null);

            // Store data from render target because the RenderTarget2D is volatile
            Color[] data = new Color[texture.Width * texture.Height];
            renderTarget.GetData(data);

            // Unset texture from graphic device and set modified data back to it
            this.graphics.Textures[0] = null;
            texture.SetData(data);

            sliced.Add(texture);
          }
        }

        this.slicedTextures[key] = sliced;
      }

      return this;
    }

    public Texture2DManager Load(string key, string path, bool preMultiplyAlpha = true)
    {
      Texture2D texture = null;
      using (var stream = File.OpenRead(path))
      {
        texture = Texture2D.FromStream(this.graphics, stream);
      }

      if (preMultiplyAlpha)
      {
        using (var renderTarget = new RenderTarget2D(this.graphics, texture.Width, texture.Height))
        {
          // Setup a render target to hold our final texture which will have premulitplied alpha values
          this.graphics.SetRenderTarget(renderTarget);
          this.graphics.Clear(Color.Black);

          // Multiply each color by the source alpha, and write in just the color values into the final texture
          this.spriteBatch.Begin(SpriteSortMode.Immediate, this.blendColor);
          this.spriteBatch.Draw(texture, texture.Bounds, Color.White);
          this.spriteBatch.End();

          // Now copy over the alpha values from the PNG source texture to the final one, without multiplying them
          this.spriteBatch.Begin(SpriteSortMode.Immediate, this.blendAlpha);
          this.spriteBatch.Draw(texture, texture.Bounds, Color.White);
          this.spriteBatch.End();

          // Release the GPU back to drawing to the screen
          this.graphics.SetRenderTarget(null);

          // Store data from render target because the RenderTarget2D is volatile
          Color[] data = new Color[texture.Width * texture.Height];
          renderTarget.GetData(data);

          // Unset texture from graphic device and set modified data back to it
          this.graphics.Textures[0] = null;
          texture.SetData(data);
        }
      }

      this.textures[key] = texture;

      return this;
    }

    public Texture2DManager CreateSquare(string key)
    {
      var square = new Texture2D(this.graphics, 2, 2);
      Color[] data = new Color[4];
      for (int i = 0; i < data.Length; ++i) data[i] = Color.White;
      square.SetData(data);

      return this.Set(key, square);
    }

    public Texture2DManager Set(string key, Texture2D texture)
    {
      this.textures[key] = texture;

      return this;
    }

    public Texture2D Get(string key)
    {
      return this.textures[key];
    }

    public Texture2D Get(string key, int index)
    {
      return this.slicedTextures[key][index];
    }

    public Texture2D this[string key]
    {
      get
      {
        return this.Get(key);
      }
      set
      {
        this.Set(key, value);
      }
    }

    public static Texture2DManager Instance
    {
      get
      {
        return instance.Value;
      }
    }
  }
}

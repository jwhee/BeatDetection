namespace BeatDetection.SampleMonoGame
{
  using System;
  using System.Collections.Generic;
  using System.IO;

  using Microsoft.Xna.Framework;
  using Microsoft.Xna.Framework.Graphics;

  public class Texture2DManager
  {
    private static readonly Lazy<Texture2DManager> instance
      = new Lazy<Texture2DManager>(() => new Texture2DManager());

    private GraphicsDevice device;
    private Dictionary<string, Texture2D> textures;

    private Texture2DManager()
    {
      this.textures = new Dictionary<string, Texture2D>();
    }

    public Texture2DManager Initialize(GraphicsDevice device)
    {
      this.device = device;
      return this;
    }

    public Texture2DManager Load(string key, string path)
    {
      using (var stream = File.OpenRead(path))
      {
        textures[key] = Texture2D.FromStream(this.device, stream);
      }

      return this;
    }

    public Texture2DManager CreateSquare(string key)
    {
      var square = new Texture2D(device, 2, 2);
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

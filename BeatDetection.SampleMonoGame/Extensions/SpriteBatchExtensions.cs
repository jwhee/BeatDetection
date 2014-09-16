namespace Microsoft.Xna.Framework.Extensions
{
  using Microsoft.Xna.Framework.Graphics;

  public static class SpriteBatchExtensions
  {
    public static void Render(
      this SpriteBatch spriteBatch,
      Texture2D texture,
      float xPos,
      float yPos,
      float rotation,
      float scale,
      Rectangle? sourceRectangle = null,
      Color? color = null,
      Vector2? origin = null)
    {
      var colorParam = Color.White;
      if (color.HasValue)
      {
        colorParam = color.Value;
      }

      var originParam = new Vector2(texture.Width / 2, texture.Height / 2);
      if (origin.HasValue)
      {
        originParam = origin.Value;
      }

      spriteBatch.Draw(
        texture,
        new Vector2(xPos, yPos),
        sourceRectangle,
        colorParam,
        rotation,
        originParam,
        scale,
        SpriteEffects.None,
        0.0f);
    }

    public static void Render(
      this SpriteBatch spriteBatch,
      Texture2D texture,
      Transform transform,
      Rectangle? sourceRectangle = null,
      Color? color = null,
      Vector2? origin = null)
    {
      spriteBatch.Render(
        texture, 
        transform.X, 
        transform.Y,
        transform.Rotation,
        transform.Size,
        sourceRectangle,
        color,
        origin);
    }
  }
}

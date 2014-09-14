namespace Microsoft.Xna.Framework.Extensions
{
  using Microsoft.Xna.Framework.Graphics;

  public abstract class Actor
  {
    public Transform Transform { get; set; }
    public abstract void Update(int elapsedMilliseconds);
    public abstract void Register(object resource);
    public abstract void Draw(SpriteBatch spriteBatch);
  }
}

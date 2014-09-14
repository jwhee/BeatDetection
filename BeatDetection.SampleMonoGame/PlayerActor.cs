namespace BeatDetection.SampleMonoGame
{
  using Microsoft.Xna.Framework;
  using Microsoft.Xna.Framework.Extensions;
  using Microsoft.Xna.Framework.Graphics;

  public sealed class PlayerActor : Actor
  {
    public override void Update(int elapsedMilliseconds)
    {
      // Do nothing
    }

    public override void Register(object resource)
    {
      // Do nothing
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
      spriteBatch.Draw(
        Texture2DManager.Instance["square"],
        new Vector2(this.Transform.X, this.Transform.Y),
        null,
        Color.Pink,
        this.Transform.Rotation,
        Vector2.One,
        this.Transform.Size,
        SpriteEffects.None,
        0.0f);
    }
  }
}

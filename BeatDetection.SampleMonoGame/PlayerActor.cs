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
      spriteBatch.Render(
        Texture2DManager.Instance["square"],
        this.Transform,
        color: Color.Pink);
    }
  }
}

namespace BeatDetection.SampleMonoGame
{
  using Microsoft.Xna.Framework;
  using Microsoft.Xna.Framework.Extensions;
  using Microsoft.Xna.Framework.Graphics;

  public class Player
  {
    private Transform transform;
    private Mover mover;
    public Player()
    {
      this.transform = new Transform() 
      {
        Size = 10.0f
      };

      this.mover = new KeyboardInputMover(transform)
      {
        Velocity = 0.5f
      };
    }

    public void Update(int elapsedMilliseconds)
    {
      this.mover.Update(elapsedMilliseconds);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
      spriteBatch.Draw(
        Texture2DManager.Instance["square"],
        new Vector2(this.transform.X, this.transform.Y),
        null,
        Color.Pink,
        this.transform.Rotation,
        Vector2.One,
        this.transform.Size,
        SpriteEffects.None,
        0.0f);
    }
  }
}

namespace Microsoft.Xna.Framework.Extensions
{
  public abstract class Mover
  {
    private Transform transform;

    public Mover(Transform transform)
    {
      this.transform = transform;
    }

    public abstract void Update(GameTime gametime);
  }
}

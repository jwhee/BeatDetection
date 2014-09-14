namespace Microsoft.Xna.Framework.Extensions
{
  public abstract class Mover
  {
    protected Transform transform;

    public Mover(Transform transform)
    {
      this.transform = transform;
    }

    public abstract void Update(int elapsedMilliseconds);
  }
}

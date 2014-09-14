namespace Microsoft.Xna.Framework.Extensions
{
  public abstract class Mover
  {
    public Transform Transform { get; set; }
    public abstract void Update(int elapsedMilliseconds);
  }
}

namespace Microsoft.Xna.Framework.Extensions
{
  using Microsoft.Xna.Framework.Graphics;

  public sealed class GameObject
  {
    private Transform transform;
    private Mover mover;
    private Actor actor;

    public GameObject(Transform transform)
    {
      this.transform = transform;
    }

    public GameObject SetMover(Mover mover)
    {
      this.mover = mover;
      this.mover.Transform = this.transform;
      return this;
    }

    public GameObject SetActor(Actor actor)
    {
      this.actor = actor;
      this.actor.Transform = this.transform;
      return this;
    }

    public GameObject GiveActor(object resource)
    {
      this.actor.Register(resource);
      return this;
    }

    public void Update(int elapsedMilliseconds)
    {
      this.mover.Update(elapsedMilliseconds);
      this.actor.Update(elapsedMilliseconds);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
      this.actor.Draw(spriteBatch);
    }
  }
}

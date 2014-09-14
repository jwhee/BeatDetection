namespace Microsoft.Xna.Framework.Extensions
{
  using Microsoft.Xna.Framework.Input;

  public class KeyboardInputMover : Mover
  {
    public string UpKey { get ; set;}
    public string DownKey { get; set; }
    public string LeftKey { get; set; }
    public string RightKey { get; set; }

    public float UpVelocity { get; set; }
    public float DownVelocity { get; set; }
    public float VerticalVelocity
    {
      set
      {
        this.UpVelocity = value;
        this.DownVelocity = value;
      }
    }

    public float LeftVelocity { get; set; }
    public float RightVelocity { get; set; }
    public float HorizontalVelocity
    {
      set
      {
        this.LeftVelocity = value;
        this.RightVelocity = value;
      }
    }

    public float Velocity
    {
      set
      {
        this.HorizontalVelocity = value;
        this.VerticalVelocity = value;
      }
    }

    public KeyboardInputMover()
    {
      this.UpKey = "up";
      this.DownKey = "down";
      this.LeftKey = "left";
      this.RightKey = "right";
      this.Velocity = 1.0f;
    }

    public override void Update(int elapsedMilliseconds)
    {
      if (InputManager.Instance.IsKeyDown(this.UpKey)
       && InputManager.Instance.IsKeyUp(this.DownKey))
      {
        // up
        this.Transform.Y -= elapsedMilliseconds * this.UpVelocity;
      }
      else if (InputManager.Instance.IsKeyDown(this.DownKey)
            && InputManager.Instance.IsKeyUp(this.UpKey))
      {
        // down
        this.Transform.Y += elapsedMilliseconds * this.DownVelocity;
      }

      if (InputManager.Instance.IsKeyDown(this.LeftKey)
       && InputManager.Instance.IsKeyUp(this.RightKey))
      {
        // left
        this.Transform.X -= elapsedMilliseconds * this.LeftVelocity;
      }
      else if (InputManager.Instance.IsKeyDown(this.RightKey)
            && InputManager.Instance.IsKeyUp(this.LeftKey))
      {
        // right
        this.Transform.X += elapsedMilliseconds * this.RightVelocity;
      }
    }
  }
}

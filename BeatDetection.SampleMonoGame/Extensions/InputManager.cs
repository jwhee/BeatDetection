namespace Microsoft.Xna.Framework.Extensions
{
  using System;

  using Microsoft.Xna.Framework.Input;

  public class InputManager
  {
    private KeyboardState keyboardState;
    private KeyboardState lastKeyboardState;

    private static readonly Lazy<InputManager> instance
      = new Lazy<InputManager>(() => new InputManager());

    private InputManager()
    {
    }

    public static InputManager Instance
    {
      get
      {
        return instance.Value;
      }
    }

    public void Update()
    {
      this.lastKeyboardState = keyboardState;
      this.keyboardState = Keyboard.GetState();
    }

    public bool IsKeyDown(Keys key)
    {
      return this.keyboardState.IsKeyDown(key);
    }

    public bool IsKeyUp(Keys key)
    {
      return this.keyboardState.IsKeyUp(key);
    }

    public bool IsKeyPressed(Keys key)
    {
      return this.lastKeyboardState.IsKeyUp(key)
          && this.keyboardState.IsKeyDown(key);
    }

    public bool IsKeyReleased(Keys key)
    {
      return this.lastKeyboardState.IsKeyDown(key)
          && this.keyboardState.IsKeyUp(key);
    }
  }
}

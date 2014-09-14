namespace Microsoft.Xna.Framework.Extensions
{
  using System;
  using System.Collections.Generic;

  using Microsoft.Xna.Framework.Input;

  public class InputManager
  {
    private KeyboardState keyboardState;
    private KeyboardState lastKeyboardState;
    private Dictionary<string, HashSet<Keys>> bindings;

    private static readonly Lazy<InputManager> instance
      = new Lazy<InputManager>(() => new InputManager());

    private InputManager()
    {
      this.bindings = new Dictionary<string, HashSet<Keys>>();
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

    public bool IsKeyDown(string alias)
    {
      var keys = this.GetBinding(alias);

      foreach(var key in keys)
      {
        if (this.keyboardState.IsKeyDown(key) == false)
        {
          return false;
        }
      }

      return true;
    }

    public bool IsKeyUp(Keys key)
    {
      return this.keyboardState.IsKeyUp(key);
    }

    public bool IsKeyUp(string alias)
    {
      var keys = this.GetBinding(alias);

      foreach (var key in keys)
      {
        if (this.keyboardState.IsKeyUp(key) == false)
        {
          return false;
        }
      }

      return true;
    }

    public bool IsKeyPressed(Keys key)
    {
      return this.lastKeyboardState.IsKeyUp(key)
          && this.keyboardState.IsKeyDown(key);
    }

    public bool IsKeyPressed(string alias)
    {
      var keys = this.GetBinding(alias);

      foreach (var key in keys)
      {
        if (this.lastKeyboardState.IsKeyUp(key) == false
         || this.keyboardState.IsKeyDown(key) == false)
        {
          return false;
        }
      }

      return true;
    }

    public bool IsKeyReleased(Keys key)
    {
      return this.lastKeyboardState.IsKeyDown(key)
          && this.keyboardState.IsKeyUp(key);
    }

    public bool IsKeyReleased(string alias)
    {
      var keys = this.GetBinding(alias);

      foreach (var key in keys)
      {
        if (this.lastKeyboardState.IsKeyDown(key) == false
         || this.keyboardState.IsKeyUp(key) == false)
        {
          return false;
        }
      }

      return true;
    }

    public InputManager Bind(string alias, Keys key)
    {
      if (!this.bindings.ContainsKey(alias))
      {
        this.bindings[alias] = new HashSet<Keys>();
      }

      this.bindings[alias].Add(key);

      return this;
    }

    public InputManager Unbind(string alias, Keys key)
    {
      if (this.bindings.ContainsKey(alias))
      {
        this.bindings[alias].Remove(key);
      }

      return this;
    }

    public IEnumerable<Keys> GetBinding(string alias)
    {
      if (!this.bindings.ContainsKey(alias))
      {
        return new List<Keys>();
      }

      return this.bindings[alias];
    }
  }
}

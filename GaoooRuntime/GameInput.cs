using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GaoooRuntime
{
    public class GameInput
    {
        public enum Button : int
        {
            OK,
            Cancel,
            Escape,

            Up,
            Down,
            Left,
            Right,

            Count
        }

        private Game1 _game;
        private int _newDown = 0;
        private int _oldDown = 0;

        public Point MousePosition { get; private set; }

        public GameInput(Game1 game)
        {
            _game = game;
        }

        public void Update(double dt, bool ignoreInput)
        {
            _oldDown = _newDown;
            _newDown = 0;

            if (ignoreInput)
            {
                return;
            }

            var ms = Mouse.GetState();
            if (ms.Position.X > 0 &&
                ms.Position.Y > 0 &&
                ms.Position.X < _game.Window.ClientBounds.Width &&
                ms.Position.Y < _game.Window.ClientBounds.Height)
            {
                _newDown |= ms.LeftButton == ButtonState.Pressed ? 1 << (int)Button.OK : 0;
                _newDown |= ms.RightButton == ButtonState.Pressed ? 1 << (int)Button.Cancel : 0;
            }

            var ks = Keyboard.GetState();
            _newDown |= ks.IsKeyDown(Keys.Enter) ? 1 << (int)Button.OK : 0;
            _newDown |= ks.IsKeyDown(Keys.Back) ? 1 << (int)Button.Cancel : 0;
            _newDown |= ks.IsKeyDown(Keys.Escape) ? 1 << (int)Button.Escape : 0;

            _newDown |= ks.IsKeyDown(Keys.Up) ? 1 << (int)Button.Up : 0;
            _newDown |= ks.IsKeyDown(Keys.Down) ? 1 << (int)Button.Down : 0;
            _newDown |= ks.IsKeyDown(Keys.Left) ? 1 << (int)Button.Left : 0;
            _newDown |= ks.IsKeyDown(Keys.Right) ? 1 << (int)Button.Right : 0;

            MousePosition = ms.Position;
        }

        public bool GetKey(Button button)
        {
            return 0 != (_newDown & (1 << (int)button));
        }

        public bool GetKeyDown(Button button)
        {
            return (0 != (_newDown & (1 << (int)button))) && (0 == (_oldDown & (1 << (int)button)));
        }

        public bool GetKeyUp(Button button)
        {
            return (0 == (_newDown & (1 << (int)button))) && (0 != (_oldDown & (1 << (int)button)));
        }
    }
}

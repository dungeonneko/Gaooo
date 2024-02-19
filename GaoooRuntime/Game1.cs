using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using FontStashSharp;
using Gaooo;
using Guooo;
using SoLoud;
using System;
using System.Collections.Generic;
using System.IO;

namespace GaoooRuntime
{
    public partial class Game1 : Game
    {
        // Systems
        CommandLineArgs _args;
        ProjectSettings _projectSettings;
        GaoooSystem _gaooo;
        GuoooSystem _guooo;
        GameInput _input;
        GameText _text;

        // Graphics
        GraphicsDeviceManager _graphics;
        RenderTarget2D _renderTargetFront;
        RenderTarget2D _renderTargetTrans;
        Dictionary<string, Texture2D> _textures;
        Dictionary<KeyValuePair<string, float>, DynamicSpriteFont> _fonts;
        SpriteBatch _spriteBatch;
        RasterizerState _scissor;
        bool _resizedInThisFrame;

        // Audio
        Soloud _soloud = null;
        Dictionary<string, uint> _handle = new Dictionary<string, uint>();

        public Game1()
        {
            IsMouseVisible = true;
            _args = CommandLineParser.Parse();
            _graphics = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            if (_args == null)
            {
                return;
            }

            _soloud = new Soloud();
            var ret = _soloud.init();
            
            _projectSettings = new ProjectSettings(loadJson("settings.json"));

            _gaooo = new GaoooSystem(true);
            _gaooo.CallbackOnGetAbsPath = (rel) => Path.GetFullPath(Path.Join(_args.WorkSpace, rel));
            _gaooo.CallbackOnGetRelPath = (abs) => Path.GetRelativePath(_args.WorkSpace, abs);
            _projectSettings.MacroScripts.ForEach(scr => _gaooo.AddMacro(scr));
            _gaooo.ProcCommand["playbgm"] = onPlayBgm;
            _gaooo.ProcCommand["stopbgm"] = onStopBgm;
            _gaooo.ProcCommand["playse"] = onPlaySe;
            _gaooo.ProcCommand["stopse"] = onStopSe;
            _gaooo.ProcCommand["ws"] = onWaitSe;
            _gaooo.ProcCommand["l"] = onTextWaitNextParagraph;
            _gaooo.ProcCommand["p"] = onTextWaitNextPage;
            _gaooo.ProcCommand["ch"] = onTextPut;
            _gaooo.ProcCommand["cm"] = onTextClear;
            _gaooo.ProcCommand["r"] = onTextNewLine;
            _gaooo.ProcCommand["branch"] = onTextBeginBranch;
            _gaooo.ProcCommand["endbranch"] = onTextEndBranch;
            _gaooo.ProcCommand["link"] = onTextBeginLink;
            _gaooo.ProcCommand["endlink"] = onTextEndLink;
            _gaooo.ProcCommand["ui_load"] = onGuiLoad;
            _gaooo.ProcCommand["ui_unload"] = onGuiUnload;
            _gaooo.ProcCommand["ui_add"] = onGuiAdd;
            _gaooo.ProcCommand["ui_remove"] = onGuiRemove;
            _gaooo.ProcCommand["ui_set"] = onGuiSet;
            _gaooo.ProcCommand["ui_branch"] = onGuiBranch;

            _guooo = new GuoooSystem();
            _guooo.Resize(_projectSettings.RenderWidth, _projectSettings.RenderHeight);

            _input = new GameInput(this);
            _text = new GameText();

            _renderTargetFront = new RenderTarget2D(GraphicsDevice, _projectSettings.RenderWidth, _projectSettings.RenderHeight, false, SurfaceFormat.Color, DepthFormat.None);
            _renderTargetTrans = new RenderTarget2D(GraphicsDevice, _projectSettings.RenderWidth, _projectSettings.RenderHeight, false, SurfaceFormat.Color, DepthFormat.None);
            _textures = new Dictionary<string, Texture2D>();
            _fonts = new Dictionary<KeyValuePair<string, float>, DynamicSpriteFont>();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _scissor = new RasterizerState() { ScissorTestEnable = true };

            Window.Title = _projectSettings.Title;
            Window.AllowUserResizing = _projectSettings.WindowResizable;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(onResize);

            _graphics.PreferredBackBufferWidth = _projectSettings.WindowWidth;
            _graphics.PreferredBackBufferHeight = _projectSettings.WindowHeight;
            _graphics.ApplyChanges();

            try
            {
                _gaooo.Jump(_projectSettings.EntryPoint, 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Failed to jump to the entry point");
                System.Windows.Forms.MessageBox.Show(
                    e.Message,
                    "ERROR",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (_args == null || (!_gaooo.IsPaused && _gaooo.IsEnd))
            {
                Exit();
                return;
            }

            if (!IsActive)
            {
                return;
            }

            var dt = gameTime.ElapsedGameTime.TotalSeconds;

            // Input
            _input.Update(dt, _resizedInThisFrame);
            if (_input.GetKeyDown(GameInput.Button.Escape))
            {
                Exit();
            }

            // Script
            _gaooo.Update(dt);

            // GUI
            var mousePosInGameScreen = ToScreen(_input.MousePosition);
            _guooo.Update(
                dt,
                mousePosInGameScreen.X,
                mousePosInGameScreen.Y,
                _input.GetKeyDown(GameInput.Button.OK),
                _input.GetKeyDown(GameInput.Button.Cancel));

            // Text
            _text.Update(dt, mousePosInGameScreen, _gaooo.CurrentTag);

            // Others
            _resizedInThisFrame = false;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Draw the transition image
            if (!_gaooo.Transition.IsEnd)
            {
                GraphicsDevice.SetRenderTarget(_renderTargetTrans);
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                drawLayer(_gaooo.LayerBack);
                _spriteBatch.End();
            }

            // Draw the front layer
            GraphicsDevice.SetRenderTarget(_renderTargetFront);
            if (_gaooo.Transition.IsEnd)
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                drawLayer(_gaooo.LayerFront);
                _spriteBatch.End();
            }
            else
            {
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);
                drawLayer(_gaooo.LayerFront);
                _spriteBatch.End();
                _spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.NonPremultiplied);
                var rate = (float)(1.0 - _gaooo.Transition.Rate);
                var color = new Color(1.0f, 1.0f, 1.0f, rate);
                _spriteBatch.Draw(_renderTargetTrans, new Rectangle(0, 0, _renderTargetTrans.Width, _renderTargetTrans.Height), color);
                _spriteBatch.End();
            }

            // GUI
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            foreach (var kv in _guooo.Components)
            {
                drawGUIComponent(kv.Value);
            }
            _spriteBatch.End();

            // Blit the render target to the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            var src = _renderTargetFront.Bounds;
            var dst = GraphicsDevice.Viewport.Bounds;
            var scale = Math.Min((float)dst.Width / _renderTargetFront.Width, (float)dst.Height / _renderTargetFront.Height);
            dst.Width = (int)Math.Round(_renderTargetFront.Width * scale);
            dst.Height = (int)Math.Round(_renderTargetFront.Height * scale);
            dst.X = (int)Math.Round((GraphicsDevice.Viewport.Width - dst.Width) / 2.0);
            dst.Y = (int)Math.Round((GraphicsDevice.Viewport.Height - dst.Height) / 2.0);
            _spriteBatch.Draw(_renderTargetFront, dst, src, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void onResize(object sender, EventArgs e)
        {
            _graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            _graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            _graphics.ApplyChanges();
            _resizedInThisFrame = true;
        }

        private bool onGetOK()
        {
            return _input.GetKeyDown(GameInput.Button.OK);
        }

        // Transform a point from the screen coordinate to the game coordinate
        private Point ToScreen(Point posOnWindow)
        {
            var src = _renderTargetFront.Bounds;
            var dst = GraphicsDevice.Viewport.Bounds;
            var scale = Math.Min((float)dst.Width / src.Width, (float)dst.Height / src.Height);
            dst.Width = (int)Math.Round(src.Width * scale);
            dst.Height = (int)Math.Round(src.Height * scale);
            dst.X = (int)Math.Round((GraphicsDevice.Viewport.Width - dst.Width) / 2.0);
            dst.Y = (int)Math.Round((GraphicsDevice.Viewport.Height - dst.Height) / 2.0);
            var sx = (posOnWindow.X - dst.X) / scale;
            var sy = (posOnWindow.Y - dst.Y) / scale;
            return new Point((int)sx, (int)sy);
        }
    }
}

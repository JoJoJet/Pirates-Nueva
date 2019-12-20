using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pirates_Nueva.Ocean;
using static Pirates_Nueva.NullableUtil;

namespace Pirates_Nueva
{
    /// <summary>
    /// An instance that can be updated from the <see cref="Master"/> object.
    /// </summary>
    internal interface IUpdatable
    {
        void Update(in UpdateParams @params);
    }
    /// <summary>
    /// Parameters for a method that is invoked every frame.
    /// </summary>
    public readonly struct UpdateParams
    {
        public Time Delta { get; }
        public Master Master { get; }

        public UpdateParams(Time time, Master master) {
            Delta = time;
            Master = master;
        }

        public void Deconstruct(out Time time, out Master master)
            => (time, master) = (Delta, Master);
    }

    /// <summary>
    /// Controls Rendering and calls the Update() functions for every type in the game.
    /// </summary>
    public sealed class Master : Game
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private UI.Font? font;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Renderer? renderer;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private PlayerController? player;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private Sea? sea;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private SpriteBatch? spriteBatch;
        private GraphicsDeviceManager graphics;

        public UI.Font Font => this.font ?? ThrowNotInitialized<UI.Font>(nameof(Master));

        public Input Input { get; }

        internal Renderer Renderer => this.renderer ?? ThrowNotInitialized<Renderer>(nameof(Master));
        public Screen Screen { get; }
        public UI.GUI GUI { get; }

        public PlayerController Player => this.player ?? ThrowNotInitialized<PlayerController>(nameof(Master));

        private SpriteBatch SpriteBatch => this.spriteBatch ?? ThrowNotInitialized<SpriteBatch>(nameof(Master));

        private Sea Sea => this.sea ?? ThrowNotInitialized<Sea>(nameof(Master));

        #region Initialization
        internal Master() {
            this.graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Screen = new Screen(this);
            GUI = new UI.GUI(this);

            Resources.Initialize(Content);
            Input = new Input(this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// <para />
        /// This is where it can query for any required services and load any non-graphic
        /// related content.
        /// <para />
        /// Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            // Make the mouse cursor visible onscreen.
            IsMouseVisible = true;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);

            this.font = Content.Load<SpriteFont>("font");

            AfterContentLoad();
        }

        /// <summary>
        /// Initialization performed after <see cref="LoadContent"/>.
        /// </summary>
        private void AfterContentLoad() {
            this.renderer = new Renderer(this, SpriteBatch);

            // Initialize the Sea object.
            this.sea = new Sea(this);

            this.player = new PlayerController(this, Sea);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
        }
        #endregion

        /// <summary>
        /// Create a new <see cref="UI.Sprite"/> with specified width, height, and pixel colors.
        /// </summary>
        public UI.Sprite CreateSprite(int width, int height, params UI.Color[] pixels) {
            var tex = new Texture2D(GraphicsDevice, width, height);
            tex.SetData(pixels);
            return new UI.Sprite(tex, 0, 0, width, height);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var delta = new Time(gameTime);

            var @params = new UpdateParams(delta, this);

            (Input as IUpdatable).Update(in @params);
            (GUI as IUpdatable).Update(in @params);

            (Player as IUpdatable).Update(in @params);
            (Sea as IUpdatable).Update(in @params);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin();

            (Sea as IDrawable<Screen>).Draw(Renderer);
            GUI.Draw(this);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public static class MasterExt
    {
        /// <summary> Merges every collection of <see cref="T"/>s into a single collection. </summary>
        public static IEnumerable<T> Union<T>(this IEnumerable<IEnumerable<T>> list) {
            foreach(var l in list) {
                foreach(var element in l)
                    yield return element;
            }
        }

        /// <summary>
        /// Determines whether the current <see cref="Type"/> derives from or is equal to the specified <see cref="Type"/>.
        /// </summary>
        public static bool IsSameOrSubclass(this Type type, Type other)
            => type == other || type.IsSubclassOf(other);

        /// <summary>
        /// Gets the inheritance depth of this <see cref="Type"/> with respect to the specified parent <see cref="Type"/>.
        /// </summary>
        public static int GetInheritanceDepth(this Type type, Type parent) {
            if(!type.IsSameOrSubclass(parent))
                throw new ArgumentException("Not a parent of this type!", nameof(parent));
            int depth = 0;
            while(type != parent) {
                ++depth;
                type = type.BaseType;
            }
            return depth;
        }
        /// <summary>
        /// Gets the inheritance depth of this <see cref="Type"/>.
        /// </summary>
        public static int GetInheritanceDepth(this Type type) {
            int depth = 0;
            while(type != null) {
                ++depth;
                type = type.BaseType;
            }
            return depth;
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pirates_Nueva.Ocean;

namespace Pirates_Nueva
{
    /// <summary>
    /// An instance that can be updated from the <see cref="Master"/> object.
    /// </summary>
    internal interface IUpdatable
    {
        void Update(Master master, Time delta);
    }
    /// <summary>
    /// An instance that can be drawn through the <see cref="Master"/> object.
    /// </summary>
    internal interface IDrawable
    {
        void Draw(Master master);
    }
    /// <summary>
    /// Controls Rendering and calls the Update() functions for every type in the game.
    /// </summary>
    public sealed class Master : Game
    {
        private Sea? _sea;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        public UI.Font Font { get; private set; }

        public Input Input { get; }

        public Renderer Renderer { get; private set; }
        public UI.GUI GUI { get; private set; }

        public PlayerController Player { get; private set; }

        internal Resources Resources { get; }

        private Sea Sea => this._sea ?? NullableUtil.ThrowNotInitialized<Sea>(nameof(Master));

        #region Initialization
        public Master() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            GUI = new UI.GUI(this);

            Resources = new Resources(this);
            Input = new Input(this);

            //
            // Satisfying the nullable reference types flow analysis.
            this.spriteBatch = null!;
            Renderer = null!;
            Font = null!;
            Player = null!;
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

            // Initialize the Def class.
            Def.Initialize(this);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(GraphicsDevice);

            Font = Content.Load<SpriteFont>("font");

            AfterContentLoad();
        }

        /// <summary>
        /// Initialization performed after <see cref="LoadContent"/>.
        /// </summary>
        private void AfterContentLoad() {
            Renderer = new Renderer(this, spriteBatch);

            // Initialize the Sea object.
            this._sea = new Sea(this);

            Player = new PlayerController(this, Sea);
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
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            if(GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var delta = new Time(gameTime);

            (Input as IUpdatable).Update(this, delta);
            (GUI as IUpdatable).Update(this, delta);

            (Player as IUpdatable).Update(this, delta);
            (Sea as IUpdatable).Update(this, delta);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            (Sea as IDrawable).Draw(this);
            (GUI as IDrawable).Draw(this);

            spriteBatch.End();

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
    }
}

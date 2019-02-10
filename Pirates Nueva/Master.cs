using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Pirates_Nueva
{
    /// <summary>
    /// An instance that can be updated from the <see cref="Master"/> object.
    /// </summary>
    internal interface IUpdatable
    {
        void Update(Master master);
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
        static Master _instance;

        GraphicsDeviceManager graphics;
        Sea sea;

        /// <summary> Prolly don't use this. Will likely be removed later. </summary>
        public static Master Instance => _instance ?? throw new InvalidOperationException($"{nameof(Master)} is uninitialized!");
        
        public SpriteBatch SpriteBatch { get; private set; }
        public SpriteFont Font { get; private set; }
        internal Resources Resources { get; }

        public GameTime FrameTime { get; private set; }
        public Input Input { get; }

        #region Initialization
        public Master() {
            _instance = this;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Resources = new Resources(this);
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

            // Initialize the Def class.
            Def.Initialize(this);
            
            this.sea = new Sea(this);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("font");

            // TODO: use this.Content to load your game content here
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

            FrameTime = gameTime;

            (Input as IUpdatable).Update(this);

            this.sea.Update(this);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            SpriteBatch.Begin();

            // TODO: Add your drawing code here
            this.sea.Draw(this);

            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }

    public static class MasterExt
    {
        /// <summary>
        /// Submit a rotated sprite for drawing in the current batch.
        /// </summary>
        public static void DrawRotated(
            this SpriteBatch spriteBatch, Texture2D texture, Rectangle destinationRectangle, float angle, Vector2 origin
            ) {
            spriteBatch.Draw(texture, destinationRectangle, null, Color.White, angle, origin, SpriteEffects.None, 0f);
        }

        /// <summary>
        /// Gets the time, in seconds, since last frame.
        /// </summary>
        public static float DeltaSeconds(this GameTime gameTime) => (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}

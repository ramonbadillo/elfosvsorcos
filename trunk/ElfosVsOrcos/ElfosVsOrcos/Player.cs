#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace ElfosVsOrcos
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
        public int Vida=10;

        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation atackAnimation;
        private Animation dieAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;
        

        private Texture2D idleI;
        private Texture2D runI;
        private Texture2D jumpI;



        // Sounds
        private SoundEffect killedSound;
        private SoundEffect jumpSound;
        private SoundEffect fallSound;

        public Level Level
        {
            get { return level; }
        }
        Level level;

        public bool IsAlive
        {
            get { return isAlive; }
        }
        bool isAlive;

        // Physics state
        public Vector2 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Vector2 position;

        private float previousBottom;

        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f; 

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        /// <summary>
        /// Gets whether or not the player's feet are on the ground.
        /// </summary>
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        /// <summary>
        /// Current user movement input.
        /// </summary>
        private float movement;

        // Jumping state
        private bool isJumping;
        public bool isAtacando;
        private bool wasJumping;
        private float jumpTime;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this player in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        /// <summary>
        /// Constructors a new player.
        /// </summary>
        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        /// <summary>
        /// Loads the player sprite sheet and sounds.
        /// </summary>
        public void LoadContent()
        {
            // Load animated textures.
            int spritemono=31;
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Nada"), 0.2f, true, spritemono);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Corre"), 0.2f, true, spritemono);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Salta"), 0.2f, true, spritemono);
            atackAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Ataca"), 0.2f, true,54);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Corre"), 0.1f, true,20);


            //idleI = new Texture2D(as,);

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.9);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.9);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
            
            // Load sounds.            
            killedSound = Level.Content.Load<SoundEffect>("Sounds/PlayerKilled");
            jumpSound = Level.Content.Load<SoundEffect>("Sounds/PlayerJump");
            fallSound = Level.Content.Load<SoundEffect>("Sounds/PlayerFall");
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState)
        {
            
            //Console.WriteLine(gameTime.ElapsedGameTime.Duration());
            GetInput(keyboardState, gamePadState);

            ApplyPhysics(gameTime);

            if (IsAlive && IsOnGround)
            {
                if (Math.Abs(Velocity.X) - 0.02f > 0)
                {
                    sprite.PlayAnimation(runAnimation);
                }
                else if (isAtacando)
                {
                    sprite.PlayAnimation(atackAnimation);
                    //killedSound.Play();
                    
                }
                else
                {
                    sprite.PlayAnimation(idleAnimation);
                }
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(
            KeyboardState keyboardState, 
            GamePadState gamePadState)
        {
            if(colormono==Color.Red)
            ti++;
            if (ti == 10)
                isAtacando = false;
            if (ti == 20)
            {
                ti = 0;
                colormono = Color.White;
                GamePad.SetVibration(PlayerIndex.One, 0, 0);
                
            }


            Console.WriteLine(ti);
                // Get analog horizontal movement.
                movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

                // Ignore small movements to prevent running in place.
                if (Math.Abs(movement) < 0.5f)
                    movement = 0.0f;


                // If any digital horizontal movement input is found, override the analog movement.
                if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                    keyboardState.IsKeyDown(Keys.Left) ||
                    keyboardState.IsKeyDown(Keys.A))
                {
                    movement = -1.0f;
                }
                else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                         keyboardState.IsKeyDown(Keys.Right) ||
                         keyboardState.IsKeyDown(Keys.D))
                {
                    movement = 1.0f;
                }
                
                
                isAtacando = keyboardState.IsKeyDown(Keys.Space)||
                    gamePadState.IsButtonDown(Buttons.B);
                
                //Console.WriteLine(isAtacando);
                // Check if the player wants to jump.
                isJumping =
                    gamePadState.IsButtonDown(JumpButton) ||
                    keyboardState.IsKeyDown(Keys.Up) ||
                    keyboardState.IsKeyDown(Keys.W);

                if (gamePadState.IsButtonDown(Buttons.Y))
                    GamePad.SetVibration(PlayerIndex.One, 0f, 0f);
                if (gamePadState.IsButtonDown(Buttons.X))
                    GamePad.SetVibration(PlayerIndex.One, 1f, 1f);
            
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            velocity.Y = DoJump(velocity.Y, gameTime);

            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        public void Atacar(GameTime gameTime) {
            
            if (isAtacando)
                sprite.PlayAnimation(atackAnimation);
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {
                    if (jumpTime == 0.0f)
                        jumpSound.Play();

                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if(isAtacando)
                        sprite.PlayAnimation(atackAnimation);
                    else
                        sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (1.0f - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;

                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        /// 
        int ti = 0;
        public void OnKilled(Enemy killedBy)
        {
            if (Vida <= 0)
            {
                isAlive = false;

                if (killedBy != null)
                {
                    killedSound.Play();
                    colormono = Color.Black;
                }
                else
                    fallSound.Play();
            }
            else
            {
                if (killedBy != null)
                {
                    Vida--;
                    colormono = Color.Red;
                    killedSound.Play();
                    GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
                    if (Velocity.X > 0)
                    {
                        Position = new Vector2(Position.X - 50, Position.Y);

                    }
                    else if (Velocity.X < 0)
                        Position = new Vector2(Position.X + 50, Position.Y);
                }
            }
            if (killedBy == null)
                isAlive = false;
            if (ti == 100) {
                ti = 0;
                GamePad.SetVibration(PlayerIndex.One, 0,0);
            }
                
            //sprite.PlayAnimation(dieAnimation);
        }
        
        public void OnKilled(FlyingEnemy killedByFl)
        {
            Console.WriteLine(ti);
            //if(colormono==Color.Red)
            //ti++;
            if (Vida <= 0)
            {
                isAlive = false;

                if (killedByFl != null)
                {
                    killedSound.Play();
                    ti = 0;
                    Vida = 10;
                }
                else
                {
                    fallSound.Play();
                    ti = 0;
                    Vida = 10;
                }
            }
            else
            {
                if (killedByFl != null)
                {
                    Vida--;
                    
                    Console.WriteLine(ti);
                    colormono = Color.Red;
                    killedSound.Play();
                    GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
                    if (Velocity.X > 0)
                    {
                        Position = new Vector2(Position.X - 50, Position.Y);

                    }
                    else if (Velocity.X < 0)
                        Position = new Vector2(Position.X + 50, Position.Y);
                }
            }
            if (killedByFl == null)
                isAlive = false;
            if (ti == 100)
            {
                ti = 0;
                colormono = Color.White;
                GamePad.SetVibration(PlayerIndex.One, 0, 0);
            }
                
        }
        public void OnKilled(LatexEnemy killedByLatex)
        {
            isAlive = false;

            if (killedByLatex != null)
                killedSound.Play();
            else
                fallSound.Play();

            //sprite.PlayAnimation(dieAnimation);
        }
        private Color colormono=Color.White;
        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(idleAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X > 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X < 0)
                flip = SpriteEffects.None;

            // Draw that sprite.
            if(this.isAlive)
                sprite.Draw(gameTime, spriteBatch, Position, flip,colormono);
        }
    }
}

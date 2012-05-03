#region File Description

#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElfosVsOrcos
{
    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Bala
    {
        public Level Level
        {
            get { return level; }
        }
        Level level;


        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
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

        public Rectangle Vision
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X - 200;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width + 200, localBounds.Height);
            }
        }

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private AnimationPlayer sprite;

        /// <summary>
        /// The direction this enemy is facing and moving along the X axis.
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;
        private FaceDirection dir = FaceDirection.Left;
        private FaceDirection dir2 = FaceDirection.Down;
        

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float MoveSpeed = 64.0f;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public Bala(Level level, Vector2 position, string spriteSet, int EAS)
        {
            this.level = level;
            this.position = position;

            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {

            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.1f, false, 24);
            //Console.WriteLine(spriteSet + "Run");
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.1f, false, 24);
            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

        }


        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        /// 
        int i = 0;
        public void Update(GameTime gameTime)
        {

            
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

           
               // dir = (FaceDirection)(int)direction;
                Vector2 velocity = new Vector2(0.0f,-(int)direction * MoveSpeed * elapsed );
                //int vel = 10;
                position=position+velocity;
               

            
            //Review if the player is near of the latex enemy
            if (level.Player.BoundingRectangle.Right >= this.Vision.Left && level.Player.BoundingRectangle.Right <= this.Vision.Right)
            {

                //dir = (FaceDirection)(int)direction;
                //Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                //position = position + velocity;
            }
            else if (level.Player.BoundingRectangle.Top >= this.Vision.Right && level.Player.BoundingRectangle.Top <= this.Vision.Right+200)
                    {
                        //dir = (FaceDirection)(-(int)direction);
                        //Vector2 velocity = new Vector2(-(int)direction * MoveSpeed * elapsed, 0.0f);
                        //position = position + velocity;
                    }

        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Stop running when the game is paused or before turning around.
            if (!Level.Player.IsAlive ||
                Level.ReachedExit ||
                Level.TimeRemaining == TimeSpan.Zero)
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }


            // Draw facing the way the enemy is moving.
            SpriteEffects flip = dir > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip, Color.White);
        }
    }
}

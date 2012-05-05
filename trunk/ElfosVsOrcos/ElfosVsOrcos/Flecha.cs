#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ElfosVsOrcos
{
    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class Flecha
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

        

        // Animations
        private Animation runAnimation;
        private Animation idleAnimation;
        private AnimationPlayer sprite;

        /// <summary>
        /// The direction of the arrow.
        /// </summary>
        private FaceDirection direction = FaceDirection.Left;


        /// <summary>
        /// The speed of the arrow.
        /// </summary>
        private const float MoveSpeed = 64.0f;
        private bool derecha;
        /// <summary>
        /// Constructs a new Arrow.
        /// </summary>
        public Flecha(Level level, Vector2 position, string spriteSet, bool derecha)
        {
            this.level = level;
            this.position = position;
            this.derecha = derecha;
            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {

            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Flecha"), 0.1f, false, 30);
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Flecha"), 0.1f, false, 30);
            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

        }


        /// <summary>
        /// Update position
        /// </summary>
        /// 
        public void Update(GameTime gameTime)
        {

            
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);


            if (!derecha)
            {
                direction = FaceDirection.Right;
                //Update position
                Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed*4, 0.0f);
                position = position + velocity;
            }
            else
            {
                direction = FaceDirection.Left;
                Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed*4, 0.0f);
                position = position + velocity;
            }
             

        }

        /// <summary>
        /// Draws the animated arrow.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Play animation
            
                sprite.PlayAnimation(runAnimation);


            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            sprite.Draw(gameTime, spriteBatch, Position, flip, Color.White);
        }
    }
}

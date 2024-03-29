﻿#region File Description
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
    class JefedeJefes
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
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X -200;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;
                
                return new Rectangle(left, top, localBounds.Width+200, localBounds.Height);
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

        /// <summary>
        /// How long this enemy has been waiting before turning around.
        /// </summary>
        private float waitTime;

        /// <summary>
        /// How long to wait before turning around.
        /// </summary>
        private const float MaxWaitTime = .03f;

        /// <summary>
        /// The speed at which this enemy moves along the X axis.
        /// </summary>
        private const float MoveSpeed = 100.0f;


        private int Vida = 10;

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public JefedeJefes(Level level, Vector2 position, string spriteSet, int TES)
        {
            this.level = level;
            this.position = position;
            this.TES = TES;
            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            // Load animations.
            spriteSet = "Sprites/" + spriteSet + "/";
            runAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Run"), 0.08f, true,93);
            //Console.WriteLine(spriteSet + "Run");
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet + "Idle"), 0.1f, true,177);
            sprite.PlayAnimation(idleAnimation);

            // Calculate bounds within texture size.
            int width = (int)(runAnimation.FrameWidth * 0.4);
            int left = (runAnimation.FrameWidth - width) / 2;
            int height = (int)(runAnimation.FrameWidth * 0.7);
            int top = runAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
            
        }


        public void menosVida() {
            Vida--;
            colormono = Color.Red;
        }

        public int getVida() {
            return Vida;
        }




        /// <summary>
        /// Paces back and forth along a platform, waiting at either end.
        /// </summary>
        /// 
        int i = 0;
        int f = 0;
        int TES;
        Color colormono = Color.White;
        public void Update(GameTime gameTime)
        {
            if (colormono == Color.Red)
                f++;
            if (f >= 20) {
                f = 0;
                colormono = Color.White;
            }
                




            i++;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (i > TES)
            {
                direction = (FaceDirection)(-(int)direction);
                i = 0;

            }
            
            elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Calculate tile position based on the side we are walking towards.
            float posX = Position.X + localBounds.Width / 2 * (int)direction;
            int tileX = (int)Math.Floor(posX / Tile.Width) - (int)direction;
            int tileY = (int)Math.Floor(Position.Y / Tile.Height);

            if (waitTime > 0)
            {
                // Wait for some amount of time.
                waitTime = Math.Max(0.0f, waitTime - (float)gameTime.ElapsedGameTime.TotalSeconds);
                if (waitTime <= 0.0f)
                {
                    // Then turn around.
                    direction = (FaceDirection)(-(int)direction);
                }
            }
            else
            {
                // If we are about to run into a wall or off a cliff, start waiting.
                if (Level.GetCollision(tileX + (int)direction, tileY - 1) == TileCollision.Impassable ||
                    Level.GetCollision(tileX + (int)direction, tileY) == TileCollision.Passable)
                {
                    waitTime = MaxWaitTime;
                }
                else
                {
                    // Move in the current direction.
                    Vector2 velocity = new Vector2((int)direction * MoveSpeed * elapsed, 0.0f);
                    position = position + velocity;
                }

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
                Level.TimeRemaining == TimeSpan.Zero ||
                waitTime > 0 || (level.Player.BoundingRectangle.Right >= this.Vision.Left && level.Player.BoundingRectangle.Right <= this.Vision.Right) || (level.Player.BoundingRectangle.Left >= this.Vision.Right && level.Player.BoundingRectangle.Left <= this.Vision.Right + 200))
            {
                sprite.PlayAnimation(idleAnimation);
            }
            else
            {
                sprite.PlayAnimation(runAnimation);
            }


            // Draw facing the way the enemy is moving.
            SpriteEffects flip = direction > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            sprite.Draw(gameTime, spriteBatch, Position, flip,colormono);
        }
    }
}

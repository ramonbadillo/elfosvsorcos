#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace ElfosVsOrcos
{
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {


        // Physical structure of the level.
        private Tile[,] tiles;
        private Texture2D[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        private List<Gem> gems = new List<Gem>();
        private List<Enemy> enemies = new List<Enemy>();
        private List<FlyingEnemy> enemiesFl = new List<FlyingEnemy>();
        private List<LatexEnemy> enemiesLatex = new List<LatexEnemy>();
        private List<Orcos> enemiesOrco = new List<Orcos>();
        private List<JefedeJefes> Jefes = new List<JefedeJefes>();
        private List<Flecha> Flechas = new List<Flecha>();
        private List<Bala> enemiesBala = new List<Bala>();



        List<Enemy> muertos = new List<Enemy>();
        List<FlyingEnemy> muertosFl = new List<FlyingEnemy>();
        List<LatexEnemy> muertosLatex = new List<LatexEnemy>();
        List<Orcos> muertosOrcos = new List<Orcos>();
        List<JefedeJefes> muertosJefes = new List<JefedeJefes>();
        List<Flecha> flechasusadas = new List<Flecha>();
        List<Bala> muertosBala = new List<Bala>();


        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed
       
        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 5;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        /// 
        int indexLevel;
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex)
        {


            this.indexLevel = levelIndex;
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromMinutes(2.0);
            
            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            
            
            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/ExitReached");
        }

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count];

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            if (exit == InvalidPosition)
                throw new NotSupportedException("A level must have an exit.");

        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y)
        {
            switch (tileType)
            {
                // Blank space
                case '.':
                    return new Tile(null, TileCollision.Passable);

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Gem
                case 'G':
                    return LoadGemTile(x, y);

                // Floating platform
                case '-':
                    return LoadTile("Platform", TileCollision.Platform);

                // Various enemies
                case 'O':
                    return LoadOrcoTile(x, y, "Orco");
                case 'H':
                   return LoadEnemyTile(x, y, "Halcon");

                // Platform block
                case '~':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Platform);

                // Passable block
                case ':':
                    return LoadVarietyTile("BlockB", 2, TileCollision.Passable);

                // Player 1 start point
                case '1':
                    return LoadStartTile(x, y);

                // Impassable block
                case '#':
                    //Console.WriteLine("BlockA" + indexLevel + "_");
                    return LoadVarietyTile("BlockA"+indexLevel+"_", 3, TileCollision.Impassable);

                case 'M':
                    return LoadTileM(x, y, "Volador");

                case 'V':
                    return LoadFlyingEnemyTile(x, y, "Volador");

                case 'L':
                    return LoadLatexEnemyTile(x, y, "Latex");


                case 'J':
                    return LoadJefeTile(x, y, "JefedeJefes");

                default:
                     throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision);
        }


        /// <summary>
        /// Loads a tile with a random appearance.
        /// </summary>
        /// <param name="baseName">
        /// The content name prefix for this group of tile variations. Tile groups are
        /// name LikeThis0.png and LikeThis1.png and LikeThis2.png.
        /// </param>
        /// <param name="variationCount">
        /// The number of variations in this group.
        /// </param>
        private Tile LoadVarietyTile(string baseName, int variationCount, TileCollision collision)
        {
            int index = random.Next(variationCount);
            return LoadTile(baseName  +index, collision);

        }


        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            return LoadTile("Exit", TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet,150));

            return new Tile(null, TileCollision.Passable);
        }
        private Tile LoadFlyingEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemiesFl.Add(new FlyingEnemy(this, position, spriteSet, 500));

            return new Tile(null, TileCollision.Passable);
        }


        private Tile LoadTileM(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemiesFl.Add(new FlyingEnemy(this, position, spriteSet, 500));

            return new Tile(null, TileCollision.Impassable);
        }

        private Tile LoadLatexEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemiesLatex.Add(new LatexEnemy(this, position, spriteSet, 150));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadOrcoTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemiesOrco.Add(new Orcos(this, position, spriteSet,200));

            return new Tile(null, TileCollision.Passable);
        }

        private Tile LoadJefeTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            Jefes.Add(new JefedeJefes(this, position, spriteSet, 50));
            return new Tile(null, TileCollision.Passable);
        }



        /// <summary>
        /// Instantiates a gem and puts it in the level.
        /// </summary>
        private Tile LoadGemTile(int x, int y)
        {
            Point position = GetBounds(x, y).Center;
            gems.Add(new Gem(this, new Vector2(position.X, position.Y)));

            return new Tile(null, TileCollision.Passable);
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }

        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>


        Camara cam;

        public Vector2 getCam() {
            return cam._pos;
        }

        public void Update(GameTime gameTime, KeyboardState keyboardState, GamePadState gamePadState,SpriteBatch sprites,Camara cam, ScreenManager screen){
            this.cam = cam;
            if (keyboardState.IsKeyDown(Keys.T)) {
                //cam.MoveRight(10.0f);
            //cam.Move(new Vector2(30,20)) ;
            //Console.WriteLine(cam.Pos.X);
            //Console.WriteLine(player.Position.X - cam.Pos.X);
            //cam.Zoom += 0.01f;
            }
             
            if (keyboardState.IsKeyDown(Keys.R))
                cam.Zoom -= 0.01f;

            if (player.Position.X - cam.Pos.X > 100)
                if (keyboardState.IsKeyDown(Keys.Right) || gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.IsButtonDown(Buttons.LeftThumbstickRight))
                {
                    //player.position.X = player.position.X - 4.5f;
                    cam.MoveRight(3.8f);
                }
            if (cam.Pos.X -player.Position.X   > 150)
                if (keyboardState.IsKeyDown(Keys.Left) || gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.IsButtonDown(Buttons.LeftThumbstickLeft))
                {
                    //player.position.X = player.position.X - 4.5f;
                    cam.MoveLeft(3.8f);
                }
            
            
            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                Player.ApplyPhysics(gameTime);

            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState, gamePadState);
                
                    
                UpdateGems(gameTime);

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.IsOnGround &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
            
        }

        /// <summary>
        /// Animates each gem and checks to allows the player to collect them.
        /// </summary>
        private void UpdateGems(GameTime gameTime)
        {
            for (int i = 0; i < gems.Count; ++i)
            {
                Gem gem = gems[i];

                gem.Update(gameTime);

                if (gem.BoundingCircle.Intersects(Player.BoundingRectangle))
                {
                    gems.RemoveAt(i--);
                    OnGemCollected(gem, Player);
                }
                
            }
        }
        
        public void addOrc(Orcos orco) {
            enemiesOrco.Add(orco);
        }

        public void addFlecha(Flecha flecha)
        {
            Flechas.Add(flecha);
        }

        public void addBal(Bala bala)
        {
            enemiesBala.Add(bala);
        }

        
        public void delHalcon(Enemy hal)
        {
            //enemiesHalcon.Add(hal);
            
            muertos.Add(hal);
        }

        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        /// 

        private void UpdateEnemies(GameTime gameTime)
        {

            foreach (Bala enemyBala in enemiesBala)
            {
                enemyBala.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemyBala.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {

                    OnPlayerKilledBala(enemyBala);
                    muertosBala.Add(enemyBala);
                }
            }
            foreach (Bala enemyBala in muertosBala)
            {
                enemiesBala.Remove(enemyBala);
            }


            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);
                
                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (!Player.Atacando)
                        OnPlayerKilled(enemy);
                    else
                        muertos.Add(enemy);
                }
                else
                {
                    foreach (Flecha flecha in Flechas)
                    {
                        if (flecha.BoundingRectangle.Intersects(enemy.BoundingRectangle))
                        {
                            muertos.Add(enemy);
                            flechasusadas.Add(flecha);
                        }

                    }

                }
                
            }
            foreach (Enemy enemy in muertos) {
                enemies.Remove(enemy);
            }


            foreach (JefedeJefes enemy in Jefes)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (!Player.Atacando)
                        OnPlayerKilledJefe(enemy);
                    else
                    {
                        muertosJefes.Add(enemy);
                        enemy.menosVida();
                    }
                }
                else {
                    foreach (Flecha flecha in Flechas) {
                        if (flecha.BoundingRectangle.Intersects(enemy.BoundingRectangle)) {
                            muertosJefes.Add(enemy);
                            enemy.menosVida();
                            flechasusadas.Add(flecha);
                        }

                    }
                
                }

            }
            foreach (JefedeJefes enemy in muertosJefes)
            {
                if(enemy.getVida()<=0)
                    Jefes.Remove(enemy);
            }

            foreach (Flecha flecha in flechasusadas)
            {
                    Flechas.Remove(flecha);
            }

            
            //para enemigos voladores

            foreach (FlyingEnemy enemyFl in enemiesFl)
            {
                enemyFl.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemyFl.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (enemyFl.Position.X == player.Position.X) {
                        
                    }

                    if (!Player.Atacando)
                        OnPlayerKilledFl(enemyFl);
                    else
                        muertosFl.Add(enemyFl);
                }
                else
                {
                    foreach (Flecha flecha in Flechas)
                    {
                        if (flecha.BoundingRectangle.Intersects(enemyFl.BoundingRectangle))
                        {
                            muertosFl.Add(enemyFl);
                            flechasusadas.Add(flecha);
                        }

                    }

                }
            }

            

            foreach (FlyingEnemy enemyFl in muertosFl)
            {
                enemiesFl.Remove(enemyFl);
            }



            foreach (Flecha flecha in Flechas)
            {
                flecha.Update(gameTime);
            }



            //para enemigos latex

            foreach (LatexEnemy enemyLatex in enemiesLatex)
            {
                enemyLatex.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemyLatex.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (enemyLatex.Position.X == player.Position.X)
                    {

                    }

                    if (!Player.Atacando)
                        OnPlayerKilledLatex(enemyLatex);
                    else
                        muertosLatex.Add(enemyLatex);
                }
                else
                {
                    foreach (Flecha flecha in Flechas)
                    {
                        if (flecha.BoundingRectangle.Intersects(enemyLatex.BoundingRectangle))
                        {
                            muertosLatex.Add(enemyLatex);
                            flechasusadas.Add(flecha);
                        }

                    }

                }
            }
            foreach (LatexEnemy enemyLatex in muertosLatex)
            {
                enemiesLatex.Remove(enemyLatex);
            }

            //Para enemigos Orcos
            foreach (Orcos enemy in enemiesOrco)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    if (!Player.Atacando)
                        OnPlayerKilledOrco(enemy);
                    else
                        muertosOrcos.Add(enemy);
                }
                else
                {
                    foreach (Flecha flecha in Flechas)
                    {
                        if (flecha.BoundingRectangle.Intersects(enemy.BoundingRectangle))
                        {

                            muertosOrcos.Add(enemy);
                            flechasusadas.Add(flecha);
                        }

                    }

                }

            }
            foreach (Orcos enemy in muertosOrcos)
            {
                enemiesOrco.Remove(enemy);
            }


        }



        /// <summary>
        /// Called when a gem is collected.
        /// </summary>
        /// <param name="gem">The gem that was collected.</param>
        /// <param name="collectedBy">The player who collected this gem.</param>
        private void OnGemCollected(Gem gem, Player collectedBy)
        {
            score += Gem.PointValue;

            gem.OnCollected(collectedBy);
        }


        public int getLifePlayer() {
            return player.Vida;
        }


        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(killedBy);
        }
        private void OnPlayerKilledJefe(JefedeJefes killedBy)
        {
            Player.OnKilled(killedBy);
        }

        private void OnPlayerKilledBala(Bala killedBy)
        {
            Player.OnKilledBA(killedBy);
        }
        private void OnPlayerKilledFl(FlyingEnemy killedByFl)
        {
            Player.OnKilled(killedByFl);
        }
        private void OnPlayerKilledLatex(LatexEnemy killedByLatex)
        {
            Player.OnKilled(killedByLatex);
        }

        private void OnPlayerKilledOrco(Orcos killedByOrco)
        {
            Player.OnKilled(killedByOrco);
        }
        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            exitReachedSound.Play();
            reachedExit = true;
            
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
            cam.Pos = new Vector2(400,240);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

    
            
             
             


            

            

            Player.Draw(gameTime, spriteBatch);


            foreach (Bala enemyBala in enemiesBala)
                enemyBala.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            foreach (JefedeJefes enemy in Jefes)
                enemy.Draw(gameTime, spriteBatch);

            foreach (FlyingEnemy enemyFl in enemiesFl)
               enemyFl.Draw(gameTime, spriteBatch);
            
            foreach (LatexEnemy enemyLatex in enemiesLatex)
                enemyLatex.Draw(gameTime, spriteBatch);


            foreach (Flecha flecha in Flechas) 
                flecha.Draw(gameTime, spriteBatch);



            foreach (Orcos enemyOrco in enemiesOrco)
                enemyOrco.Draw(gameTime, spriteBatch);
            
            
            DrawTiles(spriteBatch);
            DrawTiles(spriteBatch);
            

            //foreach (Gem gem in gems)
            //    gem.Draw(gameTime, spriteBatch);

            //foreach (Gem gem in gems)
            //    gem.Draw(gameTime, spriteBatch);



            //for (int i = EntityLayer + 1; i < layers.Length; ++i)
            //    spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
            //spriteBatch.Draw();
            //for (int i = 0; i <= ; ++i)
                //spriteBatch.Draw(layers[0], Vector2.Zero, Color.Red);
                //spriteBatch.Draw(layers[1], Vector2.Zero, Color.Blue);

            //for (int i = EntityLayer + 1; i < layers.Length; ++i)
                //spriteBatch.Draw(layers[i], Vector2.Zero, Color.White);
                //spriteBatch.Draw(layers[0], Vector2.Zero, Color.White);

        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = 0; x < Width; ++x)
                {
                    // If there is a visible tile in that position
                    
                    Texture2D texture = tiles[x , y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        #endregion
    }
}

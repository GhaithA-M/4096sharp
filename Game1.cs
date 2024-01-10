using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace _4096sharp
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D tileTexture; // Texture for the tiles
        private int[,] grid; // The game grid

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        public int tileSize = 48; // Tile size
        public int tileSpacing = 6; // Space between tiles

        protected override void Initialize()
        {
            // Calculate total size for one row/column, plus extra space for two tiles and spacing
            int totalSizeWithExtraWidth = (8 * tileSize) + (7 * tileSpacing) + (1 * (tileSize + tileSpacing));
            int totalSizeWithExtraHeight = (8 * tileSize) + (7 * tileSpacing) + (4 * (tileSize + tileSpacing));

            grid = new int[8, 8];

            // Set the window size with extra space
            _graphics.PreferredBackBufferWidth = totalSizeWithExtraWidth; // Width with extra space
            _graphics.PreferredBackBufferHeight = totalSizeWithExtraHeight; // Height with extra space
            _graphics.ApplyChanges();

            base.Initialize();
        }

        private SpriteFont gameFont;

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            tileTexture = new Texture2D(GraphicsDevice, 1, 1);
            tileTexture.SetData(new[] { Color.White });

            // Load the SpriteFont
            gameFont = Content.Load<SpriteFont>("Fonts/ClearSans-Regular");
            AddRandomTile();
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState state = Keyboard.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up))
            {
                MoveTilesUp();
            }
            else if (state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down))
            {
                MoveTilesDown();
            }
            else if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.Left))
            {
                MoveTilesLeft();
            }
            else if (state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.Right))
            {
                MoveTilesRight();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_spriteBatch == null || tileTexture == null || gameFont == null || grid == null)
            {
                return; // Exit if any essential components are null
            }

            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            int totalGridSize = (8 * tileSize) + (7 * tileSpacing);
            int startX = (_graphics.PreferredBackBufferWidth - totalGridSize) / 2;
            int startY = (_graphics.PreferredBackBufferHeight - totalGridSize) / 2;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int tileX = startX + i * (tileSize + tileSpacing);
                    int tileY = startY + j * (tileSize + tileSpacing);
                    Color tileColor = GetTileColor(grid[i, j]);

                    // Draw the tile with the color based on its value
                    _spriteBatch.Draw(tileTexture, new Rectangle(tileX, tileY, tileSize, tileSize), tileColor);

                    // Draw the number on the tile
                    if (grid[i, j] > 0)
                    {
                        string text = grid[i, j].ToString();
                        Vector2 textSize = gameFont.MeasureString(text);
                        Vector2 textPosition = new Vector2(tileX + (tileSize - textSize.X) / 2, tileY + (tileSize - textSize.Y) / 2);
                        _spriteBatch.DrawString(gameFont, text, textPosition, Color.Black); // Choose a contrasting text color
                    }
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }


        private void AddRandomTile()
        {
            var emptyTiles = new List<Point>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (grid[x, y] == 0) // 0 represents an empty tile
                    {
                        emptyTiles.Add(new Point(x, y));
                    }
                }
            }

            if (emptyTiles.Count > 0)
            {
                var random = new Random();
                var randomTile = emptyTiles[random.Next(emptyTiles.Count)];
                grid[randomTile.X, randomTile.Y] = random.Next(2) * 2 + 2; // Assigns either 2 or 4
            }
        }

        private void MoveTilesUp()
        {
            for (int col = 0; col < 8; col++)
            {
                for (int row = 1; row < 8; row++)
                {
                    if (grid[col, row] > 0)
                    {
                        int newRow = row;
                        while (newRow > 0 && grid[col, newRow - 1] == 0)
                        {
                            newRow--;
                        }

                        if (newRow > 0 && grid[col, newRow - 1] == grid[col, row])
                        {
                            // Merge tiles
                            grid[col, newRow - 1] *= 2;
                            grid[col, row] = 0;
                        }
                        else if (newRow != row)
                        {
                            // Move tile
                            grid[col, newRow] = grid[col, row];
                            grid[col, row] = 0;
                        }
                    }
                }
            }
            AddRandomTile();
        }

        private void MoveTilesDown()
        {
            bool moved = false;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 7; y >= 0; y--)
                {
                    if (grid[x, y] > 0)
                    {
                        int targetY = y;
                        while (targetY < 7 && grid[x, targetY + 1] == 0)
                        {
                            targetY++;
                        }

                        if (targetY != y)
                        {
                            grid[x, targetY] = grid[x, y];
                            grid[x, y] = 0;
                            moved = true;
                        }

                        if (targetY < 7 && grid[x, targetY + 1] == grid[x, targetY])
                        {
                            grid[x, targetY + 1] *= 2;
                            grid[x, targetY] = 0;
                            moved = true;
                        }
                    }
                }
            }

            if (moved)
            {
                AddRandomTile();
            }
        }

        private void MoveTilesLeft()
        {
            bool moved = false;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if (grid[x, y] > 0)
                    {
                        int targetX = x;
                        while (targetX > 0 && grid[targetX - 1, y] == 0)
                        {
                            targetX--;
                        }

                        if (targetX != x)
                        {
                            grid[targetX, y] = grid[x, y];
                            grid[x, y] = 0;
                            moved = true;
                        }

                        if (targetX > 0 && grid[targetX - 1, y] == grid[targetX, y])
                        {
                            grid[targetX - 1, y] *= 2;
                            grid[targetX, y] = 0;
                            moved = true;
                        }
                    }
                }
            }

            if (moved)
            {
                AddRandomTile();
            }
        }

        private void MoveTilesRight()
        {
            bool moved = false;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 7; x >= 0; x--)
                {
                    if (grid[x, y] > 0)
                    {
                        int targetX = x;
                        while (targetX < 7 && grid[targetX + 1, y] == 0)
                        {
                            targetX++;
                        }

                        if (targetX != x)
                        {
                            grid[targetX, y] = grid[x, y];
                            grid[x, y] = 0;
                            moved = true;
                        }

                        if (targetX < 7 && grid[targetX + 1, y] == grid[targetX, y])
                        {
                            grid[targetX + 1, y] *= 2;
                            grid[targetX, y] = 0;
                            moved = true;
                        }
                    }
                }
            }

            if (moved)
            {
                AddRandomTile();
            }
        }

        private Color GetTileColor(int value)
        {
            return value switch
            {
                2 => Color.LightBlue,
                4 => Color.CornflowerBlue,
                8 => Color.RoyalBlue,
                16 => Color.Orange,
                32 => Color.DarkOrange,
                64 => Color.Red,
                128 => Color.Green,
                256 => Color.LightGreen,
                512 => Color.Yellow,
                1024 => Color.Gold,
                2048 => Color.Purple,
                4096 => Color.Magenta,
                _ => Color.Gray, // Default color for blank tile
            };
        }
    }
}
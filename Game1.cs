using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;

namespace _4096sharp
{
    public class Game1 : Game
    {
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D tileTexture; // Texture for the tiles
        private int[,] grid; // The game grid
        public int tileSize = 64; // Tile size
        public int tileSpacing = 6; // Space between tiles
        private const int TilesToAddPerMove = 4; // Number of tiles to add after each move
        private bool hasWon = false;
        private bool hasLost = false;
        private KeyboardState _previousKeyboardState;
        private SpriteFont gameFont;

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

            if (state.IsKeyDown(Keys.Escape))
                Exit();

            if (IsKeyPressed(Keys.R, state))
            {
                ResetGame();
                return;
            }

            // Disable movement if the player has won or lost
            if (hasWon || hasLost)
                return;

            // Movement controls
            if (IsKeyPressed(Keys.W, state) || IsKeyPressed(Keys.Up, state))
                MoveTilesUp();
            else if (IsKeyPressed(Keys.S, state) || IsKeyPressed(Keys.Down, state))
                MoveTilesDown();
            else if (IsKeyPressed(Keys.A, state) || IsKeyPressed(Keys.Left, state))
                MoveTilesLeft();
            else if (IsKeyPressed(Keys.D, state) || IsKeyPressed(Keys.Right, state))
                MoveTilesRight();

            _previousKeyboardState = state; // Update the previous keyboard state at the end of the method

            base.Update(gameTime);
        }

        private bool IsKeyPressed(Keys key, KeyboardState currentState)
        {
            return currentState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (_spriteBatch == null || tileTexture == null || gameFont == null || grid == null)
            {
                return; // Exit if any essential components are null
            }

            GraphicsDevice.Clear(Color.DarkGray);
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

            if (hasWon || hasLost)
            {
                string endGameMessage = hasWon ? "Congratulations! You've won!" : "Game Over! No more moves available!";
                DrawEndGameMessage(endGameMessage);
                DrawResetButton();
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

        private void CheckForWin()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (grid[x, y] == 4096) // Define which tile causes a win
                    {
                        hasWon = true;
                        return;
                    }
                }
            }
        }

        private void CheckForLoss()
        {
            // Check if there are any empty spaces left
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (grid[x, y] == 0)
                        return; // An empty space is found, so the game can continue
                }
            }

            // Check if any moves are possible
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    // Check right
                    if (x < 7 && grid[x, y] == grid[x + 1, y])
                        return;

                    // Check down
                    if (y < 7 && grid[x, y] == grid[x, y + 1])
                        return;
                }
            }

            // If no empty spaces and no possible merges, the player has lost
            hasLost = true;
        }

        private void DrawEndGameMessage(string message)
        {
            Vector2 messageSize = gameFont.MeasureString(message);
            Vector2 messagePosition = new Vector2(_graphics.PreferredBackBufferWidth / 2 - messageSize.X / 2,
                                                  _graphics.PreferredBackBufferHeight / 2 - messageSize.Y / 2);

            // Grey out the game (optional, if you want to keep the grey-out effect)
            Texture2D overlay = new Texture2D(GraphicsDevice, 1, 1);
            overlay.SetData(new[] { Color.White * 0.5f });
            _spriteBatch.Draw(overlay, new Rectangle(0, 0, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight), Color.White);

            _spriteBatch.DrawString(gameFont, message, messagePosition, Color.White);
        }

        private void DrawResetButton()
        {
            string resetMessage = "Press 'R' to Reset";
            Vector2 resetSize = gameFont.MeasureString(resetMessage);
            string endGameMessage = hasWon ? "Congratulations! You've won!" : "Game Over! No more moves available!";
            Vector2 endGameSize = gameFont.MeasureString(endGameMessage);

            Vector2 resetPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2 - resetSize.X / 2,
                                                _graphics.PreferredBackBufferHeight / 2 - resetSize.Y / 2 + endGameSize.Y + 20); // Adjust Y position

            _spriteBatch.DrawString(gameFont, resetMessage, resetPosition, Color.White);
        }

        private void ResetGame()
        {
            grid = new int[8, 8];
            hasWon = false;
            hasLost = false;
            AddRandomTile(); // Start with one tile on the grid
        }

        private void AddRandomTile()
        {
            var emptyTiles = new List<Point>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (grid[x, y] == 0) // Check for empty tile
                    {
                        emptyTiles.Add(new Point(x, y));
                    }
                }
            }

            if (emptyTiles.Count > 0) // Only add a tile if there is an empty space
            {
                var random = new Random();
                var randomTile = emptyTiles[random.Next(emptyTiles.Count)];
                grid[randomTile.X, randomTile.Y] = random.Next(2) * 2 + 2; // Assigns either 2 or 4
            }
        }

        private void MoveTilesUp()
        {
            bool moved = false;
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

            if (moved)
            {
                for (int i = 0; i < TilesToAddPerMove; i++)
                {
                    AddRandomTile();
                }

                CheckForWin();
                CheckForLoss();
            }

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
                for (int i = 0; i < TilesToAddPerMove; i++)
                {
                    AddRandomTile();
                }

                CheckForWin();
                CheckForLoss();
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
                for (int i = 0; i < TilesToAddPerMove; i++)
                {
                    AddRandomTile();
                }

                CheckForWin();
                CheckForLoss();
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
                for (int i = 0; i < TilesToAddPerMove; i++)
                {
                    AddRandomTile();
                }

                CheckForWin();
                CheckForLoss();
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
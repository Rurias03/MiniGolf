using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
// using Microsoft.Xna.Framework.Audio;

namespace MiniGolf
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont text;

        private Texture2D ball;
        private Texture2D bg;
        private Texture2D map;
        private Texture2D arrow;

        private float power = 10f;

        private Vector2 ballPos;
        private Vector2 bgPos;
        private Vector2 mapPos;
        private Vector2 startPoint;
        private Vector2 endPoint;
        private Vector2 force;
        private Vector2 minPow;
        private Vector2 maxPow;
        private Vector2 arrowPos;

        private MouseState oldState;

        private float shotAngle;

        private const int screenWidth = 1080;
        private const int screenHeight = 720;
        private int strokes = 0;

        private Rectangle ballRect;
        private Rectangle speedRectTop;
        private Rectangle speedRectBottom;
        private Rectangle topWall;
        private Rectangle midWall;
        private Rectangle bottomWall;
        private Rectangle holeRect;

        // private List<SoundEffect> sounds;

        // private int delaySound = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            // sounds = new List<SoundEffect>();
        }

        protected override void Initialize()
        {
            // Adjust screen resolution
            graphics.IsFullScreen = false;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            graphics.ApplyChanges();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            bg = Content.Load<Texture2D>("grass");
            ball = Content.Load<Texture2D>("golf-ball");
            map = Content.Load<Texture2D>("map");
            text = Content.Load<SpriteFont>("strokes");
            arrow = Content.Load<Texture2D>("arrow");

            ballPos = new Vector2(161, 133);
            bgPos = new Vector2(screenWidth / 2, screenHeight / 2);
            mapPos = new Vector2(screenWidth / 2, screenHeight / 2);
            minPow = new Vector2(-30, -30);
            maxPow = new Vector2(30, 30);
            holeRect = new Rectangle(136, 568, 32, 33);
            arrowPos = new Vector2(-100, -100);

            oldState = Mouse.GetState();

            speedRectTop = new Rectangle(310, 213, 458, 138);
            speedRectBottom = new Rectangle(310, 369, 458, 132);
            topWall = new Rectangle(99, 195, 670, 17);
            midWall = new Rectangle(311, 351, 670, 17);
            bottomWall = new Rectangle(99, 501, 670, 17);

            // sounds.Add(Content.Load<SoundEffect>("golf-hit"));
            // sounds.Add(Content.Load<SoundEffect>("ball-bounce"));
            // sounds.Add(Content.Load<SoundEffect>("golf-hole"));
        }

        protected override void Update(GameTime gameTime)
        {
            // Exit the game if escape key is pressed
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState newState = Mouse.GetState();

            ballRect = new Rectangle((int)ballPos.X - 12, (int)ballPos.Y - 12, (int)(ball.Width * .01f), (int)(ball.Height * .01f));

            // Shoot the ball if forced is applied
            if (force.X != 0f || force.Y != 0f)
            {
                ballPos.X += power * force.X * (float)gameTime.ElapsedGameTime.TotalSeconds;
                ballPos.Y += power * force.Y * (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Reduce force over time
                force.X -= force.X * .01f;
                force.Y -= force.Y * .01f;

                // Stop the ball if force is very low
                if ((force.X < 1f && force.X > 0f || force.X > -1 && force.X < 0) && (force.Y < 1f && force.Y > 0f || force.Y > -1 && force.Y < 0))
                {
                    force.X = 0f;
                    force.Y = 0f;
                }
            }
            else
            {
                // Check for mouse input to calculate direction and power
                if (newState.LeftButton == ButtonState.Pressed && oldState.LeftButton == ButtonState.Released)
                {
                    startPoint = new Vector2(newState.Position.X, newState.Position.Y);
                    arrowPos = new Vector2(ballPos.X, ballPos.Y);
                }

                endPoint = new Vector2(newState.Position.X, newState.Position.Y);
                shotAngle = calcAngle(startPoint, endPoint) + (float)Math.PI;

                if (oldState.LeftButton == ButtonState.Pressed && newState.LeftButton == ButtonState.Released)
                {
                    endPoint = new Vector2(newState.Position.X, newState.Position.Y);
                    arrowPos = new Vector2(-100, -100);

                    // Calculate the direction and power based on the difference between start and end points
                    force = new Vector2(MathHelper.Clamp(startPoint.X - endPoint.X, minPow.X, maxPow.X), MathHelper.Clamp(startPoint.Y - endPoint.Y, minPow.Y, maxPow.Y));
                    // sounds[0].Play(1f, 0, 0);
                    strokes++;
                }
            }

            // Top Slope
            if (speedRectTop.Contains(ballRect))
            {
                if (force.Y == 0f)
                {
                    force.X -= .5f;
                }
                else
                {
                    force.X -= .5f;
                    force.Y -= .2f;
                }
            }

            // Bottom Slope
            if (speedRectBottom.Contains(ballRect))
            {
                if (force.Y == 0f)
                {
                    force.X += .5f;
                }
                else
                {
                    force.X += .5f;
                    force.Y += .2f;
                }
            }

            // Ball collision with sides of map
            if (ballPos.X <= map.Width - 968 || ballPos.X >= map.Width - 110)
            {
                // Reverse X speed
                force.X = -force.X;
            }

            // Ball collision with top and bottom of map
            if (ballPos.Y <= map.Height - 637 || ballPos.Y >= map.Height - 83)
            {
                // Reverse Y speed
                force.Y = -force.Y;
            }

            // Ball collision with top interior wall
            if (ballRect.Intersects(topWall))
            {
                if (ballPos.X <= topWall.Left)
                    force.X = -1f * Math.Abs(force.X);
                else if (ballPos.X >= topWall.Right)
                    force.X = Math.Abs(force.X);
                else if (ballPos.Y <= topWall.Bottom)
                    force.Y = -1f * Math.Abs(force.Y);
                else if (ballPos.Y >= topWall.Top)
                    force.Y = Math.Abs(force.Y);

                // if (delaySound <= 0)
                // {
                //     sounds[1].Play();
                //     delaySound = 3;
                // }
            }

            // Ball collision with middle wall
            if (ballRect.Intersects(midWall))
            {
                if (ballPos.X <= midWall.Left)
                    force.X = -1f * Math.Abs(force.X);
                else if (ballPos.X >= midWall.Right)
                    force.X = Math.Abs(force.X);
                else if (ballPos.Y <= midWall.Bottom)
                    force.Y = -1f * Math.Abs(force.Y);
                else if (ballPos.Y >= midWall.Top)
                    force.Y = Math.Abs(force.Y);

                // if (delaySound <= 0)
                // {
                //     sounds[1].Play();
                //     delaySound = 3;
                // }
            }

            // Ball collision with bottom wall
            if (ballRect.Intersects(bottomWall))
            {
                if (ballPos.X <= bottomWall.Left)
                    force.X = -1f * Math.Abs(force.X);
                else if (ballPos.X >= bottomWall.Right)
                    force.X = Math.Abs(force.X);
                else if (ballPos.Y <= bottomWall.Bottom)
                    force.Y = -1f * Math.Abs(force.Y);
                else if (ballPos.Y >= bottomWall.Top)
                    force.Y = Math.Abs(force.Y);

                // if (delaySound <= 0)
                // {
                //     sounds[1].Play();
                //     delaySound = 3;
                // }
            }

            // When the ball enters the hole
            if (holeRect.Contains(ballRect))
            {
                // sounds[2].Play();
                // delaySound = 3;
                // delaySound--;
                Debug.WriteLine("Finished the hole in " + strokes + " strokes");
                Exit();
            }


            // Update the old mouse state
            oldState = newState;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();


            spriteBatch.Draw(bg, bgPos, null, Color.White, 0f, new Vector2(bg.Width / 2f, bg.Height / 2f), .5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(map, mapPos, null, Color.White, 0f, new Vector2(map.Width / 2f, map.Height / 2f), .85f, SpriteEffects.None, 0f);
            spriteBatch.Draw(ball, ballPos, null, Color.White, 0f, new Vector2(ball.Width / 2f, ball.Height / 2f), .01f, SpriteEffects.None, 0f);
            spriteBatch.Draw(arrow, new Rectangle((int)arrowPos.X, (int)arrowPos.Y, 32, 32), null, Color.White, shotAngle, new Vector2(arrow.Width / 2, arrow.Height / 2), SpriteEffects.None, 0f);
            spriteBatch.DrawString(text, "Strokes: " + strokes, new Vector2(798, 10), Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected float calcAngle(Vector2 startPt, Vector2 endPt)
        {
            // 1st Quadrant
            if (endPt.X > startPt.X && endPt.Y <= startPt.Y)
            {
                return (float)Math.Atan2(endPt.X - startPt.X, startPt.Y - endPt.Y);
            }

            // 2nd Quadrant
            if (endPt.X < startPt.X && endPt.Y <= startPt.Y)
            {
                return (float)Math.Atan2(endPt.X - startPt.X, startPt.Y - endPt.Y);
            }

            // 4th Quadrant
            if (endPt.X > startPt.X && endPt.Y >= startPt.Y)
            {
                return (float)Math.Atan2(endPt.X - startPt.X, startPt.Y - endPt.Y);
            }
            else
            {
                // 3rd Quadrant
                return (float)Math.Atan2(endPt.X - startPt.X, startPt.Y - endPt.Y);
            }
        }
    }
}

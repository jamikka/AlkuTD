using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
//using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace AlkuTD
{
    public enum GameState : byte
    {
        MainMenu,
        InGame,
        Paused,
        GameOver,
        LevelComplete,
        MapEditor,
        MapTest
    }
    

    public class CurrentGame : Microsoft.Xna.Framework.Game
    {
		public static GraphicsDeviceManager graphicsDManager;
		public static GraphicsDevice graphicsDevice;
        public SpriteBatch spriteBatch;
        public static MouseState mouse;
        public static MouseState prevMouse;
        public static KeyboardState keyboard;
        public static KeyboardState prevKeyboard;
        public GameTime GameTime;
        public static uint gameTimer;

        public static string GameDir = AppDomain.CurrentDomain.BaseDirectory;
		public static string ContentDir = AppDomain.CurrentDomain.BaseDirectory + "Content\\";
		public static string SaveDir = AppDomain.CurrentDomain.BaseDirectory + "Content\\OmaData\\SaveData\\";  //@"D:\Documents\CisKood\AlkuTD\AlkuTD\OmaData\SaveData\";
		public static string MapDir = /*AppDomain.CurrentDomain.BaseDirectory + "Content\\OmaData\\Maps\\";*/ @"D:\Documents\CisKood\AlkuTD\AlkuTD\OmaData\Maps\";

		public const long LoopTimeTicks = 166667;
		public const float GeneSellRate = 0.666666f;

        public static GameState gameState;
        public static GameState prevState;
        public static SpriteFont font;
        public static Texture2D pixel;
        public static Texture2D ball;
        public static Texture2D smallBall;
        public static Texture2D[] CreatureTextures;
        public Texture2D dashLine;

        public MainMenu mainMenu;
        public static Player[] players = new Player[] { null, null };
        public static HexMap currentMap;
        public static HUD HUD;
        //Effect brightnessFX;
        public float brightnessValue;
        //XmlWriterSettings xmlSettings = new XmlWriterSettings();

        public static AudioEngine audioEngine;
        public static WaveBank waveBank;
        public static SoundBank soundBank;

        public CurrentGame()
        {
            graphicsDManager = new GraphicsDeviceManager(this);

            graphicsDManager.PreferredBackBufferWidth = 1280;
            graphicsDManager.PreferredBackBufferHeight = 720;
            graphicsDManager.IsFullScreen = false;
            Window.AllowUserResizing = true;
            IsMouseVisible = true;
            IsFixedTimeStep = true; // default 60 fps / 16.6667 ms step time
            TargetElapsedTime = new TimeSpan(LoopTimeTicks); // old ('til feb2015) (0,0,0,0,17) -------------------------------!!!!!!!!!!!!!
            //Console.WriteLine(TargetElapsedTime.Ticks);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
			graphicsDevice = GraphicsDevice;
            base.Initialize();
            /*Debug.WriteLine(GameDir);
            Debug.WriteLine(SaveDir);
            Debug.WriteLine(MapDir);
            Debug.WriteLine(AppDomain.CurrentDomain.BaseDirectory);*/
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp; 

            string[] creatureFolderItems = Directory.GetFiles(ContentDir + "Creatures\\");
            CreatureTextures = new Texture2D[creatureFolderItems.Length];
            for (int c = 0; c < creatureFolderItems.Length; c++)
            {
                CreatureTextures[c] = Content.Load<Texture2D>("Creatures\\" + Path.GetFileNameWithoutExtension(creatureFolderItems[c]));
                CreatureTextures[c].Name = Path.GetFileNameWithoutExtension(creatureFolderItems[c]);
            }

            pixel = Content.Load<Texture2D>("pixel");
            smallBall = Content.Load<Texture2D>("ball-3x3");
            ball = Content.Load<Texture2D>("ball-5x5");
            dashLine = Content.Load<Texture2D>("dashLine");
            font = Content.Load<SpriteFont>("Fonts\\minifont");
            mainMenu = new MainMenu(this);
            HUD = new HUD(this);
            //brightnessFX = Content.Load<Effect>("brightnessFX");
            //brightnessFX.Parameters["value"].SetValue(brightnessValue);
            audioEngine = new AudioEngine(ContentDir + "Sound\\soundSet.xgs");
            waveBank = new WaveBank(audioEngine, ContentDir + "Sound\\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, ContentDir + "Sound\\Sound Bank.xsb");
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }
                
        protected override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                mouse = Mouse.GetState();
                keyboard = Keyboard.GetState();

                switch (gameState)
                {
                    case GameState.MainMenu:
                        if (!IsMouseVisible) IsMouseVisible = true; //------------------------yöllinen örvellystarve
                        mainMenu.Update(mouse, keyboard);
                        break;
                    case GameState.MapTest: 
                    case GameState.InGame:
                        if (keyboard.IsKeyDown(Keys.Escape) && !prevKeyboard.IsKeyDown(Keys.Escape)) gameState = GameState.Paused;
                        if (keyboard.IsKeyDown(Keys.S))
                        {
                            if (TargetElapsedTime.Ticks != 40000)
                                TargetElapsedTime = new TimeSpan(40000);
                            if (gameTimer % 2 == 0)
                                SuppressDraw();
                        }
                        else if (keyboard.IsKeyDown(Keys.D))
                        {
                            if (TargetElapsedTime.Ticks != 400000)
                                TargetElapsedTime = new TimeSpan(400000);
                        }
                        else
                        {
							if (TargetElapsedTime.Ticks != LoopTimeTicks)
                                TargetElapsedTime = new TimeSpan(LoopTimeTicks);
                        }

                        HUD.Update(mouse, prevMouse, keyboard, prevKeyboard);
                        currentMap.Update();
                        //if (keyboard.IsKeyDown(Keys.Down)) brightnessValue -= 0.01f;
                        //else if (keyboard.IsKeyDown(Keys.Up)) brightnessValue += 0.01f;
                        //brightnessFX.Parameters["value"].SetValue(brightnessValue);
                        break;
                    case GameState.Paused:
                        if (keyboard.IsKeyDown(Keys.Space)) this.Exit();
                        else if (keyboard.IsKeyDown(Keys.Escape) && !prevKeyboard.IsKeyDown(Keys.Escape)) gameState = prevState;
                        else if (keyboard.IsKeyDown(Keys.M) && !prevKeyboard.IsKeyDown(Keys.M))
                        {
                            gameState = GameState.MainMenu;
                            mainMenu.menuState = MainMenu.MenuState.Main;
                        }
                        HUD.Update(mouse, prevMouse, keyboard, prevKeyboard);
                        break;
                    case GameState.GameOver:
						IsMouseVisible = true;
                        if (keyboard.IsKeyDown(Keys.Escape) || (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed))
                        {
                            if (prevState == GameState.InGame)
                                gameState = GameState.MainMenu;
                            else if (prevState == GameState.MapTest)
                            {
                                currentMap.ResetMap();
                                HUD.MapEditorTopButtons[1].Pos = HUD.mapEditWTEditPos;
                                gameState = GameState.MapEditor;
                            }
                        }
                        break;
                    case GameState.MapEditor:
                        if (keyboard.IsKeyDown(Keys.M) && !prevKeyboard.IsKeyDown(Keys.M) && keyboard.IsKeyDown(Keys.Escape))
                        {
                            gameState = GameState.MainMenu;
                            mainMenu.menuState = MainMenu.MenuState.Main;
                        }
                        HUD.Update(mouse, prevMouse, keyboard, prevKeyboard);
                        //currentMap.Update();
                        break;
                }
                prevKeyboard = keyboard;
                prevMouse = mouse;
                audioEngine.Update();
                base.Update(gameTime);
                GameTime = gameTime;
                gameTimer++;
                if (gameState == GameState.InGame || gameState == GameState.MapTest)
                    prevState = gameState;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            //if (IsActive)
            //{
                GraphicsDevice.Clear(Color.Black);
                //spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                switch (gameState)
                {
                    case GameState.MainMenu:
                        mainMenu.Draw(spriteBatch);
                        break;

                    case GameState.InGame:
                        //spriteBatch.End();
                        //spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, brightnessFX);
                        spriteBatch.End();

                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
                        currentMap.Draw(spriteBatch);
                        spriteBatch.End();

                        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                        HUD.Draw(spriteBatch, gameTime, mouse);
                        break;

                    case GameState.Paused:
                        currentMap.Draw(spriteBatch);
                        HUD.Draw(spriteBatch, gameTime, mouse);
                        //spriteBatch.Draw(pixel, GraphicsDevice.Viewport.Bounds, new Color(0, 0, 0, 100));
                        spriteBatch.DrawString(font, "Paused", new Vector2(GraphicsDevice.Viewport.Width * 0.05f, GraphicsDevice.Viewport.Height * 0.5f), Color.Orange);
                        break;

                    case GameState.GameOver:
                        currentMap.Draw(spriteBatch);
                        HUD.Draw(spriteBatch, gameTime, mouse);
                        spriteBatch.Draw(pixel, GraphicsDevice.Viewport.Bounds, new Color(0, 0, 0, 100));
                        spriteBatch.DrawString(font, "Ok", new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f), Color.Orange);
                        break;
                    case GameState.MapEditor:
                        currentMap.Draw(spriteBatch);
                        HUD.Draw(spriteBatch, gameTime, mouse);
                        break;
                    case GameState.MapTest:
                        currentMap.Draw(spriteBatch);
                        HUD.Draw(spriteBatch, gameTime, mouse);
                        break;
                }
                base.Draw(gameTime);
                spriteBatch.End();
            //}
        }
    }
}

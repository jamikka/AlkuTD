using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
//using System.Linq;


namespace AlkuTD
{
    public class MainMenu
    {
        CurrentGame ParentGame;        
        public enum MenuState : byte {
            Main,
            NewGame,
            Continue,    
            Options,
            MapSelection,
            MapEditor
        }
        public MenuState menuState;
        
        public Button[] RootButtons;
        public Button[] PlayerButtons;
        public Button[] OptionsButtons;
        public Button[] MapButtons;
        public Button[] NewPlayerButtons;
        public Button[] MapEditorButtons;
        public Button[] MapZoneButtons;

        //public Rectangle[] ButtonBoundses;

        public string[] PlayerFilePaths;
        public string[] PlayerNames;
        //public Player[] CurrentPlayers; // = ParentGame.players
        public int[] CurrentPlayerIndexes;
        public string currentMap;
        public string[] MapNames;
        public Dictionary<string, string> StoryMaps;
        public byte[][] PlayerAvailableTowers;
        public byte[] playerAvailableTowers;

        public SpriteFont Font;

        int padding;
        int rootButtonWidth;
        int playerButtonWidth;
        int mapButtonWidth;
        int buttonHeight;
        int rootButtonX;
        int rootButtonY;
        int playerButtonX;
        int mapButtonX;
        int mapButtonY;
        Color[] buttonColors;
        Color[] buttonTextColors;

        Texture2D bodyMapTex;
        public static Texture2D[] TowersTileringTextures;
        public Texture2D[] PlayerAvailableTowersTileringTextures;

        public MainMenu(CurrentGame game)
        {
            ParentGame = game;
            Font = CurrentGame.font;

            //if (!System.IO.Directory.Exists(game.SaveDir)) //--------------------Necessary?
            //    System.IO.Directory.CreateDirectory(game.SaveDir);

            FileInfo[] pfiles = new DirectoryInfo(CurrentGame.SaveDir).GetFiles();
            Array.Sort(pfiles, (y,x) => Comparer<DateTime>.Default.Compare(x.CreationTime, y.CreationTime)); //------------------------------lambdaa!!!!!!!!!!!!!
            //PlayerFilePaths = Directory.GetFiles(game.SaveDir);
            PlayerFilePaths = new string[pfiles.Length];
            for (int f = 0; f < pfiles.Length; f++)
                PlayerFilePaths[f] = pfiles[f].FullName;
            CurrentPlayerIndexes = new int[2];
            CurrentPlayerIndexes[0] = -1;
            PlayerNames = Array.ConvertAll<string, string>(PlayerFilePaths, Path.GetFileNameWithoutExtension); // in one line!
            StoryMaps = new Dictionary<string, string>();
            using (StreamReader sr = new StreamReader(CurrentGame.ContentDir + "StoryMaps.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string[] read = sr.ReadLine().Split('-');
                    StoryMaps.Add(read[0].Trim(), read[1].Trim());
                }
            }

            string longestName = "";
            for (int n = 0; n < PlayerNames.Length; n++)
                if (PlayerNames[n].Length > longestName.Length)
                    longestName = PlayerNames[n]; 

            padding = 10;
            rootButtonWidth = (int)Math.Round(Font.MeasureString("Map editor").X) + 2*padding; //padding 10 x 2
            playerButtonWidth = (int)Math.Max(Font.MeasureString("Min.koko").X, Font.MeasureString(longestName).X) + 2*padding;
            mapButtonWidth = (int)Math.Round(Font.MeasureString("12345678901234567890").X);
            buttonHeight = Font.LineSpacing + padding;
            //rootButtonX = (int)(game.GraphicsDevice.Viewport.Width/2 - rootButtonWidth -1); //-----Buttonses have their right side on the screen x center  
            rootButtonX = (int)(game.GraphicsDevice.Viewport.Width / 3); //-----uus vasemmal 
            rootButtonY = (int)(game.GraphicsDevice.Viewport.Height*0.5); //----------VANH: rootbuttons stack vertically down starting from 0.6 screenheight 
            playerButtonX = (int)(rootButtonX + rootButtonWidth +2); //-----right side of rootbuttons
            //mapButtonX = (int)(game.GraphicsDevice.Viewport.Width/2 - rootButtonWidth - mapButtonWidth -3); //-----Maps are on the left side of the rootbuttons
            mapButtonX = (int)(playerButtonX + playerButtonWidth + 2); //-----right side of players
            mapButtonY = (int)(rootButtonY - 7*buttonHeight);
            buttonColors = new Color[] { new Color(10,20,30), new Color(20,30,40), new Color(30,40,50) }; //----passive,hovered,pressed
            buttonTextColors = new Color[] { Color.SlateGray, Color.Orange, Color.Orange };//----passive,hovered,pressed

            RootButtons = new Button[5];
            for (int i = 0; i < RootButtons.Length; i++)
            {   switch (i)
                {
                    case 0: RootButtons[i] = new Button("New Game", rootButtonX, rootButtonY + i*buttonHeight, rootButtonWidth, buttonHeight, padding, TextAlignment.Right, buttonColors, buttonTextColors, CurrentGame.pixel); break;
                    case 1: RootButtons[i] = new Button("Continue", rootButtonX, rootButtonY + i*buttonHeight, rootButtonWidth, buttonHeight, padding, TextAlignment.Right, buttonColors, buttonTextColors, CurrentGame.pixel); break;
                    case 2: RootButtons[i] = new Button("Options", rootButtonX, rootButtonY + i*buttonHeight, rootButtonWidth, buttonHeight, padding, TextAlignment.Right, buttonColors, buttonTextColors, CurrentGame.pixel); break;
                    case 3: RootButtons[i] = new Button("Map editor", rootButtonX, rootButtonY + i*buttonHeight, rootButtonWidth, buttonHeight, padding, TextAlignment.Right, buttonColors, buttonTextColors, CurrentGame.pixel); break;
                    case 4: RootButtons[i] = new Button("Quit", rootButtonX, rootButtonY + i*buttonHeight, rootButtonWidth, buttonHeight, padding, TextAlignment.Right, buttonColors, buttonTextColors, CurrentGame.pixel); break;
                }
            }

            PlayerButtons = new Button[PlayerNames.Length];
            for (int p = 0; p < PlayerNames.Length; p++)
                PlayerButtons[p] = new Button(PlayerNames[p], playerButtonX, rootButtonY + buttonHeight + p*buttonHeight, playerButtonWidth, buttonHeight, padding, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel);

            NewPlayerButtons = new Button[3];
            NewPlayerButtons[0] = new Button("Player name", playerButtonX, rootButtonY, playerButtonWidth, buttonHeight, 15, TextAlignment.Left, Color.Transparent, Color.Orange, CurrentGame.pixel);
            NewPlayerButtons[1] = new Button("text entry", playerButtonX + playerButtonWidth/4, rootButtonY + buttonHeight, playerButtonWidth, buttonHeight, padding, TextAlignment.Left, buttonColors[1], Color.Orange, CurrentGame.pixel);
            NewPlayerButtons[2] = new Button("Name already exists", playerButtonX, rootButtonY + 2*buttonHeight, playerButtonWidth, buttonHeight, 15, TextAlignment.Left, Color.Transparent, Color.Orange, CurrentGame.pixel);

            MapNames = Array.ConvertAll<string, string>(Directory.GetFiles(CurrentGame.MapDir), Path.GetFileNameWithoutExtension);
            MapButtons = new Button[MapNames.Length];
            for (int m = 0; m < MapNames.Length; m++)
                MapButtons[m] = new Button(MapNames[m], mapButtonX, mapButtonY + m*buttonHeight, mapButtonWidth, buttonHeight, padding, TextAlignment.HalfLeft, buttonColors, buttonTextColors, CurrentGame.pixel);

            MapEditorButtons = new Button[] {new Button("New map", playerButtonX, rootButtonY + 3*buttonHeight, rootButtonWidth, buttonHeight, padding, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel)};

            #region OLD BUTTONSYSTEM
            /*ButtonWords = new string[8 + PlayerNames.Length];
            for (int i = 0; i < 8 + PlayerNames.Length; i++)
            {
                switch (i)
                {
                    case 0: ButtonWords[i] = "New Game"; break;
                    case 1: ButtonWords[i] = "Continue"; break;
                    //case 2: ButtonWords[i] = "Map editor"; break;
                    case 3: ButtonWords[i] = "Options"; break;
                    case 4: ButtonWords[i] = "Quit"; break;
                    case 4: ButtonWords[i] = "1"; break;
                    case 5: ButtonWords[i] = "2"; break;
                    case 6: ButtonWords[i] = "3"; break;
                    case 7: ButtonWords[i] = "Resolution"; break;
                    default: ButtonWords[i] = PlayerNames[i - 8]; break;
                }
            }
            

            ButtonBoundses = new Rectangle[RootButtonWords.Length];
            ButtonStates = new MenuButtonState[RootButtonWords.Length];

            for (int i = 0; i < RootButtonWords.Length; i++)
            {
                if (i <= 4) ButtonBoundses[i] = new Rectangle(rootButtonX, (int)(game.GraphicsDevice.Viewport.Height * 0.6 + i * buttonHeight), rootButtonWidth, buttonHeight); // Main buttons
                else if (i >= 4 && i <= 6) ButtonBoundses[i] = new Rectangle(mapButtonX, (int)(game.GraphicsDevice.Viewport.Height * 0.4 + (i - 4) * buttonHeight), mapButtonWidth, buttonHeight); // Map buttons
                else if (i >= 8) ButtonBoundses[i] = new Rectangle(playerButtonX, (int)(game.GraphicsDevice.Viewport.Height * 0.6 + (i - 8) * buttonHeight + buttonHeight), playerButtonWidth, buttonHeight); // Player buttons
                else ButtonBoundses[i] = new Rectangle(mapButtonX, (int)(game.GraphicsDevice.Viewport.Height * 0.4 + (i - 4) * buttonHeight), mapButtonWidth, buttonHeight); // Option buttons
            }*/
            #endregion

            MapZoneButtons = new Button[16];
            buttonColors = new Color[] { new Color(0,0,0, 0), Color.Black, new Color(30,40,50) }; //----passive,hovered,pressed
            buttonTextColors = new Color[] { Color.Red, Color.White, Color.Orange };//----passive,hovered,pressed
            MapZoneButtons[0] = new Button("+", 942 - 44, 452 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[1] = new Button("+", 950 - 44, 314 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[2] = new Button("+", 1015 - 44, 299 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[3] = new Button("+", 1034 - 44, 334 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[4] = new Button("+", 992 - 44, 385 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[5] = new Button("+", 1044 - 44, 422 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[6] = new Button("+", 1090 - 44, 391 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[7] = new Button("+", 1090 - 44, 454 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[8] = new Button("+", 1015 - 44, 469 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            MapZoneButtons[9] = new Button("+", 984 - 44, 510 - 36, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.bigBall);
            bodyMapTex = game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Menu\\bodyMap");

            PlayerAvailableTowers = new byte[6][];
            for (int i = 0; i < PlayerAvailableTowers.Length; i++)
                PlayerAvailableTowers[i] = new byte[3];
            PlayerAvailableTowers[0][0] = 1;
            playerAvailableTowers = new byte[6];
            playerAvailableTowers[0] = 1;

            PlayerAvailableTowersTileringTextures = new Texture2D[7];
            PlayerAvailableTowersTileringTextures[0] = CurrentGame.tilering;
            //PlayerAvailableTowersTileringTextures[1] = game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-1a");

            TowersTileringTextures = new Texture2D[] { game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-1a"), 
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-1b"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-1c"), 
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-2a"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-2b"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-2c"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-3a"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-3b"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-3c"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-4a"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-4b"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-4c"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-5a"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-5b"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-5c"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-6a"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-6b"),
                                                                game.Content.Load<Texture2D>(CurrentGame.ContentDir + "Tilering\\ringFill-6c") };
            PlayerAvailableTowersTileringTextures[1] = TowersTileringTextures[0];
        }

        public void LoadPlayerData(int fileIndex)
        {
            Player loadedPlayer;
            if (CurrentGame.players[0] != null && PlayerNames[fileIndex] != CurrentGame.players[0].Name)
                loadedPlayer = new Player(PlayerNames[fileIndex]);
            else if (CurrentGame.players[0] != null)
                loadedPlayer = CurrentGame.players[0];
            else 
                loadedPlayer = new Player(PlayerNames[fileIndex]);

            foreach (Button b in MapButtons) b.TextAlign = TextAlignment.HalfLeft;

            for (int i = 0; i < MapZoneButtons.Length; i++)
            {
                if (MapZoneButtons[i] != null)
                   MapZoneButtons[i].TextColors = buttonTextColors;
            }

            List<string> tempMapList = new List<string>();
            foreach (string mapName in StoryMaps.Keys)
                tempMapList.Add(mapName);
                
            foreach (string mapName in MapNames)
            {
                if (!StoryMaps.ContainsKey(mapName))
                    tempMapList.Add(mapName);
            }

            MapNames = tempMapList.ToArray();

            for (int i = 1; i < PlayerAvailableTowersTileringTextures.Length; i++)
                PlayerAvailableTowersTileringTextures[i] = null;
            PlayerAvailableTowersTileringTextures[1] = TowersTileringTextures[0];

            foreach (byte[] b in PlayerAvailableTowers)
            {
                b[0] = 0;
                b[1] = 0;
                b[2] = 0;
            }
            PlayerAvailableTowers[0][0] = 1;

            using (StreamReader reader = new StreamReader(PlayerFilePaths[fileIndex]))
            {
                string[] read = reader.ReadToEnd().Split(new string[]{ Environment.NewLine }, StringSplitOptions.None);
                for (int n = 0; n < MapButtons.Length; n++)
                {
                    MapButtons[n].Text = tempMapList[n];
                    MapButtons[n].Texts = null;
                    string mapName = tempMapList[n];
                    for (int i = 0; i < read.Length; i++)
                    {
                        if (read[i].Contains(MapButtons[n].Text + " "))
                        {
                            //MapButtons[n].Text += "  (" + read[i].Split('-')[1].Trim() + ")";
                            MapButtons[n] = new Button(MapButtons[n].Bounds.X, MapButtons[n].Bounds.Y, mapButtonWidth, buttonHeight, TextAlignment.HalfLeft, MapButtons[n].ButtonColors, MapButtons[n].TextColors, MapButtons[n].ButtonTexture, mapName, "  (" + read[i].Split('-')[1].Trim() + ")");
                            if (MapZoneButtons[n] != null) 
                                MapZoneButtons[n].TextColors = new Color[] { Color.Green, Color.White, Color.Orange };//----passive,hovered,pressed

                            if (StoryMaps.ContainsKey(mapName))
                            {
                                string[] unlocks = StoryMaps[mapName].Split(',');
                                foreach (string unlock in unlocks)
                                {
                                    byte unlockType = (byte)((unlock[0] - '0') - 1); //convert char to number by - '0', then make zero-based
                                    byte unlockTier = 0;
                                    switch (unlock[1])
                                    {
                                        case 'a': unlockTier = 0; break;
                                        case 'b': unlockTier = 1; break;
                                        case 'c': unlockTier = 2; break;
                                    }
                                    PlayerAvailableTowers[unlockType][unlockTier] = 1;
                                    playerAvailableTowers[unlockType] = (byte)(unlockTier + 1);

                                    //if (unlockType == 0 && unlockTier == 1)
                                    //    PlayerAvailableTowersTileringTextures[1] = AvailableTowersTileringTextures[1];
                                    PlayerAvailableTowersTileringTextures[unlockType +1] = TowersTileringTextures[unlockType * 3 + unlockTier];
                                }
                            }
                        }
                    }
                }
            }
            CurrentGame.players[0] = loadedPlayer;
			CurrentGame.players[1] = null;
            //CurrentPlayers = ParentGame.players;
        }

        void LoadMap(Button mapButton)
        {
            //currentMap = byte.Parse(mapName.Substring(mapName.Length -1, 1)); //-------------------------------------------------risky.
            string mapName = mapButton.Text.Split('(')[0].Trim();
            string mapFileName = mapName + ".txt";

            if (File.Exists(CurrentGame.MapDir + mapFileName))
            {
                //try
                //{
                    string[] read;
                    currentMap = mapName;
					HexMap loadedMap = new HexMap(ParentGame, currentMap, new char[1, 1], null, null, CurrentGame.players);
                    char[,] layout = new char[11, 21];
                    List<Point> spawnPoints = new List<Point>();
                    List<Point> goalPoints = new List<Point>();
                    List<Wave> waves = new List<Wave>();
                    int initLife;
                    int initEnergy;
                    int[] initGenePoints;
                    byte[] availableTowers = new byte[6];
                    List<Tower> initTowers = new List<Tower>();

                    using (StreamReader reader = new StreamReader(CurrentGame.MapDir + mapFileName))
                    {
                        //for (int i = 0; i < 12; i++)
                        //  Debug.WriteLine(reader.ReadLine());

                        for (int row = 0; row < 11; row++)
                        {
                            for (int col = 0; col < 21; col++)
                            {
                                int ascii = reader.Peek();
                                if (ascii != 32 && ascii != 13 && ascii != 39 && ascii != 46 && ascii != 48)
                                {
                                    reader.Peek();
                                }
                                while (reader.Peek() < 32) reader.Read(); //run through whitespace characters, except space (32), which is last of the whitespace in the ascii table
                                layout[row, col] = (char)reader.Read();
                                if ((int)layout[row, col] >= 49 && (int)layout[row, col] <= 57)
                                    spawnPoints.Add(new Point(col, row));
                                else if ((int)layout[row, col] >= 97 && (int)layout[row, col] <= 122)
                                    goalPoints.Add(new Point(col, row));
                                else
                                {
                                    for (int i = 0; i < HexMap.ExampleTowers.Length; i++)
                                    {
                                        if (ascii == (int)HexMap.ExampleTowers[i].Symbol)
                                        {
                                            Tower tempTower = Tower.Clone(HexMap.ExampleTowers[i]);  //-------------------------------------ADD MAPCOORD---------------------------------- tavallaan tehty mut tower-olemassaoloa pitäs hienontaa
											tempTower.ParentMap = CurrentGame.currentMap;             //---------------------------------------------------------------------- mukaanlukien nää ihme initit (tää koska MapCoordToScrLoc ei static!)
                                            tempTower.MapCoord = new Point(col, row);
                                            //tempTower.buildTimer = 0;
                                            //tempTower.buildFinishedCounter = 0;
                                            initTowers.Add(tempTower);
                                        }
                                    }
                                }
                            }
                        }
                        reader.ReadLine();
                        reader.ReadLine();

                        read = reader.ReadLine().Split(':');
                        if (read[1].Trim() == "Yes")
                            loadedMap.IsStoryMap = true;
                        read = reader.ReadLine().Split(':');
                        read = reader.ReadLine().Split(':', ' ');
                        for (int i = 0; i < availableTowers.Length; i++)
                            byte.TryParse(read[i + 3], out availableTowers[i]);

                        if (loadedMap.IsStoryMap)
                        {
                            HexMap.ProgressBlockedTowers = new byte[6];
                            for (int i = 0; i < availableTowers.Length; i++)
                            {
                                if (availableTowers[i] == 0)
                                {

                                }
                                else if (PlayerAvailableTowers[i][0] == 0)
                                {
                                    availableTowers[i] = 0;
                                    HexMap.ProgressBlockedTowers[i] = 1;
                                }
                                else if (PlayerAvailableTowers[i][2] == 1 && availableTowers[i] > 2)
                                    availableTowers[i] = 3;
                                else if (PlayerAvailableTowers[i][1] == 1 && availableTowers[i] > 1)
                                { 
                                    availableTowers[i] = 2;
                                    HexMap.ProgressBlockedTowers[i] = 1;
                                }
                                else if (PlayerAvailableTowers[i][0] == 1 && availableTowers[i] > 0)
                                { 
                                    availableTowers[i] = 1;
                                    HexMap.ProgressBlockedTowers[i] = 1;
                                }
                            }
                        }

                        //string[] test = reader.ReadLine().Split(new char[]{':', ' '}, StringSplitOptions.RemoveEmptyEntries);
                        initLife = int.Parse(reader.ReadLine().Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
                        initEnergy = int.Parse(reader.ReadLine().Split(new char[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries)[1]);
						read = reader.ReadLine().Split(':', ',');
                        initGenePoints = new int[] { int.Parse(read[1]), int.Parse(read[2]), int.Parse(read[3]) };

						loadedMap = new HexMap(ParentGame, loadedMap.Name, layout, spawnPoints.ToArray(), goalPoints.ToArray(), CurrentGame.players, loadedMap.IsStoryMap);
						CurrentGame.currentMap = loadedMap;
						CurrentGame.HUD.ParentMap = loadedMap;
                        for (int i = 0; i < initTowers.Count; i++)
                        {
                            initTowers[i].ParentMap = loadedMap;
							CurrentGame.players[0].Towers.Add(Tower.Clone(initTowers[i]));
							CurrentGame.players[0].Towers[i].MapCoord = initTowers[i].MapCoord;
                        }
                        loadedMap.InitTowers = initTowers;

                        while (!reader.ReadLine().Contains("Creamt")) ;

                        for (int w = 0; reader.Peek() == 87; w++) //wave lines begin with a W (87)
                        {
                            waves.Add(new Wave(loadedMap));
                            waves[w].TempGroups = new List<SpawnGroup>();

                            reader.ReadLine();
                            while (reader.Peek() == 9) //group lines begin with a tab (9)
                            {
                                read = reader.ReadLine().Split(new char[] { '\t', '\r', '\n' }/*, StringSplitOptions.RemoveEmptyEntries*/);
                            if (read.Length == 18)
                                read = new string[] { read[1], read[2], string.Concat(read[3], read[4]), string.Concat(read[5], read[6]), read[7], read[8], read[9], read[10], read[11], read[12], read[13], read[14], read[15], read[16], read[17] };
                            else if (read.Length == 16)
                                read = new string[] { read[1], read[2], read[3], read[4], read[5], read[6], read[7], read[8], read[9], read[10], read[11], read[12], read[13], read[14], read[15] };
                            else if (read[3].Length > 7)
                                read = new string[] { read[1], read[2], read[3], string.Concat(read[4], read[5]), read[6], read[7], read[8], read[9], read[10], read[11], read[12], read[13], read[14], read[15], read[16] };
                            else if (read[5].Length > 7)
                                read = new string[] { read[1], read[2], string.Concat(read[3], read[4]), read[5], read[6], read[7], read[8], read[9], read[10], read[11], read[12], read[13], read[14], read[15], read[16] };
                            waves[w].TempGroups.Add(new SpawnGroup(read[0] == "" ? 0 : int.Parse(read[0]),
																						   new Creature(read[1],
																										read[2],
																										loadedMap,
																										read[3],
																										read[4] == "" ? 0 : int.Parse(read[4]) - 1,
																										read[5] == "" ? 0 : (int)(char.Parse(read[5])) - 97, //Goalpoint
																										read[6] == "" ? 0 : int.Parse(read[6]),
																										//read[7] == "" ? Element.None : (Element)Enum.Parse(typeof(Element), read[7], true),
																										read[7] == "" ? 0 : float.Parse(read[7], System.Globalization.NumberFormatInfo.InvariantInfo),
																										new GeneSpecs(float.Parse(read[10].ZeroIfEmpty(), System.Globalization.NumberFormatInfo.InvariantInfo), float.Parse(read[11].ZeroIfEmpty(), System.Globalization.NumberFormatInfo.InvariantInfo), float.Parse(read[12].ZeroIfEmpty(), System.Globalization.NumberFormatInfo.InvariantInfo)),
																										read[8] == "" ? (byte)0 : byte.Parse(read[8]),
																										read[9] == "" ? 0 : int.Parse(read[9]),
																										1f), // SCALE HARDCODED-----------------------------------------------------------------
																							read[13] == "" ? 0 : int.Parse(read[13]),
																							read[14] == "" ? 0 : int.Parse(read[14]), 
                                                                                            waves[w].TempGroups.Count, w));
                            }
                            waves[w].Groups = waves[w].TempGroups.ToArray();
                            waves[w].Initialize();
                        }
                    }
                    #region OLD LOAD
                    /*using (StreamReader reader = new StreamReader(filePath))
                {
                    
                    //--------Map size------------------------------------------
                    for (int i = 0; i < 8; i++)
                        reader.ReadLine();
                    columns = reader.ReadLine().Length;
                    while (reader.ReadLine() != "")
                        rows++;
                    layout = new char[rows + 1, columns]; // Y,X -----------!!!!!

                    reader.DiscardBufferedData();
                    reader.BaseStream.Position = 0;
                    //--------Map layout----------------------------------------
                    for (int i = 0; i < 8; i++)
                        reader.ReadLine();
                    for (int row = 0; row <= layout.GetUpperBound(0); row++)
                    { for (int col = 0; col <= layout.GetUpperBound(1); col++)
                        {
                            while (reader.Peek() < 32) reader.Read(); //run through whitespace characters, except space (32), which is the last of the whitespace in the ascii table
                            int ascii = reader.Peek();                            
                            switch (ascii)
                            {
                                case 32: layout[row, col] = 0; break; // 32 = space -> empty (3)
                                case 39: layout[row, col] = 3; break; // 39 = ' -> path (1)
                                case 46: layout[row, col] = 3; break; // 46 = . -> path (1) (odd columns)
                                case 48: layout[row, col] = 1; break; // 48 = 0 -> wall (0)
                                case 49: layout[row, col] = 6; break; // 49 = 1 -> tower type 1 (6)
                            }
                            reader.Read();
                        }
                    }


                    //--------Initial resources--------------------------------
                    while (!reader.ReadLine().Contains("Life/Energy/Genes")) ;
                    initLife = byte.Parse(reader.ReadLine());
                    initEnergy = int.Parse(reader.ReadLine());
                    initUpgPoints = int.Parse(reader.ReadLine());

                    //--------Spawnpoints--------------------------------------
                    while (!reader.ReadLine().Contains("SpawnPoints")) ;
                    read = reader.ReadLine().Split('\t');
                    while (read.Length > 1)
                    {
                        spawnPoints.Add(new Point(int.Parse(read[0]), int.Parse(read[1])));
                        read = reader.ReadLine().Split('\t');
                    }

                    //--------Goalpoints and openingtimes---------------------
                    while (!reader.ReadLine().Contains("GoalPoints")) ;
                    read = reader.ReadLine().Split('\t');
                    while (read.Length > 1)
                    {
                        goalPoints.Add(new Point(int.Parse(read[0]), int.Parse(read[1])));
                        goalPointOpeningTimes.Add(int.Parse(read[2]));
                        read = reader.ReadLine().Split('\t');
                    }
                    
                    loadedMap = new HexMap(ParentGame, layout, spawnPoints.ToArray(), goalPoints.ToArray(), goalPointOpeningTimes.ToArray(), ParentGame.players);
                    
                    while (!reader.ReadLine().Contains("Wave 1")) ;

                    bool newWaveLine;
                    bool newSpawnGroupLine;
                    do
                    {
                        read = reader.ReadLine().Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                        //string[] av = read[5].Split(',');
                        spawnGroups.Add(new SpawnGroup(int.Parse(read[0]),   //Number of creatures in group
                                                       new Creature(read[1],     //Type
                                                          read[2],               //Name
                                                          loadedMap,             //Map
                                                          ParentGame.Content.Load<Texture2D>("Creatures\\" + read[3]), //Texture
                                                          int.Parse(read[4]),    //SpawnPoint
                                                          Array.ConvertAll(read[5].Split(','), int.Parse), //GoalPoints----------COOL SH*T
                                                          int.Parse(read[6]),    //InitHp
                                                          float.Parse(read[7]),  //RRes
                                                          float.Parse(read[8]),  //GRes
                                                          float.Parse(read[9]),  //BRes
                                                          float.Parse(read[10], NumberFormatInfo.InvariantInfo), //DefSpd
                                                          float.Parse(read[11], NumberFormatInfo.InvariantInfo), //CellDmg
                                                          byte.Parse(read[12]),  //LifeDmg
                                                          int.Parse(read[13]),   //GeneBounty                                  
                                                          1f, // SCALE HARDCODED-----------------------------------------------------------------
                                                          read[3]),              //TextureName -------------------------------------------not cool
                                                       int.Parse(read[14]), //Spawn rate
                                                       int.Parse(read[15])));//Wave duration
                        //Check next line
                        newWaveLine = reader.Peek() == 87; //87 = W (as in Wave)
                        newSpawnGroupLine = reader.Peek() == 9; //9 = tab (spawnGroup lines begin with a tab)
                        if (newWaveLine)
                        {
                            waves.Add(new Wave(loadedMap, spawnGroups.ToArray()));
                            spawnGroups.Clear();
                            reader.ReadLine();
                        }
                        else if (newSpawnGroupLine)
                        {
                            continue;
                        }
                        else waves.Add(new Wave(loadedMap, spawnGroups.ToArray()));
                    } while (newWaveLine || newSpawnGroupLine);

                    loadedMap.Waves = waves.ToArray();*/
                    #endregion

                    loadedMap.PlayerInitLife = (short)initLife;
                    loadedMap.PlayerInitEnergy = initEnergy;
                    loadedMap.PlayerInitGenePoints = initGenePoints;
                    loadedMap.Waves = waves.ToArray();
                    loadedMap.Pathfinder.InitializeTiles();             //-----------------------hmmmmmmmmm
                    loadedMap.SpawnPoints = spawnPoints.ToArray();
                    loadedMap.GoalPoints = goalPoints.ToArray();
                    loadedMap.AvailableTowers = availableTowers;
                    loadedMap.ResetMap();
                    CurrentGame.gameState = GameState.InitSetup;
				//}
				//catch (Exception)
				//{
				//    mapButton.Text += " (bad file!)";
				//    CurrentGame.gameState = GameState.MainMenu;
				//}
            }
            else Debug.WriteLine("\"" + mapButton.Text + "\" doesn't exist");
        }

        internal void CreateNewPlayer(string nameInput)
        {
            if (nameInput.Length <= 0) return;
            for (int i = 0; i < PlayerNames.Length; i++)
            {
                if (PlayerNames[i].Equals(nameInput, StringComparison.CurrentCultureIgnoreCase))
                {
                    nameAlreadyExists = true;
                    return;
                }
            }
            using (StreamWriter sw = new StreamWriter(CurrentGame.SaveDir + nameInput + ".txt"))
            {
                Debug.WriteLine("Creating player file!");
                sw.WriteLine("CompletedLevels:" + Environment.NewLine);
                //sw.WriteLine("HighScores:\r\n");
            }
            ;
            CurrentGame.players[0] = new Player(nameInput);
            //CurrentPlayerIndexes[0] = ParentGame.SaveDir + nameInput + ".txt";
            CurrentPlayerIndexes[0] = 0;
            RefreshPlayerSaveData();
            menuState = MenuState.MapSelection;
        }

        public void SavePlayerData(HexMap playedMap)
        {
            //string completedLevel = currentMap;
            //if (CurrentGame.players[0].UpdateScore() > CurrentGame.players[0].CompletedLevels[currentMap])
            //	CurrentGame.players[0].CompletedLevels[currentMap] = CurrentGame.players[0].Score;
            

            CurrentGame.players[0].UpdateScore(playedMap);

            if (CurrentPlayerIndexes[0] == -1)
                return;

            using (FileStream stream = new FileStream(PlayerFilePaths[CurrentPlayerIndexes[0]], FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
            {
                StreamWriter writer = new StreamWriter(stream);
                StreamReader reader = new StreamReader(stream);

                Debug.WriteLine("Saving player file!");
                stream.Position = 0;
                string[] read = reader.ReadToEnd().Split(new string[]{ Environment.NewLine }, StringSplitOptions.None);
                List<string> readList = read.ToList();
                bool containsMap = false;
                
                for (int i = 0; i < readList.Count; i++)
                {
                    if (readList[i].Contains(currentMap))
                    {
                        //update score
                        containsMap = true;
                        int oldScore = int.Parse(readList[i].Split('-')[1]);
                        if (oldScore < CurrentGame.players[0].Score)
                        {
                            readList[i] = currentMap + " - " + CurrentGame.players[0].Score;
                        }
                    }
                }

                if(!containsMap)
                {
                    readList.Insert(1, currentMap + " - " + CurrentGame.players[0].Score);
                }

                stream.Position = 0;
                foreach (string line in readList)
                {
                    writer.WriteLine(line);
                }
                writer.Flush();
                stream.SetLength(stream.Position);
            }

            /*if (CurrentGame.players[0].UpdateScore() > CurrentGame.players[0].HighScores[currentMap - 1])
                CurrentGame.players[0].HighScores[currentMap - 1] = CurrentGame.players[0].Score;
            using (StreamWriter writer = new StreamWriter(PlayerFilePaths[CurrentPlayerIndexes[0]]))
            {
                Debug.WriteLine("Saving player file!");
                if (CurrentGame.players[0].CompletedLevels < currentMap)
                {
                    writer.WriteLine("CompletedLevels:" + Environment.NewLine + currentMap);
                    CurrentGame.players[0].CompletedLevels = currentMap;
                }
                else writer.WriteLine("CompletedLevels:" + Environment.NewLine + CurrentGame.players[0].CompletedLevels);
                writer.WriteLine("HighScores:");
                writer.WriteLine(CurrentGame.players[0].HighScores[0]);
                writer.WriteLine(CurrentGame.players[0].HighScores[1]);
                writer.WriteLine(CurrentGame.players[0].HighScores[2]);
            }*/
        }
        
        public void RefreshPlayerSaveData()
        {
            FileInfo[] pfiles = new DirectoryInfo(CurrentGame.SaveDir).GetFiles();
            Array.Sort(pfiles, (y,x) => Comparer<DateTime>.Default.Compare(x.CreationTime, y.CreationTime)); //------------------------------lambdaa!!!!!!!!!!!!!
            PlayerFilePaths = new string[pfiles.Length];
            for (int f = 0; f < pfiles.Length; f++)
                PlayerFilePaths[f] = pfiles[f].FullName;
            PlayerNames = Array.ConvertAll<string, string>(PlayerFilePaths, Path.GetFileNameWithoutExtension);

            string longestName = "";
            for (int n = 0; n < PlayerNames.Length; n++)
                if (PlayerNames[n].Length > longestName.Length)
                    longestName = PlayerNames[n];
            playerButtonWidth = (int)Math.Max(Font.MeasureString("Min.koko").X, Font.MeasureString(longestName).X) + 2 * padding;

            PlayerButtons = new Button[PlayerNames.Length];
            for (int p = 0; p < PlayerNames.Length; p++)
                PlayerButtons[p] = new Button(PlayerNames[p], playerButtonX, rootButtonY + buttonHeight + p*buttonHeight, playerButtonWidth, buttonHeight, padding, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel);

            mapButtonX = (int)(playerButtonX + playerButtonWidth + 2);
            string[] mapNames = Array.ConvertAll<string, string>(Directory.GetFiles(CurrentGame.MapDir), Path.GetFileNameWithoutExtension);
            for (int m = 0; m < mapNames.Length; m++)
                MapButtons[m] = new Button(mapNames[m], mapButtonX, mapButtonY + m * buttonHeight, mapButtonWidth, buttonHeight, padding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel);
        }

        public void RefreshMapData()
        {
            Button[] tempButtonArr;
            string[] mapNames = Array.ConvertAll<string, string>(Directory.GetFiles(CurrentGame.MapDir), Path.GetFileNameWithoutExtension);
            tempButtonArr = new Button[mapNames.Length];
            for (int i = 0; i < mapNames.Length; i++)
            {
                tempButtonArr[i] = new Button(mapNames[i], MapButtons[0].Pos.X, MapButtons[0].Pos.Y + i*MapButtons[0].Height, MapButtons[0].Width, MapButtons[0].Height, MapButtons[0].Padding, MapButtons[0].TextAlign, MapButtons[0].ButtonColors, MapButtons[0].TextColors, MapButtons[0].ButtonTexture);
            }
            MapButtons = tempButtonArr;
        }

        private void AdjustAvailableTowersForMap(string mapName)
        {
            for (int i = 1; i < PlayerAvailableTowersTileringTextures.Length; i++)
                PlayerAvailableTowersTileringTextures[i] = null;
            PlayerAvailableTowersTileringTextures[1] = TowersTileringTextures[0];

            using (StreamReader sr = new StreamReader(CurrentGame.MapDir + mapName + ".txt"))
            {
                string[] read = sr.ReadToEnd().Split('\n', '\r');
                string[] availableTowers = Array.Find(read, s => s.StartsWith("Available towers: ")).Split(' ');
                for (int i = 2; i < availableTowers.Length; i++)
                {
                    if (availableTowers[i] == "-")
                        PlayerAvailableTowersTileringTextures[i - 1] = null;
                    else 
                        PlayerAvailableTowersTileringTextures[i - 1] = TowersTileringTextures[(i-2) * 3 + int.Parse(availableTowers[i]) - 1];
                }
            }
        }

        private void AdjustAvailableTowersForPlayer()
        {
            for (int i = 0; i < PlayerAvailableTowers.Length; i++)
            {
                if (PlayerAvailableTowers[i][2] == 1)
                    PlayerAvailableTowersTileringTextures[i+1] = TowersTileringTextures[i * 3 + 2];
                else if (PlayerAvailableTowers[i][1] == 1)
                    PlayerAvailableTowersTileringTextures[i+1] = TowersTileringTextures[i * 3 + 1];
                else if (PlayerAvailableTowers[i][0] == 1)
                    PlayerAvailableTowersTileringTextures[i+1] = TowersTileringTextures[i * 3];
                else
                    PlayerAvailableTowersTileringTextures[i+1] = null;
            }
        }

        string nameInput;
        bool nameAlreadyExists;
        int backspaceRefreshCounter; //-------------------------not elegant....?
        MouseState prevMouse;
        Button prevButton;
        public void Update(MouseState mouse, KeyboardState keyboard)
        {
            // AUTOMATED LEVEL SELECTOR.................................................................................................................................................!
            //LoadPlayerData(0); LoadMap(MapButtons[5]);
			//if (CurrentGame.gameState != GameState.MapEditor)
			//{
			//	CurrentGame.gameState = GameState.MapEditor;
			//	CurrentGame.HUD.MapEditorSpawnPoints = new List<Point>();
			//	CurrentGame.HUD.MapEditorGoalPoints = new List<Point>();
			//	CurrentGame.currentMap = new HexMap(ParentGame, "newMap", new char[11, 21], new Point[1], new Point[1], new Player[] { new Player("map editor person") }); //-----------------------------------------------täällä !;
			//	CurrentGame.HUD.ParentMap = CurrentGame.currentMap;
			//	CurrentGame.currentMap.MapEditorTempWaves = new List<Wave>();
   //             //CurrentGame.HUD.EditorMapLoad(MapButtons[1]);
   //             int rightIndex = 0;
   //             for (int i = 0; i < MapButtons.Length; i++)
   //             {
   //                 if (MapButtons[i].Text == "Nykymap1")
   //                 {
   //                     rightIndex = i;
   //                     break;
   //                 }
   //             }
   //             CurrentGame.HUD.EditorMapLoad(MapButtons[rightIndex]);
			//}

            if (keyboard.IsKeyDown(Keys.Escape)) menuState = MenuState.Main;

            for (int r = 0; r < RootButtons.Length; r++)
            {
                RootButtons[r].Update(mouse, CurrentGame.prevMouse);
                if (RootButtons[r].State == ButnState.Released)
                {
                    CurrentGame.soundBank.PlayCue("kansi");

                    switch (r)
                    {
                        case 0: menuState = MenuState.NewGame;
                                CurrentPlayerIndexes[0] = -1;
                                nameAlreadyExists = false;
                                backspaceRefreshCounter = 0;
                                nameInput = "";
                                break;
                        case 1: menuState = MenuState.Continue; CurrentPlayerIndexes[0] = -1; break;
                        case 2: menuState = MenuState.Options; CurrentPlayerIndexes[0] = -1; break;
                        case 3: menuState = MenuState.MapEditor; CurrentPlayerIndexes[0] = -1; foreach (Button b in MapButtons) b.TextAlign = TextAlignment.Center; break;
                        case 4: ParentGame.Exit(); break;
                    }
                }
            }
            if (menuState == MenuState.NewGame)
            {
                #region NAME INPUT & PLAYER FILE CREATION
                {
                    if (keyboard.IsKeyUp(Keys.Back)) backspaceRefreshCounter = 20;
                    foreach (Keys key in keyboard.GetPressedKeys())
                    {
                        if ((byte)key > 8 && (byte)key < 48 && key != Keys.Space && key != Keys.Enter || (byte)key > 90) continue; //unpractical exclusion of keys
                        if (key == Keys.Back)
                        {
                            if (backspaceRefreshCounter == 20 && nameInput.Length > 0) nameInput = nameInput.Remove(nameInput.Length -1, 1);
                            if (backspaceRefreshCounter == 0 && nameInput.Length > 0)
                            {
                                nameInput = nameInput.Remove(nameInput.Length - 1, 1);
                                backspaceRefreshCounter = 4;                            
                            }                        
                            backspaceRefreshCounter -= 1;
                            nameAlreadyExists = false;
                        }
                        else if (CurrentGame.prevKeyboard.IsKeyUp(key))
                        {
                            if (key == Keys.Enter)
                                CreateNewPlayer(nameInput);
                            
                            else if (nameInput.Length < 15)
                            {
                                nameAlreadyExists = false;
                                if (key == Keys.Space) nameInput += " ";
                                else if (!keyboard.IsKeyDown(Keys.LeftShift) && !keyboard.IsKeyDown(Keys.RightShift)) nameInput += key.ToString().ToLower();
                                else nameInput += key.ToString();
                            }
                        }
                    }
                }
                #endregion

                NewPlayerButtons[1].Text = nameInput;
                int width = (int)Font.MeasureString(nameInput).X;
                if (width > playerButtonWidth - padding*2)
                    NewPlayerButtons[1].Width = width + padding*2;
                RootButtons[0].State = ButnState.Pressed;
            }
            else if (menuState == MenuState.Continue || menuState == MenuState.MapSelection)
            {
                for (int p = 0; p < PlayerButtons.Length; p++)
                {
                    PlayerButtons[p].Update(mouse, CurrentGame.prevMouse);
                    if (PlayerButtons[p].State == ButnState.Released)
                    {
                        CurrentGame.soundBank.PlayCue("kansi");

                        LoadPlayerData(p);
                        CurrentPlayerIndexes[0] = p;
                        menuState = MenuState.MapSelection;
                    }
                    RootButtons[1].State = ButnState.Pressed;
                }

                
            }
            else if (menuState == MenuState.MapEditor)
            {
                RefreshMapData();
                for (int m = 0; m < MapButtons.Length; m++)
                {
                    MapButtons[m].Update(mouse, CurrentGame.prevMouse);
                    if (MapButtons[m].State == ButnState.Released)
                    {
                        CurrentGame.soundBank.PlayCue("kansi");
                        CurrentGame.gameState = GameState.MapEditor;
						CurrentGame.HUD.MapEditorSpawnPoints = new List<Point>();
						CurrentGame.HUD.MapEditorGoalPoints = new List<Point>();
						CurrentGame.currentMap = new HexMap(ParentGame, "loadedMap", new char[11, 21], new Point[1], new Point[1], new Player[] { new Player("map editor person") }); //-----------------------------------------------täällä !;
                        playerAvailableTowers = new byte[6] { 3, 3, 3, 3, 3, 3 };
                        PlayerAvailableTowers = new byte[6][] { new byte[] { 1, 1, 1, 1, 1, 1 }, new byte[] { 1, 1, 1, 1, 1, 1 }, new byte[] { 1, 1, 1, 1, 1, 1 }, new byte[] { 1, 1, 1, 1, 1, 1 }, new byte[] { 1, 1, 1, 1, 1, 1 }, new byte[] { 1, 1, 1, 1, 1, 1 } };
                        AdjustAvailableTowersForPlayer();
                        CurrentGame.HUD.ParentMap = CurrentGame.currentMap;
						CurrentGame.currentMap.MapEditorTempWaves = new List<Wave>();
						CurrentGame.HUD.EditorMapLoad(MapButtons[m]);
                    }
                }
                for (int e = 0; e < MapEditorButtons.Length; e++)
                    MapEditorButtons[e].Update(mouse, CurrentGame.prevMouse);
                //MapEditorButtons[0].State = ButnState.Released;//-------------------------------------------------------------------------------------------------------------------------------------------------------------------!!poista
                //ParentGame.HUD.inWaveEdit = true;//---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------!!poista
                if (MapEditorButtons[0].State == ButnState.Released)
                {
                    CurrentGame.soundBank.PlayCue("kansi");
                    char[,] emptyLayout = new char[11,21];
                    for (int dim1 = 0; dim1 < emptyLayout.GetLength(0); dim1++)
                        for (int dim2 = 0; dim2 < emptyLayout.GetLength(1); dim2++)
                            emptyLayout[dim1, dim2] = ' ';
                    HexMap TempMap = new HexMap(ParentGame, "newMap", emptyLayout, new Point[1], new Point[1], new Player[]{new Player("map editor person")}); //-----------------------------------------------täällä temp mappia ku static CoordToScrLoc puuttuu!
					CurrentGame.HUD.MapEditorSpawnPoints = new List<Point>();
					CurrentGame.HUD.MapEditorGoalPoints = new List<Point>();
                    TempMap.MapEditorTempWaves = new List<Wave>();
                    TempMap.MapEditorTempWaves.Add(new Wave(TempMap));
                    TempMap.MapEditorTempWaves[0].TempGroups = new List<SpawnGroup>();
                    TempMap.MapEditorTempWaves[0].TempGroups.Add(new SpawnGroup());
					CurrentGame.HUD.MapEditorResourceCells[0].Text = "0";
					CurrentGame.HUD.MapEditorResourceCells[1].Text = "0";
					CurrentGame.HUD.MapEditorResourceCells[2].Text = "0";
					CurrentGame.HUD.MapNameBox.Text = "";
					CurrentGame.currentMap = TempMap;
					CurrentGame.HUD.ParentMap = TempMap;
                    CurrentGame.gameState = GameState.MapEditor;
                }
                //RootButtons[3].State = ButnState.Pressed; //--------------------------------------MAKE AS TOGGLE--------!
            }


            if (menuState == MenuState.MapSelection)
            {
                for (int m = 0; m < MapButtons.Length; m++)
                {
                    MapButtons[m].Update(CurrentGame.mouse, CurrentGame.prevMouse);
                    if (MapButtons[m].State == ButnState.Released)
                    {
                        CurrentGame.soundBank.PlayCue("kansi");
                        LoadMap(MapButtons[m]);
                        return;
                    }
                }
                PlayerButtons[CurrentPlayerIndexes[0]].State = ButnState.Pressed;

                if (CurrentPlayerIndexes[0] >= 0)
                {
                    AdjustAvailableTowersForPlayer();
                    for (int i = 0; i < MapButtons.Length; i++)
                    {
                        if (MapButtons[i].State == ButnState.Hovered)
                        {
                            if (MapButtons[i] != prevButton)
                                CurrentGame.soundBank.PlayCue("kansi");
                            AdjustAvailableTowersForMap(MapNames[i]);
                            prevButton = MapButtons[i];
                        }
                        if (MapZoneButtons[i] != null)
                        {
                            MapZoneButtons[i].Update(mouse, prevMouse);
                            if (MapButtons[i].State == ButnState.Hovered)
                                MapZoneButtons[i].State = ButnState.Hovered;

                            if (MapZoneButtons[i].State == ButnState.Hovered)
                            {
                                if (MapButtons[i] != prevButton)
                                    CurrentGame.soundBank.PlayCue("kansi");
                                MapButtons[i].State = ButnState.Hovered;
                                AdjustAvailableTowersForMap(MapNames[i]);
                                prevButton = MapButtons[i];
                            }
                            else if (MapZoneButtons[i].State == ButnState.Released)
                            {
                                CurrentGame.soundBank.PlayCue("kansi");
                                LoadMap(MapButtons[i]);
                            }

                        }
                    }
                }
            }

            #region OLD BUTTONSYSTEM
            /*for (int i = 0; i < ButtonBoundses.Length; i++)
            {
                if ((menuState == MenuState.Main || menuState == MenuState.NewGame) && i > 3) continue; // in Main and NewGame, ignore maps, options and player buttons
                else if (menuState == MenuState.Continue && (i > 3 && i < 8)) continue; // in Continue, ignore maps and options
                else if (menuState == MenuState.Options && ((i > 3 && i < 7) || i > 7)) continue; // in Options, ignore maps and players
                else if (menuState == MenuState.MapSelection && (i < 4 || i > 4 + ParentGame.players[0].CompletedLevels)) continue; // in MapSelection, ignore main, option, player and incompleted level buttons
                
                if (ButtonBoundses[i].Contains(mouse.X, mouse.Y))
                {
                    if (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed)
                    {
                        ButtonStates[i] = MenuButtonState.Released;
                        switch (i)
                        {
                            case 0: menuState = MenuState.NewGame;
                                    nameAlreadyExists = false;
                                    backspaceRefreshCounter = 0;
                                    nameInput = ""; break;
                            case 1: menuState = MenuState.Continue; break;
                            case 2: menuState = MenuState.Options; break;
                            case 3: ParentGame.Exit(); break;
                            case 4: LoadMap("map1"); currentMap = 1; break;
                            case 5: LoadMap("map2"); currentMap = 2; break;
                            case 6: LoadMap("map3"); currentMap = 3; break;
                            case 7: break; //-----------------------------------Resolution!
                            default: LoadPlayerData(i - 8);
                                    CurrentPlayersFilePaths[0] = PlayerFilePaths[i - 8];
                                    menuState = MenuState.MapSelection;
                                    break;
                        }
                    }
                    else if (mouse.LeftButton == ButtonState.Pressed) ButtonStates[i] = MenuButtonState.Pressed;
                    else ButtonStates[i] = MenuButtonState.Hovered;
                }
                else ButtonStates[i] = MenuButtonState.Passive;                                
            }*/
            #endregion

            prevMouse = mouse;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.DrawString(CurrentGame.font, "Pöpö Defense", new Vector2(ParentGame.GraphicsDevice.Viewport.Width / 2 - CurrentGame.font.MeasureString("Alku TD Menu").X / 2,
                          ParentGame.GraphicsDevice.Viewport.Height / 6), Color.Orange, 0f, Vector2.Zero, 1, SpriteEffects.None, 0);

            #region OLD BUTTONSYSTEM
            /*for (int i = 0; i < ButtonBoundses.Length; i++)
            {
                if ((menuState == MenuState.Main || menuState == MenuState.NewGame) && i > 3) continue; // if in Main or NewGame, ignore all else (maps, options and player buttons)
                else if (menuState == MenuState.Continue && (i > 3 && i < 8)) continue; // if in Continue, ignore maps and options
                else if (menuState == MenuState.Options && ((i > 3 && i < 7) || i > 7)) continue; // if in Options, ignore maps and players
                else if (menuState == MenuState.MapSelection && (i < 4 || i > 4 + ParentGame.players[0].CompletedLevels)) continue; // if in MapSelection, ignore main, option, player and incompleted level buttons
                
                switch (ButtonStates[i])
                {
                    case MenuButtonState.Pressed:
                        sb.Draw(Game1.pixel, ButtonBoundses[i], new Color(30, 40, 50));
                        sb.DrawString(Font, RootButtonWords[i], new Vector2(ButtonBoundses[i].Right, ButtonBoundses[i].Center.Y), Color.Orange, 0f,
                                      new Vector2(Font.MeasureString(RootButtonWords[i]).X + 10, Font.MeasureString(RootButtonWords[i]).Y / 2), 1, SpriteEffects.None, 0);
                        break;
                    case MenuButtonState.Hovered:
                        sb.Draw(Game1.pixel, ButtonBoundses[i], new Color(20, 30, 40));
                        sb.DrawString(Font, RootButtonWords[i], new Vector2(ButtonBoundses[i].Right, ButtonBoundses[i].Center.Y), Color.Orange, 0f,
                                      new Vector2(Font.MeasureString(RootButtonWords[i]).X + 10, Font.MeasureString(RootButtonWords[i]).Y / 2), 1, SpriteEffects.None, 0);
                        break;
                    default:
                        sb.Draw(Game1.pixel, ButtonBoundses[i], new Color(10, 20, 30));
                        sb.DrawString(Font, RootButtonWords[i], new Vector2(ButtonBoundses[i].Right, ButtonBoundses[i].Center.Y), Color.SlateGray, 0f,
                                      new Vector2(Font.MeasureString(RootButtonWords[i]).X + 10, Font.MeasureString(RootButtonWords[i]).Y / 2), 1, SpriteEffects.None, 0);
                        break;
                }
            }*/
            #endregion

            for (int i = 0; i < RootButtons.Length; i++)
                RootButtons[i].Draw(sb);

            if (menuState == MenuState.NewGame)
            {
                NewPlayerButtons[1].Draw(sb);
                NewPlayerButtons[0].Draw(sb);
                if (nameAlreadyExists)
                    NewPlayerButtons[2].Draw(sb);
            }
            else if (menuState == MenuState.Continue || menuState == MenuState.MapSelection)
            {
                for (int p = 0; p < PlayerButtons.Length; p++)
                {
                    PlayerButtons[p].Draw(sb);
                }
                if (CurrentPlayerIndexes[0] >= 0)
                {
                    //sb.DrawString(CurrentGame.font, prevMouse.X + "," + prevMouse.Y, new Vector2(prevMouse.X + 150, prevMouse.Y), Color.White * 0.2f); //---COOOOOOOOORDS
                    sb.Draw(bodyMapTex, new Vector2(mapButtonX + MapButtons[0].Width, mapButtonY), null, Color.White, 0, Vector2.Zero, 0.3f, SpriteEffects.None, 0);
                    for (int i = 0; i < MapZoneButtons.Length; i++)
                    {
                        if (MapZoneButtons[i] != null)
                            MapZoneButtons[i].Draw(sb);
                    }

                    for (int p = PlayerAvailableTowersTileringTextures.Length-1; p >= 0; p--)
                    {
                        if (PlayerAvailableTowersTileringTextures[p] != null)
                            sb.Draw(PlayerAvailableTowersTileringTextures[p], new Vector2(rootButtonX + rootButtonWidth + 5, rootButtonY - CurrentGame.tilering.Height + buttonHeight - 5), Color.White);
                    }

                    
                }
                
            }
            if (menuState == MenuState.MapSelection || menuState == MenuState.MapEditor)
            {
                for (int m = 0; m < MapButtons.Length; m++)
                {
                    MapButtons[m].Draw(sb);
                    if (MapButtons[m].State == ButnState.Hovered)
                    {
                        sb.DrawString(Font, "AVAILABLE:", new Vector2(rootButtonX + 5, rootButtonY - buttonHeight - 5), Color.White);
                        if (StoryMaps.ContainsKey(MapButtons[m].Text))
                        { 
                            sb.DrawString(Font, "UNLOCKS:", new Vector2(rootButtonX + 5, rootButtonY - buttonHeight * 4 - 5), Color.White);
                            string[] unlocks = StoryMaps[MapButtons[m].Text].Split(',');
                            foreach (string unlock in unlocks)
                            {
                                int ttype = (int.Parse(unlock.Substring(0,1).Trim()) - 1) * 3;
                                int tlvl = unlock[1] - 97;
                                sb.Draw(TowersTileringTextures[ttype + tlvl], new Vector2(rootButtonX + rootButtonWidth + 5, rootButtonY - CurrentGame.tilering.Height * 1.8f), Color.White);
                            }
                        }
                        //sb.Draw(CurrentGame.tilering, new Vector2(rootButtonX + rootButtonWidth + 5, rootButtonY - CurrentGame.tilering.Height * 2 + buttonHeight - 5), Color.White);
                    }
                }
            }
            if (menuState == MenuState.MapEditor)
            {
                for (int e = 0; e < MapEditorButtons.Length; e++)
                    MapEditorButtons[e].Draw(sb);
            }
        }

        
    }
}

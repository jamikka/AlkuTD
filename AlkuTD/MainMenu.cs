using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

        //public Rectangle[] ButtonBoundses;

        public string[] PlayerFilePaths;
        public string[] PlayerNames;
        //public Player[] CurrentPlayers; // = ParentGame.players
        public int[] CurrentPlayerIndexes;
        public byte currentMap;

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
            PlayerNames = Array.ConvertAll<string, string>(PlayerFilePaths, Path.GetFileNameWithoutExtension); // in one line!

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
            rootButtonX = (int)(game.GraphicsDevice.Viewport.Width / 2.8); //-----uus vasemmal 
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

            string[] mapNames = Array.ConvertAll<string, string>(Directory.GetFiles(CurrentGame.MapDir), Path.GetFileNameWithoutExtension);
            MapButtons = new Button[mapNames.Length];
            for (int m = 0; m < mapNames.Length; m++)
                MapButtons[m] = new Button(mapNames[m], mapButtonX, mapButtonY + m*buttonHeight, mapButtonWidth, buttonHeight, padding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel);

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
        }

        void LoadPlayerData(int fileIndex)
        {
            Player loadedPlayer = new Player(PlayerNames[fileIndex]);

            using (StreamReader reader = new StreamReader(PlayerFilePaths[fileIndex]))
            {
                reader.ReadLine();
                loadedPlayer.CompletedLevels = Convert.ToByte(reader.ReadLine());
                reader.ReadLine();
                loadedPlayer.HighScores[0] = Convert.ToInt32(reader.ReadLine());
                loadedPlayer.HighScores[1] = Convert.ToInt32(reader.ReadLine());
                loadedPlayer.HighScores[2] = Convert.ToInt32(reader.ReadLine());
            }
            CurrentGame.players[0] = loadedPlayer;
			CurrentGame.players[1] = null;
            //CurrentPlayers = ParentGame.players;
        }

        void LoadMap(Button mapButton)
        {
            //currentMap = byte.Parse(mapName.Substring(mapName.Length -1, 1)); //-------------------------------------------------risky.

            if (File.Exists(CurrentGame.MapDir + mapButton.Text + ".txt"))
            {
                //try
                //{
                    string[] read;
					HexMap loadedMap = new HexMap(ParentGame, mapButton.Text, new char[1, 1], null, null, CurrentGame.players);
                    char[,] layout = new char[11, 21];
                    List<Point> spawnPoints = new List<Point>();
                    List<Point> goalPoints = new List<Point>();
                    List<Wave> waves = new List<Wave>();
                    byte initLife;
                    int initEnergy;
                    int[] initGenePoints;
                    byte[] availableTowers = new byte[6];
                    List<Tower> initTowers = new List<Tower>();

                    using (StreamReader reader = new StreamReader(CurrentGame.MapDir + mapButton.Text + ".txt"))
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

                        read = reader.ReadLine().Split(':', ' ');
                        for (int i = 0; i < availableTowers.Length; i++)
                            byte.TryParse(read[i + 3], out availableTowers[i]);

                        initLife = byte.Parse(reader.ReadLine().Split(':')[1]);
                        initEnergy = int.Parse(reader.ReadLine().Split(':')[1]);
						read = reader.ReadLine().Split(':', ',');
                        initGenePoints = new int[] { int.Parse(read[1]), int.Parse(read[2]), int.Parse(read[3]) };

						loadedMap = new HexMap(ParentGame, loadedMap.Name, layout, spawnPoints.ToArray(), goalPoints.ToArray(), CurrentGame.players);
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
                                read = reader.ReadLine().Split(new char[] { '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                                waves[w].TempGroups.Add(new SpawnGroup(int.Parse(read[0]),
                                                                                                    new Creature(read[1],
                                                                                                                read[2],
                                                                                                                loadedMap,
                                                                                                                read[3],
                                                                                                                int.Parse(read[4]) - 1,
                                                                                                                (int)(char.Parse(read[5])) - 97, //Goalpoint
                                                                                                                int.Parse(read[6]),
                                                                                                                //(Element)Enum.Parse(typeof(Element), read[7], true),
                                                                                                                float.Parse(read[7], System.Globalization.NumberFormatInfo.InvariantInfo),
																												new GeneSpecs(), //TÄHÄ VÄLIIN ELEMS
                                                                                                                byte.Parse(read[8]),
                                                                                                                int.Parse(read[9]),
                                                                                                                1f), // SCALE HARDCODED-----------------------------------------------------------------
                                                                                                    int.Parse(read[13]),
                                                                                                    int.Parse(read[14])));
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

                    loadedMap.PlayerInitLife = initLife;
                    loadedMap.PlayerInitEnergy = initEnergy;
                    loadedMap.PlayerInitGenePoints = initGenePoints;
                    loadedMap.Waves = waves.ToArray();
                    loadedMap.Pathfinder.InitializeTiles();             //-----------------------hmmmmmmmmm
                    loadedMap.SpawnPoints = spawnPoints.ToArray();
                    loadedMap.GoalPoints = goalPoints.ToArray();
                    loadedMap.AvailableTowers = availableTowers;
                    loadedMap.ResetMap();
                    CurrentGame.gameState = GameState.InGame;
				//}
				//catch (Exception)
				//{
				//    mapButton.Text += " (bad file!)";
				//    CurrentGame.gameState = GameState.MainMenu;
				//}
            }
            else Debug.WriteLine("\"" + mapButton.Text + "\" doesn't exist");
        }

        public void SavePlayerData()
        {
			if (CurrentGame.players[0].UpdateScore() > CurrentGame.players[0].HighScores[currentMap - 1])
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
            }            
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

        string nameInput;
        bool nameAlreadyExists;
        int backspaceRefreshCounter; //-------------------------not elegant....?
        MouseState prevMouse;
        public void Update(MouseState mouse, KeyboardState keyboard)
        {
            // AUTOMATED LEVEL SELECTOR.................................................................................................................................................!
            //LoadPlayerData(0); LoadMap(MapButtons[5]);
			if (CurrentGame.gameState != GameState.MapEditor)
			{
				CurrentGame.gameState = GameState.MapEditor;
				CurrentGame.HUD.MapEditorSpawnPoints = new List<Point>();
				CurrentGame.HUD.MapEditorGoalPoints = new List<Point>();
				CurrentGame.currentMap = new HexMap(ParentGame, "newMap", new char[11, 21], new Point[1], new Point[1], new Player[] { new Player("map editor person") }); //-----------------------------------------------täällä !;
				CurrentGame.HUD.ParentMap = CurrentGame.currentMap;
				CurrentGame.currentMap.MapEditorTempWaves = new List<Wave>();
                //CurrentGame.HUD.EditorMapLoad(MapButtons[1]);
                int rightIndex = 0;
                for (int i = 0; i < MapButtons.Length; i++)
                {
                    if (MapButtons[i].Text == "Monimaailma1")
                    {
                        rightIndex = i;
                        break;
                    }
                }
                CurrentGame.HUD.EditorMapLoad(MapButtons[rightIndex]);
			}

            if (keyboard.IsKeyDown(Keys.Escape)) menuState = MenuState.Main;

            for (int r = 0; r < RootButtons.Length; r++)
            {
                RootButtons[r].Update(mouse, CurrentGame.prevMouse);
                if (RootButtons[r].State == ButnState.Released)
                {
                    switch (r)
                    {
                        case 0: menuState = MenuState.NewGame;
                                nameAlreadyExists = false;
                                backspaceRefreshCounter = 0;
                                nameInput = ""; break;
                        case 1: menuState = MenuState.Continue; break;
                        case 2: menuState = MenuState.Options; break;
                        case 3: menuState = MenuState.MapEditor; break;
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
                                    sw.WriteLine("CompletedLevels: " + Environment.NewLine + "0");
                                    sw.WriteLine("HighScores:\r\n0\r\n0\r\n0");
                                };
								CurrentGame.players[0] = new Player(nameInput);
                                //CurrentPlayerIndexes[0] = ParentGame.SaveDir + nameInput + ".txt";
                                CurrentPlayerIndexes[0] = 0;
                                RefreshPlayerSaveData();
                                menuState = MenuState.MapSelection;
                            }
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
                        LoadPlayerData(p);
                        CurrentPlayerIndexes[0] = p;
                        menuState = MenuState.MapSelection;
                    }
                    RootButtons[1].State = ButnState.Pressed;
                }
            }
            else if (menuState == MenuState.MapEditor)
            {
                for (int m = 0; m < MapButtons.Length; m++)
                {
                    MapButtons[m].Update(mouse, CurrentGame.prevMouse);
                    if (MapButtons[m].State == ButnState.Released)
                    {
                        CurrentGame.gameState = GameState.MapEditor;
						CurrentGame.HUD.MapEditorSpawnPoints = new List<Point>();
						CurrentGame.HUD.MapEditorGoalPoints = new List<Point>();
						CurrentGame.currentMap = new HexMap(ParentGame, "newMap", new char[11, 21], new Point[1], new Point[1], new Player[] { new Player("map editor person") }); //-----------------------------------------------täällä !;
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
                        HexMap TempMap = new HexMap(ParentGame, "newMap", new char[11, 21], null, null, new Player[] { new Player("map editor person") }); //----------täällä temp mappia ku static CoordToScrLoc puuttuu!
						CurrentGame.currentMap = TempMap;
                        LoadMap(MapButtons[m]);
                    }
                }
                PlayerButtons[CurrentPlayerIndexes[0]].State = ButnState.Pressed;
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
            sb.DrawString(CurrentGame.font, "Alku TD Menu", new Vector2(ParentGame.GraphicsDevice.Viewport.Width / 2 - CurrentGame.font.MeasureString("Alku TD Menu").X / 2,
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
            }
            if (menuState == MenuState.MapSelection || menuState == MenuState.MapEditor)
            {
                for (int m = 0; m < MapButtons.Length; m++)
                {
                    MapButtons[m].Draw(sb);
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

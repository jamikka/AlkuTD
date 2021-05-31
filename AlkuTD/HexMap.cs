using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace AlkuTD
{
    public class HexMap
    {
        public CurrentGame ParentGame;

        public string Name;

        //public byte[,] InitLayout; //--[row,col]
        //public byte[,] Layout;
        public char[,] InitLayout;
        public char[,] Layout;

        public Random rnd;
        public Pathfinder Pathfinder;
        public Point[] SpawnPoints;
        public Point[] GoalPoints;
        public int[] GoalPointTimetable;
        public Vector2[] Path;
        public int Radius;
        public Point drawPos;            //---------NÄÄ VOIS OLLA STATIC!
        public int TileWidth;            //---------
        public int TileHalfWidth;        //---------
        public int stackedWidth;         //---------
        public int TileHeight;           //---------
        public int TileHalfHeight;       //---------
        public Vector2 wallNorm;
        public Vector2 wallPerpNorm;



        public Wave[] Waves;

        public List<Wave> MapEditorTempWaves; //-------------------------------------------------täällä   !
        public List<SpawnGroup> MapEditorTempGroups;

        public static Tower[] ExampleTowers;
        public List<Tower> InitTowers;
        public byte[] AvailableTowers;
        public short PlayerInitLife;
        public int PlayerInitEnergy;
        public int[] PlayerInitGenePoints;
        public Player[] Players;
        public List<Creature> AliveCreatures;
        public List<FloatingParticle> FloatingParticles;
        //public List<Tower> Towers;

        //public int[] SpawnTimes;
        public bool initSetupOn;

        public uint mapTimer;
        public int currentWave;
        //public int waveTimer;

        //public Texture2D[] tileTextures;
        public Texture2D[] wallTextures;
        public Texture2D pathTexture;
        

        public Vector2 tileTexCenter;

        public SoundEffect sound;
        public SoundEffectInstance soundInstance;
        
        public Cue towerCue;
        public Cue creatureCue;

		public Vector3[] cubeCoords;

        public HexMap(CurrentGame game, string name, char[,] initLayout, Point[] spawnPoints, Point[] goalPoints, /*int[] goalPointTimetable,*/ Player[] players)
        {
            ParentGame = game;
            Name = name;
			CurrentGame.currentMap = this;
            //tileTextures = new Texture2D[]{ ParentGame.Content.Load<Texture2D>("Tiles\\patternhex-66-57BOU2"/*"hex-66-57-04e"*/), //-----0: Seinä1
            //                                ParentGame.Content.Load<Texture2D>("Tiles\\polkuhex-66-57borderless"/*"hex-66-57-00"*/), //-----1: Polku
            /*                                ParentGame.Content.Load<Texture2D>("Tiles\\hex-66-57-04c"), //----2: Seinä2
                                            ParentGame.Content.Load<Texture2D>("Tiles\\hex-66-57-04purpl"),
                                            ParentGame.Content.Load<Texture2D>("Tiles\\hex-66-57-overlay"),
                                            ParentGame.Content.Load<Texture2D>("Tilering\\tileringGlow"),
                                            ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1"),
                                            ParentGame.Content.Load<Texture2D>("pixel"),
                                            ParentGame.Content.Load<Texture2D>("Tilering\\ringpartFill2"),
                                            ParentGame.Content.Load<Texture2D>("Tilering\\tileringPohj")};*/

            wallTextures = new Texture2D[]{ ParentGame.Content.Load<Texture2D>("Tiles\\hex-66-57-02"),//patternhex-66-57BOU2"/*"hex-66-57-04e"*/),
                                            ParentGame.Content.Load<Texture2D>("Tiles\\hex-66-57-04c"), //----1: Seinä2
                                            ParentGame.Content.Load<Texture2D>("Tiles\\hex-66-57-outline")}; //------grid
            pathTexture = ParentGame.Content.Load<Texture2D>("Tiles\\hex-66-57-00");//polkuhex-66-57borderless"/*"hex-66-57-00"*/);

            tileTexCenter = new Vector2((float)Math.Round(wallTextures[0].Width / 2f), (float)Math.Round(wallTextures[0].Height / 2f));

            TileWidth = wallTextures[0].Width;
            TileHalfWidth = TileWidth/2;
            stackedWidth = (int)(TileWidth * 0.75); //OLD:  (int)Math.Round(TileWidth * 0.75) -1; //-----MINUS for texture tesselation!!
            TileHeight = wallTextures[0].Height % 2 == 1 ? wallTextures[0].Height - 1 : wallTextures[0].Height; //-----MINUS for ODD-dimensioned texture tesselation!!
                                                    //TileWidth * (float)Math.Sqrt(3) / 2 or width * Math.Sin(60 * Math.PI/180)
            TileHalfHeight = TileHeight/2;

            //wallNorm = Vector2.Subtract(ToScreenLocation(1, 1), ToScreenLocation(0, 0));
            //wallNorm.Normalize();
            //wallPerpNorm = new Vector2(-wallNorm.X, wallNorm.Y);

            rnd = new Random();
            InitLayout = initLayout;
            Layout = new char[initLayout.GetLength(0), initLayout.GetLength(1)];            
            Array.Copy(InitLayout, Layout, InitLayout.Length);
            
            drawPos = new Point((int)(ParentGame.GraphicsDevice.Viewport.Width/2 - Layout.GetUpperBound(1)*stackedWidth /2),
                                (int)(ParentGame.GraphicsDevice.Viewport.Height/2 - Layout.GetUpperBound(0)*TileHeight /2));
            SpawnPoints = spawnPoints;
            GoalPoints = goalPoints;
            //GoalPointTimetable = goalPointTimetable;
            /*for (int i = 0; i < spawnPoints.Length; i++)
                InitLayout[spawnPoints[i].Y, spawnPoints[i].X] = 4;
            for (int i = 0; i < goalPoints.Length; i++)
                InitLayout[goalPoints[i].Y, goalPoints[i].X] = 5;*/

            Pathfinder = new Pathfinder(this);

            AliveCreatures = new List<Creature>(); //before: max 20 (why?)
            FloatingParticles = new List<FloatingParticle>();
            currentWave = -1;
            //ExampleTowers = new Tower[6] { new Tower("0", this, ParentGame.HUD.activeTileCoord, 75, 55, new Texture2D[] { game.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1") }, (float)Math.PI * 0.5f, CurrentGame.pixel, 12f, 1, 0, new int[] { 0, 0 }, Element.None, 10, 200),
            //                               new Tower("1", this, ParentGame.HUD.activeTileCoord, 110, 20, new Texture2D[] { game.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1") }, (float)Math.PI * 0.5f, CurrentGame.pixel, 12f, 25, 0, new int[] { 0, 0 }, Element.None, 100, 200),
            //                               new Tower("2", this, ParentGame.HUD.activeTileCoord, 80, 20, new Texture2D[] { game.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1") }, (float)Math.PI * 0.5f, CurrentGame.pixel, 12f, 25, 0, new int[] { 0, 0 }, Element.None, 100, 200),
            //                               new Tower("3", this, ParentGame.HUD.activeTileCoord, 90, 20, new Texture2D[] { game.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1") }, (float)Math.PI * 0.5f, CurrentGame.pixel, 12f, 25, 0, new int[] { 0, 0 }, Element.None, 100, 200),
            //                               new Tower("4", this, ParentGame.HUD.activeTileCoord, 120, 20, new Texture2D[] { game.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1") }, (float)Math.PI * 0.5f, CurrentGame.pixel, 12f, 25, 0, new int[] { 0, 0 }, Element.None, 100, 200),
            //                               new Tower("5", this, ParentGame.HUD.activeTileCoord, 150, 20, new Texture2D[] { game.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1") }, (float)Math.PI * 0.5f, CurrentGame.pixel, 12f, 25, 0, new int[] { 0, 0 }, Element.None, 100, 200)};
            //new Tower(ParentGame, "2", this, ParentGame.HUD.activeTileCoord, 100, 20, tileTextures[6], (float)Math.PI * 0.5f, tileTextures[7], null, 12f, 25, 0, new int[] { 0, 0 }, 0, 0, 0, 100, 200),
            ExampleTowers = new Tower[18] {new Tower('A', "Pruiter 1", Point.Zero, 75, 55, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1") }, new GeneSpecs(), CurrentGame.ball, 12f, 1, 0, 0, new float[] {0,0}, 10, 200, true),
                                           new Tower('E', "Splasher 1", Point.Zero, 110, 50, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN1") }, new GeneSpecs(), CurrentGame.ball, 2f, 2, DmgType.Splash, 15, new float[] {0,0}, 20, 200, true),
                                           new SniperTower(Point.Zero, UpgLvl.Basic, true),
                                           //new Tower('I', "Sniper 1", Point.Zero, 200, 100, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\solubug") }, new GeneSpecs(), CurrentGame.ball, 12f, 5, 0, 0, new float[] {0,0}, 30, 200, true),
                                           new Tower('O', "Slower 1", Point.Zero, 90, 75, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri3") }, new GeneSpecs(), CurrentGame.ball, 8f, 0, 0, 0, new float[] {0.2f,200}, 10, 200, true),
                                           new ParticleEaterTower(Point.Zero, UpgLvl.Basic, true),
                                           //new Tower('U', "Grabber 1", Point.Zero, 120, 20, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri4") }, new GeneSpecs(), CurrentGame.ball, 12f, 1, 0, 0, new float[] {0,0}, 50, 200, true),
                                           new Tower('|', "Booster 1", Point.Zero, 350, 60, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57") }, new GeneSpecs(), CurrentGame.ball, 8f, 1, 0, 0, new float[] {0,0}, 15, 200, true),

                                           new Tower('Ä', "Pruiter 2", Point.Zero, 90, 55, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri6") }, new GeneSpecs(), CurrentGame.ball, 12f, 2, 0, 0, new float[] {0,0}, 8, 300, true),
                                           new Tower('Ë', "Splasher 2", Point.Zero, 250, 50, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN2") }, new GeneSpecs(), CurrentGame.ball, 2f, 3, DmgType.Splash, 25, new float[] {0,0}, 20, 200, true),
                                           new SniperTower(Point.Zero, UpgLvl.Advanced, true),
                                           //new Tower('Ï', "Sniper 2", Point.Zero, 225, 100, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\solubug") }, new GeneSpecs(), CurrentGame.ball, 12f, 15, 0, 0, new float[] {0,0}, 30, 200, true),
                                           new Tower('Ö', "Slower 2", Point.Zero, 90, 75, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri3") }, new GeneSpecs(), CurrentGame.ball, 8f, 0, 0, 0, new float[] {0.4f,200}, 10, 200, true),
                                           new ParticleEaterTower(Point.Zero, UpgLvl.Advanced, true),
                                           //new Tower('Ü', "Grabber 2", Point.Zero, 125, 20, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri4") }, new GeneSpecs(), CurrentGame.ball, 12f, 1, 0, 0, new float[] {0,0}, 50, 200, true),
                                           new Tower('†', "Booster 2", Point.Zero, 155, 20, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57") }, new GeneSpecs(), CurrentGame.ball, 12f, 1, 0, 0, new float[] {0,0}, 15, 200, true),

                                           new Tower('Â', "Pruiter 3", Point.Zero, 125, 70, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri7") }, new GeneSpecs(), CurrentGame.ball, 18f, 3, 0, 0, new float[] {0,0}, 10, 200, true),
                                           new Tower('Ê', "Splasher 3", Point.Zero, 120, 50, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN4") }, new GeneSpecs(), CurrentGame.ball, 12f, 4, DmgType.Splash, 360, new float[] {0,0}, 20, 200, true),
                                           new SniperTower(Point.Zero, UpgLvl.Max, true),
                                           //new Tower('Î', "Sniper 3", Point.Zero, 250, 100, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\solubug") }, new GeneSpecs(), CurrentGame.ball, 12f, 25, 0, 0, new float[] {0,0}, 30, 200, true),
                                           new Tower('Ô', "Slower 3", Point.Zero, 90, 75, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri3") }, new GeneSpecs(), CurrentGame.ball, 8f, 0, 0, 0, new float[] {0.6f,200}, 10, 200, true),
                                           new ParticleEaterTower(Point.Zero, UpgLvl.Max, true),
                                           //new Tower('Û', "Grabber 3", Point.Zero, 130, 20, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri4") }, new GeneSpecs(), CurrentGame.ball, 12f, 1, 0, 0, new float[] {0,0}, 50, 200, true),
                                           new Tower('‡', "Booster 3", Point.Zero, 160, 20, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57") }, new GeneSpecs(), CurrentGame.ball, 12f, 1, 0, 0, new float[] {0,0}, 15, 200, true)};

            Players = players;

            AvailableTowers = new byte[6] { 3, 3, 3, 3, 3, 3 };

			cubeCoords = new Vector3[Layout.GetLength(0) * Layout.GetLength(1)];
			int i = 0;
			for (int row = 0; row < Layout.GetLength(0); row++)
			{	for (int col = 0; col < Layout.GetLength(1); col++)
				{
					cubeCoords[i].X = col;
					cubeCoords[i].Z = row - (col - (col & 1)) / 2;
					cubeCoords[i].Y = -cubeCoords[i].X - cubeCoords[i].Z;
					i++;
				}
			}
        }

        /*//Old Setpath
        /// <summary>
        /// Initializes a collection of waypoints (Vector2-array) from a table of x and y tile value pairs
        /// </summary>
        /// <param name="indexes">[x-value in tiles, y-value in tiles]</param>
        public void SetPath(float[,] indexes)
        {
            Path = new Vector2[(indexes.GetUpperBound(0) +1)];
            for (int i = 0; i <= indexes.GetUpperBound(0); i++)
              {                
                float xPos = indexes[i,0] * stackedWidth;
                float yPos = indexes[i,1] * TileHeight + ((float)Math.Round(indexes[i,0]) % 2 * TileHeight / 2);
                Path[i] = new Vector2(xPos + drawPos.X, yPos + drawPos.Y);
              }
        }*/

        /// <summary> Converts map coordinates into a pixel-domain Vector2 </summary>
        /// <param name="x">[column]</param> <param name="y">[row]</param>
        public  Vector2 ToScreenLocation(int x, int y)
        {
            int oddColumnSwitch = Convert.ToInt32(x % 2 != 0);
            return new Vector2(x * stackedWidth + drawPos.X, y * TileHeight + (TileHalfHeight * oddColumnSwitch) + drawPos.Y); //tilehalfheight paha?
        }

        /// <summary> Converts a map coordinate Point into a pixel-domain Vector2 </summary>
        public Vector2 ToScreenLocation(Point mapCoord)
        {            
            return ToScreenLocation(mapCoord.X, mapCoord.Y);
        }
        
        public Point ToMapCoordinate(float x, float y)
        {
            float colF = (x - drawPos.X + TileWidth/4) / (float)stackedWidth; //get column by dividing x by stackedwidth. Zero is at 1/4 width (tile up.left corner).
            int col = colF < 0 ? (int)colF -1 : (int)colF;                    //if below zero, round down so that first column left of map is already -1

            bool oddColumn = col % 2 != 0;
                                                                              //get row by dividing y by tileheight substract 0.5 if on an odd column
            float rowF = (y - drawPos.Y + TileHeight/2) / (float)TileHeight - Convert.ToInt32(oddColumn) * 0.5f;
            int row = rowF < 0 ? (int)rowF -1 : (int)rowF;
            
            colF = (colF % 1 +1) % 1;  //wrap floats into 0-1 (position inside each tile) --
            rowF = (rowF % 1 +1) % 1;  //-- so that -0.2 = 0.8

            // Get comparable x-y values (0-1) of the problem zone (rightmost quarter of tile)
            float disputedX = (colF - 0.666667f) / 0.333333f;  //horizontal center half of tile (stackedwidth's 2/3) to range -2-0, rightmost quarter of tile to 0-1
            float disputedY = rowF * 2 % 1;                    //split height to two 0-1's (upper & lower half)

            if (colF > 0.666667f) //---if horizontally at rightmost quarter of tile (DISPUTED ZONE)
            {
                if (rowF < 0.5) //---UPPER HALF OF TILE
                {
                    if (disputedX > disputedY)
                    {
                        col++;
                        if (!oddColumn) row--;
                    }
                }
                else //-------------LOWER HALF OF TILE
                {
                    if (disputedX > 1 - disputedY)
                    {
                        col++;
                        if (oddColumn) row++;
                    }
                }
            }
            return new Point(col, row);
        }

        public Point ToMapCoordinate(Vector2 position)
        {
            return ToMapCoordinate(position.X, position.Y);
        }

        #region OLD ToMapCoord
        /* //VANHA BUGGA
            float mouseColF = (mouse.X - drawPos.X + TileWidth / 4) / (float)stackedWidth;                        //sp.DrawString(font, mouseColF.ToString(), new Vector2(mouse.X + 90, mouse.Y + 20), Color.Wheat);
            int mouseCol = mouseColF < 0 ? (int)mouseColF -1 : (int)mouseColF;                                    //sp.DrawString(font, "old " + mouseCol.ToString(), new Vector2(mouse.X, mouse.Y - 20), Color.Wheat);
            bool oddColumn = mouseCol % 2 != 0;                                                                   //sp.DrawString(font, oddColumn.ToString(), new Vector2(mouse.X + 100, mouse.Y + 120), Color.Wheat);
            float mouseRowF = (mouse.Y - drawPos.Y + TileHeight / 2) / (float)TileHeight - Convert.ToInt32(oddColumn) * 0.5f;   //sp.DrawString(font, mouseRowF.ToString(), new Vector2(mouse.X +90, mouse.Y + 40), Color.Wheat); 
            int mouseRow = mouseRowF < 0 ? (int)mouseRowF - 1 : (int)mouseRowF;                                   //sp.DrawString(font, mouseRow.ToString(), new Vector2(mouse.X + 50, mouse.Y - 20), Color.Wheat);

            
            mouseColF = (mouseColF % 1 + 1) % 1;                            //sp.DrawString(font, "ColF: " + Math.Round(mouseColF,1).ToString(), new Vector2(mouse.X, mouse.Y + 20), Color.Wheat);
            mouseRowF = (mouseRowF % 1 + 1) % 1;                            //sp.DrawString(font, "Row: " + Math.Round(mouseRowF,1).ToString(), new Vector2(mouse.X, mouse.Y + 40), Color.Wheat);
            float disputedX = (mouseColF - (2 / 3f)) / (1 / 3f);            
            float disputedY = mouseRowF * 2 % 1;                            //sp.DrawString(font, Math.Round(disputedX,1).ToString() + Environment.NewLine + Math.Round(disputedY,1).ToString(), new Vector2(mouse.X, mouse.Y + 80), Color.Wheat);

            if (mouseColF > 2 / 3f) //---DISPUTED ZONE
            {
                if (mouseRowF < 0.5) //---UPPER HALF OF TILE
                {
                    if (disputedX > disputedY)
                    {
                        mouseCol++;
                        if (!oddColumn) mouseRow--;
                    }
                    //sp.DrawString(font, "UPDISP", new Vector2(mouse.X+120, mouse.Y+100),Color.Yellow);
                }
                else //------------------LOWER HALF OF TILE
                {
                    if (disputedX > 1 - disputedY)
                    {
                        mouseCol++;
                        if (oddColumn) mouseRow++;
                    }
                    //sp.DrawString(font, "DOWNDISP", new Vector2(mouse.X + 120, mouse.Y + 100), Color.Yellow);
                }
            }
            sp.DrawString(font, mouseCol.ToString(), new Vector2(mouse.X + 10, mouse.Y), Color.Wheat);
            sp.DrawString(font, mouseRow.ToString(), new Vector2(mouse.X + 30, mouse.Y), Color.Wheat);*/
        #endregion

        public void BuildTower(Tower t)
        {
            Layout[t.MapCoord.Y, t.MapCoord.X] = t.Symbol;
            Players[0].EnergyPoints -= t.Cost;
            Players[0].Towers.Add(t);
            //Players[0].Towers.Insert(Players[0].Towers.Count, t);  //---- joku vanha numerotorni-idea?
        } //----käytetään kerran ja on lyhyt, oks järkee?

        //public void AddTower(Tower t) //--------oston ja lisäyksen erottelua ei sit vissiin tarvitukaan
        //{
        //    Layout[t.mapCoord.Y, t.mapCoord.X] = t.Symbol;
        //    Players[0].Towers.Insert(Players[0].Towers.Count, t);
        //}
        
        public void ResetMap()
        {
            Array.Copy(InitLayout, Layout, InitLayout.Length);
            /*for (int y = 0; y <= Layout.GetUpperBound(0); y++)
            { for (int x = 0; x <= Layout.GetUpperBound(1); x++)
              {
                  if (Layout[y, x] != 1) continue;

                  int RND = rnd.Next(0, 100);
                  if (RND < 50)
                    Layout[y, x] = 1;
                  else Layout[y, x] = 2;
              }
            }*/
            mapTimer = 0;
            currentWave = -1;
            //waveTimer = 0;
            initSetupOn = true;
            AliveCreatures.Clear();
            FloatingParticles.Clear();

            Players[0].Alive = true;
            Players[0].LifePoints = PlayerInitLife;
            Players[0].EnergyPoints = PlayerInitEnergy;
            PlayerInitGenePoints.CopyTo(Players[0].GenePoints, 0);
			CurrentGame.HUD.UpdateGeneBars();
			Players[0].Towers.Clear();
			if (InitTowers != null)
			{
				for (int i = 0; i < InitTowers.Count; i++)
				{
					Players[0].Towers.Add(Tower.Clone(InitTowers[i]));
					Players[0].Towers[i].MapCoord = InitTowers[i].MapCoord;
					Players[0].Towers[i].buildFinishedCounter = 0; //----------------------------paska
					Players[0].Towers[i].buildTimer = 0; //--------------------------------------paska
					Players[0].Towers[i].Built = true; //----------------------------------------paska

				}
			}
            
            CurrentGame.HUD.HUDbuttons[0].Text = "Start the waves";

            for (int w = 0; w < Waves.Length; w++)
            {   for (int g = 0; g < Waves[w].Groups.Length; g++)
                {   for (int c = 0; c < Waves[w].Groups[g].Creatures.Length; c++)
                    {
                        Waves[w].Groups[g].Creatures[c].Born = false;
                        Waves[w].Groups[g].Creatures[c].Alive = true;
                        Waves[w].Groups[g].Creatures[c].Location = ToScreenLocation(SpawnPoints[Waves[w].Groups[g].Creatures[c].SpawnPointIndex]);
                        Waves[w].Groups[g].Creatures[c].nextWaypoint = 1;
                        Waves[w].Groups[g].Creatures[c].hp = Waves[w].Groups[g].Creatures[c].InitHp;
                        Waves[w].Groups[g].Creatures[c].Speed = Waves[w].Groups[g].Creatures[c].defSpeed;
                        Waves[w].Groups[g].Creatures[c].Splatter.Reset();
                    }
                    Waves[w].Groups[g].FindPath();
                    Waves[w].Groups[g].spawnTimer = 0;
                }
            }
			HUD.NexWaveInfoBoxes = new List<GroupInfoBox>();
			HUD.CurrWaveInfoBoxes = new List<GroupInfoBox>();
			HUD.BugBoxes = new List<BugInfoBox>();
			HUD.UpdateWaveInfo();
        }

        public void SaveMap(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                for (int y = 0; y < Layout.GetLength(0); y++)
                {   for (int x = 0; x < Layout.GetLength(1); x++)
                    {
                        writer.Write(Layout[y, x]);
                    }
                    writer.WriteLine();
                }

                writer.WriteLine();
                writer.Write("Available towers:");
                for (int i = 0; i < AvailableTowers.Length; i++)
                {
					writer.Write(" " + CurrentGame.HUD.AvailableTowersCells[i].Text);
                }

                writer.WriteLine();
				writer.WriteLine("Life:".PadRight(8) + CurrentGame.HUD.MapEditorResourceCells[4].Text);
				writer.WriteLine("Energy:".PadRight(8) + CurrentGame.HUD.MapEditorResourceCells[0].Text);
				writer.WriteLine("Genes:".PadRight(8) + CurrentGame.HUD.MapEditorResourceCells[1].Text + ", " + CurrentGame.HUD.MapEditorResourceCells[2].Text + ", " + CurrentGame.HUD.MapEditorResourceCells[3].Text);

                writer.WriteLine();
                writer.WriteLine("\tCreamt\tType\tName\t\tTexture\t\tSpawnP\tGoals\tHP\tSPD\tLifeDmg\tEnergy\tRRes\tGRes\tBRes\tSpawnD\tDuration");

                int tableRow = 0;
                for (int w = 0; w < MapEditorTempWaves.Count; w++)
                {
                    writer.WriteLine("Wave " + (w + 1) + "------------------------------------------------------------------------------------------------------------------------------------------");
                    for (int g = 0; g < MapEditorTempWaves[w].TempGroups.Count; g++)
                    {
						for (int c = tableRow * (CurrentGame.HUD.MapEditorTableCols - 1); c < tableRow * (CurrentGame.HUD.MapEditorTableCols - 1) + (CurrentGame.HUD.MapEditorTableCols - 1); c++)
                        {
							if ((c % (CurrentGame.HUD.MapEditorTableCols - 1) == 2 || c % (CurrentGame.HUD.MapEditorTableCols - 1) == 3) && CurrentGame.HUD.MapEditorTableCells[c].Text.Length < 8)
								writer.Write('\t' + CurrentGame.HUD.MapEditorTableCells[c].Text + '\t');
							else writer.Write('\t' + CurrentGame.HUD.MapEditorTableCells[c].Text);
                        }
                        tableRow++;
                        writer.WriteLine();
                    }
                }
            }
			CurrentGame.HUD.MapEditorMenuButtons[2].PopulateDropDownMenu(Array.ConvertAll<string, string>(Directory.GetFiles(CurrentGame.MapDir), System.IO.Path.GetFileNameWithoutExtension));
        }

        public void Update()
        {
            //if (Players[0].Towers.Count > 0) Players[0].Towers[Players[0].Towers.Count - 1].Range = Math.Max(mouse.ScrollWheelValue / 200, 100); //-----------------SCROLLRANGE-----------------

			if (AliveCreatures.Count == 0) //&& Waves[Waves.GetUpperBound(0)].Groups[0].spawnTimer >= Waves[Waves.GetUpperBound(0)].Duration + 100) // if no alive creatures and last wave's first group's spawntimer is beyond last wave's duration + arbitrary some
            {
				Wave lastWave = Waves[Waves.GetUpperBound(0)];
				SpawnGroup lastGroup = lastWave.Groups[lastWave.Groups.GetUpperBound(0)];
				if (lastGroup.IsWholeGroupBorn)
					CurrentGame.gameState = GameState.GameOver;
                /*if (CurrentGame.gameState == GameState.InGame)
                {
                    ParentGame.mainMenu.SavePlayerData(); //------------neeeds planning
                    ParentGame.gameState = Game1.GameState.LevelComplete;  //----------------------TODO: LEVEL COMPLETION STATE-------------------------------!!
                }*/
                //else
                   // CurrentGame.gameState = GameState.GameOver;
            }
            if (!initSetupOn)
            {
                /*for (int o = 1; o < GoalPointTimetable.Length; o++)
                {
                    if (mapTimer == GoalPointTimetable[o]) //----------------------------------------------------------------------------------------HUOM maptimer ei huomioi wave-aikaistuksia
                    {
                        foreach (Creature c in AliveCreatures)
                        {
                            //if (c.GoalPointIndex.Length > 1)
                                c.FindPath();
                        }
                        creatureCue = ParentGame.soundBank.GetCue("plurputus1");
                        creatureCue.Play();
                    }
                }*/

                //---------------------------------------------------------------eikös tänkin vois laittaa on-demand, öröjen vastuulle
				bool PKeyDown = CurrentGame.keyboard.IsKeyDown(Keys.P);
                for (int w = 0; w < Waves.Length; w++)
                {   for (int g = 0; g < Waves[w].Groups.Length; g++)
                    {
                        Waves[w].Groups[g].ShowingPath = PKeyDown;

                        if (Waves[w].Groups[g].BugBox.locked)
                            Waves[w].Groups[g].ShowingPath = true;
                        

                        //             for (int c = 0; c < Waves[w].Groups[g].Creatures.Length; c++)
                        //                 {
                        //Waves[w].Groups[g].Creatures[c].ShowingPath = PKeyDown;
                        //                 }
                    }
                    if (w <= currentWave)
						Waves[w].Update();
                }
                if (Waves[currentWave].Groups[0].spawnTimer == Waves[currentWave].Duration -1 && currentWave != Waves.GetUpperBound(0)) //------------------------------------------hmmmm
                {
                    currentWave++;
					HUD.UpdateWaveInfo();
                }

                for (int i = 0; i < FloatingParticles.Count; i++)
                {
                    FloatingParticles[i].Update();
                }

                mapTimer++;
            }
			else //initSetupOn
			{
                bool isNoneLocked = true;
				for (int w = 0; w < Waves.Length; w++)
				{   for (int g = 0; g < Waves[w].Groups.Length; g++)
					{
                        if (Waves[w].Groups[g].BugBox.locked)
                        {
                            foreach (SpawnGroup sg in Waves[w].Groups)
                            {
                                if (!sg.BugBox.locked)
                                    sg.ShowingPath = false;
                            }
                            Waves[w].Groups[g].ShowingPath = true;
                            isNoneLocked = false;
                            continue;
                        }

                        if (isNoneLocked)
                            Waves[w].Groups[g].ShowingPath = true;
                        else
                            Waves[w].Groups[g].ShowingPath = false;
                        //                  for (int c = 0; c < Waves[w].Groups[g].Creatures.Length; c++)
                        //{
                        //	Waves[w].Groups[g].Creatures[c].ShowingPath = true;
                        //}
                    }
                }
			}

            for (int i = 0; i < Players[0].Towers.Count; i++)
            {
                Players[0].Towers[i].Update(AliveCreatures);
                //Type chekType = Players[0].Towers[i].GetType();
            }
        }

        public void Draw(SpriteBatch sb)
        {
            for (int y = 0; y < Layout.GetLength(0); y++)
            { for (int x = 0; x < Layout.GetLength(1); x++)
              {
                Vector2 screenPos = ToScreenLocation(x, y);
                switch (Layout[y, x])
                {
                    case ' ': if (CurrentGame.gameState == GameState.MapEditor) sb.Draw(wallTextures[2], screenPos, null, Color.White * 0.1f, 0, tileTexCenter, 1, SpriteEffects.None, 1); break; //3=VOID
                    case '0': sb.Draw(wallTextures[0], screenPos, null, Color.Cyan /*Color.DarkCyan*/ /*new Color(240, 240, 240)*/, 0, tileTexCenter, 1, SpriteEffects.None, 1); break; //0=OPEN WALL TILES (light)
					case '.': if ((y > 0 && y < Layout.GetUpperBound(0)) && (x > 0 && x < Layout.GetUpperBound(1)) &&
                              Layout[y-1, x] != '0' && Layout[y, x+1] != '0' && Layout[y+1, x+1] != '0' && Layout[y+1, x] != '0' && Layout[y+1, x-1] != '0' && Layout[y, x-1] != '0')
                                sb.Draw(pathTexture, screenPos, null, Color.DarkSlateBlue/*new Color(150,200,175)*/, 0, tileTexCenter, 1, SpriteEffects.None, 1); //1=PATH IN THE OPEN
                            else
                                sb.Draw(pathTexture, screenPos, null, Color.SlateBlue/*new Color(150,200,175)*/, 0, tileTexCenter, 1, SpriteEffects.None, 1); break; //1=PATH CLOSE TO WALL
                    case '\'': if ((y>0 && y<Layout.GetUpperBound(0)) && (x > 0 && x < Layout.GetUpperBound(1)) &&
                                Layout[y-1,x] != '0' && Layout[y-1, x+1] != '0' && Layout[y, x+1] != '0' && Layout[y+1, x] != '0' && Layout[y, x-1] != '0' && Layout[y-1, x-1] != '0') 
                                sb.Draw(pathTexture, screenPos, null, Color.DarkSlateBlue/*new Color(150,200,175)*/, 0, tileTexCenter, 1, SpriteEffects.None, 1); //1=PATH IN THE OPEN
                            else
                                sb.Draw(pathTexture, screenPos, null, Color.SlateBlue/*new Color(150,200,175)*/, 0, tileTexCenter, 1, SpriteEffects.None, 1); break; //1=PATH CLOSE TO WALL
                        case '1': case '2': case '3': case '4': case '5': case '6': case '7': case '8': //4=SPAWNPOINTS 
					case '9': sb.Draw(CurrentGame.HUD.tileOverlay, screenPos, null, new Color(62, 51, 129), 0, tileTexCenter, 1, SpriteEffects.None, 1); break; //4=SPAWNPOINTS
                    case 'a': case 'b': case 'c': case 'd': case 'e': case 'f': case 'g': case 'h': //5=GOALPOINTS
					case 'i': sb.Draw(CurrentGame.HUD.tileOverlay, screenPos, null, Color.Salmon * 0.7f, 0, tileTexCenter, 1, SpriteEffects.None, 0); break; //5=GOALPOINTS
                    default: sb.Draw(wallTextures[0], screenPos, null, Color.Cyan, 0, tileTexCenter, 1, SpriteEffects.None, 1); break; //0=OPEN WALL TILES (light)
                    }
				//sb.DrawString(CurrentGame.font, x + "," + y, screenPos + new Vector2(-20,0), Color.White *0.2f); //---COOOOOOOOORDS
				//sb.DrawString(CurrentGame.font, cubeCoords[k].X.ToString() + "," + cubeCoords[k].Y.ToString() + "," + cubeCoords[k].Z.ToString(), screenPos - new Vector2(25,5), Color.White * 0.2f);
              }
            }

            //------PATH TILE HOVER (DRAW UNDER CREATURES) ------------not cool
			if (CurrentGame.HUD.hoveredCoord.X >= 0 && CurrentGame.HUD.hoveredCoord.X < Layout.GetLength(1) && CurrentGame.HUD.hoveredCoord.Y >= 0 && CurrentGame.HUD.hoveredCoord.Y < Layout.GetLength(0))
            {
				if (Layout[CurrentGame.HUD.hoveredCoord.Y, CurrentGame.HUD.hoveredCoord.X] == 3 && !(CurrentGame.HUD.newTileRingActive || CurrentGame.HUD.towerTileRingActive)) 
                {
                    if (CurrentGame.mouse.LeftButton == ButtonState.Pressed)
						sb.Draw(pathTexture, CurrentGame.HUD.overlayPos, null, Color.RosyBrown, 0, tileTexCenter, 1, SpriteEffects.None, 0);
					else sb.Draw(pathTexture, CurrentGame.HUD.overlayPos, null, Color.RosyBrown * 0.5f * (1 - (CurrentGame.HUD.tileHoverFade / HUD.hoverFadeCycles)), 0, tileTexCenter, 1, SpriteEffects.None, 0);
                }
            }

            #region AWFUL TILE BORDER ANIMATION
            //Draw spawntimer lines around spawnpoints
            if (Waves != null && currentWave >= 0 && currentWave < Waves.GetUpperBound(0))
            {
                float wavePhase = Waves[currentWave].Groups[0].spawnTimer / (float)Waves[currentWave].Duration;
                if (Waves[currentWave].Groups[0].spawnTimer < Waves[currentWave].Duration)
                {
                    Color lineColor = Color.Yellow;
                    Color borderColor = Color.Black * 0.6f;
                    if (wavePhase > 0.84f) lineColor = Color.White;
                    //lineColor *= buildFinishedCounter / (float)buildFinishedInit;
                    //List<Vector2> spawnTiles = new List<Vector2>();
                    for (int i = 0; i < SpawnPoints.Length; i++)
                    {
                        //if (Waves[currentWave + 1].Groups[0].SpawnPointIndex == i)
                        for (int g = 0; g < Waves[currentWave +1].Groups.Length; g++)
                        {
                            if (Waves[currentWave + 1].Groups[g].SpawnPointIndex == i)
                            {
                                //Vector2 spawnTilePos = ToScreenLocation(SpawnPoints[Waves[currentWave + 1].Groups[0].SpawnPointIndex]);
                                Vector2 spawnTilePos = ToScreenLocation(SpawnPoints[i]);

                                sb.Draw(CurrentGame.pixel, new Rectangle((int)(spawnTilePos.X - TileWidth / 4 - 1), (int)spawnTilePos.Y - TileHeight / 2 - 1, TileWidth / 2, 4),
                                        null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, 0);
                                sb.Draw(CurrentGame.pixel, new Rectangle((int)(spawnTilePos.X + TileWidth / 4 + 2), (int)spawnTilePos.Y - TileHeight / 2, TileWidth / 2, 4),
                                        null, borderColor, MathHelper.ToRadians(60.9f), Vector2.Zero, SpriteEffects.None, 0);
                                sb.Draw(CurrentGame.pixel, new Rectangle((int)(spawnTilePos.X + TileWidth / 2 + 1), (int)spawnTilePos.Y, TileWidth / 2, 4),
                                        null, borderColor, MathHelper.ToRadians(118.2f), Vector2.Zero, SpriteEffects.None, 0);
                                sb.Draw(CurrentGame.pixel, new Rectangle((int)(spawnTilePos.X + TileWidth / 4 + 1), (int)spawnTilePos.Y + TileHeight / 2 + 2, TileWidth / 2 + 1, 4),
                                        null, borderColor, (float)Math.PI, Vector2.Zero, SpriteEffects.None, 0);
                                sb.Draw(CurrentGame.pixel, new Rectangle((int)(spawnTilePos.X - TileWidth / 4 - 2), (int)spawnTilePos.Y + TileHeight / 2 + 2, TileWidth / 2 + 1, 4),
                                        null, borderColor, MathHelper.ToRadians(240.8f), Vector2.Zero, SpriteEffects.None, 0);
                                sb.Draw(CurrentGame.pixel, new Rectangle((int)(spawnTilePos.X - TileWidth / 2 - 1), (int)spawnTilePos.Y, TileWidth / 2, 4),
                                        null, borderColor, (float)Math.PI * (5 / 3f), Vector2.Zero, SpriteEffects.None, 0);

                                sb.Draw(CurrentGame.pixel, new Rectangle((int)(spawnTilePos.X - TileWidth / 4 - 1), (int)(spawnTilePos.Y - TileHeight / 2),
                                        (int)Math.Min(TileWidth / 2, TileWidth / 2 * wavePhase * 6), 2), null, lineColor, 0f, Vector2.Zero, SpriteEffects.None, 0);
                                if (wavePhase >= 1 / 6f)
                                    sb.Draw(CurrentGame.pixel, new Vector2(spawnTilePos.X + TileWidth / 4 + 1, spawnTilePos.Y - TileHeight / 2), null, lineColor, MathHelper.ToRadians(60.9f),
                                            Vector2.Zero, new Vector2(Math.Min(TileWidth / 2, TileWidth / 2 * (wavePhase - 1 / 6f) * 6), 2), SpriteEffects.None, 0);
                                if (wavePhase >= 2 / 6f)
                                    sb.Draw(CurrentGame.pixel, new Vector2(spawnTilePos.X + TileWidth / 2, spawnTilePos.Y), null, lineColor, MathHelper.ToRadians(118.2f) /*(float)Math.PI * (1.98f / 3f)*/, //118.95f
                                            Vector2.Zero, new Vector2(Math.Min(TileWidth / 2, TileWidth / 2 * (wavePhase - 2 / 6f) * 6), 2), SpriteEffects.None, 0);
                                if (wavePhase >= 3 / 6f)
                                    sb.Draw(CurrentGame.pixel, new Vector2(spawnTilePos.X + TileWidth / 4, spawnTilePos.Y + TileHeight / 2 + 1), null, lineColor, (float)Math.PI,
                                            Vector2.Zero, new Vector2(Math.Min(TileWidth / 2, TileWidth / 2 * (wavePhase - 3 / 6f) * 6), 2), SpriteEffects.None, 0);
                                if (wavePhase >= 4 / 6f)
                                    sb.Draw(CurrentGame.pixel, new Vector2(spawnTilePos.X - TileWidth / 4 - 1, spawnTilePos.Y + TileHeight / 2 + 1), null, lineColor, MathHelper.ToRadians(240.8f),//(float)Math.PI * (4f / 3f)
                                            Vector2.Zero, new Vector2(Math.Min(TileWidth / 2, TileWidth / 2 * (wavePhase - 4 / 6f) * 6), 2), SpriteEffects.None, 0);
                                if (wavePhase >= 5 / 6f)
                                    sb.Draw(CurrentGame.pixel, new Vector2(spawnTilePos.X - TileWidth / 2, spawnTilePos.Y), null, lineColor, (float)Math.PI * (5 / 3f),
                                            Vector2.Zero, new Vector2(Math.Min(TileWidth / 2 - 1, TileWidth / 2 * (wavePhase - 5 / 6f) * 6), 2), SpriteEffects.None, 0);
                            }
                        }
                    }
                }
            }
            #endregion

            for (int i = 0; i < FloatingParticles.Count; i++)
            {
                FloatingParticles[i].Draw(sb);
            }

			if (CurrentGame.gameState != GameState.MapEditor)
			{
				for (int w = 0; w < Waves.Length; w++)
				{
					for (int g = 0; g < Waves[w].Groups.Length; g++)
					{
						Waves[w].Groups[g].ShowPath(sb);
						//for (int c = 0; c < Waves[w].Groups[g].Creatures.Length; c++)
						//{
						//	Waves[w].Groups[g].Creatures[c].ShowPath(sb);
						//}
					}
					Waves[w].Draw(sb);
				}
			}
			else if (CurrentGame.gameState == GameState.MapEditor)
			{

			}

			bool showRadius = CurrentGame.keyboard.IsKeyDown(Keys.F);
            for (int i = 0; i < Players[0].Towers.Count; i++)
            {
                Players[0].Towers[i].ShowRadius = showRadius; //-----------------------------------------------------------------------------------------------------------mur! drawissa
				if (CurrentGame.HUD.hoveredCoord == Players[0].Towers[i].MapCoord && !CurrentGame.HUD.newTileRingActive)
                    Players[0].Towers[i].ShowRadius = true;

                Players[0].Towers[i].Draw(sb);
            }
			for (int i = 0; i < Players[0].Towers.Count; i++) // jotta luodit tornien päälle.. EI TARVIIS JOS KÄYTTÄS DRAW-LAYER-DEPTHEJÄ!
			{
				Players[0].Towers[i].DrawBullets(sb);
			}

            Pathfinder.Draw(sb);

        }
    }
}

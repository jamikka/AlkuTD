using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace AlkuTD
{
    public class HUD
    {
		public static Dictionary<string, Color> GeneColors;

        CurrentGame ParentGame;
        public HexMap ParentMap;

        #region MapEditor fields
        //----------------------------------------------MapEditor------------.
        public enum DrawMode {
            Void,
            WallPath,
            Point,
            Tower
        }
        public DrawMode drawMode;
        public bool inWaveEdit;
        public bool MapEdited;
        public Button[] HUDbuttons;
        public Button[] MapEditorMenuButtons;
        public Button[] MapEditorToolButtons;
        public Button[] MapEditorTopButtons;
        public Button[] MapEditorLabelButtons;
        public Button[] MapEditorTableAddButtons;
        public Button[] MapEditorTableDelButtons;
        public Button[] MapEditorWaveLabels;
        public Button BackToEditButton;
        public TextCell[] MapEditorTableCells;
        public TextCell[] MapEditorResourceCells;
        public TextCell[] AvailableTowersCells;
        public TextCell MapNameBox;
        public int MapEditorGroupAmt;
        public int MapEditorTableRows;
        public int MapEditorTableCols;
        Rectangle TableBackground;
        Rectangle TableBackground1;
        Rectangle TableBackground2;
        Rectangle TableBackground3;
        Rectangle TableBackground4;
        Rectangle TableBackground5;
        List<Rectangle> BoundsList = new List<Rectangle>();

        public List<Point> MapEditorSpawnPoints;
        public List<Point> MapEditorGoalPoints;

        int totalGroups; //eehh

        bool ErrorShow;
        List<Button> ErrorButtons;

        int[] clipBoard;

        int tableButtonYDist;
        int onePadding;
        int menuButtonHeight;
        int cellButtonHeight;
        int topButtonX;
        int topButtonY;
        int menuButtonWidth;
        int menuButtonX;
        int menuButtonY;
        int toolButtonWidth;
        int toolButtonX;
        int toolButtonY;
        public Point mapTestWTEditPos;
        public Point mapEditWTEditPos;
        Color[] buttonColors;
        //Color[] buttonColors { get { return buttonColorsS; }
        //    set { buttonColorsS = value; } }
        Color[] tableCellColors;
        Color[] buttonTextColors; //--------------------äijäiäi------------'
        #endregion

        public Rectangle[] ButtonBoundses;
        public ButnState[] ButtonStates;
        public string[] ButtonWords;

        public Texture2D[] ringFills;
        public Texture2D tileringGlow;
        public Texture2D tileOverlay;
        public Texture2D tilering;
		public Texture2D tileringSlot;

        public Vector2 tileringCenter;

        SpriteFont font;

        public Cue hudCue;

        public Point hoveredCoord;
        public Vector2 hoveredTilePos;

		//Vector2 checkVisTile;

		public Bar[] GeneBars;
		public static List<BugInfoBox> BugBoxes;
		public static List<GroupInfoBox> NexWaveInfoBoxes;
		public static List<GroupInfoBox> CurrWaveInfoBoxes;
		static TowerInfoBox HoveredTowerInfoBox;
		static InfoBox TileRingInfoBox;

        public HUD(CurrentGame game)
        {
            #region MAIN INITIALIZATIONS
            ParentGame = game;
			CurrentGame.HUD = this;
            font = CurrentGame.font;
			ParentMap = CurrentGame.currentMap;

            ringFills = new Texture2D[] { game.Content.Load<Texture2D>("Tilering\\ringFill-NW"),
                                          game.Content.Load<Texture2D>("Tilering\\ringFill-NE"), };
            tileOverlay = game.Content.Load<Texture2D>("Tiles\\hex-66-57-overlay");
            tileringGlow = game.Content.Load<Texture2D>("Tilering\\tileringGlow");
			tilering = game.Content.Load<Texture2D>("Tilering\\tilering7");
			tileringSlot = game.Content.Load<Texture2D>("Tilering\\singleSlot");

            tileringCenter = new Vector2((float)Math.Round(tilering.Width / 2f), (float)Math.Round(tilering.Height / 2f));

            onePadding = 9; //orig 10
            menuButtonHeight = font.LineSpacing + onePadding -2;
            cellButtonHeight = font.LineSpacing;
            menuButtonWidth = (int)Math.Round(font.MeasureString("MENU").X) + 2 * onePadding;
            toolButtonWidth = (int)Math.Round(font.MeasureString("00").X) + 2 * onePadding;
            menuButtonX = menuButtonWidth / 2;
            menuButtonY = (int)(game.GraphicsDevice.Viewport.Height * 0.5f - 2.5 * menuButtonHeight);
            topButtonX = (int)(menuButtonWidth * 1.5f);
            topButtonY = 20;
            toolButtonX = game.GraphicsDevice.Viewport.Width - toolButtonWidth * 2;
            toolButtonY = (int)(game.GraphicsDevice.Viewport.Height * 0.5 - 2*tileOverlay.Height);
            buttonColors = new Color[] { new Color(20,30,40), new Color(30,40,50), new Color(40,50,60) }; //----passive,hovered,pressed
            buttonTextColors = new Color[] { Color.SlateGray, Color.Orange, Color.Orange };//----passive,hovered,pressed
            Color[] transparent = new Color[] { Color.Transparent, Color.Transparent, Color.Transparent };

            MapEdited = true;

            HUDbuttons = new Button[] { new Button("Start the waves", topButtonX, topButtonY, (int)font.MeasureString("Start the waves").X + 2*onePadding, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel),
                                        new Button("Restart", (int)(game.GraphicsDevice.Viewport.Height*0.4f), topButtonY, (int)font.MeasureString("Restart").X + 2*onePadding, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel)};
			extendedPoints = new Vector2[20];

			GeneColors = new Dictionary<string, Color>();
			GeneColors.Add("Red", Color.Red);
			GeneColors.Add("Green", Color.ForestGreen);
			GeneColors.Add("Blue", Color.CornflowerBlue);

            #endregion

            #region MAP EDITOR INITIALIZATIONS
            drawMode = HUD.DrawMode.WallPath;
            MapEditorMenuButtons = new Button[] { new Button("TEST", menuButtonX, menuButtonY, menuButtonWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel),
                                                  new Button("SAVE", menuButtonX, menuButtonY + menuButtonHeight, menuButtonWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel),
                                                  new Button("LOAD", menuButtonX, menuButtonY + 2*menuButtonHeight, menuButtonWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel),
                                                  new Button("MENU", menuButtonX, menuButtonY + 3*menuButtonHeight, menuButtonWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel),
                                                  new Button("QUIT", menuButtonX, menuButtonY + 4*menuButtonHeight, menuButtonWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel)};

            //MapEditorMenuButtons[1].InToggleMode = true;            
            MapEditorMenuButtons[1].DDMenuPos = DropDownMenuPos.Right;
            MapEditorMenuButtons[1].PopulateDropDownMenu(new string[] { "Overwrite", "Keep old" });
            for (int i = 0; i < MapEditorMenuButtons[1].DropDownButtons.Length; i++)
                MapEditorMenuButtons[1].DropDownButtons[i].TextAlign = TextAlignment.Left;

            //for (int i = 0; i < MapEditorMenuButtons[1].DropDownButtons.Length; i++)
              //  MapEditorMenuButtons[1].DropDownButtons[i].TextAlign = Button.TextAlignment.Left;
            MapEditorMenuButtons[2].IsDropDownMenu = true;
            MapEditorMenuButtons[2].InToggleMode = true;
            MapEditorMenuButtons[2].DDMenuPos = DropDownMenuPos.Right;
            MapEditorMenuButtons[2].PopulateDropDownMenu(Array.ConvertAll<string, string>(Directory.GetFiles(CurrentGame.MapDir), Path.GetFileNameWithoutExtension));
            for (int i = 0; i < MapEditorMenuButtons[2].DropDownButtons.Length; i++)
                MapEditorMenuButtons[2].DropDownButtons[i].TextAlign = TextAlignment.Left;

            MapEditorToolButtons = new Button[] { new Button("void", toolButtonX, toolButtonY, tileOverlay.Width, tileOverlay.Height, onePadding, TextAlignment.Center, (Color[])buttonColors.Clone(), buttonTextColors, tileOverlay),
                                                  new Button("wall/\r\npath", toolButtonX, toolButtonY + tileOverlay.Height, tileOverlay.Width, tileOverlay.Height, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, tileOverlay),
                                                  new Button("point", toolButtonX, toolButtonY + 2*tileOverlay.Height, tileOverlay.Width, tileOverlay.Height, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, tileOverlay),
                                                  new Button("tower", toolButtonX, toolButtonY + 3*tileOverlay.Height, tileOverlay.Width, tileOverlay.Height, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, tileOverlay)};

            MapEditorToolButtons[1].State = ButnState.Active;

            foreach (Button b in MapEditorToolButtons)
                b.InToggleMode = true;

			int mapNameBoxXPOS = (int)(game.GraphicsDevice.Viewport.Width*0.17f) - (int)font.MeasureString("Map name:").X - 2*onePadding;
			int resCellWidth = (int)font.MeasureString("200").X + onePadding;
			Rectangle nrgLabelBOUNDS = new Rectangle((int)(game.GraphicsDevice.Viewport.Width * 0.5f), topButtonY, (int)font.MeasureString("Energy: ").X, menuButtonHeight);
			Rectangle geneLabelBOUNDS = new Rectangle(nrgLabelBOUNDS.Right + resCellWidth + onePadding*2, topButtonY, (int)font.MeasureString("Genes: ").X, menuButtonHeight);
			Rectangle lifeLabelBOUNDS = new Rectangle(geneLabelBOUNDS.Right + resCellWidth * 3 + onePadding*2, topButtonY, (int)font.MeasureString("Life: ").X, menuButtonHeight);
            MapEditorTopButtons = new Button[] { new Button("Map name:", mapNameBoxXPOS, topButtonY, (int)font.MeasureString("Map name:").X + 2*onePadding, menuButtonHeight, onePadding, TextAlignment.Center, transparent, buttonTextColors, CurrentGame.pixel),
                                                 //new Button("Edit waves", game.GraphicsDevice.Viewport.Width/2 - ((int)font.MeasureString("Edit waves").X + 2*padding) /2 /*(int)(game.GraphicsDevice.Viewport.Width*0.43f)*/, topButtonY, (int)font.MeasureString("Edit waves").X + 2*padding, buttonHeight, padding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel),
                                                 //new Button("Energy:", (int)(game.GraphicsDevice.Viewport.Width*0.597f), topButtonY, (int)font.MeasureString("200").X + 2*padding, buttonHeight, padding, TextAlignment.Center, transparent, buttonTextColors, CurrentGame.pixel),
                                                 //new Button("Genes:", (int)(game.GraphicsDevice.Viewport.Width*0.7035f), topButtonY, (int)font.MeasureString("200").X + 2*padding, buttonHeight, padding, TextAlignment.Center, transparent, buttonTextColors, CurrentGame.pixel),
												 new Button("Edit waves", (int)(game.GraphicsDevice.Viewport.Width * 0.4f) - ((int)font.MeasureString("Edit waves").X + 2*onePadding) /2 /*(int)(game.GraphicsDevice.Viewport.Width*0.43f)*/, topButtonY, (int)font.MeasureString("Edit waves").X + 2*onePadding, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel),
                                                 new Button("Energy:", nrgLabelBOUNDS.X, topButtonY, nrgLabelBOUNDS.Width, menuButtonHeight, onePadding, TextAlignment.Center, transparent, buttonTextColors, CurrentGame.pixel),
                                                 new Button("Genes:", geneLabelBOUNDS.X, topButtonY, geneLabelBOUNDS.Width, menuButtonHeight, onePadding, TextAlignment.Center, transparent, buttonTextColors, CurrentGame.pixel),
                                                 //new Button("Life:", (int)(game.GraphicsDevice.Viewport.Width*0.807f), topButtonY, (int)font.MeasureString("200").X + 2*padding, buttonHeight, padding, TextAlignment.Center, transparent, buttonTextColors, CurrentGame.pixel)};
                                                 new Button("Life:", lifeLabelBOUNDS.X, topButtonY, lifeLabelBOUNDS.Width, menuButtonHeight, onePadding, TextAlignment.Center, transparent, buttonTextColors, CurrentGame.pixel)};
            MapEditorTopButtons[1].InToggleMode = true;

            mapEditWTEditPos = MapEditorTopButtons[1].Pos;

            MapNameBox = new TextCell("", MapEditorTopButtons[0].Bounds.Right, topButtonY, (int)(menuButtonWidth*2.4f), menuButtonHeight, onePadding, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel, InputType.text, false, false);

            MapEditorResourceCells = new TextCell[] { new TextCell("0", nrgLabelBOUNDS.Right, topButtonY, resCellWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel, InputType.integer, false, true),
													  new TextCell("0", geneLabelBOUNDS.Right, topButtonY, resCellWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel, InputType.integer, false, true),
                                                      new TextCell("0", geneLabelBOUNDS.Right + resCellWidth, topButtonY, resCellWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel, InputType.integer, false, true),
                                                      new TextCell("0", geneLabelBOUNDS.Right + resCellWidth*2, topButtonY, resCellWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel, InputType.integer, false, true),
                                                      new TextCell("0", lifeLabelBOUNDS.Right, topButtonY, resCellWidth, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel, InputType.integer, false, true)};

			MapEditorResourceCells[1].ButtonColors = new Color[] { new Color(60, 10, 20), new Color(70, 20, 30), new Color(80, 30, 40) }; //----passive,hovered,pressed
			MapEditorResourceCells[2].ButtonColors = new Color[] { new Color(20, 60, 30), new Color(30, 70, 40), new Color(40, 80, 50) };
			MapEditorResourceCells[3].ButtonColors = new Color[] { new Color(20, 40, 60), new Color(30, 50, 70), new Color(40, 60, 80) };

			for (int i = 1; i < 4; i++)
				MapEditorResourceCells[i].InputMade += new TextInputDelegate(InitGenesChanged);

            MapEditorTableRows = 30; //orig 22
            MapEditorTableCols = 16;
            int tableButtonWidth = (int)(game.GraphicsDevice.Viewport.Width * 0.8f / MapEditorTableCols);
            tableButtonYDist = (int)(game.GraphicsDevice.Viewport.Height * 0.8f / (MapEditorTableRows-2)); //orig -2

            int cellWideWidth = (int)(game.GraphicsDevice.Viewport.Width * 0.8f / MapEditorTableCols * 2.2f);
            int cellSmallWidth = (int)(game.GraphicsDevice.Viewport.Width * 0.8f / MapEditorTableCols * 0.78);//0.9143f); //0.8846 = jäljellejäävä kuudestoistaosa kolmen leveän solun jälkeen
            int cellGap = 3;

            int tableButtonY = (int)(game.GraphicsDevice.Viewport.Height * 0.1f) + tableButtonYDist;
            int tableLabelButtonX = (int)(game.GraphicsDevice.Viewport.Width*0.115f) + tableButtonWidth;
            int tableLabelButtonY = (int)(game.GraphicsDevice.Viewport.Height*0.1f);
            Color[] tableLabelColors = new Color[] { Color.DarkSlateGray, Color.Transparent, Color.Transparent }; //----passive,hovered,pressed
            Color[] tableLabelTextColors = new Color[] { Color.BlanchedAlmond, Color.Orange, Color.Orange };//----passive,hovered,pressed
            tableCellColors = new Color[] { new Color(15,25,35), new Color(25,35,45), new Color(35,45,55) }; //----passive,hovered,pressed

            int tableCellPadding = 6;
            MapEditorLabelButtons = new Button[15];
            for (int i = 0; i < MapEditorLabelButtons.Length; i++)
            {
                switch (i)
                {
                    case 0: MapEditorLabelButtons[i] = new Button("Amt", tableLabelButtonX + i*cellSmallWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding /*onePadding*/, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
                    case 1: MapEditorLabelButtons[i] = new Button("Type", tableLabelButtonX + i*cellSmallWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.text, false); break;
                    case 2: MapEditorLabelButtons[i] = new Button("Name", tableLabelButtonX + i*cellSmallWidth, tableLabelButtonY, cellWideWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.text, false); break;
                    case 3: MapEditorLabelButtons[i] = new Button("Texture", tableLabelButtonX + (i-1)*cellSmallWidth + cellWideWidth, tableLabelButtonY, cellWideWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.text, true); break;
                    case 4: MapEditorLabelButtons[i] = new Button("SpwP", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, true); break;
                    case 5: MapEditorLabelButtons[i] = new Button("Goals", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.text, true); break;
                    case 6: MapEditorLabelButtons[i] = new Button("HP", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
                    //case 7: MapEditorLabelButtons[i] = new Button("Elem", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, buttonHeight, padding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, true); break;
					//case 8: MapEditorLabelButtons[i] = new Button("SPD", tableLabelButtonX + (i - 2) * cellSmallWidth + 2 * cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, buttonHeight, padding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.floating, false); break;
					//case 9: MapEditorLabelButtons[i] = new Button("CDmg", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, buttonHeight, padding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
					//case 10: MapEditorLabelButtons[i] = new Button("LDmg", tableLabelButtonX + (i - 2) * cellSmallWidth + 2 * cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, buttonHeight, padding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
					//case 11: MapEditorLabelButtons[i] = new Button("Energ", tableLabelButtonX + (i - 2) * cellSmallWidth + 2 * cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, buttonHeight, padding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
					//case 12: MapEditorLabelButtons[i] = new Button("Genes", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, buttonHeight, padding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
					case 7: MapEditorLabelButtons[i] = new Button("SPD", tableLabelButtonX + (i - 2) * cellSmallWidth + 2 * cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.floating, false); break;
                    case 8: MapEditorLabelButtons[i] = new Button("LDmg", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
                    case 9: MapEditorLabelButtons[i] = new Button("Energ", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
					case 10: MapEditorLabelButtons[i] = new Button("RRes", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.floating, false); break;
					case 11: MapEditorLabelButtons[i] = new Button("GRes", tableLabelButtonX + (i - 2) * cellSmallWidth + 2 * cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.floating, false); break;
					case 12: MapEditorLabelButtons[i] = new Button("BRes", tableLabelButtonX + (i - 2) * cellSmallWidth + 2 * cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.floating, false); break;
                    case 13: MapEditorLabelButtons[i] = new Button("SpwD", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth - cellGap, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
                    case 14: MapEditorLabelButtons[i] = new Button("Dur", tableLabelButtonX + (i-2)*cellSmallWidth + 2*cellWideWidth, tableLabelButtonY, cellSmallWidth, cellButtonHeight, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false); break;
                }
            }
            //foreach (Button b in MapEditorLabelButtons)
            //    b.NonDeselectGroup = true;
            MapEditorTableCells = new TextCell[MapEditorTableRows * (MapEditorTableCols-1)];
            for (int i = 0; i < MapEditorTableCells.Length; i++)
            {
                MapEditorTableCells[i] = new TextCell("",
                                                      MapEditorLabelButtons[i % (MapEditorTableCols-1)].Pos.X,
                                                      tableButtonY + i/(MapEditorTableCols-1)%MapEditorTableRows * tableButtonYDist,
                                                      MapEditorLabelButtons[i % (MapEditorTableCols-1)].Width,
                                                      cellButtonHeight,
                                                      tableCellPadding,
                                                      TextAlignment.Center,
                                                      i%(MapEditorTableCols-1) % 2 == 0? (Color[])buttonColors.Clone() : (Color[])tableCellColors.Clone(),
                                                      tableLabelTextColors,
                                                      CurrentGame.pixel,
                                                      MapEditorLabelButtons[i % (MapEditorTableCols-1)].inputType,
                                                      MapEditorLabelButtons[i % (MapEditorTableCols-1)].IsDropDownMenu,
													  i % (MapEditorTableCols-1) >= 6);
                //if (i % (MapEditorTableCols-1) == 7)
                //    MapEditorTableCells[i].Text = "-";
                MapEditorTableCells[i].Index = i; //------------------------------------------------NOLOUS
            }

            clipBoard = new int[MapEditorTableCells.Length];

            for (int i = 0; i < MapEditorTableRows; i++)
            {
                MapEditorTableCells[3 + i*(MapEditorTableCols-1)].PopulateDropDownMenu(Array.ConvertAll<string, string>(Directory.GetFiles(CurrentGame.ContentDir + "Creatures\\"), Path.GetFileNameWithoutExtension));
                //MapEditorTableCells[7 + i*(MapEditorTableCols-1)].DropDownButtons[1].ButtonColors = new Color[] {Color.Red*0.8f, Color.Red, Color.Red*0.7f};        MapEditorTableCells[7 + i*(MapEditorTableCols-1)].DropDownButtons[1].TextColors = new Color[] {Color.Red*0.8f, Color.Red, Color.Red*0.7f};
                //MapEditorTableCells[7 + i*(MapEditorTableCols-1)].DropDownButtons[2].ButtonColors = new Color[] {Color.Green*0.8f, Color.Green, Color.Green*0.7f};  MapEditorTableCells[7 + i*(MapEditorTableCols-1)].DropDownButtons[2].TextColors = new Color[] {Color.Green*0.8f, Color.Green, Color.Green*0.7f};
                //MapEditorTableCells[7 + i*(MapEditorTableCols-1)].DropDownButtons[3].ButtonColors = new Color[] {Color.Blue*0.8f, Color.Blue, Color.Blue*0.7f};     MapEditorTableCells[7 + i*(MapEditorTableCols-1)].DropDownButtons[3].TextColors = new Color[] {Color.Blue*0.8f, Color.Blue, Color.Blue*0.7f};
            }

            MapEditorWaveLabels = new Button[MapEditorTableRows];
            int WaveLabelWidth = (int)font.MeasureString("Wave 2").X + 2*onePadding;
            for (int i = 0; i < MapEditorWaveLabels.Length; i++)
                MapEditorWaveLabels[i] = new Button("Wave 1", menuButtonX + menuButtonWidth + cellGap, tableButtonY + i * tableButtonYDist, WaveLabelWidth - cellGap, menuButtonHeight, onePadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false);
            
            MapEditorWaveLabels[1].Text = "Add";

            int addButtonWidth = (int)font.MeasureString("+").X + onePadding;
            int addButtonX = tableLabelButtonX - addButtonWidth - cellGap - 1;
            MapEditorTableAddButtons = new Button[MapEditorTableRows +1];
            for (int i = 0; i < MapEditorTableAddButtons.Length; i++)
            {
                MapEditorTableAddButtons[i] = new Button("+", addButtonX, (int)(tableButtonY - 0.5 * tableButtonYDist + i*tableButtonYDist), addButtonWidth, addButtonWidth, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false);
                MapEditorTableAddButtons[i].YPadding = 2;
                MapEditorTableAddButtons[i].RealignText();
            }
            int delButtonX = tableLabelButtonX + (MapEditorTableCols-3) * cellSmallWidth + 2*cellWideWidth + cellGap;
            MapEditorTableDelButtons = new Button[MapEditorTableRows];
            for (int i = 0; i < MapEditorTableDelButtons.Length; i++)
                MapEditorTableDelButtons[i] = new Button("x", delButtonX, tableButtonY + i * tableButtonYDist, addButtonWidth, addButtonWidth, tableCellPadding, TextAlignment.Center, tableLabelColors, tableLabelTextColors, CurrentGame.pixel, InputType.integer, false);

            TableBackground = new Rectangle(topButtonX, topButtonY + menuButtonHeight *2, ParentGame.GraphicsDevice.Viewport.Width - topButtonX * 2 + 15, ParentGame.GraphicsDevice.Viewport.Height - (menuButtonHeight +7) * 2);
            TableBackground1 = new Rectangle(MapEditorTopButtons[0].Pos.X, topButtonY, MapEditorTopButtons[0].Width + MapNameBox.Width, menuButtonHeight);
            TableBackground2 = new Rectangle(menuButtonX, menuButtonY, menuButtonWidth, menuButtonHeight*5);
            TableBackground3 = new Rectangle(MapEditorTopButtons[2].Pos.X -5, topButtonY, (int)(ParentGame.GraphicsDevice.Viewport.Width*0.3f), menuButtonHeight);
            TableBackground4 = TableBackground3;
            TableBackground5 = TableBackground3;

            BoundsList.AddRange(new Rectangle[]{ TableBackground, TableBackground1, TableBackground2, TableBackground3, TableBackground4, TableBackground5 });
            //foreach (TextCell c in MapEditorTableCells)
            //    BoundsList.Add(c.Bounds);
            //BoundsList.AddRange(Array.

            ErrorButtons = new List<Button>();

            BackToEditButton = new Button("Back to edit", menuButtonX, HUDbuttons[0].Bounds.Bottom + 5, (int)CurrentGame.font.MeasureString("Back to edit").X + onePadding*2, menuButtonHeight, onePadding, TextAlignment.Center, buttonColors, buttonTextColors, CurrentGame.pixel);

            mapTestWTEditPos = new Point(BackToEditButton.Bounds.Right + cellGap, BackToEditButton.Pos.Y);

            Vector2[] tileCorners = new Vector2[6] { new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width/4 - menuButtonHeight/2, MapEditorToolButtons[3].Pos.Y - menuButtonHeight/2 + 2), //up-L
                                                     new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width/4 * 3 +1 - menuButtonHeight/2, MapEditorToolButtons[3].Pos.Y - menuButtonHeight/2 +2),  //up-R
                                                     new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width -1 - menuButtonHeight/2, MapEditorToolButtons[3].Pos.Y + tileOverlay.Height/2 - menuButtonHeight/2),         //R
                                                     new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width/4 * 3 +1 - menuButtonHeight/2, MapEditorToolButtons[3].Pos.Y + tileOverlay.Height -1 - menuButtonHeight/2),    //lo-R
                                                     new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width/4 - menuButtonHeight/2, MapEditorToolButtons[3].Pos.Y + tileOverlay.Height -1 - menuButtonHeight/2), //lo-L
                                                     new Vector2(MapEditorToolButtons[3].Pos.X +1 - menuButtonHeight/2, MapEditorToolButtons[3].Pos.Y + tileOverlay.Height/2 - menuButtonHeight/2)};        //L
            AvailableTowersCells = new TextCell[6];
            for (int i = 0; i < AvailableTowersCells.Length; i++)
            {
                AvailableTowersCells[i] = new TextCell("3", (int)tileCorners[i].X, (int)tileCorners[i].Y, menuButtonHeight, menuButtonHeight, onePadding, TextAlignment.Center, new Color[] { Color.Transparent, Color.Transparent, buttonColors[1] }, buttonTextColors, CurrentGame.pixel, InputType.integer, true, true);
                AvailableTowersCells[i].PopulateDropDownMenu(new string[] { "-", "1", "2", "3" });
            }

            #endregion

			GeneBars = new Bar[3] { new Bar(new Rectangle(geneLabelBOUNDS.Right + resCellWidth/3, geneLabelBOUNDS.Bottom + onePadding/2, 20, 60), 0, 100, 0), 
								    new Bar(new Rectangle(geneLabelBOUNDS.Right + resCellWidth + resCellWidth/3, geneLabelBOUNDS.Bottom + onePadding/2, 20, 60), 0, 100, 1), 
									new Bar(new Rectangle(geneLabelBOUNDS.Right + (resCellWidth*2) + resCellWidth/3, geneLabelBOUNDS.Bottom + onePadding/2, 20, 60), 0, 100, 2) };

			BugBoxes = new List<BugInfoBox>();
			NexWaveInfoBoxes = new List<GroupInfoBox>();
			CurrWaveInfoBoxes = new List<GroupInfoBox>();
        }

		void InitGenesChanged()
		{
			//hudCue = CurrentGame.soundBank.GetCue("bui");
			//hudCue.Play();
			UpdateGeneBarsMAPEDIT();
		}

        //noooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo.
        public void EditorMapLoad(Button mapButton)
        {
            if (File.Exists(CurrentGame.MapDir + mapButton.Text + ".txt"))
            {
                //try //------------------------------------------------------------------------------------------------jänkkää silti tyhjään .txtiin
                //{
                    byte[] availableTowers = new byte[6];
                    List<Tower> initTowers = new List<Tower>();
                    ParentMap.Players[0].Towers.Clear();

                    using (StreamReader reader = new StreamReader(CurrentGame.MapDir + mapButton.Text + ".txt"))
                    {
                        string[] read;
						Dictionary<int, Point> spawnPoints = new Dictionary<int, Point>();
                        //List<Point> spawnPoints = new List<Point>();
                        List<Point> goalPoints = new List<Point>();

                        MapNameBox.Text = mapButton.Text;
                        ParentMap.Name = mapButton.Text;

                        for (int row = 0; row < 11; row++)
                        {
                            for (int col = 0; col < 21; col++)
                            {
                                int ascii = reader.Peek();
                                while (ascii < 32)
                                {
                                    reader.Read();
                                    ascii = reader.Peek();
                                }
                                ParentMap.Layout[row, col] = (char)ascii;
                                reader.Read();
                                //char bs = (char)ascii;
                                if (ascii == 32 || ascii == 39 || ascii == 46 || ascii == 48)
                                    continue;

								//int bg = (int)char.GetNumericValue((char)ascii);
                                if (ascii >= 49 && ascii <= 57) // mark SPOINT if ascii 49-57 (digits 1-9)
									spawnPoints.Add((int)char.GetNumericValue((char)ascii) - 1, new Point(col, row));
                                else if (ascii >= 97 && ascii <= 122) // mark GPOINT if ascii 97-122 (lowercase letters a-z)
                                    goalPoints.Add(new Point(col, row));
                                else
                                {
									//Tower tempTower = Tower.NewFromModel(Array.Find<Tower>(HexMap.ExampleTowers, t => (int)t.Symbol == ascii), new Point(col, row)); //---- jos lambdalla haluis mennä..
                                    for (int i = 0; i < HexMap.ExampleTowers.Length; i++)
                                    {
                                        if (ascii == (int)HexMap.ExampleTowers[i].Symbol)
                                        {
											Tower tempTower = Tower.NewFromModel(HexMap.ExampleTowers[i], new Point(col, row));
                                            initTowers.Add(tempTower);
                                            ParentMap.Players[0].Towers.Add(tempTower);
                                        }
                                    }
                                }
								
                            }
                        }
                        ParentMap.CurrentLayout = new MapLayout(ParentMap.Layout);
                        ParentMap.InitialLayout = new MapLayout(ParentMap.Layout);

						if (initTowers != null)
						{
							//ParentMap.InitTowers = initTowers;
							ParentMap.InitTowers = new List<Tower>();
							for (int i = 0; i < initTowers.Count; i++)
								ParentMap.InitTowers.Add(Tower.Clone(initTowers[i]));
						}
						else
							ParentMap.InitTowers = new List<Tower>();

                        ParentMap.Pathfinder.InitializeTiles();             //-----------------------hmmmmmmmmm
                        ParentMap.MapEditorTempWaves.Clear();
                        MapEditorSpawnPoints.Clear();
                        MapEditorGoalPoints.Clear();
						for (int i = 0; i < spawnPoints.Count; i++)
							MapEditorSpawnPoints.Add(spawnPoints[i]);
                        MapEditorGoalPoints.InsertRange(0, goalPoints);
						ParentMap.SpawnPoints = MapEditorSpawnPoints.ToArray();
                        ParentMap.GoalPoints = goalPoints.ToArray();

                        reader.ReadLine();
                        reader.ReadLine();
                        read = reader.ReadLine().Split(':', ' ');
                        for (int i = 0; i < availableTowers.Length; i++)
                        {
                            byte.TryParse(read[i + 3], out availableTowers[i]);  // makes byte-array 0 where not a number
                            AvailableTowersCells[i].Text = read[i + 3];
                        }
                        ParentMap.AvailableTowers = availableTowers;

                        MapEditorResourceCells[4].Text = reader.ReadLine().Split(':')[1].Trim();
                        MapEditorResourceCells[0].Text = reader.ReadLine().Split(':')[1].Trim();
						read = reader.ReadLine().Split(' ' , ',');
						MapEditorResourceCells[1].Text = read[2];
						MapEditorResourceCells[2].Text = read[4];
						MapEditorResourceCells[3].Text = read[6];

                        while (!reader.ReadLine().Contains("Creamt")) ;

                        totalGroups = 0;
                        //int peek = reader.Peek();
                        for (int w = 0; reader.Peek() == 87; w++) //wave lines begin with a W (87)
                        {
                            ParentMap.MapEditorTempWaves.Add(new Wave(ParentMap));
							Wave currWave = ParentMap.MapEditorTempWaves[w];
                            currWave.TempGroups = new List<SpawnGroup>();
                            MapEditorWaveLabels[totalGroups].Text = "Wave " + ParentMap.MapEditorTempWaves.Count;

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

                               // float geneFloat;

                                currWave.TempGroups.Add(new SpawnGroup(read[0] == "" ? 0 : int.Parse(read[0]),
																						   new Creature(read[1],
																										read[2],
																										ParentMap,
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
                                                                                            currWave.TempGroups.Count, w));


								for (int c = 0; c < (MapEditorTableCols - 1); c++)
								{
									MapEditorTableCells[c + totalGroups * (MapEditorTableCols - 1)].Text = read[c];
								}
								totalGroups++;
                            }
                            MapEditorWaveLabels[totalGroups].Text = "Add";
                            MapEdited = true; // -----------------------FOR COLOR UPDATE eeghhH!!
							UpdateGeneBarsMAPEDIT();
                        }
                    }
                //}
                //catch (Exception)
                //{
                //    mapButton.Text += " (bad file!)";
                 //   CurrentGame.gameState = GameState.MainMenu;
                //} 
            }
			//checkVisTile = ParentMap.ToScreenLocation(MapEditorSpawnPoints[0]);

        }
        //noooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo'

		void UpdateGeneBarsMAPEDIT()
		{
			CurrentGame.players[0].GenePoints[0] = int.Parse(MapEditorResourceCells[1].Text);
			CurrentGame.players[0].GenePoints[1] = int.Parse(MapEditorResourceCells[2].Text);
			CurrentGame.players[0].GenePoints[2] = int.Parse(MapEditorResourceCells[3].Text);
			foreach (var gbar in GeneBars)
				gbar.UpdateFill();
		}

		public void UpdateGeneBars()
		{
			foreach (var gbar in GeneBars)
				gbar.UpdateFill();
		}

        public bool ValidateMap()
        {
            bool valid = true;

            List<int>[] invalidLists = new List<int>[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
            string[] listDefinitions = { "Spawnpoint undefined", "Invalid spawnpoint", "Goalpoint undefined", "Invalid goalpoint" };
            /*List<int> invalidSpawnpoints = new List<int>();
            List<int> invalidGoalpoints = new List<int>();
            List<int> missingSpawnpoints = new List<int>();
            List<int> missingGoalpoints = new List<int>();*/

            ErrorButtons.Clear();

            if (MapEditorSpawnPoints.Count < 1)
            {
                valid = false;
                ErrorShow = true;
                ErrorButtons.Add(new Button("No spawnpoints", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * menuButtonHeight, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
            }
            if (MapEditorGoalPoints.Count < 1)
            {
                valid = false;
                ErrorShow = true;
                ErrorButtons.Add(new Button("No goalpoints", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * menuButtonHeight, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
            }
            if (MapEditorSpawnPoints.Count > 0 && MapEditorGoalPoints.Count > 0)
            {
                for (int i = 0; i < totalGroups; i++)
                {
                    if (MapEditorTableCells[4 + i * (MapEditorTableCols - 1)].Text == "") //INPUT IS NOT ZERO-BASED----------!!!!!!!
                        invalidLists[0].Add(i);
                        //missingSpawnpoints.Add(i);
                    else if (int.Parse(MapEditorTableCells[4 + i * (MapEditorTableCols - 1)].Text) > MapEditorSpawnPoints.Count)
                        invalidLists[1].Add(i);
                        //invalidSpawnpoints.Add(i);

                    if (MapEditorTableCells[5 + i * (MapEditorTableCols - 1)].Text == "")
                        invalidLists[2].Add(i);
                        //missingGoalpoints.Add(i);
                    else if ((int)char.Parse(MapEditorTableCells[5 + i * (MapEditorTableCols - 1)].Text) - 97 >= MapEditorGoalPoints.Count)
                        invalidLists[3].Add(i);
                        //invalidGoalpoints.Add(i);

                    int test, test2;
                    if (MapEditorTableCells[4 + i * (MapEditorTableCols-1)].Text != "" & MapEditorTableCells[5 + i * (MapEditorTableCols - 1)].Text != "")
                    {
                        test = (int)char.Parse(MapEditorTableCells[5 + i * (MapEditorTableCols - 1)].Text) - 97;
                        test2 = int.Parse(MapEditorTableCells[4 + i * (MapEditorTableCols - 1)].Text) - 1;
                    }

                    if (!invalidLists[1].Contains(i) && !invalidLists[3].Contains(i) &&
                        MapEditorTableCells[4 + i * (MapEditorTableCols-1)].Text != "" && // SP-arrays not up to date---------------------------------------------------------!!
                        MapEditorTableCells[5 + i * (MapEditorTableCols-1)].Text != "" && 
                        ParentMap.Pathfinder.FindPath(ParentMap.SpawnPoints[int.Parse(MapEditorTableCells[4 + i * (MapEditorTableCols-1)].Text) -1],
                                                      ParentMap.GoalPoints[(int)char.Parse(MapEditorTableCells[5 + i * (MapEditorTableCols-1)].Text) -97]).Count < 1)
                    {
                        ErrorButtons.Add(new Button("Path not found (group " + (i+1).ToString() + ")", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * menuButtonHeight, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
                    }
                }

                for (int m = 0; m < invalidLists.Length; m++)
                {
                    if (invalidLists[m].Count > 0)
                    {
                        valid = false;
                        ErrorShow = true;

                        if (invalidLists[m].Count > 1)
                            ErrorButtons.Add(new Button(listDefinitions[m] + " (groups ", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * menuButtonHeight, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
                        else ErrorButtons.Add(new Button(listDefinitions[m] + " (group ", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * menuButtonHeight, TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
                        for (int i = 0; i < invalidLists[m].Count; i++)
                        {
                            if (i > 0)
                                ErrorButtons[ErrorButtons.Count-1].Text += ", " + (invalidLists[m][i] + 1).ToString();
                            else ErrorButtons[ErrorButtons.Count-1].Text += (invalidLists[m][i] + 1).ToString();
                        }
                        ErrorButtons[ErrorButtons.Count-1].Text += ")";
                        ErrorButtons[ErrorButtons.Count-1].ResizeBounds();
                    }
                }
                #region UN-ENCAPSULATED OLD
                /*if (missingSpawnpoints.Count > 0)
                {
                    if (missingSpawnpoints.Count > 1)
                        ErrorButtons.Add(new Button("Spawnpoint undefined (groups ", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * buttonHeight, Button.TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
                    else ErrorButtons.Add(new Button("Spawnpoint undefined (group ", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * buttonHeight, Button.TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
                    for (int i = 0; i < missingSpawnpoints.Count; i++)
                    {
                        if (i > 0)
                            ErrorButtons[ErrorButtons.Count].Text += ", " + (missingSpawnpoints[i] + 1).ToString();
                        else ErrorButtons[ErrorButtons.Count].Text += (missingSpawnpoints[i] + 1).ToString();
                    }
                    ErrorButtons[ErrorButtons.Count].Text += ")";
                    ErrorButtons[ErrorButtons.Count].ResizeBounds();
                }

                if (missingGoalpoints.Count > 0)
                {
                    if (missingGoalpoints.Count > 1)
                        ErrorButtons.Add(new Button("Goalpoint undefined (groups ", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * buttonHeight, Button.TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
                    else ErrorButtons.Add(new Button("Goalpoint undefined (group ", MapEditorMenuButtons[0].Bounds.Right, menuButtonY + ErrorButtons.Count * buttonHeight, Button.TextAlignment.Left, buttonColors, buttonTextColors, CurrentGame.pixel));
                    for (int i = 0; i < missingGoalpoints.Count; i++)
                    {
                        if (i > 0)
                            ErrorButtons[ErrorButtons.Count].Text += ", " + (missingGoalpoints[i] + 1).ToString();
                        else ErrorButtons[ErrorButtons.Count].Text += (missingGoalpoints[i] + 1).ToString();
                    }
                    ErrorButtons[ErrorButtons.Count].Text += ")";
                    ErrorButtons[ErrorButtons.Count].ResizeBounds();
                }*/
                #endregion
            }
            return valid;
        }

		Vector2 Magnetize(Vector2[] points)
		{
			Vector2 dirFromTile = mousePos - activeTilePos; //--------------------äpp äpp, tee itsenäinen muuttuja jotta hienovaraiset pos-updatemuutokset toimii!
			float angle = (float)Math.Atan2(dirFromTile.Y, dirFromTile.X);
			float angleOffset = (float)(Math.Abs(angle % (Math.PI / 3)) / (Math.PI / 6));

			if (angleOffset > 1)
				angleOffset = 2 - angleOffset;                      //hex corners zero -- side centers one
			angleOffset = (float)Math.Round(angleOffset * 4.5f); //TileWidth/2f - TileHeight/2f;
			if (dirFromTile.Length() > ParentMap.TileHalfWidth - angleOffset) //if out of tile
			{
				//Pyöristeles------------------------------------------------------------------------------------------------------------------------------------!
				dirFromTile = (ParentMap.TileHalfWidth - angleOffset) * (dirFromTile / dirFromTile.Length());
				//Mouse.SetPosition((int)Math.Round(dirFromTile.X + activeTilePos.X), (int)Math.Round(dirFromTile.Y +activeTilePos.Y));
			}
			dirFromTile += activeTilePos;
			Vector2 partDist = mousePos - points[selectedRingPart];
			Vector2 dir = partDist / partDist.Length();
			if (partDist.Length() <= 1)
				dirFromTile = points[selectedRingPart];
			else if (/*partDist.Length() < 18 && */CurrentGame.gameTimer % 2 == 0)
				dirFromTile -= dir * 0.8f;

			Mouse.SetPosition((int)Math.Round(dirFromTile.X), (int)Math.Round(dirFromTile.Y));

			return dirFromTile - new Vector2(tileringGlow.Width / 2);
		}

		Vector2 MagnetizeExtended(Vector2[] points)
		{
			Vector2 dirFromTile = mousePos - activeTilePos; //--------------------äpp äpp, tee itsenäinen muuttuja jotta hienovaraiset pos-updatemuutokset toimii!
			if (dirFromTile.Length() > ParentMap.TileWidth) //if out of tile
				dirFromTile = ParentMap.TileWidth * (dirFromTile / dirFromTile.Length());
			dirFromTile += activeTilePos;

			Vector2 nearestPoint = points[CheckNearestPoint(points)];
			Vector2 partDist = mousePos - nearestPoint;
			Vector2 dir = partDist / partDist.Length();
			if (partDist.Length() <= 1)
				dirFromTile = nearestPoint;
			else if (CurrentGame.gameTimer % 2 == 0)
				dirFromTile -= dir * 0.8f;

			Mouse.SetPosition((int)Math.Round(dirFromTile.X), (int)Math.Round(dirFromTile.Y));

			return dirFromTile - new Vector2(tileringGlow.Width / 2);
		}

		int CheckNearestPoint(Vector2[] points)
		{
			int selection = 0;
			float closest = 1000;
			for (int i = points.GetUpperBound(0); i >= 0; i--) //start from the end so that 0 (center) has the last word
			{
				float dist = Vector2.Distance(mousePos, points[i]);
				if (dist < closest)
				{
					selection = i;
					closest = dist;
				}
			}
			return selection;
		}

        //------------------TODO: Real Smart waveInfoBox positioning
		public static void UpdateWaveInfo() 
		{
			NexWaveInfoBoxes.Clear();
			CurrWaveInfoBoxes.Clear();
			HexMap map = CurrentGame.currentMap;
			int totalWaves = map.Waves == null ? 0 : map.Waves.Length;
			int currWaveNum = map.currentWave;
			int nextWaveNum = currWaveNum + 1;
			int boxWidth = GroupInfoBox.boxWidth;

			if (currWaveNum < totalWaves - 1) //-------------------------------------Next wave info (at spawnpoints)
			{
				Wave nextWave = map.Waves[nextWaveNum];
				List<SpawnGroup> spGroups = new List<SpawnGroup>();
				for (int i = 0; i < map.SpawnPoints.Length; i++)
				{
					spGroups.Clear();
					Vector2 spawnpoint = map.ToScreenLocation(map.SpawnPoints[i]);
					Vector2 creatureFirstMoveTile = Vector2.Zero;

					for (int g = 0; g < map.Waves[nextWaveNum].Groups.Length; g++)
					{
						if (map.Waves[nextWaveNum].Groups[g].SpawnPointIndex == i)
						{
							spGroups.Add(map.Waves[nextWaveNum].Groups[g]);
							creatureFirstMoveTile = map.Waves[nextWaveNum].Groups[g].Creatures[0].OrigPath[1];
						}
					}
					if (spGroups.Count < 1)
						continue;

					int groupsOnSP = spGroups.Count;
					bool aboveSP = true;
					Vector2 firstBoxPos = spawnpoint - new Vector2(groupsOnSP * boxWidth * 0.5f, 0);
					float pathDir = (float)Math.Atan2(spawnpoint.Y - creatureFirstMoveTile.Y, spawnpoint.X - creatureFirstMoveTile.X);
					if (pathDir > 0)
					{
						firstBoxPos.Y += map.TileHalfHeight + 2;
						aboveSP = false;
					}
					else
						firstBoxPos.Y -= map.TileHalfHeight + GroupInfoBox.boxHeight;
					for (int b = 0; b < spGroups.Count; b++)
					{
						NexWaveInfoBoxes.Add(new GroupInfoBox(firstBoxPos + new Vector2(b * boxWidth, 0), spGroups[b], aboveSP));
					}
				}
			}

			if (currWaveNum >= 0) //------------------- CURRWAVEINFO (at screen top center)
			{
				int waveGroupsTotal = map.Waves[currWaveNum].Groups.Length;
				Vector2 firstBoxPos = new Vector2(CurrentGame.graphicsDevice.Viewport.Width * 0.5f - waveGroupsTotal * boxWidth * 0.5f, 50);
				for (int i = 0; i < waveGroupsTotal; i++)
				{
					CurrWaveInfoBoxes.Add(new GroupInfoBox(firstBoxPos + new Vector2(i * boxWidth, 0), map.Waves[currWaveNum].Groups[i], false));
				}
			}
		}

		void OpenTilering(bool onOpenTile)
		{
			activeTileCoord = new Point(hoveredCoord.X, hoveredCoord.Y);
			Mouse.SetPosition((int)activeTilePos.X, (int)activeTilePos.Y);
            CurrentGame.soundEffects[0].Play(0.18f,0,0);
			selectedRingPart = 0;
            if (!onOpenTile)
                TileRingInfoBox = new TowerInfoBox(selectedTower, activeTilePos - new Vector2(TowerInfoBox.DefaultWidth * 0.5f, -(ParentMap.TileHalfHeight + tileringSlot.Height * 0.5f)), false);
            ///TileRingInfoBox.Pos = activeTilePos - new Vector2(TowerInfoBox.DefaultWidth * 0.5f, -(ParentMap.TileHalfHeight + tileringSlot.Height * 0.5f));
            tileCorners = new Vector2[7] { activeTilePos,   //Center
													new Vector2(activeTilePos.X - ParentMap.TileWidth/4 -1, activeTilePos.Y - ParentMap.TileHalfHeight), //up-L
													new Vector2(activeTilePos.X + ParentMap.TileWidth/4 +1 /* +1 jotta ei näy upgia mietties kokoajan vanha range*/, activeTilePos.Y - ParentMap.TileHalfHeight),  //up-R
													new Vector2(activeTilePos.X + ParentMap.TileHalfWidth /* ennen 28.5.16 -1, mut vaihto jotta ei näy upgia mietties kokoajan vanha range*/, activeTilePos.Y),    //R
													new Vector2(activeTilePos.X + ParentMap.TileWidth/4, activeTilePos.Y + ParentMap.TileHalfHeight),    //lo-R
													new Vector2(activeTilePos.X - ParentMap.TileWidth/4 -1, activeTilePos.Y + ParentMap.TileHalfHeight), //lo-L
													new Vector2(activeTilePos.X - ParentMap.TileHalfWidth, activeTilePos.Y)};                            //L

			for (int i = 0; i < 7; i++)
				extendedPoints[i] = tileCorners[i];
			extendedPoints[7] = new Vector2(tileCorners[2].X - 13, tileCorners[2].Y - ParentMap.TileHalfHeight);
			extendedPoints[8] = new Vector2(tileCorners[2].X + ParentMap.TileHalfWidth / 2, tileCorners[2].Y - ParentMap.TileHalfHeight);
			extendedPoints[9] = new Vector2(tileCorners[2].X + 31, tileCorners[2].Y - 3);
			extendedPoints[10] = new Vector2(tileCorners[3].X + ParentMap.TileHalfWidth * 0.75f, tileCorners[3].Y - ParentMap.TileHalfHeight * 0.5f);
			extendedPoints[11] = new Vector2(tileCorners[3].X + ParentMap.TileHalfWidth * 0.75f, tileCorners[3].Y + ParentMap.TileHalfHeight * 0.5f);
			extendedPoints[12] = new Vector2(tileCorners[4].X + tileringSlot.Width - 5, tileCorners[4].Y + 15);
			extendedPoints[13] = new Vector2(tileCorners[4].X, tileCorners[4].Y + tileringSlot.Width - 1);
			extendedPoints[14] = new Vector2(tileCorners[5].X, tileCorners[5].Y + tileringSlot.Width - 1);
			extendedPoints[15] = new Vector2(tileCorners[5].X - tileringSlot.Width + 5, tileCorners[5].Y + 15);
			extendedPoints[16] = new Vector2(tileCorners[6].X - ParentMap.TileHalfWidth * 0.75f, tileCorners[6].Y + ParentMap.TileHalfHeight * 0.5f);
			extendedPoints[17] = new Vector2(tileCorners[6].X - ParentMap.TileHalfWidth * 0.75f, tileCorners[6].Y - ParentMap.TileHalfHeight * 0.5f);
			extendedPoints[18] = new Vector2(tileCorners[1].X - tileringSlot.Width + 5, tileCorners[1].Y - 15);
			extendedPoints[19] = new Vector2(tileCorners[1].X, tileCorners[1].Y - tileringSlot.Width);

			//hudCue = CurrentGame.soundBank.GetCue("bui");
			//hudCue.Play();
		}

		#region UPDATEVARIABLES
		int tabRefreshCounter;
        int dirKeyRefreshCounter;
        int oldActive;
        int newActive;
        bool waitingForSaveChoice;
        bool cellActive;
        string fileName;
		//Vector2 dirFromTile;  //-..................uutta kaggaa
		Creature hoveredCreature;
        SpawnGroup hoveredGroup;
        public int bugHoverCounter;
		public const int bugHoverFade = 120;
		#endregion
		public void Update(MouseState mouse, MouseState prevMouse, KeyboardState keyboard, KeyboardState prevKeyboard)
        {
            hoveredCoord = ParentMap.ToMapCoordinate(mouse.X, mouse.Y);
            hoveredTilePos = ParentMap.ToScreenLocation(hoveredCoord);

            if (CurrentGame.gameState != GameState.MapEditor)
                #region BASIC HUD
            {

                for (int i = 0; i < HUDbuttons.Length; i++)
                    HUDbuttons[i].Update(mouse, prevMouse);

                //INITSETUP WAVEINFOBOXES
                if (CurrentGame.gameState == GameState.InitSetup || CurrentGame.gameState == GameState.MapTestInitSetup/*!initSetupOn*/)
                {
                    bool isNoneLocked = true;
                    for (int w = 0; w < ParentMap.Waves.Length; w++)
                    {   for (int g = 0; g < ParentMap.Waves[w].Groups.Length; g++)
                        {
                            SpawnGroup spwnG = ParentMap.Waves[w].Groups[g];
                            if (spwnG.BugBox.locked || spwnG.BugBox.hoveredOver)
                            {
                                foreach (SpawnGroup sg in ParentMap.Waves[w].Groups)
                                {
                                    if (!sg.BugBox.locked)
                                        sg.ShowingPath = false;
                                }
                                spwnG.ShowingPath = true;
                                isNoneLocked = false;
                                continue;
                            }
                            if (isNoneLocked)
                                spwnG.ShowingPath = true;
                            else
                                spwnG.ShowingPath = false;
                        }
                    }
                }
                else //INGAME WAVEINFOBOXES
                {
                    //---------------------------------------------------------------eikös tänkin vois laittaa on-demand, öröjen vastuulle
                    bool PKeyDown = CurrentGame.keyboard.IsKeyDown(Keys.P);
                    for (int w = 0; w < ParentMap.Waves.Length; w++)
                    {   for (int g = 0; g < ParentMap.Waves[w].Groups.Length; g++)
                        {
                            SpawnGroup spwnG = ParentMap.Waves[w].Groups[g];
                            if (!spwnG.BugBox.locked)
                                spwnG.ShowingPath = false;
                            if (PKeyDown)
                                spwnG.ShowingPath = true;

                            if (spwnG.BugBox.locked || spwnG.BugBox.hoveredOver)
                            {
                                foreach (SpawnGroup sg in ParentMap.Waves[w].Groups)
                                {
                                    if (!sg.BugBox.locked)
                                        sg.ShowingPath = false;
                                }
                                spwnG.ShowingPath = true;
                                continue;
                            }
                        }
                    }
                }

                #region MAPTEST
                if (CurrentGame.gameState == GameState.MapTestInGame)
                {
                    BackToEditButton.Update(mouse, prevMouse);
                    if (BackToEditButton.State == ButnState.Released)
                    {
                        MapEditorTopButtons[1].Pos = mapEditWTEditPos;
                        ParentMap.ResetMap();
						UpdateGeneBarsMAPEDIT();
                        CurrentGame.gameState = GameState.MapEditor;
                    }
                    MapEditorTopButtons[1].Update(mouse, prevMouse);

                    if (keyboard.IsKeyDown(Keys.F1) && prevKeyboard.IsKeyUp(Keys.F1))
                        MapEditorTopButtons[1].State = MapEditorTopButtons[1].State == ButnState.Active ? ButnState.Passive : ButnState.Active;
				}
				#endregion

				#region NEXTWAVE & RESTART
				if (HUDbuttons[0].State == ButnState.Released || (keyboard.IsKeyDown(Keys.Space) && !prevKeyboard.IsKeyDown(Keys.Space)))
                {
                    if (ParentMap.currentWave == -1)
                    {
                        ParentMap.currentWave = 0;
						HUD.UpdateWaveInfo();
                        HUDbuttons[0].Text = "Send next wave";
                        //ButtonWords[0] = "Send next wave";
                        //ParentMap.initSetupOn = false;
                        if (CurrentGame.gameState == GameState.MapTestInitSetup)
                            CurrentGame.gameState = GameState.MapTestInGame;
                        else
                            CurrentGame.gameState = GameState.InGame;

      //                  for (int w = 0; w < ParentMap.Waves.Length; w++)
						//{   for (int g = 0; g < ParentMap.Waves[w].Groups.Length; g++)
						//	{
      //                          ParentMap.Waves[w].Groups[g].ShowingPath = false;
      //                          //for (int c = 0; c < ParentMap.Waves[w].Groups[g].Creatures.Length; c++)
      //                          //{
      //                          //	ParentMap.Waves[w].Groups[g].Creatures[c].ShowingPath = false;
      //                          //}
      //                      }
						//}
                    }
                    else if (ParentMap.currentWave + 1 < ParentMap.Waves.Length)
                    {
						ParentMap.currentWave++;
						HUD.UpdateWaveInfo();
						if (ParentMap.currentWave == ParentMap.Waves.GetUpperBound(0))
                            HUDbuttons[0].Text = "Final Wave";
                        //ButtonWords[0] = "Final Wave";
                    }
                }
                else if (HUDbuttons[1].State == ButnState.Released)
                    ParentMap.ResetMap();
				#endregion

				#region WAVEINFOBOXES
				for (int i = 0; i < CurrWaveInfoBoxes.Count; i++)
					CurrWaveInfoBoxes[i].Update(mouse, prevMouse);
				for (int i = 0; i < NexWaveInfoBoxes.Count; i++)
					 NexWaveInfoBoxes[i].Update(mouse, prevMouse);

				#endregion

				#region MOVING_INFOBOXES
				for (int i = 0; i < ParentMap.AliveCreatures.Count; i++)
				{
					Creature currentCreature = ParentMap.AliveCreatures[i];
                    //currentCreature.ParentGroup.ShowingPath = false;
                    if (Vector2.Distance(mousePos, currentCreature.Location) <= 15)
                    {
                        if (BugBoxes.Count > 0)
                        {
                            if (!BugBoxes[0].locked)
                                BugBoxes[0] = new BugInfoBox(currentCreature.Location - new Vector2(66, 20), currentCreature, true, false, null);
                            else if (BugBoxes[0].Target != currentCreature)
                                BugBoxes.Insert(0, new BugInfoBox(currentCreature.Location - new Vector2(66, 20), currentCreature, true, false, null));
                        }
                        else
                            BugBoxes.Add(new BugInfoBox(currentCreature.Location - new Vector2(66, 20), currentCreature, true, false, null));


                        hoveredCreature = currentCreature;
                        hoveredGroup = hoveredCreature.ParentGroup;
                        hoveredCreature.ParentGroup.ShowingPath = true;

                        //hoveredCreature.ParentGroup.showPathFade = hoveredCreature.ParentGroup.showPathFadeCycles;
                        bugHoverCounter = bugHoverFade;
                        if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
                        {
                            BugBoxes[0].locked = !BugBoxes[0].locked;
                            BugBoxes[0].justRemoteLocked = true;
                            break;
                        }
                    }
                    else
                        hoveredCreature = null;
				}

                for (int i = 0; i < BugBoxes.Count; i++)
				{
					if (BugBoxes[i].locked)
					{
						BugBoxes[i].Pos = BugBoxes[i].Target.Location;
						BugBoxes[i].Update(mouse, prevMouse);
                        BugBoxes[i].Target.ParentGroup.ShowingPath = true;
						if (!BugBoxes[i].locked)
						{
                            //BugBoxes[i].Target.ParentGroup.ShowingPath = false;
							BugBoxes.Remove(BugBoxes[i]);
						}
					}
					else
					{
						BugBoxes[i].Pos = BugBoxes[i].Target.Location;
						bugHoverCounter--;
                        if (bugHoverCounter <= 0)
                        {
                            BugBoxes[i].Target.ParentGroup.ShowingPath = false;
                            BugBoxes.Remove(BugBoxes[i]);
                        }
                        else
                        {
                            BugBoxes[i].Update(mouse, prevMouse);
                            //BugBoxes[i].Target.ParentGroup.ShowingPath = BugBoxes[i].hoveredOver;
                        }
                    }
				}
                #endregion

                // TÄNNE KUULUISI MOUSEUPDATELOGIIKAT (erottelua tiedossa, Drawit pois ja kommunikaatio siihen luuppiin)
            }
                #endregion
            else
                #region MAP EDITOR
            {
                

                #region ---------------------------   BUTTON UPDATES   --------------------------------
                #region MENU BUTTONS
                for (int m = 0; m < MapEditorMenuButtons.Length; m++)
                {
                    MapEditorMenuButtons[m].Update(mouse, prevMouse);

                    if (keyboard.IsKeyDown(Keys.F5) && prevKeyboard.IsKeyUp(Keys.F5)) // F5 starts mapTest
                        MapEditorMenuButtons[0].State = ButnState.Released;

                    if (MapEditorMenuButtons[m].State == ButnState.Released)
                    {
                        switch (m)
						{
							case 0: 
								    #region LOAD TEST
									ParentMap.SpawnPoints = MapEditorSpawnPoints.ToArray(); // case 0: TEST
                                    ParentMap.GoalPoints = MapEditorGoalPoints.ToArray();
                                    Array.Copy(ParentMap.Layout, ParentMap.InitLayout, ParentMap.Layout.Length);
                                    ParentMap.Pathfinder.InitializeTiles();

                                    if (ValidateMap()) //--------------------------------table cell info (at least dur) doesn't refresh---OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOÖÖÖÖÖÖÖLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLL>>>>>>>>>>>>>>>>
                                    {
                                        int groupRow = 0;
                                        ParentMap.Waves = ParentMap.MapEditorTempWaves.ToArray();
                                        for (int w = 0; w < ParentMap.Waves.Length; w++)
                                        {
                                            ParentMap.Waves[w].Groups = ParentMap.MapEditorTempWaves[w].TempGroups.ToArray();
                                            for (int g = 0; g < ParentMap.MapEditorTempWaves[w].Groups.Length; g++)
                                            {
                                                int indexRow = groupRow * (MapEditorTableCols - 1);

												int creAmt = MapEditorTableCells[0 + indexRow].Text == "" ? 0 : int.Parse(MapEditorTableCells[0 + indexRow].Text);
												string type = MapEditorTableCells[1 + indexRow].Text;
												string name = MapEditorTableCells[2 + indexRow].Text;
												string texName = MapEditorTableCells[3 + indexRow].Text;
												int spIndex = MapEditorTableCells[4 + indexRow].Text == "" ? 0 : int.Parse(MapEditorTableCells[4 + indexRow].Text) - 1;
												int gpIndex =  MapEditorTableCells[5 + indexRow].Text == "" ? 0 : (int)char.Parse(MapEditorTableCells[5 + indexRow].Text) - 97; //Goalpoint
												int hp = MapEditorTableCells[6 + indexRow].Text == "" ? 0 : int.Parse(MapEditorTableCells[6 + indexRow].Text);
												float spd = MapEditorTableCells[7 + indexRow].Text == "" ? 0 : float.Parse(MapEditorTableCells[7 + indexRow].Text, System.Globalization.NumberFormatInfo.InvariantInfo);
												byte ldmg = MapEditorTableCells[8 + indexRow].Text == "" ? (byte)0 : byte.Parse(MapEditorTableCells[8 + indexRow].Text);
												int nrg = MapEditorTableCells[9 + indexRow].Text == "" ? 0 : int.Parse(MapEditorTableCells[9 + indexRow].Text);
												float rArm = MapEditorTableCells[10 + indexRow].Text == "" ? 0 : float.Parse(MapEditorTableCells[10 + indexRow].Text, System.Globalization.NumberFormatInfo.InvariantInfo);
												float gArm = MapEditorTableCells[11 + indexRow].Text == "" ? 0 : float.Parse(MapEditorTableCells[11 + indexRow].Text, System.Globalization.NumberFormatInfo.InvariantInfo);
												float bArm = MapEditorTableCells[12 + indexRow].Text == "" ? 0 : float.Parse(MapEditorTableCells[12 + indexRow].Text, System.Globalization.NumberFormatInfo.InvariantInfo);
												GeneSpecs armors = new GeneSpecs(rArm, gArm, bArm);
												int spwnFrq = MapEditorTableCells[13 + indexRow].Text == "" ? 0 : int.Parse(MapEditorTableCells[13 + indexRow].Text);
												int spwnDur = MapEditorTableCells[14 + indexRow].Text == "" ? 0 : int.Parse(MapEditorTableCells[14 + indexRow].Text);

                                                ParentMap.Waves[w].Groups[g] = new SpawnGroup(creAmt, new Creature(type, name, ParentMap, texName, spIndex, gpIndex, hp, spd, armors, ldmg, nrg, 1f), spwnFrq, spwnDur, g, w);
                                                                                                            
                                                groupRow++;
                                            }

                                            ParentMap.Waves[w].Initialize();
                                        }
                                        ParentMap.PlayerInitEnergy = int.Parse(MapEditorResourceCells[0].Text);
										ParentMap.PlayerInitGenePoints = new int[] { int.Parse(MapEditorResourceCells[1].Text), int.Parse(MapEditorResourceCells[2].Text), int.Parse(MapEditorResourceCells[3].Text) };
                                        ParentMap.PlayerInitLife = short.Parse(MapEditorResourceCells[4].Text);

										for (int i = 0; i < AvailableTowersCells.Length; i++)
											byte.TryParse(AvailableTowersCells[i].Text, out ParentMap.AvailableTowers[i]);

                                        //ParentMap.SaveMap(CurrentGame.MapDir + "temp.txt");
                                        //EditorMapLoad("temp");
                                        ParentMap.ResetMap();
                                        MapEditorTopButtons[1].Pos = mapTestWTEditPos;
                                    }   break;
							#endregion
							case 1:
								    #region SAVE IF NOT EXISTING
								if (MapNameBox.Text == "")
                                        fileName = "unnamed";
                                    else fileName = MapNameBox.Text;

                                    if (File.Exists(CurrentGame.MapDir + fileName + ".txt"))
                                    {
                                        MapEditorMenuButtons[1].IsDropDownMenu = true;
                                        MapEditorMenuButtons[1].State = ButnState.Active;
                                        waitingForSaveChoice = true;
                                        break;
                                        //MapEditorMenuButtons[1].WaitForDecision(); //----------------------------------------------------------------------how please. bool waiting? events? multi-threading?
                                    }
                                    ParentMap.SaveMap(CurrentGame.MapDir + fileName + ".txt");

                                    break;
								#endregion
                            case 2: break; // LOAD MAP HAPPENS BELOW (oh why...
                            case 3: ParentGame.mainMenu.RefreshMapData();
                                    ParentGame.mainMenu.menuState = MainMenu.MenuState.Main;
                                    CurrentGame.gameState = GameState.MainMenu; break;
                            case 4: ParentGame.Exit(); break;
                        }
					}
					if (m == 1 && waitingForSaveChoice)
					#region SAVE IF EXISTING
					{
                        for (int i = 0; i < MapEditorMenuButtons[1].DropDownButtons.Length; i++)
                        {
                            if (MapEditorMenuButtons[1].DropDownButtons[i].State == ButnState.Released)
                            {
                                if (i == 1)
                                {
                                    int existingNumber = Directory.GetFiles(CurrentGame.MapDir, fileName + "*").Length;
                                    fileName += "(" + existingNumber + ")";
                                }
                                ParentMap.SaveMap(CurrentGame.MapDir + fileName + ".txt");
                                fileName = "";
                                MapEditorMenuButtons[1].IsDropDownMenu = false;
                                waitingForSaveChoice = false;
                                MapEditorMenuButtons[1].DropDownButtons[i].State = ButnState.Passive; //----------HYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYI
                            }
                        }
					}
					#endregion
					else if (m == 2)
					#region LOAD MAP
					{
                        for (int i = 0; i < MapEditorMenuButtons[2].DropDownButtons.Length; i++)
                        {
                            if (MapEditorMenuButtons[2].DropDownButtons[i].State == ButnState.Released)
                            {
                                EditorMapLoad(MapEditorMenuButtons[2].DropDownButtons[i]);
                                MapEditorMenuButtons[2].DropDownButtons[i].State = ButnState.Passive; //----------HYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYI
                            }
                        }
                        if (MapEditorMenuButtons[2].State == ButnState.Active && MapEditorMenuButtons[2].DropDownButtons != null)
                            TableBackground5 = new Rectangle(MapEditorMenuButtons[2].DropDownButtons[0].Pos.X, MapEditorMenuButtons[2].DropDownButtons[0].Pos.Y, MapEditorMenuButtons[2].DropDownButtons[0].Width, MapEditorMenuButtons[2].DropDownButtons[0].Height * MapEditorMenuButtons[2].DropDownButtons.Length);
					}
					#endregion
				}
				#endregion

				#region TOOL BUTTONS
				for (int i = 0; i < MapEditorToolButtons.Length; i++)
                {
                    MapEditorToolButtons[i].Update(mouse, prevMouse);
                    if (MapEditorToolButtons[i].State == ButnState.Active)
                        drawMode = (DrawMode)i;
                }

                if (mouse.ScrollWheelValue != prevMouse.ScrollWheelValue)
                {
                    MapEditorToolButtons[(int)drawMode].State = ButnState.Passive;
                    if (mouse.ScrollWheelValue - prevMouse.ScrollWheelValue < 0 && (int)drawMode < 3)
                        drawMode++;
                    else if (mouse.ScrollWheelValue - prevMouse.ScrollWheelValue > 0 && (int)drawMode > 0)
                        drawMode--;
                    MapEditorToolButtons[(int)drawMode].State = ButnState.Active;
				}
				#endregion

				#region TOP BUTTONS
				for (int i = 0; i < MapEditorResourceCells.Length; i++)
                    MapEditorResourceCells[i].Update(mouse, prevMouse, keyboard, prevKeyboard);

                MapNameBox.Update(CurrentGame.mouse, CurrentGame.prevMouse, CurrentGame.keyboard, CurrentGame.prevKeyboard);

                for (int i = 0; i < MapEditorTopButtons.Length; i++)
                {
                    MapEditorTopButtons[i].Update(mouse, prevMouse);
                    if (MapEditorTopButtons[i].State == ButnState.Released)
                    {
						if (i == 0)
							MapNameBox.State = ButnState.Active;
						else if (i == 1)
							inWaveEdit = !inWaveEdit;
						else if (i == 2)
							MapEditorResourceCells[0].State = ButnState.Active;
						else if (i == 4)
							MapEditorResourceCells[4].State = ButnState.Active;
                    }
				}
				#endregion

				for (int i = 0; i < AvailableTowersCells.Length; i++)
                    AvailableTowersCells[i].Update(mouse, prevMouse, keyboard, prevKeyboard);

                #endregion

                #region --------------------------- ON ANY MOUSE CLICK ---------------------------
                /*if (BoundsList.Exists(rec => rec.Contains(mouse.X, mouse.Y)))
                {
                    oldActive = oldActive;
                }*/


                if (mouse.LeftButton == ButtonState.Released && prevMouse.LeftButton == ButtonState.Pressed)
                {
                    if (MapEditorTopButtons[1].State == ButnState.Passive && MapEditorTopButtons[1].PrevState == ButnState.Active &&
                        (TableBackground.Contains(mouse.X, mouse.Y) ||
                         TableBackground1.Contains(mouse.X, mouse.Y) ||
                         TableBackground2.Contains(mouse.X, mouse.Y) ||
                         TableBackground3.Contains(mouse.X, mouse.Y) ||
                         TableBackground4.Contains(mouse.X, mouse.Y) ||
                         TableBackground5.Contains(mouse.X, mouse.Y)))
                    {
                        MapEditorTopButtons[1].State = ButnState.Active;
                    }

                    TableBackground4 = Rectangle.Empty;
                    TableBackground5 = Rectangle.Empty;

                    if (MapEditorToolButtons[(int)drawMode].State != ButnState.Active)
                        MapEditorToolButtons[(int)drawMode].State = ButnState.Active;

                    if (!MapEditorMenuButtons[0].Bounds.Contains(mouse.X, mouse.Y))
                        ErrorShow = false;
                }
                #endregion

				#region ----------------------------   WAVE TABLE  -----------------------------------

				if (keyboard.IsKeyDown(Keys.F1) && prevKeyboard.IsKeyUp(Keys.F1))
                    MapEditorTopButtons[1].State = MapEditorTopButtons[1].State == ButnState.Active ? ButnState.Passive : ButnState.Active;

                if (MapEditorTopButtons[1].State == ButnState.Active) //---------------------------------------WAVE TABLE OPEN-------.
                {
                    //for (int i = 0; i < MapEditorLabelButtons.Length; i++) //COLUMN LABEL UPDATES----------
                     //   MapEditorLabelButtons[i].Update(mouse, prevMouse);

                    #region ---------------------------   CELL UPDATE/SELECTION  --------------------------------
                    if (keyboard.IsKeyUp(Keys.Tab)) tabRefreshCounter = 30;
                    if (keyboard.IsKeyUp(Keys.Up) && keyboard.IsKeyUp(Keys.Right) && keyboard.IsKeyUp(Keys.Down) && keyboard.IsKeyUp(Keys.Left))
                        dirKeyRefreshCounter = 30;


                    bool overlapRisk = cellActive && (oldActive % (MapEditorTableCols - 1) == 3 || oldActive % (MapEditorTableCols - 1) == 4 || oldActive % (MapEditorTableCols - 1) == 5);
                    cellActive = false;
                    for (int i = 0; i < totalGroups * (MapEditorTableCols - 1); i++) //CELLS--------------------
                    {
                        if (overlapRisk && i > oldActive && i % (MapEditorTableCols - 1) == oldActive % (MapEditorTableCols - 1))
                            continue;
                        MapEditorTableCells[i].Update(mouse, prevMouse, keyboard, prevKeyboard);

                        /*if (MapEditorTableCells[i].State == ButnState.Pressed) //----------------TODO: drag selection--------------!
                        {
                            MapEditorTableCells[i].State = ButnState.Selected;
                        }
                        else */
                        if (MapEditorTableCells[i].State == ButnState.Active)
                        {
                            cellActive = true;
                            oldActive = i;
                            ErrorShow = false;
                            ErrorButtons.Clear();
                        }

                        if (MapEditorTableCells[i].State == ButnState.Released)
						{
							#region MULTIPLE CELL SELECT
							if (keyboard.IsKeyDown(Keys.LeftShift))
                            {
                                int startingCol = Math.Min(i % (MapEditorTableCols - 1), oldActive % (MapEditorTableCols - 1));
                                int startingRow = Math.Min(i, oldActive) / (MapEditorTableCols - 1);
                                int startingCell = startingCol + (startingRow * (MapEditorTableCols - 1));
                                int xRange = Math.Abs((i % (MapEditorTableCols - 1) - oldActive % (MapEditorTableCols - 1))) + 1;
                                int rows = Math.Abs(startingRow - Math.Max(i, oldActive) / (MapEditorTableCols - 1)) + 1;

                                for (int k = startingCell; k < xRange * rows + startingCell; k++)
                                {
                                    if (keyboard.IsKeyDown(Keys.LeftAlt))
                                        MapEditorTableCells[(k % xRange + startingCell + ((k - startingCell) / xRange * (MapEditorTableCols - 1)))].State = ButnState.Active;
                                    else MapEditorTableCells[k % xRange + startingCell + ((k - startingCell) / xRange * (MapEditorTableCols - 1))].State = ButnState.Selected;
                                }

                                /*int j = i; //----------------Shift-click select all in between
                                int dir = i-oldActive > 0 ? -1 : 1;
                                while (true)
                                {
                                    if (keyboard.IsKeyDown(Keys.LeftAlt))
                                        MapEditorTableCells[j].State = ButnState.Active;
                                    else MapEditorTableCells[j].State = ButnState.Selected;
                                    if (j == oldActive)
                                        break;
                                    j += dir;
                                }         */
							}
							else if (keyboard.IsKeyDown(Keys.LeftControl) && keyboard.IsKeyUp(Keys.LeftAlt))
                            {
                                if (MapEditorTableCells[oldActive].PrevState == ButnState.Active)
                                    MapEditorTableCells[oldActive].State = ButnState.Selected;
							}
							#endregion

							if (MapEditorTableCells[i].IsDropDownMenu && MapEditorTableCells[i].PrevState == ButnState.Passive && MapEditorTableCells[i].DropDownButtons != null)
                                TableBackground4 = new Rectangle(MapEditorTableCells[i].DropDownButtons[0].Pos.X, MapEditorTableCells[i].DropDownButtons[0].Pos.Y, MapEditorTableCells[i].DropDownButtons[0].Width, MapEditorTableCells[i].DropDownButtons[0].Height * MapEditorTableCells[i].DropDownButtons.Length);
                            else TableBackground4 = Rectangle.Empty;
                        }
                    }

                    if (tabRefreshCounter == 29 || tabRefreshCounter == 0 || dirKeyRefreshCounter == 29 || dirKeyRefreshCounter == 0)//----------------NAVIGATION BY TAB AND DIRECTION KEYS
                    {
                        if (oldActive > 0 && ((keyboard.IsKeyDown(Keys.Tab) && keyboard.IsKeyDown(Keys.LeftShift)) || (keyboard.IsKeyDown(Keys.Left) && oldActive % (MapEditorTableCols - 1) != 0)))
                            newActive = oldActive - 1;
                        else if (oldActive < totalGroups * (MapEditorTableCols - 1) - 1 && ((keyboard.IsKeyDown(Keys.Tab) && keyboard.IsKeyUp(Keys.LeftShift)) || (keyboard.IsKeyDown(Keys.Right) && oldActive % (MapEditorTableCols - 1) < (MapEditorTableCols - 2))))
                            newActive = oldActive + 1;
                        else if (oldActive >= (MapEditorTableCols - 1) && (keyboard.IsKeyDown(Keys.Up)) && !MapEditorTableCells[oldActive].IsDropDownMenu)
                            newActive = oldActive - (MapEditorTableCols - 1);
                        else if (oldActive < (totalGroups - 1) * (MapEditorTableCols - 1) && (keyboard.IsKeyDown(Keys.Down)) && !MapEditorTableCells[oldActive].IsDropDownMenu)
                            newActive = oldActive + (MapEditorTableCols - 1);

                        MapEditorTableCells[oldActive].State = ButnState.Passive;
                        MapEditorTableCells[newActive].State = ButnState.Active;
                        if (tabRefreshCounter == 0) tabRefreshCounter = 5;
                        if (dirKeyRefreshCounter == 0) dirKeyRefreshCounter = 5;
                    }
                    #endregion

                    #region ---------------------------   DELETE ROW   --------------------------------
                    for (int i = 0; i < totalGroups; i++) //----------DELETE ROW---------------------------.
                    {
                        MapEditorTableDelButtons[i].Update(mouse, prevMouse);
                        if (MapEditorTableDelButtons[i].State == ButnState.Released)
                        {
                            int waveIndex = 0;
                            int groupIndex = i;
                            while (groupIndex >= ParentMap.MapEditorTempWaves[waveIndex].TempGroups.Count)
                            {
                                groupIndex -= ParentMap.MapEditorTempWaves[waveIndex].TempGroups.Count;
                                waveIndex++;
                            }
                            ParentMap.MapEditorTempWaves[waveIndex].TempGroups.RemoveAt(groupIndex);
                            if (ParentMap.MapEditorTempWaves[waveIndex].TempGroups.Count == 0)
                                ParentMap.MapEditorTempWaves.RemoveAt(waveIndex);

                            for (int j = i * (MapEditorTableCols - 1); j < (MapEditorTableCols - 1) * (MapEditorTableRows - 1); j++)
                            {
                                MapEditorTableCells[j].Text = MapEditorTableCells[j + (MapEditorTableCols - 1)].Text;
                                if (j % (MapEditorTableCols - 1) == 7) //-------------move element color
                                    MapEditorTableCells[j].ButtonColors = MapEditorTableCells[j + (MapEditorTableCols - 1)].ButtonColors;
                            }
                            MapEditorWaveLabels[totalGroups - 1].Text = "Add";
                            MapEdited = true;
                        }
                        else if (MapEditorTableDelButtons[i].State == ButnState.Hovered)
                        {
                            for (int k = 0; k < MapEditorTableCols - 1; k++)
                            {
                                if (MapEditorTableCells[k + i * (MapEditorTableCols - 1)].State != ButnState.Active)
                                    MapEditorTableCells[k + i * (MapEditorTableCols - 1)].State = ButnState.Hovered;
                            }
                        }
                    }
                    #endregion

                    #region ---------------------------   WAVE ADD   --------------------------------
                    MapEditorWaveLabels[totalGroups].Update(mouse, prevMouse);
                    if (MapEditorWaveLabels[totalGroups].State == ButnState.Released && totalGroups < MapEditorTableRows - 1)
                    {
                        Wave newWave = new Wave(ParentMap);
                        ParentMap.MapEditorTempWaves.Add(newWave);
                        newWave.TempGroups = new List<SpawnGroup>();
                        newWave.TempGroups.Add(new SpawnGroup());
                        MapEditorWaveLabels[totalGroups].Text = "Wave " + ParentMap.MapEditorTempWaves.Count;
                        MapEditorWaveLabels[totalGroups + 1].Text = "Add";
                        MapEdited = true;
                    }
                    #endregion

                    #region ---------------------------   GROUP ADD   --------------------------------
                    if (totalGroups != 0)
                    {
                        for (int i = 0; i < totalGroups + 1; i++)
                        {
                            MapEditorTableAddButtons[i].Update(mouse, prevMouse);
                            if (MapEditorTableAddButtons[i].State == ButnState.Released && totalGroups < MapEditorTableRows - 1)
                            {
                                int waveIndex = 0;
                                int groupIndex = i;
                                while (groupIndex > ParentMap.MapEditorTempWaves[waveIndex].TempGroups.Count)
                                {
                                    groupIndex -= ParentMap.MapEditorTempWaves[waveIndex].TempGroups.Count;
                                    waveIndex++;
                                }
                                ParentMap.MapEditorTempWaves[waveIndex].TempGroups.Insert(groupIndex, new SpawnGroup());

                                for (int j = (MapEditorTableCols - 1) * MapEditorTableRows - 1; j > (i + 1) * (MapEditorTableCols - 1) - 1; j--) // end-to-top -copy and reset
                                {
                                    MapEditorTableCells[j].Text = MapEditorTableCells[j - (MapEditorTableCols - 1)].Text;
                                    MapEditorTableCells[j - (MapEditorTableCols - 1)].Text = "";
                                    MapEditorTableCells[j].ButtonColors = MapEditorTableCells[j - (MapEditorTableCols - 1)].ButtonColors; //color copy (backward)
                                    MapEditorTableCells[j - (MapEditorTableCols - 1)].ButtonColors = j % (MapEditorTableCols - 1) % 2 == 0 ? (Color[])buttonColors.Clone() : (Color[])tableCellColors.Clone(); //color reset
                                }
                                //MapEditorTableCells[i * (MapEditorTableCols - 1) + 7].Text = "-";
                                //MapEditorTableCells[(i+1) * (MapEditorTableCols-1) +7].ButtonColors = MapEditorTableCells[i * (MapEditorTableCols-1) +7].ButtonColors;
                                //MapEditorTableCells[i * (MapEditorTableCols-1) +7].ButtonColors = tableCellColors;
                                MapEditorWaveLabels[totalGroups + 1].Text = "Add";
                                MapEdited = true;
                            }
                        }
                    }
                    #endregion

                    #region ---------------------------   CELL COPY   --------------------------------
                    if (keyboard.IsKeyDown(Keys.LeftControl))
                    {
                        if (keyboard.IsKeyDown(Keys.C) && prevKeyboard.IsKeyUp(Keys.C))
                        {
                            for (int i = 0; i < MapEditorTableCells.Length; i++)
                            {
                                if (MapEditorTableCells[i].State == ButnState.Active || MapEditorTableCells[i].State == ButnState.Selected)
                                    clipBoard[i] = i;
                                else clipBoard[i] = -1;
                            }
                        }
                        else if (keyboard.IsKeyDown(Keys.V) && prevKeyboard.IsKeyUp(Keys.V))
                        {
                            int trimStart = 0;
                            for (int i = 0; i < clipBoard.Length && clipBoard[i] < 0; i++)
                                trimStart++;

                            TextCell[] selectedCells = Array.FindAll<TextCell>(MapEditorTableCells, c => c.State == ButnState.Active || c.State == ButnState.Selected);

                            for (int C = 0; C < selectedCells.Length; C++)
                            {
                                for (int i = trimStart; i < MapEditorTableCells.Length - oldActive; i++)
                                {
                                    if (clipBoard[i] >= 0 &&
                                            !(i % (MapEditorTableCols - 1) == 7 && (i + selectedCells[C].Index - trimStart) % (MapEditorTableCols - 1) != 7) &&
                                            !(i % (MapEditorTableCols - 1) == 3 && (i + selectedCells[C].Index - trimStart) % (MapEditorTableCols - 1) != 3) &&
                                            !(i % (MapEditorTableCols - 1) != 3 && (i + selectedCells[C].Index - trimStart) % (MapEditorTableCols - 1) == 3))
                                    {
                                        MapEditorTableCells[i + selectedCells[C].Index - trimStart].Text = MapEditorTableCells[clipBoard[i]].Text;
                                        if (i % (MapEditorTableCols - 1) == 7 && (i + selectedCells[C].Index - trimStart) % (MapEditorTableCols - 1) == 7)
                                            MapEditorTableCells[i + selectedCells[C].Index - trimStart].ButtonColors = (Color[])MapEditorTableCells[clipBoard[i]].ButtonColors.Clone();
                                    }
                                }
                            }
                            MapEdited = true;
                        }
                    }
                    #endregion

                    #region ---------------------------   WAVELABEL UPDATE   --------------------------------
                    int labelPos = 0; //---------------------------WAVELABEL UPDATE-----
                    for (int i = 0; i < ParentMap.MapEditorTempWaves.Count; i++)
                    {
                        if (i == 0)
                            labelPos = 0;
                        else
                            labelPos += ParentMap.MapEditorTempWaves[i - 1].TempGroups.Count;
                        MapEditorWaveLabels[labelPos].Update(mouse, prevMouse);
                        if (MapEditorWaveLabels[labelPos].State == ButnState.Released)
                        {
                            for (int k = labelPos * (MapEditorTableCols - 1); k < labelPos * (MapEditorTableCols - 1) + ParentMap.MapEditorTempWaves[i].TempGroups.Count * (MapEditorTableCols - 1); k++)
                                MapEditorTableCells[k].State = ButnState.Selected;
                        }
                    }
                    #endregion

                    tabRefreshCounter--;
                    dirKeyRefreshCounter--;
                }//------------------------------------------------------------------------------------------------------------------'
				#endregion
				else
                {
                    #region ---------------------------   MAP EDITS   --------------------------------
					#region DRAWS
					// if not towermode && either mouse button pressed && cursor in bounds   (towermode-edit in Draw() -_-)
                    if (drawMode != DrawMode.Tower && (mouse.LeftButton == ButtonState.Pressed || mouse.RightButton == ButtonState.Pressed) && hoveredCoord.X >= 0 && hoveredCoord.X < ParentMap.Layout.GetLength(1) && hoveredCoord.Y >= 0 && hoveredCoord.Y < ParentMap.Layout.GetLength(0)) 
                    {
                        char hoveredChar = ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X];
                        if (hoveredChar >= '1' && hoveredChar <= '9') //ascii 49-57 = 1-9   --------------------   s/gpointtien & tornien overwrite
                            MapEditorSpawnPoints.Remove(hoveredCoord);
                        else if (hoveredChar >= 'a' && hoveredChar <= 'i') //ascii 97–105 = a–i
                            MapEditorGoalPoints.Remove(hoveredCoord);
                        else if (hoveredChar != '.' && hoveredChar != '\'' && hoveredChar != '0' && hoveredChar != ' ')
                        {
                            ParentMap.InitTowers.Remove(ParentMap.InitTowers.Find(tower => tower.MapCoord == hoveredCoord)); //--------------------------HELPPOO TÖRKEETÄ LAMBDAA
                            ParentMap.Players[0].Towers.Remove(ParentMap.Players[0].Towers.Find(t => t.MapCoord == hoveredCoord)); //--------------------HELPPOO TÖRKEETÄ LAMBDAA
                        }

                        if (mouse.LeftButton == ButtonState.Pressed)
                        {
                            switch (drawMode)
                            {
                                case DrawMode.Void: ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] = ' ';
                                    break;
                                case DrawMode.WallPath: ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] = '0';
                                    break;
                                case DrawMode.Point: if (MapEditorSpawnPoints.Count < 9) //single digits only (1–9)
                                    {
                                        ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] = (char)(MapEditorSpawnPoints.Count + '1');
                                        MapEditorSpawnPoints.Add(hoveredCoord);
                                    } break;
                            }
                        }
                        else if (mouse.RightButton == ButtonState.Pressed)
                        {
                            switch (drawMode)
                            {
                                case DrawMode.Void: break; //---------------------------------------------------------------mites tän right click -voidin kanssa?
                                case DrawMode.WallPath: ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] = hoveredCoord.X % 2 == 0 ? '\'' : '.';
                                    break;
                                case DrawMode.Point: if (MapEditorGoalPoints.Count < 9)
                                    {
                                        ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] = (char)(MapEditorGoalPoints.Count + 97);
                                        MapEditorGoalPoints.Add(hoveredCoord);
                                    } break;
                            }
                        }
                        MapEdited = true;
					}
					#endregion
					#region MoveTiles
					if (keyboard.IsKeyDown(Keys.LeftControl))
					{
						if (keyboard.IsKeyDown(Keys.NumPad4) && prevKeyboard.IsKeyUp(Keys.NumPad4))
						{
							for (int row = 0; row < ParentMap.Layout.GetLength(0) - 1; row++)
							{
								for (int col = 0; col < ParentMap.Layout.GetLength(1) - 1; col++)
								{
									char cToPaste = ParentMap.Layout[row + (col % 2), col + 1];
									ParentMap.Layout[row, col] = cToPaste;
									if (cToPaste >= '1' && cToPaste <= '9')
									{
										MapEditorSpawnPoints.Remove(new Point(col + 1, row + (col % 2)));
										MapEditorSpawnPoints.Add(new Point(col, row));
									}
									else if (cToPaste >= 'a' && cToPaste <= 'i')
									{
										MapEditorGoalPoints.Remove(new Point(col + 1, row + (col % 2)));
										MapEditorGoalPoints.Add(new Point(col, row));
									}
								}
							}
							for (int lastColRow = 0; lastColRow < ParentMap.Layout.GetLength(0); lastColRow++)
							{
								ParentMap.Layout[lastColRow, ParentMap.Layout.GetUpperBound(1)] = ' ';
							}
							MapEdited = true;
						}
						if (keyboard.IsKeyDown(Keys.NumPad6) && prevKeyboard.IsKeyUp(Keys.NumPad6))
						{
							for (int row = 1; row < ParentMap.Layout.GetLength(0) - 1; row++)
							{
								for (int col = ParentMap.Layout.GetUpperBound(1); col > 0; col--)
								{
									char cToPaste = ParentMap.Layout[row + (col % 2), col - 1];
									ParentMap.Layout[row, col] = cToPaste;
									if (cToPaste >= '1' && cToPaste <= '9')
									{
										MapEditorSpawnPoints.Remove(new Point(col - 1, row + (col % 2)));
										MapEditorSpawnPoints.Add(new Point(col, row));
									}
									else if (cToPaste >= 'a' && cToPaste <= 'i')
									{
										MapEditorGoalPoints.Remove(new Point(col - 1, row + (col % 2)));
										MapEditorGoalPoints.Add(new Point(col, row));
									}
								}
							}
							for (int firstColRow = 0; firstColRow < ParentMap.Layout.GetLength(0); firstColRow++)
							{
								ParentMap.Layout[firstColRow, 0] = ' ';
							}
							MapEdited = true;
						}
						if (keyboard.IsKeyDown(Keys.NumPad8) && prevKeyboard.IsKeyUp(Keys.NumPad8))
						{
							for (int row = 0; row < ParentMap.Layout.GetUpperBound(0); row++)
							{
								for (int col = 0; col < ParentMap.Layout.GetLength(1); col++)
								{
									char cToPaste = ParentMap.Layout[row + 1, col];
									ParentMap.Layout[row, col] = cToPaste;
									if (cToPaste >= '1' && cToPaste <= '9')
									{
										MapEditorSpawnPoints.Remove(new Point(col, row + 1));
										MapEditorSpawnPoints.Add(new Point(col, row));
									}
									else if (cToPaste >= 'a' && cToPaste <= 'i')
									{
										MapEditorGoalPoints.Remove(new Point(col, row + 1));
										MapEditorGoalPoints.Add(new Point(col, row));
									}
								}
							}
							for (int lastRowCol = 0; lastRowCol < ParentMap.Layout.GetLength(1); lastRowCol++)
							{
								ParentMap.Layout[ParentMap.Layout.GetUpperBound(0), lastRowCol] = ' ';
							}
							MapEdited = true;
						}
						if (keyboard.IsKeyDown(Keys.NumPad2) && prevKeyboard.IsKeyUp(Keys.NumPad2))
						{
							for (int row = ParentMap.Layout.GetUpperBound(0); row > 0; row--)
							{
								for (int col = 0; col < ParentMap.Layout.GetLength(1); col++)
								{
									char cToPaste = ParentMap.Layout[row - 1, col];
									ParentMap.Layout[row, col] = cToPaste;
									if (cToPaste >= '1' && cToPaste <= '9')
									{
										MapEditorSpawnPoints.Remove(new Point(col, row - 1));
										MapEditorSpawnPoints.Add(new Point(col, row));
									}
									else if (cToPaste >= 'a' && cToPaste <= 'i')
									{
										MapEditorGoalPoints.Remove(new Point(col, row - 1));
										MapEditorGoalPoints.Add(new Point(col, row));
									}
								}
							}
							for (int firstRowCol = 0; firstRowCol < ParentMap.Layout.GetLength(1); firstRowCol++)
							{
								ParentMap.Layout[0, firstRowCol] = ' ';
							}
							MapEdited = true;
						}
					#endregion
					#endregion
					}
				}

                //MapEdited = false; //törkeästi pakotettu wavelabeltextupdate
                #region --------------------------- ON ANY MAP CHANGE ---------------------------
                if (MapEdited)
                {
                    string[] numList = new string[MapEditorSpawnPoints.Count];
                    for (int i = 0; i < MapEditorSpawnPoints.Count; i++)
                        numList[i] = (i + 1).ToString();
                    if (numList.Length > 0)
                        for (int i = 0; i < MapEditorTableRows; i++)
                            MapEditorTableCells[4 + i * (MapEditorTableCols - 1)].PopulateDropDownMenu(numList);
					numList = new string[MapEditorGoalPoints.Count];
					for (int i = 0; i < MapEditorGoalPoints.Count; i++)
						numList[i] = ((char)(i + 97)).ToString();
					if (numList.Length > 0)
						for (int i = 0; i < MapEditorTableRows; i++)
							MapEditorTableCells[5 + i * (MapEditorTableCols - 1)].PopulateDropDownMenu(numList);

                    totalGroups = 0; //-----------------------------------------------------------------äijäiäi
                    for (int i = 0; i < ParentMap.MapEditorTempWaves.Count; i++)
                        totalGroups += ParentMap.MapEditorTempWaves[i].TempGroups.Count;

                    int labelPos = 0; //---------------------------WAVELABEL UPDATE-----TODO: select wave by clicking waveLabel
                    for (int i = 1; i < ParentMap.MapEditorTempWaves.Count; i++)
                    {
                        labelPos += ParentMap.MapEditorTempWaves[i - 1].TempGroups.Count;
                        MapEditorWaveLabels[labelPos].Text = "Wave " + (i + 1);
                        for (int j = labelPos * (MapEditorTableCols - 1); j < (labelPos + ParentMap.MapEditorTempWaves[i].TempGroups.Count) * (MapEditorTableCols - 1); j++)
                        {
                            if (j % (MapEditorTableCols - 1) != 7)
                            {
                                if (j % (MapEditorTableCols - 1) % 2 == 0)
                                    MapEditorTableCells[j].ButtonColors = new Color[] { buttonColors[0] * (1 - (i % 2 * 0.3f)), buttonColors[1] * (1 - (i % 2 * 0.3f)), buttonColors[2] * (1 - (i % 2 * 0.3f)) };
                                else MapEditorTableCells[j].ButtonColors = new Color[] { tableCellColors[0] * (1 - (i % 2 * 0.3f)), tableCellColors[1] * (1 - (i % 2 * 0.3f)), tableCellColors[2] * (1 - (i % 2 * 0.3f)) };
                            }
                        }
                    }

                    TableBackground.Height = tableButtonYDist * (totalGroups + 2);

                    MapEdited = false;
                }
                #endregion
            }
            #endregion
        }

		#region DRAWVARIABLES
		public bool newTileRingActive;
        public bool towerTileRingActive;
        public bool priorityRingActive;
        Vector2 activeTilePos;
        public Point activeTileCoord;
        int selectedRingPart;
        int prevRingPart;
        Vector2 mousePos;
        public Vector2 overlayPos;
        const float tileRingFadeCycles = 7;
        public const float hoverFadeCycles = 7;
		const int towerHoverCycles = 20;
        const int towerHoverPopUpDelayCycles = 15;
        int tileRingFade;
        public int tileHoverFade;
		float towerHoverCounter;
        Tower selectedTower;
		Vector2[] extendedPoints;
		Vector2[] tileCorners;
        private Vector2 LOSFromTilePos;
        private bool isTileChecked;
        private Vector2 LOSCollisionPos;
        private int LOSPixelDistanceCounter;

        public Vector2 PrevLOSCollisionPos { get; private set; }
        #endregion
        public void Draw(SpriteBatch sb, GameTime gameTime, MouseState mouse)
        {
            mousePos = new Vector2(mouse.X, mouse.Y);

            if (overlayPos == Vector2.Zero)
                overlayPos = new Vector2(mousePos.X, mousePos.Y);
            overlayPos += (hoveredTilePos - overlayPos) / 2.5f;
            //sb.DrawString(font, hoveredCoord.ToString(), mousePos - new Vector2(20, 20), Color.WhiteSmoke);
            //foreach (Tile t in ParentMap.)

            // InGame-------------------------------------------------------.
            if (CurrentGame.gameState != GameState.MapEditor) 
            {
                for (int i = 0; i < HUDbuttons.Length; i++)
                    HUDbuttons[i].Draw(sb);

                //-------!!
                #region OIKEESTI UPDATEEN KUULUVA MOUSELOGIC
                if (newTileRingActive || towerTileRingActive || priorityRingActive)
                {
                    if (tileRingFade < tileRingFadeCycles) 
						tileRingFade++;
                    ParentGame.IsMouseVisible = false;

                    //foreach (Vector2 point in tileCorners) sb.Draw(CurrentGame.pixel, point, null, new Color(150, 150, 150, 150));

                    #region TILERING LOGIC
                    #region NEW TOWER TILERING
                    if (newTileRingActive)  //----------------------------------------------------------------------------------------------------EIKÖS TÄMÄN PITÄISI OLLA UPDATESSA?!
                    {
						selectedRingPart = CheckNearestPoint(tileCorners);
						if (selectedRingPart > 0)
                        {
                            HexMap.ExampleTowers[selectedRingPart - 1].ShowRadius = true;
                            HexMap.ExampleTowers[selectedRingPart - 1].MapCoord = activeTileCoord;
                            if (ParentMap.AvailableTowers[selectedRingPart - 1] > 0)
                            {
                                HexMap.ExampleTowers[selectedRingPart - 1].Draw(sb);

								if (selectedRingPart != prevRingPart)
								{
									TileRingInfoBox = new TowerInfoBox(HexMap.ExampleTowers[selectedRingPart - 1], activeTilePos, true);
									TileRingInfoBox.Pos = activeTilePos - new Vector2(TowerInfoBox.DefaultWidth * 0.5f, -(ParentMap.TileHalfHeight + tileringSlot.Height * 0.5f));
								}

                                if (ParentMap.Players[0].EnergyPoints >= HexMap.ExampleTowers[selectedRingPart - 1].Cost) //-------------------------------tämä ehkä joku joskus harkittu visuaalibonus?
                                    sb.Draw(tileringGlow, tileCorners[selectedRingPart] - new Vector2(tileringGlow.Width / 2, tileringGlow.Height / 2), Color.White);
                                //else sb.Draw(ParentMap.tileTextures[5], tileCorners[selectedRingPart] - new Vector2(ParentMap.tileTextures[5].Width / 2, ParentMap.tileTextures[5].Height / 2), Color.Red);
                            }
                            else
                            {
                                TileRingInfoBox = new TileringInfoBox(activeTilePos, null, new string[]{ "Tower type", "not available" });
                                TileRingInfoBox.Pos = activeTilePos - new Vector2(TowerInfoBox.DefaultWidth * 0.5f, -(ParentMap.TileHalfHeight + tileringSlot.Height * 0.5f));
                            }
                        }
                        //Tähän ringFillit ennen tileringiä jotta afford-hohdot symbolien alle--------------------------------------------------------------------------------------------------------------------------------O
                        sb.Draw(ringFills[0], activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                        //sb.Draw(ringFills[1], activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);

                        sb.Draw(tilering, activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);

                        if (mouse.LeftButton == ButtonState.Released)
                        {
                            if (selectedRingPart > 0 && ParentMap.AvailableTowers[selectedRingPart -1] != 0)
                            {
                                if (ParentMap.Players[0].EnergyPoints >= HexMap.ExampleTowers[selectedRingPart -1].Cost)
                                {
                                    Tower t = Tower.Clone(HexMap.ExampleTowers[selectedRingPart -1]);
                                    t.MapCoord = activeTileCoord;
                                    ParentMap.BuildTower(t);
                                    CurrentGame.soundEffects[0].Play(0.18f, -1, 0);
                                    goto skip_TileringExitSound;
                                }
                            }

                            //Mouse.SetPosition((int)activeTilePos.X, (int)activeTilePos.Y);
                            //if (hudCue.IsPlaying) hudCue.Stop(AudioStopOptions.AsAuthored);
                            if ((/*ParentMap.initSetupOn*/(CurrentGame.gameState == GameState.InitSetup || CurrentGame.gameState == GameState.MapTestInitSetup) && selectedRingPart == 0) || (CurrentGame.gameState != GameState.InitSetup && CurrentGame.gameState != GameState.MapTestInitSetup)/*!ParentMap.initSetupOn*/)
                            {
                                //hudCue = CurrentGame.soundBank.GetCue("buiRev");
                                //hudCue.Play();
                                CurrentGame.soundEffects[1].Play(0.1f, -0.1f, 0);
                            }
                            skip_TileringExitSound:
                            newTileRingActive = false;
                        }
						sb.Draw(tileringGlow, Magnetize(tileCorners), Color.White * 0.7f); //restricted & magnetized object
					}
					#endregion
					#region OLD TOWER TILERING
					else if (towerTileRingActive)
                    {
						selectedRingPart = CheckNearestPoint(tileCorners);

						sb.Draw(ringFills[0], activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                        //sb.Draw(ringFills[1], activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                        sb.Draw(tilering, activeTilePos, null, Color.Gray * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                        //sb.Draw(tilering, new Vector2(activeTilePos.X - tilering.Width / 2, activeTilePos.Y - tilering.Height / 2), Color.White);
                        //ParentMap.Players[0].Towers[ParentMap.Layout[activeTileCoord.Y, activeTileCoord.X] - 6].ShowRadius = true;

						#region hoverInfo
						//if (/*selectedRingPart > 0 && */selectedRingPart != prevRingPart)
						//{
							string geneCostStr;
							Vector2 belowTilePos = activeTilePos - new Vector2(TowerInfoBox.DefaultWidth * 0.5f, -(ParentMap.TileHalfHeight + tileringSlot.Height * 0.5f));
							switch (selectedRingPart)
							{
                                case 0: TileRingInfoBox = new TowerInfoBox(selectedTower, belowTilePos, false);

                                        break;
                                case 1: if (selectedTower.UpgradeLvl < UpgLvl.Max)
										{
											HexMap.ExampleTowers[selectedTower.towerTypeIdx + 6].ShowRadius = true;
											HexMap.ExampleTowers[selectedTower.towerTypeIdx + 6].MapCoord = activeTileCoord;
											TileRingInfoBox = new TowerInfoBox(HexMap.ExampleTowers[selectedTower.towerTypeIdx + 6], belowTilePos, true);
										}
										break;
								case 2: geneCostStr = selectedTower.GeneSpecs.BaseTiers[0] == 2 ? "34" : GeneSpecs.TierSize.ToString();
										TileRingInfoBox = new TileringInfoBox(tileCorners[selectedRingPart] + new Vector2(tileringSlot.Width * 0.5f, -TileringInfoBox.DefaultHeight * 0.5f), new Color?[] { GeneColors["Red"] }, "Red specialization " + (selectedTower.GeneSpecs.BaseTiers[0] +1).ToString() + "/3", "Cost: ", geneCostStr);
										break;
								case 3: geneCostStr = selectedTower.GeneSpecs.BaseTiers[1] == 2 ? "34" : GeneSpecs.TierSize.ToString();
										TileRingInfoBox = new TileringInfoBox(tileCorners[selectedRingPart] + new Vector2(tileringSlot.Width * 0.5f, -TileringInfoBox.DefaultHeight * 0.5f), new Color?[] { GeneColors["Green"] }, "Green specialization " + (selectedTower.GeneSpecs.BaseTiers[1] + 1).ToString() + "/3", "Cost: ", geneCostStr);
										break;
								case 4: geneCostStr = selectedTower.GeneSpecs.BaseTiers[2] == 2 ? "34" : GeneSpecs.TierSize.ToString();
										TileRingInfoBox = new TileringInfoBox(tileCorners[selectedRingPart] + new Vector2(tileringSlot.Width * 0.5f, -TileringInfoBox.DefaultHeight * 0.5f), new Color?[] { GeneColors["Blue"] }, "Blue specialization " + (selectedTower.GeneSpecs.BaseTiers[2] + 1).ToString() + "/3", "Cost: ", geneCostStr);
										break;
								case 5: if (selectedTower.GeneSpecs.HasAny)
											TileRingInfoBox = new TileringInfoBox(belowTilePos, new Color?[] { GeneColors[selectedTower.GeneSpecs.GetPrimaryElem().ToString()] }, "Withdraw 1/3 genes", "Yield: ", "22");
										else
											TileRingInfoBox = new TileringInfoBox(belowTilePos, null, "Withdraw genes", "if specialized");
										break;
								case 6: Color?[] crs = null;
										int geneYield = 0;
										int energyYield = 0;
                                        if (CurrentGame.gameState != GameState.InitSetup && CurrentGame.gameState != GameState.MapTestInitSetup)
                                        {
                                            for (int i = 0; i <= (int)selectedTower.UpgradeLvl; i++)
                                                energyYield += (int)Math.Round(HexMap.ExampleTowers[selectedTower.towerBranch + i * 6].Cost * CurrentGame.GeneSellRate);
                                        }
                                        else
                                        {
                                            for (int i = 0; i <= (int)selectedTower.UpgradeLvl; i++)
                                                energyYield += HexMap.ExampleTowers[selectedTower.towerBranch + i * 6].Cost;
                                        }
										if (selectedTower.GeneSpecs.HasAny)
										{
											crs = new Color?[] { GeneColors[selectedTower.GeneSpecs.GetPrimaryElem().ToString()] };
											geneYield = (int)Math.Round(selectedTower.GeneSpecs.GetPrimaryElemStrength() * 100 * CurrentGame.GeneSellRate);
										}
										
										if (geneYield > 0)
											TileRingInfoBox = new TileringInfoBox(belowTilePos, crs, "Disassemble", "Yield: ", energyYield.ToString(), geneYield.ToString());
										else
											TileRingInfoBox = new TileringInfoBox(belowTilePos, crs, "Disassemble", "Yield: ", energyYield.ToString());
										break;
							}

							if (selectedRingPart >= 2 && selectedRingPart <= 4 && selectedTower.GeneSpecs.GetPrimaryElemStrength() >= 0.99f)
							{
								TileRingInfoBox = new TileringInfoBox(tileCorners[selectedRingPart] + new Vector2(tileringSlot.Width * 0.5f, TileringInfoBox.DefaultHeight * -0.5f), null, "Already at max");
								TileRingInfoBox.LineColors = new List<Color>() { GeneColors[selectedTower.GeneSpecs.GetPrimaryElem().ToString()] };
							}
						//}
						#endregion

						#region PICK
						if (mouse.LeftButton == ButtonState.Released)
                        {   // if mouse on same hexcorner && allowed upglvl && sufficient money
							//if (selectedRingPart == 1 && (int)selectedTower.UpgradeLvl +1 < ParentMap.AvailableTowers[towerBranch] && ParentMap.Players[0].EnergyPoints >= HexMap.ExampleTowers[towerBranch + (((int)selectedTower.UpgradeLvl+1)*6)].Cost)
							//{
							//    ParentMap.Players[0].EnergyPoints -= HexMap.ExampleTowers[towerBranch + (((int)selectedTower.UpgradeLvl+1)*6)].Cost;
							//    selectedTower.Upgrade();
							//}
                            switch (selectedRingPart)
                            {
								case 1: if ((int)selectedTower.UpgradeLvl + 1 < ParentMap.AvailableTowers[selectedTower.towerBranch] && ParentMap.Players[0].EnergyPoints >= HexMap.ExampleTowers[selectedTower.towerBranch + (((int)selectedTower.UpgradeLvl + 1) * 6)].Cost)
										{
											ParentMap.Players[0].EnergyPoints -= HexMap.ExampleTowers[selectedTower.towerBranch + (((int)selectedTower.UpgradeLvl + 1) * 6)].Cost;
											selectedTower.Upgrade();
										}
										break;
								case 2: selectedTower.AddGeneTier(GeneType.Red); break;
								case 3: selectedTower.AddGeneTier(GeneType.Green); break;
								case 4: selectedTower.AddGeneTier(GeneType.Blue); break;
								case 5: selectedTower.WithdrawMainGeneTier(); break;
								case 6: selectedTower.Disassemble(); break;
                            }
                            if (hudCue.IsPlaying) hudCue.Stop(AudioStopOptions.AsAuthored);
                            hudCue = CurrentGame.soundBank.GetCue("buiRev");
                            hudCue.Play();
                            towerTileRingActive = false;

							HoveredTowerInfoBox = new TowerInfoBox(selectedTower, belowTilePos, false);
						}
						#endregion

						if (selectedRingPart == 1 && (int)selectedTower.UpgradeLvl + 1 < ParentMap.AvailableTowers[selectedTower.towerBranch] && selectedTower.UpgradeLvl < UpgLvl.Max)
                                HexMap.ExampleTowers[selectedTower.towerTypeIdx + 6].Draw(sb);

						sb.Draw(tileringGlow, Magnetize(tileCorners), Color.White * 0.7f); //restricted & magnetized object
					}
					#endregion
					#region PRIORITY TILERING
					else if (priorityRingActive)
                    {
						selectedRingPart = CheckNearestPoint(extendedPoints);

						sb.Draw(tilering, activeTilePos, null, Color.Teal * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);

						switch (selectedRingPart)
						{
							case 2: case 7: case 8: case 9: {									
									sb.Draw(tileringSlot, extendedPoints[7], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									sb.Draw(tileringSlot, extendedPoints[8], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									sb.Draw(tileringSlot, extendedPoints[9], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0); 
									break; }
							case 3: case 10: case 11: {
									sb.Draw(tileringSlot, extendedPoints[10], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									sb.Draw(tileringSlot, extendedPoints[11], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									break; }
							case 4: case 12: case 13: {
									sb.Draw(tileringSlot, extendedPoints[12], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									sb.Draw(tileringSlot, extendedPoints[13], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									break; }
							case 5: case 14: case 15: {
									sb.Draw(tileringSlot, extendedPoints[14], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									sb.Draw(tileringSlot, extendedPoints[15], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									break; }
							case 6: case 16: case 17: {
									sb.Draw(tileringSlot, extendedPoints[17], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									sb.Draw(tileringSlot, extendedPoints[16], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									break; }
							case 1: case 18: case 19: {
									sb.Draw(tileringSlot, extendedPoints[18], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									sb.Draw(tileringSlot, extendedPoints[19], null, Color.Aquamarine, 0, new Vector2(tileringSlot.Width / 2, tileringSlot.Height / 2), 1, SpriteEffects.None, 0);
									break; }
						}

						#region DEBUGSHOW PRIORITY
						ColorPriority hoveredEPriority = ColorPriority.None;
						TargetPriority hoveredTPriority = TargetPriority.None;
						switch (selectedRingPart)
						{
							case 2: hoveredEPriority = ColorPriority.None; break;
							case 7: hoveredEPriority = ColorPriority.Red; break;
							case 8: hoveredEPriority = ColorPriority.Green; break;
							case 9: hoveredEPriority = ColorPriority.Blue; break;
							case 10: hoveredTPriority = TargetPriority.First; break;
							case 11: hoveredTPriority = TargetPriority.Last; break;
							case 12: hoveredTPriority = TargetPriority.Tough; break;
							case 13: hoveredTPriority = TargetPriority.Weak; break;
							case 14: hoveredTPriority = TargetPriority.Fast; break;
							case 15: hoveredTPriority = TargetPriority.Slow; break;
							case 16: hoveredTPriority = TargetPriority.Mob; break;
							case 17: hoveredTPriority = TargetPriority.Far; break;
						}
						#endregion
						if (mouse.RightButton == ButtonState.Released)
                        {
                            priorityRingActive = false;
							switch (selectedRingPart)
							{
								case 2: selectedTower.ElemPriority = ColorPriority.None; break;
								case 1:
								case 3:
								case 4:
								case 5:
								case 6: selectedTower.TargetPriority = TargetPriority.None; break;
								case 7: selectedTower.ElemPriority = ColorPriority.Red; break;
								case 8: selectedTower.ElemPriority = ColorPriority.Green; break;
								case 9: selectedTower.ElemPriority = ColorPriority.Blue; break;
								case 10: selectedTower.TargetPriority = TargetPriority.First; break;
								case 11: selectedTower.TargetPriority = TargetPriority.Last; break;
								case 12: selectedTower.TargetPriority = TargetPriority.Tough; break;
								case 13: selectedTower.TargetPriority = TargetPriority.Weak; break;
								case 14: selectedTower.TargetPriority = TargetPriority.Fast; break;
								case 15: selectedTower.TargetPriority = TargetPriority.Slow; break;
								case 16: selectedTower.TargetPriority = TargetPriority.Mob; break;
								case 17: selectedTower.TargetPriority = TargetPriority.Far; break;
							}
                        }

						sb.Draw(tileringGlow, MagnetizeExtended(extendedPoints), Color.White * 0.7f); //restricted & magnetized object
						sb.DrawString(font, selectedRingPart >= 7 && selectedRingPart <= 9 ? hoveredEPriority.ToString() : selectedTower.ElemPriority.ToString(), selectedTower.ScreenLocation + new Vector2(-20, -20), Color.FloralWhite);
						sb.DrawString(font, selectedRingPart >= 10 ? hoveredTPriority.ToString() : selectedTower.TargetPriority.ToString(), selectedTower.ScreenLocation + new Vector2(-20, 10), Color.FloralWhite);
					}
					#endregion

					#region IN ALL TILERINGS
                    if (tileHoverFade > 0) 
						tileHoverFade--;
                    if (selectedRingPart > 0) 
                    {
                        if (selectedRingPart != prevRingPart)
                        {
                            hudCue = CurrentGame.soundBank.GetCue("pluip5");
                            hudCue.Play();
							if (selectedRingPart < 7) 
								HexMap.ExampleTowers[selectedRingPart - 1].radiusFade = 0;
						}
                        //----------------------------AFFORDCOLOR-----------------------------------------------------------------|  (28.5.16: riittämättömyyshohto symbolien päälle
                        //if (ParentMap.Players[0].EnergyPoints - ParentMap.ExampleTowers[selectedRingPart - 1].Cost >= 0)
                        //    sb.Draw(ParentMap.tileTextures[5], tileCorners[selectedRingPart] - new Vector2(ParentMap.tileTextures[5].Width / 2, ParentMap.tileTextures[5].Height / 2), Color.PowderBlue);
                        if (selectedRingPart < 7 && !priorityRingActive && ParentMap.AvailableTowers[selectedRingPart -1] > 0 && ParentMap.Players[0].EnergyPoints < HexMap.ExampleTowers[selectedRingPart - 1].Cost)
                            sb.Draw(tileringGlow, tileCorners[selectedRingPart] - new Vector2(tileringGlow.Width / 2, tileringGlow.Height / 2), Color.IndianRed); //----------------------TODO: geneAffords & noRedsOnSellETC
                        //foreach (Vector2 point in tileCorners)
                        //    sb.Draw(ParentMap.tileTextures[4], point, null, new Color(150, 150, 150, 150));
					}
					#endregion
					#endregion
				}
                else if (hoveredCoord.X >= 0 && hoveredCoord.X < ParentMap.Layout.GetLength(1) && hoveredCoord.Y >= 0 && hoveredCoord.Y < ParentMap.Layout.GetLength(0)) // HOVERING OVER THE MAP
                #region MAP HOVER & CLICK
                {
                    ParentGame.IsMouseVisible = true;
                    char tileChar = ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X];
					#region switchOption
					//switch (tileChar)
					//{
					//    case '\'': 
					//    case '.':
					//    case 'a':
					//    case 'b':
					//    case 'c':
					//    case 'd':
					//    case 'e':
					//    case 'f':
					//    case 'g':
					//    case 'h':
					//    case 'i':
					//    case '1':
					//    case '2':
					//    case '3':
					//    case '4':
					//    case '5':
					//    case '6':
					//    case '7':
					//    case '8':
					//    case '9':
					//    case '0': if (mouse.LeftButton == ButtonState.Pressed)
					//        {
					//            activeTilePos = hoveredTilePos;
					//            newTileRingActive = true;
					//            activeTileCoord = new Point(hoveredCoord.X, hoveredCoord.Y);
					//            Mouse.SetPosition((int)activeTilePos.X, (int)activeTilePos.Y);
					//            selectedRingPart = 0;
					//            //selectionBall = hoveredTilePos;
					//            hudCue = CurrentGame.soundBank.GetCue("bui");
					//            hudCue.Play();
					//        }
					//        if (tileHoverFade < hoverFadeCycles) 
					//            tileHoverFade++;
					//        //ColorConversion.SetLightness(ref color, 100f);
					//        break;
					//    default:
					//}
					#endregion
					//HOVERING OVER OPEN TILE OR TOWER
					if (tileChar == '0' || !(tileChar == ' ' || tileChar == '\'' || tileChar == '.' || (tileChar >= 'a' && tileChar <= 'i') || (tileChar >= '1' && tileChar <= '9')))
                    {
						activeTilePos = hoveredTilePos;
                        Vector2 belowTilePos = activeTilePos - new Vector2(TowerInfoBox.DefaultWidth * 0.5f, -(ParentMap.TileHalfHeight + tileringSlot.Height * 0.5f));
                        if (tileChar == '0') //-----OPEN TILE 
						{
							if (mouse.LeftButton == ButtonState.Pressed)
							{
								newTileRingActive = true;
								OpenTilering(true);
							}
						}
						else //---------------------TOWER
						{
							if (mouse.LeftButton == ButtonState.Pressed)
							{
								towerTileRingActive = true;
								OpenTilering(false);
							}
							if (mouse.RightButton == ButtonState.Pressed)
							{
								priorityRingActive = true;
								OpenTilering(false);
							}
							for (int i = 0; i < ParentMap.Players[0].Towers.Count; i++)
							{
								if (ParentMap.Players[0].Towers[i].MapCoord == hoveredCoord)
								{
									selectedTower = ParentMap.Players[0].Towers[i];
									if (HoveredTowerInfoBox == null || HoveredTowerInfoBox.Target != selectedTower)
									{
										HoveredTowerInfoBox = new TowerInfoBox(selectedTower, belowTilePos, false);
										towerHoverCounter = -towerHoverPopUpDelayCycles;
									}
									if (towerHoverCounter < towerHoverCycles)
										towerHoverCounter++;
									break;
								}
							}
						}

						if (HoveredTowerInfoBox != null && hoveredCoord != HoveredTowerInfoBox.Target.MapCoord)
							towerHoverCounter--;

						if (tileHoverFade < hoverFadeCycles) 
							tileHoverFade++;
					}
                    else //----------------------------------------------HOVERING OVER VOID OR PATH
                    {
						if (tileHoverFade > 0)
							tileHoverFade--;
						towerHoverCounter--;
					}
					//----------------------------------------------------------------------------------------------------------------'
                    if (tileRingFade > 0)
                    {
                        tileRingFade--;
                        sb.Draw(tilering, activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                    }
                }
				//else
				//{
				//    if (tileHoverFade > 0) tileHoverFade--;
				//}
                #endregion
                #endregion
                sb.Draw(tileOverlay, overlayPos, null, Color.White * (tileHoverFade / hoverFadeCycles) * 0.8f, 0, ParentMap.tileTexCenter, 1, SpriteEffects.None, 0); //HOVER OVERLAY-------------------------------------DISAPPEARS AT GAMEOVER (HOVERFADES STOP)

                #region CREATURES
                //--------------------HP BARS---------------------------------.
                for (int c = 0; c < ParentMap.AliveCreatures.Count; c++)
                {	if (ParentMap.AliveCreatures[c].hp != ParentMap.AliveCreatures[c].InitHp)
                    {
						Creature creature = ParentMap.AliveCreatures[c];
						Color hpBarColor = new Color(1 - creature.hp / creature.InitHp, creature.hp / creature.InitHp, 0);
						creature.HpBarColor = hpBarColor;
						sb.Draw(CurrentGame.pixel, new Rectangle((int)creature.Location.X - creature.hpBarWidth / 2, (int)(creature.Location.Y - creature.Height * creature.SpriteScale / 2 - 1), creature.hpBarWidth, 4), Color.Black); //black background
						sb.Draw(CurrentGame.pixel, new Rectangle((int)creature.Location.X - creature.hpBarWidth / 2 + 1, (int)(creature.Location.Y - creature.Height * creature.SpriteScale / 2), (int)((creature.hpBarWidth - 2) * (creature.hp / creature.InitHp)), 2), hpBarColor);
                    }
                }//-----------------------------------------------------------'
                for (int w = 0; w < ParentMap.Waves.Length; w++) //-----------SPLATTERS-------------------
                {	for (int g = 0; g < ParentMap.Waves[w].Groups.Length; g++)
                    {	for (int c = 0; c < ParentMap.Waves[w].Groups[g].Creatures.Length; c++)
                        {
							//ParentMap.Waves[w].Groups[g].Creatures[c].TrailEngine.Draw(sb); //----------TRAIL
                            if (ParentMap.Waves[w].Groups[g].Creatures[c].Splatter.IsActive)
                                ParentMap.Waves[w].Groups[g].Creatures[c].Splatter.Draw(sb); 
                        }
                    }
                }

                #endregion

                #region INFOBOXES
                if (newTileRingActive && selectedRingPart > 0 && TileRingInfoBox != null)
                    TileRingInfoBox.Draw(sb, 1);
                else if (towerTileRingActive && TileRingInfoBox != null)
                    TileRingInfoBox.Draw(sb, 1);
                else if (HoveredTowerInfoBox != null && selectedTower != null && !(towerTileRingActive || newTileRingActive || priorityRingActive))
                {
                    HoveredTowerInfoBox.Draw(sb, Math.Min(towerHoverCounter / towerHoverCycles, 1));
                }
                for (int i = 0; i < BugBoxes.Count; i++)
                {
                    if (!BugBoxes[i].locked)
                        BugBoxes[i].Draw(sb, Math.Min((float)bugHoverCounter / (bugHoverFade * 0.333f), 1));
                    else
                        BugBoxes[i].Draw(sb, 1);
                }
                for (int i = 0; i < NexWaveInfoBoxes.Count; i++)
                    NexWaveInfoBoxes[i].Draw(sb, 1);
                for (int i = 0; i < CurrWaveInfoBoxes.Count; i++)
                    CurrWaveInfoBoxes[i].Draw(sb, 1);
                #endregion

                #region HARDCODED TEXTS
                string waveXofN = "Wave " + (ParentMap.currentWave + 1) + " / " + (ParentMap.Waves == null ? 0 : ParentMap.Waves.Length);

                sb.DrawString(font, "fps " + (int)Math.Round(1 / gameTime.ElapsedGameTime.TotalSeconds), new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.9f, 20), Color.DarkTurquoise);
                if (ParentMap.currentWave >= 0)
                    sb.DrawString(font, waveXofN, new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.5f - (int)font.MeasureString(waveXofN).X / 2, 20), Color.Orange);//, 0, Vector2.Zero,2f, SpriteEffects.None,0);
                else sb.DrawString(font, "Setup phase", new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.5f - (int)font.MeasureString("Setup phase").X / 2, 50), Color.Orange);
                sb.DrawString(font, "Life:   " + ParentMap.Players[0].LifePoints, new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.7f, 20), Color.IndianRed);
                sb.DrawString(font, "Energy: " + ParentMap.Players[0].EnergyPoints, new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.7f, 40), Color.LightSeaGreen);
                sb.DrawString(font, "Genes: ", new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.8f, 20), Color.LightGray);
                sb.DrawString(font, ParentMap.Players[0].GenePoints[0].ToString(), new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.8f, 40), Color.Red);
                sb.DrawString(font, ParentMap.Players[0].GenePoints[1].ToString(), new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.83f, 40), Color.Green);
                sb.DrawString(font, ParentMap.Players[0].GenePoints[2].ToString(), new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.86f, 40), Color.Blue);
                //sb.DrawString(font, "Genes:  " + ParentMap.Players[0].UpgradePoints, new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.7f, 60), Color.Plum);
                //sb.DrawString(font, Environment.CurrentDirectory, new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.3f, 70), Color.Plum);
                //sb.DrawString(font, "Td, aT, at, dT, a7, AVAK T;, zgj", new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.4f, 90), Color.Plum);
                //sb.DrawString(font, "mapTime:  " + CurrentMap.mapTimer, new Vector2(ParentGame.GraphicsDevice.Viewport.Width * 0.7f, 80), Color.Wheat);
                #endregion

                //DrawWaveInfo(sb);

                if (CurrentGame.gameState == GameState.MapTestInGame || (CurrentGame.prevState == GameState.MapTestInGame && CurrentGame.gameState == GameState.Paused))
                #region MAPTEST WAVETABLE
                {
                    BackToEditButton.Draw(sb);
                    MapEditorTopButtons[1].Draw(sb);
                    if (MapEditorTopButtons[1].State == ButnState.Active)
                    {
                        sb.Draw(CurrentGame.pixel, new Rectangle(topButtonX, topButtonY + menuButtonHeight, ParentGame.GraphicsDevice.Viewport.Width - topButtonX * 2, ParentGame.GraphicsDevice.Viewport.Height - (menuButtonHeight + 7) * 2), Color.Black * 0.9f);
                        for (int i = 0; i < MapEditorLabelButtons.Length; i++)
                        {
                            MapEditorLabelButtons[i].Draw(sb);
                        }
                        for (int i = totalGroups * (MapEditorTableCols - 1) - 1; i >= 0; i--) // taulukkosolut lopusta alkuun jotta dropdownmenut näkyy
                        {
                            if (MapEditorTableCells[i] != null)
                                MapEditorTableCells[i].Draw(sb);
                        }

                        if (totalGroups > 0)
                        {
                            for (int i = 0; i < totalGroups + 1; i++)
                                MapEditorTableAddButtons[i].Draw(sb);
                        }

                        if (ParentMap.MapEditorTempWaves.Count > 0)
                            MapEditorWaveLabels[0].Draw(sb);
                        if (totalGroups < MapEditorTableRows - 1)
                            MapEditorWaveLabels[totalGroups].Draw(sb);
                        int labelPos = 0;
                        for (int i = 1; i < ParentMap.MapEditorTempWaves.Count; i++)
                        {
                            labelPos += ParentMap.MapEditorTempWaves[i - 1].TempGroups.Count;
                            MapEditorWaveLabels[labelPos].Draw(sb);
                        }
                        for (int i = 0; i < totalGroups; i++)
                            MapEditorTableDelButtons[i].Draw(sb);
                    }
                }
                #endregion
            }
            else // Mapeditor---------------------------------------------------------------------------------.
            #region EDITOR
            {
                
                for (int i = 0; i < MapEditorTopButtons.Length; i++)
                    MapEditorTopButtons[i].Draw(sb);
                for (int i = 0; i < MapEditorResourceCells.Length; i++)
                    MapEditorResourceCells[i].Draw(sb);
                for (int i = 0; i < MapEditorToolButtons.Length; i++)
                    MapEditorToolButtons[i].Draw(sb);

                MapNameBox.Draw(sb);

                Vector2 spawnTextOrigin = font.MeasureString("spawn") / 2;
                for (int i = 0; i < MapEditorSpawnPoints.Count; i++)
                {
                    sb.DrawString(font, "spawn", ParentMap.ToScreenLocation(MapEditorSpawnPoints[i]) - spawnTextOrigin, Color.AntiqueWhite);
                    sb.DrawString(font, (i+1).ToString(), ParentMap.ToScreenLocation(MapEditorSpawnPoints[i]) - new Vector2(font.MeasureString((i+1).ToString()).X/2, -4), Color.AntiqueWhite);
                }
                Vector2 goalTextOrigin = font.MeasureString("goal") / 2;
                for (int i = 0; i < MapEditorGoalPoints.Count; i++)
                {
                    sb.DrawString(font, "goal", ParentMap.ToScreenLocation(MapEditorGoalPoints[i]) - goalTextOrigin, Color.AntiqueWhite);
                    sb.DrawString(font, ((char)(i+97)).ToString(), ParentMap.ToScreenLocation(MapEditorGoalPoints[i]) - new Vector2(font.MeasureString(((char)(i+97)).ToString()).X/2, -4), Color.AntiqueWhite);
                }

                if (MapEditorTopButtons[1].State == ButnState.Active)
                {
                    sb.Draw(CurrentGame.pixel, new Rectangle(topButtonX, topButtonY + menuButtonHeight, ParentGame.GraphicsDevice.Viewport.Width - topButtonX * 2, ParentGame.GraphicsDevice.Viewport.Height - (menuButtonHeight + 7) * 2), Color.Black * 0.9f);
                    for (int i = 0; i < MapEditorLabelButtons.Length; i++)
                    {
                        MapEditorLabelButtons[i].Draw(sb);
                    }
                    for (int i = totalGroups * (MapEditorTableCols -1) -1; i >= 0; i--) // taulukkosolut lopusta alkuun jotta dropdownmenut näkyy
                    {
                        if (MapEditorTableCells[i] != null)
                            MapEditorTableCells[i].Draw(sb);
                    }

                    if (totalGroups > 0)
                    {
                        for (int i = 0; i <= totalGroups; i++)
                            MapEditorTableAddButtons[i].Draw(sb);
                    }

                    if (ParentMap.MapEditorTempWaves.Count > 0)
                        MapEditorWaveLabels[0].Draw(sb);
                    if (totalGroups < MapEditorTableRows-1)
                        MapEditorWaveLabels[totalGroups].Draw(sb);
                    int labelPos = 0;
                    for (int i = 1; i < ParentMap.MapEditorTempWaves.Count; i++)
                    {
                        labelPos += ParentMap.MapEditorTempWaves[i-1].TempGroups.Count;
                        MapEditorWaveLabels[labelPos].Draw(sb);
                    }
                    for (int i = 0; i < totalGroups; i++)
                        MapEditorTableDelButtons[i].Draw(sb);
                }
                else
                {
                    if (hoveredCoord.X >= 0 && hoveredCoord.X < ParentMap.Layout.GetLength(1) && hoveredCoord.Y >= 0 && hoveredCoord.Y < ParentMap.Layout.GetLength(0))
                    {
                        if (!(drawMode == DrawMode.Tower && mouse.LeftButton == ButtonState.Pressed))
                        {
                            sb.Draw(tileOverlay, overlayPos, null, Color.White * 0.4f, 0, ParentMap.tileTexCenter, 1, SpriteEffects.None, 0);
                        }
                    }
                }

                for (int m = 0; m < MapEditorMenuButtons.Length; m++) //THAT DROPDONWS CAN SHOW OVER THE TABLE
                    MapEditorMenuButtons[m].Draw(sb);

                if (ErrorShow)
                {
                    for (int i = 0; i < ErrorButtons.Count; i++)
                    {
                        ErrorButtons[i].Draw(sb);
                    } 
                }

                
                if (drawMode == DrawMode.Tower)
                #region UUDESTAAN UPDATEEN KUULUVA MOUSELOGIC !!!
                {
                    if (newTileRingActive || towerTileRingActive)
                    {
                        ParentGame.IsMouseVisible = false;
                        Vector2[] tileCorners = new Vector2[7] { activeTilePos,   //Center
                                                             new Vector2(activeTilePos.X - ParentMap.TileWidth/4 -1, activeTilePos.Y - ParentMap.TileHalfHeight), //up-L
                                                             new Vector2(activeTilePos.X + ParentMap.TileWidth/4 +1 /* +1 jottei näy upgia mietties kokoajan vanha range*/, activeTilePos.Y - ParentMap.TileHalfHeight),  //up-R
                                                             new Vector2(activeTilePos.X + ParentMap.TileHalfWidth /* ennen 28.5.16 -1, mut vaihto jottei näy upgia mietties kokoajan vanha range*/, activeTilePos.Y),    //R
                                                             new Vector2(activeTilePos.X + ParentMap.TileWidth/4, activeTilePos.Y + ParentMap.TileHalfHeight),    //lo-R
                                                             new Vector2(activeTilePos.X - ParentMap.TileWidth/4 -1, activeTilePos.Y + ParentMap.TileHalfHeight), //lo-L
                                                             new Vector2(activeTilePos.X - ParentMap.TileHalfWidth, activeTilePos.Y)};                            //L


						#region TILERING LOGIC
						if (newTileRingActive)  //--------------------------------------------------------------------------------------------------------------------------------------JA TÄMÄN?!
                        {
							selectedRingPart = CheckNearestPoint(tileCorners);
                            if (tileRingFade < tileRingFadeCycles) tileRingFade++;
                            if (selectedRingPart > 0)
                            {
								TileRingInfoBox = new TowerInfoBox(HexMap.ExampleTowers[selectedRingPart - 1], activeTilePos, true);
                                HexMap.ExampleTowers[selectedRingPart - 1].ShowRadius = true;
                                HexMap.ExampleTowers[selectedRingPart - 1].MapCoord = activeTileCoord;
                                HexMap.ExampleTowers[selectedRingPart - 1].Draw(sb);

                                sb.Draw(tileringGlow, tileCorners[selectedRingPart] - new Vector2(tileringGlow.Width / 2, tileringGlow.Height / 2), Color.White);
                            }
                            //Tähän ringFillit ennen tileringiä jotta afford-hohdot symbolien alle--------------------------------------------------------------------------------------------------------------------------------O
                            sb.Draw(ringFills[0], activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                            sb.Draw(ringFills[1], activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);

                            sb.Draw(tilering, activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);

                            if (mouse.LeftButton == ButtonState.Released)
                            {
                                if (selectedRingPart > 0)
                                {
                                    Tower t = Tower.Clone(HexMap.ExampleTowers[selectedRingPart - 1]);
                                    t.ParentMap = ParentMap;
                                    t.MapCoord = activeTileCoord;
                                    t.buildFinishedCounter = 0;
                                    t.buildTimer = 0;
                                    t.Built = true;
                                    ParentMap.InitLayout[activeTileCoord.Y, activeTileCoord.X] = HexMap.ExampleTowers[selectedRingPart - 1].Symbol;
                                    ParentMap.Layout[activeTileCoord.Y, activeTileCoord.X] = HexMap.ExampleTowers[selectedRingPart - 1].Symbol;
                                    //ParentMap.Players[0].Towers.Add(t);
                                    ParentMap.InitTowers.Add(t);
                                    ParentMap.Players[0].Towers.Add(t);

									//if (hudCue.IsPlaying) hudCue.Stop(AudioStopOptions.AsAuthored);
									hudCue = CurrentGame.soundBank.GetCue("pluip2");
									hudCue.Play();
                                }
 
                                newTileRingActive = false;
                            }
							sb.Draw(tileringGlow, Magnetize(tileCorners), Color.White * 0.7f); //restricted & magnetized object
                        }
                        else if (towerTileRingActive)
                        {
							selectedRingPart = CheckNearestPoint(tileCorners);
                            if (tileRingFade < tileRingFadeCycles) tileRingFade++;

                            sb.Draw(ringFills[0], activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                            sb.Draw(ringFills[1], activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                            sb.Draw(tilering, activeTilePos, null, Color.Gray * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                            //sb.Draw(tilering, new Vector2(activeTilePos.X - tilering.Width / 2, activeTilePos.Y - tilering.Height / 2), Color.White);
                            //ParentMap.Players[0].Towers[ParentMap.Layout[activeTileCoord.Y, activeTileCoord.X] - 6].ShowRadius = true;

                            int towerBranch = selectedTower.towerTypeIdx % 6;
                            if (mouse.LeftButton == ButtonState.Released)
                            {
                                if (selectedRingPart == 1)
                                {
                                    selectedTower.Upgrade();
                                }
                                /*switch (selectedRingPart)
                                {
                                    case 1: break;
                                    case 2: break;
                                    case 3: break;
                                    case 4: break;
                                    case 5: break;
                                    case 6: break;
                                }*/
                                towerTileRingActive = false;
                                if (hudCue.IsPlaying) hudCue.Stop(AudioStopOptions.AsAuthored);
                                hudCue = CurrentGame.soundBank.GetCue("buiRev");
                                hudCue.Play();
                            }
                            if (selectedRingPart == 1)
                            {
                                if (selectedTower.UpgradeLvl != UpgLvl.Max)
                                {
                                    HexMap.ExampleTowers[selectedTower.towerTypeIdx + 6].ShowRadius = true;
                                    HexMap.ExampleTowers[selectedTower.towerTypeIdx + 6].MapCoord = activeTileCoord;
                                    HexMap.ExampleTowers[selectedTower.towerTypeIdx + 6].Draw(sb);
                                }
                            }
                        }
                        if (tileHoverFade > 0) tileHoverFade--;
						sb.Draw(tileringGlow, Magnetize(tileCorners), Color.White * 0.7f); //restricted & magnetized object
						//sb.Draw(tileOverlay, activeTilePos, null, Color.White * (hoverFade / hoverFadeCycles) * 0.8f /*new Color(230, 230, 250, 255)*/, 0, ParentMap.tileTexCenter, 1, SpriteEffects.None, 0);

                        if (selectedRingPart > 0) //-----------kaikissa tileringeissä
                        {
                            if (selectedRingPart != prevRingPart)
                            {
                                HexMap.ExampleTowers[selectedRingPart - 1].radiusFade = 0;
                                hudCue = CurrentGame.soundBank.GetCue("pluip5");
                                hudCue.Play();
                            }
                            //HexMap.ExampleTowers[selectedRingPart - 1].ShowRadius = true;
                            //HexMap.ExampleTowers[selectedRingPart - 1].mapCoord = activeTileCoord;
                            //HexMap.ExampleTowers[selectedRingPart - 1].Draw(sb);
                            //----------------------------AFFORDCOLOR-----------------------------------------------------------------|  (28.5.16: riittämättömyyshohto symbolien päälle
                            //if (ParentMap.Players[0].EnergyPoints - ParentMap.ExampleTowers[selectedRingPart - 1].Cost >= 0)
                            //    sb.Draw(ParentMap.tileTextures[5], tileCorners[selectedRingPart] - new Vector2(ParentMap.tileTextures[5].Width / 2, ParentMap.tileTextures[5].Height / 2), Color.PowderBlue);
                              //if (ParentMap.Players[0].EnergyPoints - HexMap.ExampleTowers[selectedRingPart - 1].Cost < 0)
                              //    sb.Draw(tileringGlow, tileCorners[selectedRingPart] - new Vector2(tileringGlow.Width / 2, tileringGlow.Height / 2), Color.IndianRed);
                            //foreach (Vector2 point in tileCorners)
                            //    sb.Draw(ParentMap.tileTextures[4], point, null, new Color(150, 150, 150, 150));
                        }
                        #endregion
                    }
                    else if (hoveredCoord.X >= 0 && hoveredCoord.X < ParentMap.Layout.GetLength(1) && hoveredCoord.Y >= 0 && hoveredCoord.Y < ParentMap.Layout.GetLength(0)) //if in map bounds
                    #region HOVER & CLICK
                    {
                        ParentGame.IsMouseVisible = true;

                        if (ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] == '0') //-----OPEN TILE
                        {
                            if (mouse.LeftButton == ButtonState.Pressed)
                            {
                                activeTilePos = hoveredTilePos;
                                newTileRingActive = true;
                                activeTileCoord = new Point(hoveredCoord.X, hoveredCoord.Y);
                                Mouse.SetPosition((int)activeTilePos.X, (int)activeTilePos.Y);
                                selectedRingPart = 0;
                                //selectionBall = hoveredTilePos;
                                hudCue = CurrentGame.soundBank.GetCue("bui");
                                hudCue.Play();
                            }
                            if (tileHoverFade < hoverFadeCycles) tileHoverFade++;
                            //ColorConversion.SetLightness(ref color, 100f);
                        }//------PATH OR S/GPOINT
                        else if (ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] == '\'' || ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] == '.' || (ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] >= '1' && ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] <= '9')) 
                        {
                            //if (mouse.LeftButton == ButtonState.Pressed)
                            //    sb.Draw(ParentMap.tileTextures[1], overlayPos, null, Color.RosyBrown, 0, ParentMap.tileTexCenter, 1, SpriteEffects.None, 0);
                            //else sb.Draw(ParentMap.tileTextures[1], overlayPos, null, Color.RosyBrown * 0.5f * (1 - (hoverFade / hoverFadeCycles)), 0, ParentMap.tileTexCenter, 1, SpriteEffects.None, 0);
                            if (tileHoverFade > 0) tileHoverFade--;
                        }
                        else if (ParentMap.Layout[hoveredCoord.Y, hoveredCoord.X] == ' ') //--------VOID
                        {
                            if (tileHoverFade > 0) tileHoverFade--;
                        }
                        else //------TOWER-----------------------------------------------!!!
                        {

                            if (mouse.LeftButton == ButtonState.Pressed)
                            {
                                towerTileRingActive = true;
                                activeTileCoord = new Point(hoveredCoord.X, hoveredCoord.Y);
                                Mouse.SetPosition((int)activeTilePos.X, (int)activeTilePos.Y);
                                selectedRingPart = 0;
                                hudCue = CurrentGame.soundBank.GetCue("bui");
                                hudCue.Play();
                            }
                            for (int i = 0; i < ParentMap.Players[0].Towers.Count; i++)
                            {
                                if (ParentMap.Players[0].Towers[i].MapCoord == activeTileCoord)
                                    selectedTower = ParentMap.Players[0].Towers[i];
                            }
                            activeTilePos = hoveredTilePos;
                            if (tileHoverFade < hoverFadeCycles) tileHoverFade++;
                        }

                        if (tileRingFade > 0)
                        {
                            tileRingFade--;
                            sb.Draw(tilering, activeTilePos, null, Color.White * (tileRingFade / tileRingFadeCycles), 0, tileringCenter, tileRingFade / tileRingFadeCycles, SpriteEffects.None, 0);
                        }
                    }
                    else
                    {
                        if (tileHoverFade > 0) tileHoverFade--;
                    }
                    #endregion 
					sb.Draw(tileOverlay, overlayPos, null, Color.White * (tileHoverFade / hoverFadeCycles) * 0.8f, 0, ParentMap.tileTexCenter, 1, SpriteEffects.None, 0); //HOVER OVERLAY-------------------------------------DISAPPEARS AT GAMEOVER (HOVERFADES STOP)
				}
                #endregion

                for (int i = 0; i < AvailableTowersCells.Length; i++)
                {
                    AvailableTowersCells[i].Draw(sb);
                }

                /*Vector2[] tileCorn = new Vector2[6] { new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width/4, MapEditorToolButtons[3].Pos.Y), //up-L
                                                     new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width/4 * 3 +1, MapEditorToolButtons[3].Pos.Y),  //up-R
                                                     new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width -1, MapEditorToolButtons[3].Pos.Y + tileOverlay.Height/2),         //R
                                                     new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width/4 * 3 +1, MapEditorToolButtons[3].Pos.Y + tileOverlay.Height -1),    //lo-R
                                                     new Vector2(MapEditorToolButtons[3].Pos.X + tileOverlay.Width/4, MapEditorToolButtons[3].Pos.Y + tileOverlay.Height -1), //lo-L
                                                     new Vector2(MapEditorToolButtons[3].Pos.X +1, MapEditorToolButtons[3].Pos.Y + tileOverlay.Height/2)};        //L
                for (int i = 0; i < tileCorn.Length; i++)
                {
                    sb.Draw(CurrentGame.pixel, tileCorn[i], Color.Tomato); 
                }*/

                /*sb.Draw(CurrentGame.pixel, TableBackground, Color.BlueViolet * 0.5f);
                sb.Draw(CurrentGame.pixel, TableBackground1, Color.BlueViolet * 0.5f);
                sb.Draw(CurrentGame.pixel, TableBackground2, Color.BlueViolet * 0.5f);
                sb.Draw(CurrentGame.pixel, TableBackground3, Color.BlueViolet * 0.5f);
                sb.Draw(CurrentGame.pixel, TableBackground4, Color.BlueViolet * 0.5f);
                sb.Draw(CurrentGame.pixel, TableBackground5, Color.BlueViolet * 0.5f);*/
            }
            #endregion

            #region LOStest
            //if (Keyboard.GetState().IsKeyDown(Keys.A))
            //{
            //    LOSFromTilePos = hoveredTilePos;
            //    //LOSCollisionPos = Vector2.Zero;
            //    isTileChecked = false;
            //    LOSPixelDistanceCounter = 0;
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.D))
            //{
            //    if (hoveredTilePos != PrevLOSCollisionPos)
            //    {
            //        Vector2 lookPixel = LOSFromTilePos;
            //        Vector2 broadLookPixel;
            //        Vector2 dir = Vector2.Normalize(hoveredTilePos - LOSFromTilePos);
            //        lookPixel += (ParentMap.TileHalfHeight + 6) * dir;
            //        int targetDist = (int)Vector2.Distance(hoveredTilePos, lookPixel);
            //        while (hoveredTilePos != PrevLOSCollisionPos && LOSPixelDistanceCounter < targetDist)
            //        {
            //            broadLookPixel = LOSPixelDistanceCounter % 2 == 1 ? lookPixel + new Vector2(0, 1) : lookPixel - new Vector2(0, 1);
            //            Point coord = ParentMap.ToMapCoordinate(broadLookPixel);
            //            if (ParentMap.Layout[coord.Y, coord.X] != '.' && ParentMap.Layout[coord.Y, coord.X] != '\'')
            //            {
            //                isTileChecked = true;
            //                PrevLOSCollisionPos = hoveredTilePos;
            //                LOSCollisionPos = lookPixel;
            //            }
            //            else
            //                lookPixel += dir;
            //            LOSPixelDistanceCounter++;
            //        } 
            //    }
            //}

            //sb.Draw(CurrentGame.pixel, new Rectangle((int)LOSFromTilePos.X, (int)LOSFromTilePos.Y, (int)Vector2.Distance(LOSFromTilePos, hoveredTilePos), 1), null, Color.Wheat, (float)Math.Atan2(hoveredTilePos.Y - LOSFromTilePos.Y, hoveredTilePos.X - LOSFromTilePos.X), Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            //if (isTileChecked)
            //{
            //    sb.Draw(CurrentGame.ball, LOSCollisionPos, Color.Red);
            //    sb.DrawString(font, LOSPixelDistanceCounter.ToString(), LOSCollisionPos, Color.PowderBlue);
            //}
            #endregion


            //GeneBars[0].Draw(sb);
            //GeneBars[1].Draw(sb);
            //GeneBars[2].Draw(sb);

            prevRingPart = selectedRingPart;

            
		}
    }
}

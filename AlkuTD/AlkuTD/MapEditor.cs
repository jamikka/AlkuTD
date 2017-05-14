/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace AlkuTD
{
    class MapEditor
    {
        Game1 ParentGame;

        public enum DrawMode
        {
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
        public TextCell[] MapEditorTableCells;
        public TextCell[] MapEditorResourceCells;
        public int MapEditorGroupAmt;
        public int MapEditorTableRows;
        public int MapEditorTableCols;
        public Rectangle TableBackground;
        public Rectangle TableBackground2;
        public Rectangle TableBackground3;

        public List<Point> MapEditorSpawnPoints;
        public List<Point> MapEditorGoalPoints;

        int totalGroups; //eehh

        int padding;
        int buttonHeight;
        int topButtonX;
        int topButtonY;
        int menuButtonWidth;
        int menuButtonX;
        int menuButtonY;
        int toolButtonWidth;
        int toolButtonX;
        int toolButtonY;
        Color[] buttonColors;
        Color[] buttonTextColors;


        public MapEditor(Game1 game)
        {
            ParentGame = game;

            padding = 10;
            buttonHeight = Game1.font.LineSpacing + padding - 2;
            menuButtonWidth = (int)Math.Round(Game1.font.MeasureString("MENU").X) + 2 * padding;
            toolButtonWidth = (int)Math.Round(Game1.font.MeasureString("00").X) + 2 * padding;
            menuButtonX = menuButtonWidth / 2;
            menuButtonY = (int)(game.GraphicsDevice.Viewport.Height * 0.5f - 2.5 * buttonHeight);
            topButtonX = (int)(menuButtonWidth * 1.5f);
            topButtonY = 20;
            toolButtonX = game.GraphicsDevice.Viewport.Width - toolButtonWidth * 2;
            toolButtonY = (int)(game.GraphicsDevice.Viewport.Height * 0.5 - 2 * game.HUD.tileOverlay.Height);
            buttonColors = new Color[] { new Color(10, 20, 30), new Color(20, 30, 40), new Color(30, 40, 50) }; //----passive,hovered,pressed
            buttonTextColors = new Color[] { Color.SlateGray, Color.Orange, Color.Orange };//----passive,hovered,pressed
            Color[] transparent = new Color[] { Color.Transparent, Color.Transparent, Color.Transparent };
        }

    }
}*/

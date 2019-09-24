using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class Bar
    {        
        int currentValue;
		public int Min;
        public int Max;
        public int Value
            {
                get { return currentValue; } 
                set { 
					  if (value < Min) 
						  currentValue = Min;
					  else if (value > Max) 
						  currentValue = Max;
					  else currentValue = value;
					  //UpdateFill();
					}
            }
		Rectangle wholeRectangle;
		Rectangle innerRectangle;
		Rectangle fillRectangle;
		public Rectangle Bounds { get { return wholeRectangle; } set { UpdateRectangles(value); } }
        Texture2D texture;
		const int borderWidth = 1;
		int geneIndex;
		public Color[] Colors;
		public bool showingNum;
		Vector2 numDrawPos;

        public Bar(Rectangle bounds, int minValue, int maxValue, int whichGene)
        {
            Min = minValue;
            Max = maxValue;
            texture = CurrentGame.pixel;
			geneIndex = whichGene;

			wholeRectangle = bounds;
			innerRectangle = new Rectangle(Bounds.X + borderWidth, Bounds.Y + borderWidth, Bounds.Width - 2 * borderWidth, Bounds.Height - 2 * borderWidth);
			fillRectangle = innerRectangle;

			if (geneIndex == 0)
				Colors = new Color[3] { new Color(20,20,20), new Color(30,30,30), new Color(60,10,20) }; //R
			else if (geneIndex == 1)
				Colors = new Color[3] { new Color(20,20,20), new Color(30,30,30), new Color(20,60,30) }; //G
			else
				Colors = new Color[3] { new Color(20,20,20), new Color(30,30,30), new Color(20,40,60) }; //B

			numDrawPos = new Vector2(wholeRectangle.Center.X - CurrentGame.font.MeasureString("0").X / 2, wholeRectangle.Bottom);
			showingNum = true;
        }

		public void UpdateFill()
		{
			Value = CurrentGame.players[0].GenePoints[geneIndex];

			numDrawPos.X = wholeRectangle.Center.X - CurrentGame.font.MeasureString(currentValue.ToString()).X / 2;

			fillRectangle.Height = innerRectangle.Height * currentValue / Max;
			fillRectangle.Y = innerRectangle.Bottom - fillRectangle.Height;
		}

		void UpdateRectangles(Rectangle newWholeRect)
		{
			wholeRectangle = newWholeRect;
			innerRectangle = new Rectangle(wholeRectangle.X + borderWidth, wholeRectangle.Y + borderWidth, wholeRectangle.Width - 2 * borderWidth, wholeRectangle.Height - 2 * borderWidth);
			fillRectangle.Height = innerRectangle.Height * currentValue / Max;
			fillRectangle.Y = innerRectangle.Bottom - fillRectangle.Height;
		}

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(texture, Bounds, Colors[0]);
			sb.Draw(texture, innerRectangle, Colors[1]);
			sb.Draw(texture, fillRectangle, Colors[2]);

			if (CurrentGame.gameState != GameState.MapEditor)
			{
				sb.DrawString(CurrentGame.font, currentValue.ToString(), numDrawPos, Color.Wheat);
			}
        }
    }
}

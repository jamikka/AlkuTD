using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace AlkuTD
{
	public abstract class InfoBox
	{
		public List<string> Lines;
		public List<Color> LineColors;
		string text;
		public string Text { get { return text; } set { text = value; } }
		public const int Padding = 4;
		public const int YPadding = 2;
		public Vector2 TextPos;
		public const int DefaultWidth = 66;
		public const int DefaultHeight = 56;
		public static string enter = Environment.NewLine;
		Rectangle bounds;
		public Rectangle Bounds { get { return bounds; } set { bounds = value; UpdateTextPos(); } }
		public int Width { get { return bounds.Width; } set { bounds.Width = value; } }
		public int Height { get { return bounds.Height; } set { bounds.Height = value; } }
		public int PosX { get { return bounds.X; } set { bounds.X = value; UpdateTextPos(); } }
		public int PosY { get { return bounds.Y; } set { bounds.Y = value; UpdateTextPos(); } }
		public Vector2 Pos { get { return new Vector2(Bounds.X, Bounds.Y); } set { bounds.Location = new Point((int)value.X, (int)value.Y); UpdateTextPos(); } }
		//public int Width { get { return Bounds.Width; } set { Bounds.Width = value; } }
		//public int Height { get { return Bounds.Height; } set { Bounds.Height = value; } }
		internal bool hoveredOver;
		internal bool locked;
		public bool justRemoteLocked;


		public InfoBox(Vector2 pos)
		{
			Bounds = new Rectangle((int)pos.X, (int)pos.Y, DefaultWidth, DefaultHeight);
			TextPos = new Vector2(Bounds.X + Padding, Bounds.Y + YPadding);
		}

		internal virtual void UpdateTextPos()
		{
			TextPos = new Vector2(bounds.X + Padding, bounds.Y + YPadding);
		}

		public virtual void Update(MouseState mouse, MouseState prevMouse)
		{
			if (Bounds.Contains(mouse.X, mouse.Y))
			{
				hoveredOver = true;
				if (mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released && !justRemoteLocked)
				{
					locked = !locked;
				}
			}
			else hoveredOver = false;

			justRemoteLocked = false;
		}

		public virtual void Draw(SpriteBatch sb, float fadeAmt)
		{
			if (locked)
			{
				Rectangle borderRec = new Rectangle(Bounds.X - 1, Bounds.Y - 1, Bounds.Width + 2, Bounds.Height + 2);
				sb.Draw(CurrentGame.pixel, borderRec, Color.DimGray * 0.5f * fadeAmt);
			}
			sb.Draw(CurrentGame.pixel, Bounds, new Color(20,20,20) * 0.75f * fadeAmt);
		}
	}
}

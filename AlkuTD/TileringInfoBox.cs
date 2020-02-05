using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace AlkuTD
{
	class TileringInfoBox : InfoBox
	{
		static float costStringWidth;
		public new const int DefaultWidth = 120;
		public new const int DefaultHeight = 38;

		public TileringInfoBox(Vector2 pos, Color?[] specColors, params string[] lines)
			: base(pos)
		{
			Lines = new List<string>();
			LineColors = new List<Color>();
			if (specColors != null)
			{
				for (int i = 0; i < specColors.Length; i++)
				{
					LineColors.Add(specColors[i].Value);
				}
			}
			costStringWidth = CurrentGame.font.MeasureString("Cost: ").X;
			for (int i = 0; i < lines.Length; i++)
			{
				string enters;
				if (i == lines.Length -1 && LineColors.Count > 0)
					enters = string.Concat(Enumerable.Repeat<string>(Environment.NewLine, Math.Max(i-1, 0)));
				else 
					enters = string.Concat(Enumerable.Repeat<string>(Environment.NewLine, i));

				Lines.Add(enters + lines[i]);
			}
			GenerateText();
		}

		void GenerateText()
		{
			if (LineColors.Count > 0)
				Height = Math.Max(Lines.Count - 1, 1) * CurrentGame.font.LineSpacing + YPadding;
			else
				Height = Lines.Count * CurrentGame.font.LineSpacing + YPadding;

			float widest = 0;
			for (int i = 0; i < Lines.Count; i++)
			{
				float currentLineWidth = CurrentGame.font.MeasureString(Lines[i]).X;
				if (currentLineWidth > widest)
					widest = currentLineWidth;
			}
			Width = (int)widest + 2 * Padding;
		}

		public override void Draw(SpriteBatch sb, float fadeAmt)
		{
			base.Draw(sb, fadeAmt);
			for (int i = 0; i < Lines.Count; i++)
			{
				if (Lines.Count > 1)
				{
					if (i == Lines.Count - 1 && LineColors.Count > 0)
						sb.DrawString(CurrentGame.font, Lines[i], TextPos + new Vector2(costStringWidth, 0), LineColors[0] * fadeAmt);
					else
						sb.DrawString(CurrentGame.font, Lines[i], TextPos, Color.LightGray * fadeAmt);
				}
				else if (LineColors.Count > 0)
					sb.DrawString(CurrentGame.font, Lines[i], TextPos, LineColors[0] * fadeAmt);
				else
					sb.DrawString(CurrentGame.font, Lines[i], TextPos, Color.LightGray * fadeAmt);

			}
		}
	}
}

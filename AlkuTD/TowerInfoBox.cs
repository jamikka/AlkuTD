using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlkuTD
{
	class TowerInfoBox : InfoBox
	{
		public Tower Target;
		public new const int DefaultWidth = 90;
		public new const int DefaultHeight = 56;
		Rectangle NameHighlightRec;
		Rectangle CostHighlightRec;
		public bool IsBuildInfo;

		public TowerInfoBox(Tower target, Vector2 location, bool isBuildInfo)
			: base(location)
		{
			Target = target;
			GenerateText();

			IsBuildInfo = isBuildInfo;
			if (isBuildInfo)
			{
				string enters = string.Concat(Enumerable.Repeat<string>(Environment.NewLine, Lines.Count));
				Lines.Add(enters + " Cost: " + target.Cost);
				Height += CurrentGame.font.LineSpacing;
				float costStringWidth = CurrentGame.font.MeasureString(" Cost: " + target.Cost).X;
				if (costStringWidth + TowerInfoBox.Padding * 2 > Width)
					Width = (int)costStringWidth + TowerInfoBox.Padding * 2;
				LineColors.Add(Color.White);
			}
		}

		internal override void UpdateTextPos()
		{
			TextPos = new Vector2(PosX + Padding, PosY + YPadding);
			NameHighlightRec = new Rectangle(PosX, PosY, Width, CurrentGame.font.LineSpacing);
			CostHighlightRec = new Rectangle(PosX, Bounds.Bottom - CurrentGame.font.LineSpacing - YPadding, Width, CurrentGame.font.LineSpacing + YPadding);
		}

		void GenerateText()
		{
			Lines = new List<string>();
			LineColors = new List<Color>();

			Lines.Add(Target.Name);
			LineColors.Add(Color.SlateGray);

			if (Target.slow[0] > 0)
			{
				Lines.Add(enter + "Slo: " + ((int)(Target.slow[0] * 100)).ToString() + " (" + Math.Round(((Target.slow[1] * CurrentGame.LoopTimeTicks * 0.0000001)), 2) + "s)");
				LineColors.Add(Color.Aquamarine);
			}
			else
			{
				Lines.Add(enter + "Dmg: " + Target.Dmg.ToString());
				LineColors.Add(Color.Orange);
			}

			Lines.Add(enter + enter + "Spd: " + Math.Round(Target.FireRateSec, 2).ToString());
			LineColors.Add(Color.PaleTurquoise);

			if (Target.slow[0] == 0)
			{
				Lines.Add(enter + enter + enter + "DPS: " + Math.Round(Target.DPS, 2).ToString() + enter);
				LineColors.Add(Color.Red);
			}

			if (Target.GeneSpecs.HasAny)
			{
				string geneString = "";
				GeneType gt = Target.GeneSpecs.GetPrimaryElem();
				switch (gt)
				{
					case GeneType.Red: geneString = "R: "; LineColors.Add(Color.Red); break;
					case GeneType.Green: geneString = "G: "; LineColors.Add(Color.ForestGreen); break;
					case GeneType.Blue: geneString = "B: "; LineColors.Add(Color.CornflowerBlue); break;
				}
				string enters = string.Concat(Enumerable.Repeat<string>(enter, Lines.Count));
				Lines.Add(enters + geneString + Math.Round(Target.GeneSpecs.GetPrimaryElemStrength() * 100).ToString() + "%");
			}

			if (Target.SplashRange > 0)
			{
				string enters = string.Concat(Enumerable.Repeat<string>(enter, Lines.Count));
				Lines.Add(enters + "Blast: " + Target.SplashRange);
				LineColors.Add(Color.Yellow);
			}

			Height = Lines.Count * CurrentGame.font.LineSpacing + YPadding;
			float widest = 0;
			for (int i = 0; i < Lines.Count; i++)
			{
				float currentLineWidth = CurrentGame.font.MeasureString(Lines[i]).X;
				if (currentLineWidth > widest)
					widest = currentLineWidth;
			}
			Width = (int)widest + 2 * Padding;

			//Pos = Target.ScreenLocation - new Vector2(-20, Height * 0.5f);
		}

		public override void Draw(SpriteBatch sb, float fadeAmt)
		{
			base.Draw(sb, fadeAmt);
			sb.Draw(CurrentGame.pixel, NameHighlightRec, Color.Black * 0.8f * fadeAmt);
			if (IsBuildInfo)
				sb.Draw(CurrentGame.pixel, CostHighlightRec, Color.Black * 0.8f * fadeAmt);
			for (int i = 0; i < Lines.Count; i++)
				sb.DrawString(CurrentGame.font, Lines[i], TextPos, LineColors[i] * fadeAmt);
		}
	}
}

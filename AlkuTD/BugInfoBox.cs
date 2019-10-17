using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlkuTD
{
	public class BugInfoBox : InfoBox
	{
		public Creature Target;
		GroupInfoBox ParentBox;
		float hpStringWidth;
		float initHpStringWidth;
		readonly float slashCharWidth;
		bool IsMovingTarget;
		bool abovePoint;

		public BugInfoBox(Vector2 pos)
			: base(pos)
		{}

		public BugInfoBox (Vector2 pos, Creature target, bool isMovingTarget, bool aboveSpawnPoint, GroupInfoBox gibParent) 
			: base (pos)
		{
			Target = target;
			IsMovingTarget = isMovingTarget;
			abovePoint = aboveSpawnPoint;
			ParentBox = gibParent;
			initHpStringWidth = CurrentGame.font.MeasureString(((int)Target.InitHp).ToString()).X;
			slashCharWidth = CurrentGame.font.MeasureString("/").X;

			UpdateText();
		}

		void UpdateText()
		{
			//Text = "";
			int lines = 2;
			string hpString = ((int)Target.Hp).ToString();
			hpStringWidth = CurrentGame.font.MeasureString(hpString).X;
			float nameStringWidth = CurrentGame.font.MeasureString(Target.Name).X;
			Lines = new List<string>();
			LineColors = new List<Color>();
			Lines.Add(/*"Name: " + */Target.Name);
			LineColors.Add(Color.SlateGray);
			Lines.Add(/*"HP: " + */enter + Target.Hp); //+ "/" + target.InitHp);
			LineColors.Add(Color.Orange);
			if (IsMovingTarget)
			{
				Lines.Add(/*enter + hpDigits + */enter + '/' + Target.InitHp.ToString());
				LineColors.Add(Color.SlateGray);

				Width = Padding * 2 + (int)Math.Max(hpStringWidth + slashCharWidth + initHpStringWidth, nameStringWidth);
			}
			else
				Width = Padding * 2 + (int)nameStringWidth;

			if (Target.ElemArmors.HasAny)
			{
				GeneType gt = Target.ElemArmors.GetPrimaryElem();
				Lines.Add(/*target.ElemArmors.GetPrimaryElem().ToString() + ": " + */enter + enter + ((int)((Target.ElemArmors.GetPrimaryElemStrength()) * 100)).ToString() + "%");
				lines++;
				switch (gt)
				{
					case GeneType.Red: LineColors.Add(Color.Red); break;
					case GeneType.Green: LineColors.Add(Color.ForestGreen); break;
					case GeneType.Blue: LineColors.Add(Color.CornflowerBlue); break;
				}
			}
            if (Target.isSlowed)
            {
                lines++;
                Lines.Add(enter + enter + enter + "Spd ." + (int)((Target.Speed / Target.defSpeed) * 100));
                LineColors.Add(Color.Aquamarine);
                Width = Padding * 2 + (int)Math.Max(Width, CurrentGame.font.MeasureString(Lines.Last<string>()).X);
            }
				
			Height = lines * CurrentGame.font.LineSpacing + YPadding;
			if (ParentBox != null)
			{
				if (lines < 3)
				{
					if (abovePoint)
						PosY = ParentBox.Bounds.Top - Bounds.Height;
					else
						PosY = ParentBox.Bounds.Bottom;
				}
				PosX = (int)(ParentBox.Bounds.Center.X - Width * 0.5f);
			}

			//for (int i = 0; i < Lines.Count; i++)
			//    Text += Lines[i] + Environment.NewLine;
		}

		public override void Update(MouseState mouse, MouseState prevMouse)
		{
			base.Update(mouse, prevMouse);
			UpdateText();
		}

		public override void Draw(SpriteBatch sb, float fadeAmt)
		{
			base.Draw(sb, fadeAmt);
			if (IsMovingTarget)
			{
				for (int i = 0; i < Lines.Count; i++)
				{
					if (i == 1)
						sb.DrawString(CurrentGame.font, Lines[i], TextPos, Target.HpBarColor * fadeAmt);
					else if (i == 2)
						sb.DrawString(CurrentGame.font, Lines[i], TextPos + new Vector2(hpStringWidth, 0), LineColors[i] * fadeAmt);
					else sb.DrawString(CurrentGame.font, Lines[i], TextPos, LineColors[i] * fadeAmt);
				}
				return;
			}

			for (int i = 0; i < Lines.Count; i++)
				sb.DrawString(CurrentGame.font, Lines[i], TextPos, LineColors[i] * fadeAmt);
		}
	}
}

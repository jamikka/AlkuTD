using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlkuTD
{
	public class GroupInfoBox : InfoBox
	{
		SpawnGroup Group;
		Texture2D SmallBugTex;
		Vector2 TexOrigin;
		const float textScale = 0.66f;
		const float bugScale = 2f;
		public const int boxWidth = 27;
		public const int boxHeight = 36;
		public BugInfoBox BugBox;
		//internal bool AbovePoint;

		public GroupInfoBox(Vector2 pos, SpawnGroup spg, bool aboveSpawnPoint)
			: base(pos)
		{
			Group = spg;
			SmallBugTex = spg.InfoTexture;
			TexOrigin = spg.infoTexOrigin;
			//AbovePoint = aboveSpawnPoint;
			//TexOrigin.X += 0.5f;
			Bounds = new Rectangle((int)pos.X, (int)pos.Y, boxWidth, boxHeight);//(int)(CurrentGame.font.LineSpacing * textScale) + boxWidth);
			//TextPos = new Vector2(Bounds.X + Padding, Bounds.Y + YPadding);
			if (aboveSpawnPoint)
			{
				PosY -= 2;
				BugBox = new BugInfoBox(new Vector2(Bounds.Center.X - DefaultWidth * 0.5f, Bounds.Top - DefaultHeight), Group.ExampleCreature, false, aboveSpawnPoint, this);
			}
			else
			{
				PosY += 1;
				BugBox = new BugInfoBox(new Vector2(Bounds.Center.X - DefaultWidth * 0.5f, Bounds.Bottom), Group.ExampleCreature, false, aboveSpawnPoint, this);
			}
		}

		public override void Update(MouseState mouse, MouseState prevMouse)
		{
			base.Update(mouse, prevMouse);

			if (hoveredOver && mouse.LeftButton == ButtonState.Pressed && prevMouse.LeftButton == ButtonState.Released)
				BugBox.locked = locked;
		}

		public override void Draw(SpriteBatch sb, float fadeAmt)
		{
			base.Draw(sb, fadeAmt);
			//sb.Draw(CurrentGame.pixel, Bounds, Color.LightGray * 0.4f * fadeAmt);
			string creatureCount = Group.AliveCreatures.Count.ToString();
			int textOriginX = (int)(textScale * CurrentGame.font.MeasureString(creatureCount).X * 0.5f);
            Vector2 numTexPos = new Vector2(Bounds.Center.X - textOriginX, Bounds.Y + YPadding + 2);

            sb.DrawString(CurrentGame.font, creatureCount, numTexPos, Color.White * fadeAmt, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
			sb.Draw(SmallBugTex, new Vector2(Bounds.Center.X, Bounds.Bottom - SmallBugTex.Height), null, Color.White * fadeAmt, 0, TexOrigin, bugScale, SpriteEffects.None, 0); //---------vanhakomment: suhteuta infoTex.Width scaleen!
			if (hoveredOver || locked)
			{
				BugBox.Draw(sb, 1);
			}
            //string whatString = (((float)Group.spawnTimer / Group.GroupDuration)*100).ToString();
            //sb.DrawString(CurrentGame.font, whatString, numTexPos + new Vector2(5, 5), Color.Beige);

            // GroupDurationBars
            sb.Draw(CurrentGame.pixel, new Rectangle(PosX, (int)(numTexPos.Y + boxHeight - 5 - 1), boxWidth, 4), Color.Black); // black background
            sb.Draw(CurrentGame.pixel, new Rectangle(PosX + 1, (int)(numTexPos.Y + boxHeight - 5), (int)((boxWidth - 2) * Math.Min((float)Group.spawnTimer / Group.GroupDuration, 1)), 2), Color.White);
        }
	}
}

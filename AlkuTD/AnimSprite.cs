using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class AnimSprite
    {
        public Texture2D Spritesheet { get; set; }
        Rectangle frameRectangle;
        public Rectangle FrameRectangle { get { return frameRectangle; } set { frameRectangle = value; } }
        public Vector2 Location
        {
            get { return new Vector2(frameRectangle.X, frameRectangle.Y); }
            set { frameRectangle.Location = new Point((int)value.X, (int)value.Y); }
        }
        public int Rows { get; set; }
        public int Columns { get; set; }
        int totalFrames;
        int currentFrame = 0;
        public int CurrentFrame { get { return currentFrame; } set { currentFrame = value; } }
		Vector2 Origin;

        byte AnimationUpdatePhase = 0;


        public AnimSprite(Texture2D spritesheet, Point location, int rows, int columns)
        {
            Spritesheet = spritesheet;            
            Rows = rows;
            Columns = columns;
            totalFrames = rows * columns;
            frameRectangle = new Rectangle(location.X, location.Y, Spritesheet.Width / Columns, Spritesheet.Height / Rows);
			Origin = new Vector2(frameRectangle.Width / 2, frameRectangle.Height / 2);
        }

        public void Update()
        {
            currentFrame++;
            if (currentFrame == totalFrames) currentFrame = 0;
        }
        
        public void MetroidUpdate()
        {            
            AnimationUpdatePhase++;
            if (AnimationUpdatePhase == 50) AnimationUpdatePhase = 0;

            if (AnimationUpdatePhase < 20) currentFrame = 0;
            else if (AnimationUpdatePhase >= 30 && AnimationUpdatePhase < 40) currentFrame = 2;
            else currentFrame = 1;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int row = (int)((float)currentFrame / (float)Columns);
            int column = currentFrame % Columns;
            Rectangle sourceRect = new Rectangle(FrameRectangle.Width * column, FrameRectangle.Height * row, FrameRectangle.Width, FrameRectangle.Height);

            spriteBatch.Draw(Spritesheet, frameRectangle, sourceRect, Color.White);
        }

		public void JustDraw(Vector2 pos, float splashRange, SpriteBatch spriteBatch)
		{
			if (AnimationUpdatePhase == 16) 
				AnimationUpdatePhase = 0;
			currentFrame = Columns * AnimationUpdatePhase / 16;

			Rectangle sourceRect = new Rectangle(FrameRectangle.Width * currentFrame, 0, FrameRectangle.Width, FrameRectangle.Height);

			float sizeModifier = splashRange / (FrameRectangle.Width / 2f);

			spriteBatch.Draw(Spritesheet, pos, sourceRect, Color.White, 0, Origin, sizeModifier, SpriteEffects.None, 0);
			AnimationUpdatePhase++;
		}

    }
}

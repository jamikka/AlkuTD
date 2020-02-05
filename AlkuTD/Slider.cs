using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    class Slider
    {        
        double sliderValue;
        public double Min { get; set; }
        public double Max { get; set; }
        public double Value
            {
                get { return sliderValue; } 
                set { if (value < Min) sliderValue = Min;
                     else if (value > Max) sliderValue = Max;
                     else sliderValue = value; }
            }
        public Rectangle BoxRectangle { get; set; }
        Rectangle sliderRectangle;
        Texture2D texture;



        public Slider(Rectangle sliderBoxRectangle, double minValue, double maxValue, Texture2D texture)
        {
            BoxRectangle = sliderBoxRectangle;
            Min = minValue;
            Max = maxValue;
            this.texture = texture;

            sliderRectangle = new Rectangle(BoxRectangle.X + 1, BoxRectangle.Y + 2, BoxRectangle.Width-2, 2);
        }

        public void Update()
        {
            sliderRectangle.Y = (int)((sliderValue * ((BoxRectangle.Height - 5) / (Max - Min)))+ BoxRectangle.Y) + 2;
        }

        public void Draw(SpriteBatch s)
        {
            s.Draw(texture, BoxRectangle, Color.Aqua);
            s.Draw(texture, sliderRectangle, Color.Black);
        }
    }
}

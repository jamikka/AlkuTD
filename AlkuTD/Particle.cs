using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class Particle
    {
        public Texture2D Texture { get; set; }
        public Rectangle? TexPortion { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 Position;
        public Vector2 Speed; //to get to modify X and Y hmmmmm....
        public Color Color { get; set; }
        public float Size { get; set; }        
        public float RotationAngle { get; set; }
        public float RotationSpeed { get; set; }
        public int TTL { get; set; }
        int InitTTL;
        public float Deceleration;

        public Particle(Texture2D t, Rectangle? texPortion, Vector2 pos, Vector2 speed, float acc, Color color, float size, float rotationAngle, float rotationSpeed, int timeToLive)
        {
            Texture = t;
            TexPortion = (texPortion.HasValue) ? texPortion.Value : new Rectangle(0,0,Texture.Width,Texture.Height);
            Origin = new Vector2(Texture.Width / 2, Texture.Height / 2);
            Position = pos;
            Speed = speed;
            Color = color;
            Size = size != 0 ? size : 1f;
            RotationAngle = rotationAngle;
            RotationSpeed = rotationSpeed;
            TTL = timeToLive;
            InitTTL = TTL;
            Deceleration = acc;
        }

        public void Update()
        {
            TTL--;
            Position += Speed;
            Speed *= Deceleration;
            //RotationAngle += RotationSpeed;     
        }

        public void DropUpdate()
        {
            TTL--;
            Speed.X *= 0.995f;
            Speed.Y *= 1.035f;
            Position += Speed;
            RotationAngle += RotationSpeed;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, TexPortion, Color * Math.Min(1,(TTL / (InitTTL * 0.4f))) * 0.6f, RotationAngle, Origin, Size, SpriteEffects.None, 0.42f);
        }

    }
}

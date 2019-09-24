using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class Bullet
    {
        //------------Fields-------------------------------
		HexMap ParentMap;
        Random Rnd;
        public Vector2 Target; //------maahan ampuvia torneja varten, ei vielä implemented
        public Creature targetCreature;
        public float speed;
        public float angle;
        public float dmg;
		public DmgType DmgType;
        public float[] slow = new float[2];    //[0] = percentage, [1] = duration 
        public Vector2 originPoint;
        public bool active;
		public Texture2D texture;
		public AnimSprite ExplosionAnim;
        Vector2 textureOrigin;
        public Vector2 location;

        Vector2 dir; //----------hm!

		public float SplashRange;

		public bool Exploding;
		int ExplosionCounter;
		Vector2 ExplosionLocation;

		public GeneSpecs ElemSpecs;

        //--------Constructors------------------------------
        public Bullet(Creature targetCreature,
                      float speed,
                      float dmg,
					  DmgType dmgType,
					  float splashRange,
                      float[] slow,
                      GeneSpecs elems,
                      Vector2 originPoint,
                      Texture2D texture,
					  HexMap currMap)
        {
            this.targetCreature = targetCreature;
            this.speed = speed;
            this.dmg = dmg;
            DmgType = dmgType;
            this.slow = slow;
			ElemSpecs = elems;
            this.originPoint = originPoint;
            this.texture = texture;

            //active = true;
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);

            //Rnd = targetCreature.ParentMap.rnd;
            //rndWobble = new Vector2((float)(Rnd.NextDouble() - 0.5), (float)(Rnd.NextDouble() - 0.5));

            //ShootAt(targetCreature);

			SplashRange = splashRange;
			ParentMap = currMap;

			ExplosionAnim = new AnimSprite(ParentMap.ParentGame.Content.Load<Texture2D>("MetroPlos"), Point.Zero, 1, 5);
        }

        public Bullet(Vector2 target,
                      float speed,
                      float dmg,
					  DmgType dmgType,
                      float[] slow,
                      Vector2 originPoint,
                      Texture2D texture)
        {
            this.speed = speed;
            this.dmg = dmg;
            DmgType = dmgType;
            this.slow = slow;
            this.originPoint = originPoint;
            this.texture = texture;

            //active = true;
            textureOrigin = new Vector2(texture.Width / 2, texture.Height / 2);

            Rnd = targetCreature.ParentMap.rnd;
            //ShootAt(target);
        }

        #region--------Destructor------------------------------
        //~Bullet()
        //{
        //    Debug.WriteLine("Bullet is being destructed ({0})", GetHashCode());
        //}
        #endregion

        //------------Methods-------------------------
        
        public void ShootAt(Creature targetCreature)
        {
            location = originPoint;
            this.targetCreature = targetCreature;
            angle = (float)Math.Atan2(location.Y - targetCreature.PosY, location.X - targetCreature.PosX); //---------ei välttis tarvii jos ammutaan palloja (animoidut ja muotoillut toki)
            active = true;
        }
		public void ShootAt(Vector2 targetLocation)
		{
			location = originPoint;
			Target = targetLocation;
			//angle = (float)Math.Atan2(location.Y - target.Y, location.X - target.X); //---------ei välttis tarvii jos ammutaan palloja (animoidut ja muotoillut toki)
			active = true;
		}
        #region 2 Old ShootAt
        /*
        public void ShootAt(Vector2 target)
        {
            location = origin;
            this.targetCreature = null;
            this.target = target;
            angle = (float)Math.Atan2(location.Y - target.Y, location.X - target.X);
            active = true;
        }
        */
        #endregion

		void Explode()
		{
			for (int i = 0; i < ParentMap.AliveCreatures.Count; i++)
			{
				Creature currCreature = ParentMap.AliveCreatures[i];
				if (Vector2.Distance(currCreature.Location, location) <= SplashRange)
				{
					currCreature.TakeAHit(this);
				}
			}

			ExplosionLocation = location;
			ExplosionCounter = 16;
			Exploding = true;

			active = false;
		}

        public Vector2 rndWobble;
        public void Update()
        {
            if (active)
            {
                if (DmgType == AlkuTD.DmgType.Splash) //--------------splash-torneja varten, ei vielä käytössä
                {
                    if (Vector2.Distance(location, Target) <= speed)
                    {
						Explode();
                        return;
                    }

                    //angle = (float)Math.Atan2(location.Y - target.Y, location.X - target.X);
					dir = Vector2.Normalize(Target - location);
				}
                else
                {
                    if (Vector2.Distance(location, targetCreature.Location) <= speed)
                    {
                        targetCreature.TakeAHit(this);
                        active = false;
                        return;
                    }
                    //angle = (float)Math.Atan2(location.Y - targetCreature.PosY, location.X - targetCreature.PosX);
					dir = Vector2.Normalize(targetCreature.Location - location);
				}
                //location.X -= speed * (float)Math.Cos(angle);
                //location.Y -= speed * (float)Math.Sin(angle);
                //rndWobble = new Vector2(CurrentGame.


                location += dir * speed /*+ rndWobble*/;
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (active)
                spritebatch.Draw(texture, location, null, Color.Cornsilk, angle, textureOrigin, 1f, SpriteEffects.None, 1);
        }

		public void DrawExplosion(SpriteBatch spritebatch)
		{
			if (ExplosionCounter > 0)
			{
				ExplosionCounter--;
				ExplosionAnim.JustDraw(ExplosionLocation, SplashRange, spritebatch);
			}
			else
				Exploding = false;
		}

        #region Disabled Print & Getsetters
        /*Printing method
        public void GetInfo()
        {
            Console.WriteLine("Bullet info\n" +
                              " Speed:\t\t{0}\n" +                              
                              " Damage:\t{1}\n" +
                              " Damage type:\t{2}\n" +
                              " Slow amount:\t{3} %\n" +
                              " Slow duration:\t{4}\n" +
                              " Elemental multipliers:\n" +
                              "    Red:\t{5} %\n" +
                              "    Green:\t{6} %\n" +
                              "    Blue:\t{7} %\n" +
                              " Origin: x:{8} y:{9}\n",
           speed, dmg, dmgType, slow[0], slow[1],
           redMult, greenMult, blueMult, origin[0], origin[1]);                        
        }
        */
        //----------Getsetters---------------------------
        /*public float[] Location
        {
            get { return location; }
            set { location = value; }
        }*//*
        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        public short Dmg
        {
            get { return dmg; }
            set { dmg = value; }
        }
        public byte DmgType
        {
            get { return dmgType; }
            set { dmgType = value; }
        }
        public int[] Slow
        {
            get { return slow; }
            set { slow = value; }
        }
        public int SlowAmount
        {
            get { return slow[0]; }
            set { slow[0] = value; }
        }
        public int SlowDuration
        {
            get { return slow[1]; }
            set { slow[1] = value; }
        }
        /*public int this[int slowindex]
        {
            get { return slow[slowindex]; }
            set { slow[slowindex] = value; }
        }*//*
        public byte RedMult
        {
            get { return redMult; }
            set { redMult = value; }
        }
        public byte GreenMult
        {
            get { return greenMult; }
            set { greenMult = value; }
        }
        public byte BlueMult
        {
            get { return blueMult; }
            set { blueMult = value; }
        }
        public Vector2 Origin
        {
            get { return origin; }
            set { origin = value; }
        }*/
        #endregion
    }


        
        
}

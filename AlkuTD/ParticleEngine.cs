using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class ParticleEngine
    {
        public Creature SourceCreature;
        public int MinParticleLifeTime;
        public int MaxParticles;
        public int InitMaxParticles; // for trail fade out in Creature.Update();
        public float ParticleSpeed;
        public float ParticleDeceleration;
        public Texture2D Texture;
        Vector2 location;
        Random Random;
        List<Particle> Particles;
        public Vector2 Location { get { return location; } set { location = value; foreach (Particle p in Particles) p.Position = value; } }
        public float PosX { get { return location.X; } set { location.X = value; } }
        public float PosY { get { return location.Y; } set { location.Y = value; } }
        Vector2 emitterAngle;
        public Vector2 EmitterAngle { get { return emitterAngle; } set { emitterAngle = value; foreach (Particle p in Particles) p.Speed += value * 2; } } // ------- DÄÄL JÄNDZGII JUDUU
        public bool IsActive { get; set; }

		int timer; //-----------TRAIL


        //-----------Constructors---------

        public ParticleEngine(Texture2D particleTexture, Vector2 emitterLocation, int maxParticles)
        {
            Texture = particleTexture;
            location = emitterLocation;
            Particles = new List<Particle>();
            MaxParticles = maxParticles;
            InitMaxParticles = maxParticles;
            if (MinParticleLifeTime == 0)
                MinParticleLifeTime = 5;
            ParticleSpeed = 10;
            ParticleDeceleration = 0.4f; //jkiva 0.65 & speed 8
        }

        public ParticleEngine(Texture2D particleTexture, Vector2 emitterLocation, int maxParticles, int minParticleLifeTime, Random rnd)
            : this(particleTexture, emitterLocation, maxParticles)
        {
            MinParticleLifeTime = minParticleLifeTime;
            Random = rnd;
            //for (int i = 0; i < MaxParticles; i++)
            //   Particles.Add(GenerateNew());
        }

        public ParticleEngine(Texture2D particleTexture, Vector2 emitterLocation, int minParticleLifeTime, int maxParticles, Random rnd, Vector2 emitterAngle)
            : this(particleTexture, emitterLocation, maxParticles, minParticleLifeTime, rnd)
        {
            EmitterAngle = emitterAngle;
        }

        //-----------Methods---------------

        public Particle GenerateNew()
        {
            float rnd1 = (float)(Random.NextDouble() - 0.5) * 1.5f;
            float rnd2 = (float)(Random.NextDouble() - 0.5) * 1.5f;
            if (/*rnd1 > 0.2 || rnd1 < -0.2*/ Random.Next(100) > 5)
                rnd1 = (float)(Random.NextDouble() - 0.5) * 0.5f;
            if (/*rnd2 > 0.2 || rnd2 < -0.2*/ Random.Next(100) > 5)
                rnd2 = (float)(Random.NextDouble() - 0.5) * 0.5f;
            Vector2 speed = new Vector2(rnd1 * ParticleSpeed, rnd2 * ParticleSpeed);
            //Vector2 speed = new Vector2((float)(Math.Cos(random.NextDouble()) * Math.Cos(EmitterAngle)),
            //                            (float)(Math.Sin(random.NextDouble()) * Math.Sin(EmitterAngle)));
            //Color color = new Color(0.5f + (float)random.NextDouble() * 0.5f,
            //                        0.5f + (float)random.NextDouble() * 0.5f,
            //                        0.5f + (float)random.NextDouble() * 0.5f);
            //color *= 0.5f + (float)random.NextDouble(); //----alpha amount (doesn't work above with white textures (?))
			Color color = Color.AntiqueWhite;
            float size = 1;
            float rotationAngle = 0.0f;
            float rotationSpeed = 0.0f;
            int ttl = 20;
			//int ttl = MinParticleLifeTime; //-----------TRAIL

            return new Particle(Texture, null, location, speed, ParticleDeceleration, color, size, rotationAngle, rotationSpeed, ttl);
        }

		public Particle GenerateNewTrailParticle()
		{
			Vector2 speed = Vector2.Zero;
			Color color = Color.AntiqueWhite * 0.2f;
			float size = 3;
			float rotationAngle = 0.1f;
			float rotationSpeed = 0.0f;
			int ttl = MinParticleLifeTime;

			return new Particle(Texture, null, location, speed, ParticleDeceleration, color, size, rotationAngle, rotationSpeed, ttl);
		}

        public void Reset()
        {
            Particles.Clear();
            for (int i = 0; i < MaxParticles; i++)
                Particles.Add(GenerateNew());
        }
        

        //private Particle GenerateNewMultipleTex()
        //{
        //    Texture2D t = Texture[Random.Next(Texture.Count)];
        //    Vector2 speed = new Vector2((float)Random.NextDouble(), (float)Random.NextDouble());
        //    Color color = new Color(255, 255, 255);
        //    float size = 2;
        //    float rotationAngle = 0;
        //    float rotationSpeed = 0.01f;
        //    int ttl = 20 + Random.Next(50);

        //    return new Particle(t, null, location, speed, color, size, rotationAngle, rotationSpeed, ttl);
        //}


        //public void UpdateMultipleTex()
        //{
        //    for (int i = 0; i < MaxParticles; i++) Particles.Add(GenerateNewMultipleTex());
        //    for (int j = 0; j < Particles.Count; j++)
        //    {
        //        Particles[j].Update();
        //        if (Particles[j].TTL <= 0)
        //        {
        //            Particles.RemoveAt(j);
        //            j--;
        //        }
        //    }
        //}


        public void Update()
        {
            if (IsActive)
            {
                for (int j = 0; j < Particles.Count; j++)
                {
                    Particles[j].Update();
                    if (Particles[j].TTL <= 0)
                    {
                        Particles.RemoveAt(j);
                        j--;
                    }
                }
                if (Particles.Count == 0)
                    IsActive = false;
            }
        }

		public void UpdateTrail()
		{
			IsActive = true;
			location = SourceCreature.Location;
			for (int j = 0; j < Particles.Count; j++)
			{
				Particles[j].Update();
				if (Particles[j].TTL <= 0)
				{
					Particles.RemoveAt(j);
					j--;
				}
			}
			timer++;
			if (Particles.Count < MaxParticles && timer % 2 == 0)
				Particles.Add(GenerateNewTrailParticle());
            else if (Particles.Count > MaxParticles)
            {
                Particles.RemoveRange(0, Particles.Count - MaxParticles);
            }
		}

        public void Draw(SpriteBatch spriteBatch)
        {
            for (int k = 0; k < Particles.Count; k++)
                Particles[k].Draw(spriteBatch);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;

namespace AlkuTD
{
    public class FloatingParticle
    {
        const float FloatSpeed = 0.5f;
        public CurrentGame ParentGame;
        public HexMap ParentMap;
        public Texture2D Spritesheet { get; set; }
        public string Type; //hmm
        public Vector2 Location;
        public List<Vector2> Path;
        public List<Vector2> OrigPath;
        int nextWaypoint;
        public GeneSpecs Elems;
        public int EnergyBounty;
        public float Speed;
        public float Angle; //ehkei tarvi jos pallo tms
        public float Spin; //ehkei tarvi jos pallo tms
        public float DistanceToGoal; // tää bugaa koska öröt pyöristää reitin kulmia (alempana if (distanceToNextWaypoint < 20) 
        public List<PseudoPod> ArmsTargetingThis;
        public bool SuppressUpdate;
        Color ParticleElementColor;
        float BountySizeScale;

        public Creature SourceCreature;

        public FloatingParticle (Creature sourceCreature)
        {
            ParentGame = sourceCreature.ParentGame;
            ParentMap = sourceCreature.ParentMap;
            Location = sourceCreature.Location;
            EnergyBounty = sourceCreature.EnergyBounty;
            Elems = sourceCreature.ElemArmors;
            Path = sourceCreature.Path;
            OrigPath = sourceCreature.OrigPath;
            nextWaypoint = sourceCreature.nextWaypoint;
            SourceCreature = sourceCreature;
            Speed = FloatSpeed;
            Spin = (float)(ParentMap.rnd.NextDouble() - 0.5f) * 0.08f;
            BountySizeScale = Math.Min((EnergyBounty) * 0.66f, 3f);
            //Spritesheet = CurrentGame.ball;
            Spritesheet = ParentMap.ParentGame.Content.Load<Texture2D>("flake-5x5");
            ArmsTargetingThis = new List<PseudoPod>();
            switch (Elems.GetPrimaryElem())
            {
                case AlkuTD.GeneType.None:
                    ParticleElementColor = Color.White;
                    break;
                case AlkuTD.GeneType.Red: 
                    ParticleElementColor = Color.Red;
                    break;
                case AlkuTD.GeneType.Green:
                    ParticleElementColor = Color.Green;
                    break;
                case AlkuTD.GeneType.Blue:
                    ParticleElementColor = Color.Blue;
                    break;
            }
        }

        public void ReleaseYieldAtGoal()
        {
            ParentMap.Players[0].EnergyPoints += EnergyBounty;
            ParentMap.FloatingParticles.Remove(this);
            ParentMap.towerCue = CurrentGame.soundBank.GetCue("pluip2");
            ParentMap.towerCue.Play();
        }

        public void ReleaseYieldAtEater(ParticleEaterTower eater, PseudoPod arm)
        {
			ParentMap.Players[0].EnergyPoints += (int)Math.Round(EnergyBounty * eater.EnergyMultiplier);
            ParentMap.Players[0].GenePoints[0] += (int)Math.Round(Elems[GeneType.Red] * 10 * eater.GeneMultiplier);
            ParentMap.Players[0].GenePoints[1] += (int)Math.Round(Elems[GeneType.Green] * 10 * eater.GeneMultiplier);
            ParentMap.Players[0].GenePoints[2] += (int)Math.Round(Elems[GeneType.Blue] * 10 * eater.GeneMultiplier);
            CurrentGame.HUD.UpdateGeneBars();
            arm.LeaveTarget();
            ParentMap.FloatingParticles.Remove(this);
            ParentMap.towerCue = CurrentGame.soundBank.GetCue("kansi");
            ParentMap.towerCue.Play();
        }

        const float distToBeginTurn = 20;
        const float turnDistModifier = 1.75f;
        float imagDist;
        float imagDistOrig;
        Vector2 imagDest;
		Vector2 imagPos;
        Vector2 dirPrev;
        public Vector2 dir;
		float vecPos;
        bool turning;
        public void Update()
        {
            if (!SuppressUpdate)
            {
                float distanceToNextWaypoint = Vector2.Distance(Location, Path[nextWaypoint]);

                if (distanceToNextWaypoint < distToBeginTurn)
                {
                    if (nextWaypoint >= Path.Count - 1)
                    {
                        ReleaseYieldAtGoal();
                        //TakeLifePoints(ParentMap.Players);
                        return;
                    }
                    else
                    {
                        dirPrev = Vector2.Normalize(Path[nextWaypoint] - Location);
                        nextWaypoint++;
                        turning = true;
                        imagDest = Location + (dirPrev * distanceToNextWaypoint * turnDistModifier); //--------------------täällä mutkan mitta
                        imagDistOrig = Vector2.Distance(Location, imagDest);
                        imagPos = Location;
                    }
                }
                //prevLoc = Location;
                dir = Vector2.Normalize(Path[nextWaypoint] - Location);

                if (turning)
                {
                    imagPos += dirPrev * Speed;
                    imagDist = Vector2.Distance(imagPos, imagDest);
                    vecPos = imagDist / imagDistOrig;
                    Location += Vector2.Normalize(Vector2.Lerp(dir, dirPrev, vecPos)) * Speed;
                    if (imagDist < Speed)
                        turning = false;
                }
                else
                    Location += dir * Speed;

                Angle += Spin;

                CheckDistToGoal(); 
            }
        }
        void CheckDistToGoal()
        {
            float dist = Vector2.Distance(Location, OrigPath[nextWaypoint]);
            for (int i = nextWaypoint; i < OrigPath.Count - 1; i++)
                dist += Vector2.Distance(OrigPath[i], OrigPath[i + 1]);
            DistanceToGoal = dist;
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(Spritesheet, Location, null, ParticleElementColor, Angle, Vector2.Zero, BountySizeScale, SpriteEffects.None, 0);
            //sb.DrawString(CurrentGame.font, this.ToString(), Location, Color.Red);
        }

        public override string ToString()
        {
            return ((int)DistanceToGoal).ToString() /*+ " " + SourceCreature.Name*/;
        }
    }
}

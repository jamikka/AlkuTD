using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AlkuTD
{
    class SprayTower : Tower
    {
        static char[] defChar = { 'Â', 'Â', 'Â' };
        static string[] defName = { "Pruiter 3", "Pruiter 3", "Pruiter 3" };
        static float[] defRange = { 65, 65, 65 };
        static float[] defFirerate = { 70, 70, 70 };
        static float defBulletspeed = 8f;
        static short[] defDmg = { 1, 1, 1 };
        static int defSplashRange = 0;
        static int[] defCost = { 20, 20, 20 };
        static int[] defBuildTime = { 200, 300, 400 };
        static int bulletCount = 20;
        static float sprayRandomizationDistance = 35f;
        public static int sprayDegrees = 60; 

        public SprayTower(Point pos, UpgLvl upgLvl, bool isExample)
                : base(defChar[(int)upgLvl], defName[(int)upgLvl], pos, defRange[(int)upgLvl], defFirerate[(int)upgLvl], new Texture2D[] { CurrentGame.currentMap.ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri6") }, new GeneSpecs(), CurrentGame.smallBall, defBulletspeed, defDmg[(int)upgLvl], DmgType.Basic, defSplashRange, new float[] { 0, 0 }, defCost[(int)upgLvl], defBuildTime[(int)upgLvl], isExample)
        {
            //new Tower('A', "Pruiter 1", Point.Zero, 75, 55, new Texture2D[] { ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57-väri1") }, new GeneSpecs(), CurrentGame.ball, 12f, 1, 0, 0, new float[] { 0, 0 }, 10, 200, true)
            DmgType = DmgType.Spray;

            ParentMap = CurrentGame.currentMap;
            UpgradeLvl = upgLvl;
            angleOffset = (float)(Math.PI * 1.5);
            TargetPriority = TargetPriority.close;
            Bullets = new List<Bullet>(bulletCount);
            DPS = defDmg[(int)upgLvl] * bulletCount * FireRateSec;
        }

        internal override void Shoot(Creature targetCreature)
        {
            //Bullet freeBullet = Bullets.Find(b => b.active == false);
            Bullet freeBullet;
            if (Bullets.Count == 0)
            {
                freeBullet = new Bullet(targetCreature, BulletSpeed, Dmg, DmgType, SplashRange, slow, GeneSpecs, ScreenLocation, bulletTexture, ParentMap);
                Vector2 target = targetCreature.Location;
                Bullets.Add(freeBullet);
                freeBullet.ShootAt(target);

                float angle = (float)Math.Atan2(ScreenLocation.Y - target.Y, ScreenLocation.X - target.X);
                float angleOffset = 0;
                Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                //dir *= Vector2.Distance(ScreenLocation, target); //+ 10; // make target past the creature
                float sprayRads = (float)(Math.PI * (sprayDegrees / 180f));

                while (Bullets.Count < bulletCount)
                {
                    Vector2 newDir = dir * (Range + (float)(ParentMap.rnd.NextDouble() - 0.5f) * sprayRandomizationDistance);
                    //Vector2 newDir = dir * Range;
                    angleOffset = sprayRads * ((float)ParentMap.rnd.NextDouble() - 0.5f);
                    float ca = (float)Math.Cos(angleOffset);
                    float sa = (float)Math.Sin(angleOffset);
                    Vector2 sprayTarget = new Vector2(newDir.X*ca - newDir.Y*sa, newDir.X*sa + newDir.Y*ca);
                    sprayTarget = ScreenLocation - sprayTarget;
                    //Vector2 sprayTarget = new Vector2(target.X + ((float)ParentMap.rnd.NextDouble() - 0.5f) * sprayRandomizationDistance, target.Y + ((float)ParentMap.rnd.NextDouble() - 0.5f) * sprayRandomizationDistance);
                    //Vector2 sprayTarget = target;
                   
                    Bullet sprayBullet = new Bullet(sprayTarget, BulletSpeed + (float)(ParentMap.rnd.NextDouble() - 0.5f) * 10, Dmg, DmgType, slow, GeneSpecs, ScreenLocation, bulletTexture, ParentMap);
                    Bullets.Add(sprayBullet);
                    sprayBullet.ShootAt(sprayTarget);
                }
                
                firerateCounter = (int)FireRate;
                ParentMap.towerCue = CurrentGame.soundBank.GetCue("kansi"); //-----------------------------randomization implemented in XACT

            }
            else
            {
                //UpdateBullet(freeBullet);
                for (int i = 0; i < Bullets.Count; i++)
                {
                    if (Bullets[i].active == false)
                        Bullets.Remove(Bullets[i]);
                    else;
                        //UpdateBullet(Bullets[i]);
                }
                //freeBullet.targetCreature = targetCreature;
            }

            while (targetCreature.hp - targetCreature.DmgHeadedThisWay.Sum(x => x.Value) <= 0) // filter off creatures that are already doomed
            {
                PossibleTargets.Remove(targetCreature);
                if (PossibleTargets.Count > 0)
                {
                    targetCreature.TowersTargetingThis.Remove(this);
                    targetCreature = ChooseTarget();
                }
                else return;
            }
            
            
            if (PossibleTargets.Count > 1)
            {
                PossibleTargets.Remove(targetCreature);
                targetCreature.TowersTargetingThis.Remove(this);
                targetCreature = ChooseTarget();

                while (targetCreature.hp - targetCreature.DmgHeadedThisWay.Sum(x => x.Value) <= 0) // filter off creatures that are already doomed
                {
                    PossibleTargets.Remove(targetCreature);
                    if (PossibleTargets.Count > 0)
                    {
                        targetCreature.TowersTargetingThis.Remove(this);
                        targetCreature = ChooseTarget();
                    }
                    else return;
                }
            }
        }
    }
}



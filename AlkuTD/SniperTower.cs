using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    class SniperTower : Tower
    {
        /*static char defChar = 'I';
        static string defName = "Sniper 1";
        static float defRange = 200;
        static float defFirerate = 100;
        static float defBulletspeed = 12f;
        static short defDmg = 5;
        static int defSplashRange = 0;
        static int defCost = 30;
        static int defBuildTime = 200;
        static int preAimRangeBonus = 75;
        static int loadIters = 55;*/

        static char[] defChar = { 'I', 'Ï', 'Î' };
        static string[] defName = { "Sniper 1", "Sniper 2", "Sniper 3" };
        static float[] defRange = { 200, 225, 250 };
        static float[] defFirerate = { 100, 100, 100 };
        static float defBulletspeed = 14f;
        static short[] defDmg = { 5, 15, 25 };
        static int defSplashRange = 0;
        static int[] defCost = { 30, 50, 60 };
        static int[] defBuildTime = { 200, 300, 400 };
        static int preAimRangeBonus = 75;
        static int loadIters = 55;

        List<Creature> CreaturesInPreAimRange;
        int loadCounter;
        bool isLoaded;
        bool charging;

        public SniperTower(Point pos, UpgLvl upgLvl, bool isExample) 
            : base(defChar[(int)upgLvl], defName[(int)upgLvl], pos, defRange[(int)upgLvl], defFirerate[(int)upgLvl], new Texture2D[] { CurrentGame.currentMap.ParentGame.Content.Load<Texture2D>("Towers\\solubug") }, new GeneSpecs(), CurrentGame.ball, defBulletspeed, defDmg[(int)upgLvl], DmgType.Basic, defSplashRange, new float[] {0,0}, defCost[(int)upgLvl], defBuildTime[(int)upgLvl], isExample)
        {
            ParentMap = CurrentGame.currentMap;
            UpgradeLvl = upgLvl;
            loadCounter = 0;
            isLoaded = false;
            charging = false;
            CreaturesInPreAimRange = new List<Creature>();
            angleOffset = (float)(Math.PI * 1.5);
        }

        void ChargeShot ()
        {
            charging = true;
            if (loadCounter >= loadIters)
                isLoaded = true;
            else
            {
                loadCounter++;
                isLoaded = false;
            }
            
            return;
        }

        void DeCharge()
        {
            if (charging) // If charging is interrupted (e.g. currentTarget flees preAimRange) scale & set loadCounter to current firerateCounter
            {
                loadCounter = (int)Math.Round(loadIters * (1- (firerateCounter / FireRate)));
                charging = false;
            }
            if (loadCounter > 0)
                loadCounter--;
            isLoaded = false;
        }

        internal override void Hunt(List<Creature> aliveCreatures)
        {
            if (aliveCreatures.Count == 0) // Stop Hunting if there's no creatures
            {
                DeCharge();
                return; 
            }

            CreaturesInRange.Clear();
            CreaturesInPreAimRange.Clear();
            ColoredInRange.Clear();
            PossibleTargets.Clear();
            for (int i = 0; i < aliveCreatures.Count; i++) // Fill CreaturesInPreAimRange & ColoredInRange from that list
            {
                if (aliveCreatures[i].Born && Vector2.Distance(aliveCreatures[i].Location, ScreenLocation) <= Range + preAimRangeBonus)
                {
                    CreaturesInPreAimRange.Add(aliveCreatures[i]);
                    if (ElemPriority != AlkuTD.ColorPriority.none && aliveCreatures[i].ElemArmors[ElemPriority] > 0)
                        ColoredInRange.Add(aliveCreatures[i]);
                }
            }     

            if (ElemPriority != AlkuTD.ColorPriority.none && ColoredInRange.Count > 0) // Fill PossibleTargets from CreaturesInPreAimRange or ColoredInRange if tower has a ElemPriority
                PossibleTargets = ColoredInRange;
            else PossibleTargets = CreaturesInPreAimRange;

            if (previousTargets != null) // Remove targeted status from creatures that fled range
            {
                for (int i = 0; i < previousTargets.Count; i++)
                {
                    if (!PossibleTargets.Contains(previousTargets[i]))
                        previousTargets[i].TowersTargetingThis.Remove(this);
                }
            }
            previousTargets = new List<Creature>(PossibleTargets);

            if (PossibleTargets.Count == 0) // DeCharge & stop Hunting if there's no PossibleTargets
            {
                DeCharge();
                return;
            }

            //------ There are PossibleTargets!

            currentTarget = ChooseTarget();

            angle = angle = (float)Math.Atan2(ScreenLocation.Y - currentTarget.Location.Y, ScreenLocation.X - currentTarget.Location.X);
            ChargeShot();

            for (int i = 0; i < PossibleTargets.Count; i++)
            {
                if (Vector2.Distance(PossibleTargets[i].Location, ScreenLocation) <= Range)
                {
                    CreaturesInRange.Add(aliveCreatures[i]);
                }
            }

            if (CreaturesInRange.Count == 0)
            {
                return;
            }

            // --------------SHOOT!

            if (slow[0] > 0 && currentTarget != prevTarget) // Share planned target to splash towers for predicting creature location
                nextHitIteration = CurrentGame.gameTimer + (uint)Math.Max(firerateCounter, 0) + (uint)Math.Round(ParentMap.TileHeight / BulletSpeed);

            //angle = (float)Math.Atan2(currentTarget.Location.Y - ScreenLocation.Y, currentTarget.Location.X - ScreenLocation.X);
            if (firerateCounter <= 0 && isLoaded)
            {
                Shoot(currentTarget);
                isLoaded = false;
                loadCounter = loadIters;
            }

            prevTarget = currentTarget;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb); // FireRateStatusBar is drawn pointlessly under the following PreLoadAnimation (10.10.2019)

            if (buildTimer == 0)
            {
                //------PreLoadAnimation
                sb.Draw(CurrentGame.pixel, new Rectangle((int)ScreenLocation.X - loadBarWidth / 2, (int)(ScreenLocation.Y + ParentMap.TileHeight * 0.44f - 1), loadBarWidth, 4), null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 0.1f); // black background
                if (isLoaded)
                    sb.Draw(CurrentGame.pixel, new Rectangle((int)ScreenLocation.X - loadBarWidth / 2 + 1, (int)(ScreenLocation.Y + ParentMap.TileHeight * 0.44f), (int)((loadBarWidth - 2) * ((FireRate - firerateCounter) / FireRate)), 2), null, loadBarColor, 0,Vector2.Zero, SpriteEffects.None, 0.9f);
                else
                    sb.Draw(CurrentGame.pixel, new Rectangle((int)ScreenLocation.X - loadBarWidth / 2 + 1, (int)(ScreenLocation.Y + ParentMap.TileHeight * 0.44f), (int)((loadBarWidth - 2) * (loadCounter / (float)loadIters)), 2), null, loadBarColor, 0, Vector2.Zero, SpriteEffects.None, 0.09f);
            }
        }

        /*public override void Shoot(Creature targetCreature) // ----Tää tarvii overridettää toistaseks ainoastaan laukasusaundia vaihtaakseen
        {
            Bullet freeBullet = Bullets.Find(b => b.active == false);
            if (freeBullet == null && Bullets.Count < 10)
            {
                freeBullet = new Bullet(targetCreature, BulletSpeed, Dmg, DmgType, SplashRange, slow, GeneSpecs, ScreenLocation, bulletTexture, ParentMap);
                Bullets.Add(freeBullet);
            }
            else
            {
                UpdateBullet(freeBullet);
                freeBullet.targetCreature = targetCreature;
            }

            while (targetCreature.hp - targetCreature.DmgHeadedThisWay.Sum(x => x.Value) <= 0) // filter off creatures that are already doomed
            {
                PossibleTargets.Remove(targetCreature);
                if (PossibleTargets.Count > 0)
                {
                    //targetCreature.TowersTargetingThis.Remove(this);
                    targetCreature = ChooseTarget();
                }
                else return;
            }

            freeBullet.ShootAt(targetCreature);
            firerateCounter = (int)FireRate;
            targetCreature.DmgHeadedThisWay.Add(new KeyValuePair<uint, int>(CurrentGame.gameTimer + (uint)Math.Round(Vector2.Distance(ScreenLocation, targetCreature.Location) / BulletSpeed + 20), Dmg));
            targetCreature.TowersTargetingThis.Remove(this);
            ParentMap.towerCue = CurrentGame.soundBank.GetCue("kansi"); //-----------------------------randomization implemented in XACT
            ParentMap.towerCue.Play();
        }*/
    }
}

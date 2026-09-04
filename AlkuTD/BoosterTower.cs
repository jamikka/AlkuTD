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
    class BoosterTower : Tower
    {
        static char[] defChar = { '|', '†', '‡' };
        static string[] defName = { "Booster 1", "Booster 2", "Booster 3" };
        static float[] defRange = { 60, 60, 60 };
        static float[] defFirerate = { 0, 0, 0 };
        static float defBulletspeed = 0f;
        static short[] defDmg = { 0, 0, 0 };
        static int defSplashRange = 0;
        static int[] defCost = { 20, 40, 60 };
        static int[] defBuildTime = { 200, 300, 400 };
        static float[] defRangeBoostFactor = { 1.2f, 1.4f, 1.6f };
        static float[] defFirerateBoostFactor = { 0.8f, 0.66f, 0.5f };

        public BoosterTower(Point pos, UpgLvl upgLvl, bool isExample)
                : base(defChar[(int)upgLvl], defName[(int)upgLvl], pos, defRange[(int)upgLvl], defFirerate[(int)upgLvl], new Texture2D[] { CurrentGame.currentMap.ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57") }, new GeneSpecs(), CurrentGame.smallBall, defBulletspeed, defDmg[(int)upgLvl], DmgType.None, defSplashRange, new float[] { 0, 0 }, defCost[(int)upgLvl], defBuildTime[(int)upgLvl], isExample)
        {
            DmgType = DmgType.None;

            ParentMap = CurrentGame.currentMap;
            UpgradeLvl = upgLvl;
            angleOffset = (float)(Math.PI * 1.5);
            TargetPriority = TargetPriority.close;
            DPS = 0;
        }

        public override void Update(List<Creature> aliveCreatures)
        {
            base.Update(aliveCreatures);
            for (int i = 0; i < ParentMap.Players[0].Towers.Count; i++)
            {
                if (ParentMap.Players[0].Towers[i] != this && Vector2.Distance(ScreenLocation, ParentMap.Players[0].Towers[i].ScreenLocation) < Range)
                    BoostTower(ParentMap.Players[0].Towers[i]);
            }
        }

        private void BoostTower(Tower tower)
        {
            tower.Range = tower.InitRange * defRangeBoostFactor[(int)UpgradeLvl];
            tower.FireRate = tower.InitFireRate * defFirerateBoostFactor[(int)UpgradeLvl];
            tower.FireRateSec = 1000 / (tower.FireRate * (float)ParentMap.ParentGame.TargetElapsedTime.TotalMilliseconds);
            tower.DPS = tower.Dmg * tower.FireRateSec;
        }
    }
}

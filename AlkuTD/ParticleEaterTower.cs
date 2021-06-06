using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class ParticleEaterTower : Tower
    {
        static char[] defChar = { 'U', 'Ü', 'Û' };
        static string[] defName = { "Debris Eater 1", "Debris Eater 2", "Debris Eater 3" };
        static float[] defRange = { 100, 120, 140 };
        static float[] defFirerate = { 60, 80, 100 };
        static int[] defCost = { 5, 30, 60 };
        static int[] defBuildTime = { 200, 300, 400 };
        static float[] defEnergyMultiplier = { 1, 1, 1.5f };
        static float[] defGeneMultipliers = { 1, 1.5f, 2 };
        static float defBulletspeed = 3f;
        static short[] defDmg = { 0, 0, 0 };
        static int defSplashRange = 0;

        List<FloatingParticle> FloatingParticlesInRange;
        List<FloatingParticle> ColoredFloatingParticlesInRange;
        List<FloatingParticle> PossibleFloatingTargets;
        internal FloatingParticle CurrentDebrisTarget;
        internal List<FloatingParticle> CurrentTargets;
        internal List<FloatingParticle> PreviousFloatingTargets;

        List<PseudoPod> EaterArms;
        public float GeneMultiplier { get { return defGeneMultipliers[(int)UpgradeLvl]; } }
        public float EnergyMultiplier { get { return defEnergyMultiplier[(int)UpgradeLvl]; } }


        public ParticleEaterTower(Point pos, UpgLvl upgLvl, bool isExample) 
            : base(defChar[(int)upgLvl], defName[(int)upgLvl], pos, defRange[(int)upgLvl], defFirerate[(int)upgLvl], new Texture2D[] { CurrentGame.currentMap.ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57") }, new GeneSpecs(), CurrentGame.ball, defBulletspeed, defDmg[(int)upgLvl], DmgType.Basic, defSplashRange, new float[] { 0, 0 }, defCost[(int)upgLvl], defBuildTime[(int)upgLvl], isExample)
        {
            ParentMap = CurrentGame.currentMap;
            UpgradeLvl = upgLvl;
            angleOffset = (float)(Math.PI * 1.5);
            DmgType = DmgType.None;
            FloatingParticlesInRange = new List<FloatingParticle>();
            ColoredFloatingParticlesInRange = new List<FloatingParticle>();
            PossibleFloatingTargets = new List<FloatingParticle>();
            EaterArms = new List<PseudoPod>();
            EaterArms.Add(new PseudoPod(this, ParentMap.ParentGame.Content.Load<Texture2D>("Towers\\solubug")));
            EaterArms.Add(new PseudoPod(this, ParentMap.ParentGame.Content.Load<Texture2D>("Towers\\solubug")));
            CurrentTargets = new List<FloatingParticle>();
        }

        internal void HuntDebris(List<FloatingParticle> floatingParticles)
        {

            if (ParentMap.FloatingParticles.Count == 0)
                return;

            FloatingParticlesInRange.Clear();
            ColoredFloatingParticlesInRange.Clear();
            PossibleFloatingTargets.Clear();
            for (int i = 0; i < floatingParticles.Count; i++)
            {
                if (Vector2.Distance(floatingParticles[i].Location, ScreenLocation) <= Range)
                {
                    FloatingParticlesInRange.Add(floatingParticles[i]);
                    if (ElemPriority != AlkuTD.ColorPriority.None && floatingParticles[i].Elems[ElemPriority] > 0)
                        ColoredFloatingParticlesInRange.Add(floatingParticles[i]);
                }
            }

            if (ElemPriority != AlkuTD.ColorPriority.None && ColoredFloatingParticlesInRange.Count > 0)
                PossibleFloatingTargets = ColoredFloatingParticlesInRange;
            else PossibleFloatingTargets = FloatingParticlesInRange;

            if (PreviousFloatingTargets != null) // Remove targeted status from creatures that fled range
            {
                for (int i = 0; i < PreviousFloatingTargets.Count; i++)
                {
                    if (!PossibleFloatingTargets.Contains(PreviousFloatingTargets[i]))
                    {
                        foreach (PseudoPod arm in EaterArms)
                        {
                            if (arm.CurrentTarget == PreviousFloatingTargets[i])
                            {
                                PreviousFloatingTargets[i].ArmsTargetingThis.Remove(arm);
                                arm.CurrentTarget = null;
                                arm.State = PseudoPod.ActivityState.Detracting;
                            }
                        }
                    }
                        
                }
            }
            PreviousFloatingTargets = new List<FloatingParticle>(PossibleFloatingTargets);

            if (FloatingParticlesInRange.Count == 0)
                return;

            for (int i = 0; i < EaterArms.Count; i++)
            {
                if (EaterArms[i].CurrentTarget == null)
                {
                    EaterArms[i].CurrentTarget = ChooseTarget();
                    Eat(EaterArms[i], EaterArms[i].CurrentTarget);
                }
            }
        }

        //new internal FloatingParticle ChooseTarget()
        //{
        //    PseudoPod freeArm = EaterArms.Find(arm => arm.State == PseudoPod.ActivityState.Ready || arm.State == PseudoPod.ActivityState.Detracting);
        //    if (freeArm != null)
        //    {
        //        switch (TargetPriority)
        //        {
        //            case TargetPriority.Last:
        //                float biggestDistToGoal = 0;
        //                for (int i = 0; i < PossibleFloatingTargets.Count; i++)
        //                {
        //                    if (PossibleFloatingTargets[i].ArmsTargetingThis.Count == 0 && PossibleFloatingTargets[i].DistanceToGoal > biggestDistToGoal)
        //                    {
        //                        biggestDistToGoal = PossibleFloatingTargets[i].DistanceToGoal;
        //                        CurrentDebrisTarget = PossibleFloatingTargets[i];
        //                    }
        //                }
        //                break;
        //            case TargetPriority.Tough:
        //                int mostHp = 0;
        //                for (int i = 0; i < PossibleFloatingTargets.Count; i++)
        //                {
        //                    if (PossibleFloatingTargets[i].ArmsTargetingThis.Count == 0 && PossibleFloatingTargets[i].EnergyBounty > mostHp)
        //                    {
        //                        mostHp = (int)PossibleFloatingTargets[i].EnergyBounty;
        //                        CurrentDebrisTarget = PossibleFloatingTargets[i];
        //                    }
        //                }
        //                break;
        //            case TargetPriority.Weak:
        //                int leastHp = int.MaxValue;
        //                for (int i = 0; i < PossibleFloatingTargets.Count; i++)
        //                {
        //                    if (PossibleFloatingTargets[i].ArmsTargetingThis.Count == 0 && PossibleFloatingTargets[i].EnergyBounty < leastHp)
        //                    {
        //                        leastHp = (int)PossibleFloatingTargets[i].EnergyBounty;
        //                        CurrentDebrisTarget = PossibleFloatingTargets[i];
        //                    }
        //                }
        //                break;
        //            case TargetPriority.None:
        //            case TargetPriority.First:
        //            default:
        //                float smallestDistToGoal = float.MaxValue;
        //                for (int i = 0; i < PossibleFloatingTargets.Count; i++)
        //                {
        //                    if (PossibleFloatingTargets[i].ArmsTargetingThis.Count == 0 && PossibleFloatingTargets[i].DistanceToGoal < smallestDistToGoal)
        //                    {
        //                        smallestDistToGoal = PossibleFloatingTargets[i].DistanceToGoal;
        //                        CurrentDebrisTarget = PossibleFloatingTargets[i];
        //                    }
        //                };
        //                break;
        //        }
        //        //if (!CurrentDebrisTarget.ArmsTargetingThis.Contains(freeArm))
        //        //    CurrentDebrisTarget.ArmsTargetingThis.Add(freeArm);

        //        //freeArm.ReachFor(CurrentDebrisTarget);
        //        return CurrentDebrisTarget;
        //    }
        //    else
        //        return null;
        //}

        new internal FloatingParticle ChooseTarget()
        {
            CurrentDebrisTarget = null;
            PseudoPod freeArm = EaterArms.Find(arm => arm.State == PseudoPod.ActivityState.Ready || arm.State == PseudoPod.ActivityState.Detracting);
            if (freeArm != null)
            {
                for (int i = 0; i < PossibleFloatingTargets.Count; i++)
                {
                    if (PossibleFloatingTargets[i].ArmsTargetingThis.Count > 0)
                        continue;

                    switch (TargetPriority)
                    {
                        case TargetPriority.Last:
                            float biggestDistToGoal = 0;
                            if (PossibleFloatingTargets[i].DistanceToGoal > biggestDistToGoal)
                            {
                                biggestDistToGoal = PossibleFloatingTargets[i].DistanceToGoal;
                                CurrentDebrisTarget = PossibleFloatingTargets[i];
                            }
                            break;
                        case TargetPriority.Tough:
                            int mostHp = 0;
                            if (PossibleFloatingTargets[i].EnergyBounty > mostHp)
                            {
                                mostHp = (int)PossibleFloatingTargets[i].EnergyBounty;
                                CurrentDebrisTarget = PossibleFloatingTargets[i];
                            }
                            break;
                        case TargetPriority.Weak:
                            int leastHp = int.MaxValue;
                            if (PossibleFloatingTargets[i].EnergyBounty < leastHp)
                            {
                                leastHp = (int)PossibleFloatingTargets[i].EnergyBounty;
                                CurrentDebrisTarget = PossibleFloatingTargets[i];
                            }
                            break;
                        case TargetPriority.None:
                        case TargetPriority.First:
                        default:
                            float smallestDistToGoal = float.MaxValue;
                            if (PossibleFloatingTargets[i].DistanceToGoal < smallestDistToGoal)
                            {
                                smallestDistToGoal = PossibleFloatingTargets[i].DistanceToGoal;
                                CurrentDebrisTarget = PossibleFloatingTargets[i];
                            }
                            break;
                    }
                }
                return CurrentDebrisTarget;
            }
            else
                return null;
        }

        //internal void Eat(FloatingParticle target)
        //{
        //    for (int i = 0; i < EaterArms.Count; i++)
        //    {
        //        if (EaterArms[i].State == PseudoPod.ActivityState.Ready || EaterArms[i].State == PseudoPod.ActivityState.Detracting)
        //        {
        //            if (target.ArmsTargetingThis.Contains(EaterArms[i]))
        //                EaterArms[i].ReachFor(target);
        //        }
        //    }

        //}

        internal void Eat(PseudoPod arm, FloatingParticle target)
        {
            if (target != null && (arm.State == PseudoPod.ActivityState.Ready || arm.State == PseudoPod.ActivityState.Detracting))
            {
                if (!target.ArmsTargetingThis.Contains(arm))
                {
                    target.ArmsTargetingThis.Add(arm);
                    arm.ReachFor(target);
                }
            }
        }

        float oldRange;


        public override void Update(List<Creature> aliveCreatures)
        {
            if (CurrentGame.gameState == GameState.InitSetup || CurrentGame.gameState == GameState.MapTestInitSetup/*ParentMap.initSetupOn*/)
                buildTimer = 0;

            if (buildTimer == 0)
            {
                //if (firerateCounter > 0)
                //    firerateCounter--;

                if (Built == false)
                {
                    Built = true;
                    ParentMap.towerCue = CurrentGame.soundBank.GetCue("pluip2");
                    ParentMap.towerCue.Play();
                }

                if (buildFinishedCounter > 0) buildFinishedCounter--; //---afterglow effect

                HuntDebris(ParentMap.FloatingParticles);

                for (int i = 0; i < EaterArms.Count; i++)
                {
                    EaterArms[i].Update();
                }
            }
            else buildTimer--;

            if (Range != oldRange)
                MakeRadiusCircle();
            oldRange = Range;
        }

        public override void Draw(SpriteBatch sb)
        {
            float buildPhase = (BuildTime - buildTimer) / (float)BuildTime;

            if (buildTimer == 0)
            {
                

                sb.Draw(Textures[0], ScreenLocation, null, Color.White, angle + angleOffset, texOrigin, 1, SpriteEffects.None, 0);
                
                for (int i = 0; i < EaterArms.Count; i++)
                {
                    if (EaterArms[i].State != PseudoPod.ActivityState.Ready && EaterArms[i].State != PseudoPod.ActivityState.Preparing)
                        EaterArms[i].Draw(sb);
                    //sb.DrawString(CurrentGame.font, EaterArms[i].State.ToString(), ScreenLocation + new Vector2(-50, 11 * i), Color.Coral);
                }
                //---FirerateLoadBars 

                //sb.Draw(CurrentGame.pixel, new Rectangle((int)ScreenLocation.X - loadBarWidth / 2, (int)(ScreenLocation.Y + ParentMap.TileHeight * 0.44f - 1), loadBarWidth, 4), Color.Black); //black background
                //sb.Draw(CurrentGame.pixel, new Rectangle((int)ScreenLocation.X - loadBarWidth / 2 + 1, (int)(ScreenLocation.Y + ParentMap.TileHeight * 0.44f), (int)((loadBarWidth - 2) * ((FireRate - firerateCounter) / FireRate)), 2), loadBarColor);

                if (GeneSpecs.HasAny)
                {
                    GeneType gt = GeneSpecs.GetPrimaryElem();
                    int geneIdx = (int)gt - 1;
                    Color geneUpgColor = Color.DarkGray;
                    switch (gt)
                    {
                        case GeneType.Red: geneUpgColor = Color.Red; break;
                        case GeneType.Green: geneUpgColor = Color.Green; break;
                        case GeneType.Blue: geneUpgColor = Color.Blue; break;
                    }
                    Vector2 firstLocation = new Vector2(ScreenLocation.X + CurrentGame.ball.Width / 2 - GeneSpecs.BaseTiers[geneIdx] * 11 / 2, ScreenLocation.Y - ParentMap.TileHalfHeight);
                    for (int i = 0; i < GeneSpecs.BaseTiers[geneIdx]; i++)
                    {
                        sb.Draw(CurrentGame.ball, firstLocation + new Vector2(i * 11, 0), geneUpgColor);
                    }
                }
            }
            else if (IsExample)
                sb.Draw(Textures[0], ScreenLocation, null, Color.White * 0.6f, 0, texOrigin, 1, SpriteEffects.None, 0);

            if (buildFinishedCounter > 0 && buildTimer < BuildTime)
            {
                #region UGLY OUTLINE ANIMATION
                Color lineColor = buildTimer == 0 ? Color.White : Color.GreenYellow;
                Color borderColor = Color.Black * 0.4f;
                lineColor *= buildFinishedCounter / (float)buildFinishedInit;
                borderColor *= buildFinishedCounter / (float)buildFinishedInit;

                sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 1), (int)ScreenLocation.Y - ParentMap.TileHeight / 2 - 1, ParentMap.TileWidth / 2, 4),
                            null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, 0);
                sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileWidth / 4 + 2), (int)ScreenLocation.Y - ParentMap.TileHeight / 2, ParentMap.TileWidth / 2, 4),
                        null, borderColor, MathHelper.ToRadians(60.9f), Vector2.Zero, SpriteEffects.None, 0);
                sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileWidth / 2 + 1), (int)ScreenLocation.Y, ParentMap.TileWidth / 2, 4),
                        null, borderColor, MathHelper.ToRadians(118.2f), Vector2.Zero, SpriteEffects.None, 0);
                sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileWidth / 4 + 1), (int)ScreenLocation.Y + ParentMap.TileHeight / 2 + 2, ParentMap.TileWidth / 2 + 1, 4),
                        null, borderColor, (float)Math.PI, Vector2.Zero, SpriteEffects.None, 0);
                sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 2), (int)ScreenLocation.Y + ParentMap.TileHeight / 2 + 2, ParentMap.TileWidth / 2 + 1, 4),
                        null, borderColor, MathHelper.ToRadians(240.8f), Vector2.Zero, SpriteEffects.None, 0);
                sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 2 - 1), (int)ScreenLocation.Y, ParentMap.TileWidth / 2, 4),
                        null, borderColor, (float)Math.PI * (5 / 3f), Vector2.Zero, SpriteEffects.None, 0);

                sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 1), (int)(ScreenLocation.Y - ParentMap.TileHeight / 2),
                        (int)Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * buildPhase * 6), 2), lineColor);
                if (buildPhase >= 1 / 6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 4 + 1, ScreenLocation.Y - ParentMap.TileHeight / 2), null, lineColor, MathHelper.ToRadians(60.9f),
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 1 / 6f) * 6), 2), SpriteEffects.None, 0);
                if (buildPhase >= 2 / 6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 2, ScreenLocation.Y), null, lineColor, MathHelper.ToRadians(118.2f) /*(float)Math.PI * (1.98f / 3f)*/, //118.95f
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 2 / 6f) * 6), 2), SpriteEffects.None, 0);
                if (buildPhase >= 3 / 6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 4, ScreenLocation.Y + ParentMap.TileHeight / 2 + 1), null, lineColor, (float)Math.PI,
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 3 / 6f) * 6), 2), SpriteEffects.None, 0);
                if (buildPhase >= 4 / 6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X - ParentMap.TileWidth / 4 - 1, ScreenLocation.Y + ParentMap.TileHeight / 2 + 1), null, lineColor, MathHelper.ToRadians(240.8f),//(float)Math.PI * (4f / 3f)
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 4 / 6f) * 6), 2), SpriteEffects.None, 0);
                if (buildPhase >= 5 / 6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X - ParentMap.TileWidth / 2, ScreenLocation.Y), null, lineColor, (float)Math.PI * (5 / 3f),
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2 - 1, ParentMap.TileWidth / 2 * (buildPhase - 5 / 6f) * 6), 2), SpriteEffects.None, 0);
                #endregion
            }

            if (ShowRadius || radiusFade > 0) //-------------------RADIUS
            {
                if (ShowRadius && radiusFade < radiusFadeCycles) radiusFade++;
                else if (!ShowRadius) radiusFade--;

                if (radiusFade > 0)
                {
                    if (Range == InitRange)
                        sb.Draw(radiusTextures[0], new Rectangle((int)(ScreenLocation.X - radiusTextures[0].Width / 2), (int)(ScreenLocation.Y - radiusTextures[0].Height / 2), radiusTextures[0].Width, radiusTextures[0].Height), Color.White * (radiusFade / radiusFadeCycles));
                    else
                    {
                        sb.Draw(radiusTextures[0], new Rectangle((int)(ScreenLocation.X - radiusTextures[0].Width / 2), (int)(ScreenLocation.Y - radiusTextures[0].Height / 2), radiusTextures[0].Width, radiusTextures[0].Height), Color.White * (radiusFade / radiusFadeCycles) * 0.5f);
                        sb.Draw(radiusTextures[1], new Rectangle((int)(ScreenLocation.X - radiusTextures[1].Width / 2), (int)(ScreenLocation.Y - radiusTextures[1].Height / 2), radiusTextures[1].Width, radiusTextures[1].Height), Color.White * (radiusFade / radiusFadeCycles));
                    }
                }
            }
        }
    }
}

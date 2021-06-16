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
        public static int preAimRangeBonus = 60;

        List<FloatingParticle> DebrisInPreAimRange;
        List<FloatingParticle> DebrisInRange;
        List<FloatingParticle> ColoredDebrisInPreAimRange;
        List<FloatingParticle> ColoredDebrisInRange;
        List<FloatingParticle> PossibleDebrisTargets;
        internal FloatingParticle CurrentDebrisTarget;
        internal List<FloatingParticle> CurrentTargets;
        internal List<FloatingParticle> PreviousDebrisTargets;

        public List<PseudoPod> EaterArms;
        public float GeneMultiplier { get { return defGeneMultipliers[(int)UpgradeLvl]; } }
        public float EnergyMultiplier { get { return defEnergyMultiplier[(int)UpgradeLvl]; } }

        bool isOnlyPreaimTargets;
        Texture2D ArmTexture;

        public ParticleEaterTower(Point pos, UpgLvl upgLvl, bool isExample) 
            : base(defChar[(int)upgLvl], defName[(int)upgLvl], pos, defRange[(int)upgLvl], defFirerate[(int)upgLvl], new Texture2D[] { CurrentGame.currentMap.ParentGame.Content.Load<Texture2D>("Towers\\TORN-66-57") }, new GeneSpecs(), CurrentGame.ball, defBulletspeed, defDmg[(int)upgLvl], DmgType.Basic, defSplashRange, new float[] { 0, 0 }, defCost[(int)upgLvl], defBuildTime[(int)upgLvl], isExample)
        {
            ParentMap = CurrentGame.currentMap;
            UpgradeLvl = upgLvl;
            angleOffset = (float)(Math.PI * 1.5);
            DmgType = DmgType.None;
            DebrisInPreAimRange = new List<FloatingParticle>();
            DebrisInRange = new List<FloatingParticle>();
            ColoredDebrisInPreAimRange = new List<FloatingParticle>();
            ColoredDebrisInRange = new List<FloatingParticle>();
            PossibleDebrisTargets = new List<FloatingParticle>();
            EaterArms = new List<PseudoPod>();
            ArmTexture = ParentMap.ParentGame.Content.Load<Texture2D>("Towers\\solubug");
            EaterArms.Add(new PseudoPod(this, ArmTexture));
            EaterArms.Add(new PseudoPod(this, ArmTexture));
            CurrentTargets = new List<FloatingParticle>();
            TargetPriority = TargetPriority.tough;
        }

        internal void HuntDebris(List<FloatingParticle> floatingParticles)
        {

            DebrisInRange.Clear();
            ColoredDebrisInRange.Clear();
            DebrisInPreAimRange.Clear();
            ColoredDebrisInPreAimRange.Clear();
            PossibleDebrisTargets.Clear();
            if (ParentMap.FloatingParticles.Count == 0)
                return;
            float distanceToParticle;
            FloatingParticle newTarget;

            for (int i = 0; i < floatingParticles.Count; i++)
            {
                distanceToParticle = Vector2.Distance(floatingParticles[i].Location, ScreenLocation);
                if (distanceToParticle <= Range + preAimRangeBonus)
                {
                    DebrisInPreAimRange.Add(floatingParticles[i]);
                    if (ElemPriority != AlkuTD.ColorPriority.none && floatingParticles[i].Elems.HasAny)
                        ColoredDebrisInPreAimRange.Add(floatingParticles[i]);
                }
                if (distanceToParticle <= Range)
                {
                    DebrisInRange.Add(floatingParticles[i]);
                    if (ElemPriority != AlkuTD.ColorPriority.none && floatingParticles[i].Elems.HasAny)
                        ColoredDebrisInRange.Add(floatingParticles[i]);
                }
            }

            if (DebrisInPreAimRange.Count == 0)
            {
                foreach (PseudoPod arm in EaterArms)
                    arm.LeaveTarget();
                return;
            }
            else if (DebrisInRange.Count == 0)
            {
                if (ElemPriority != AlkuTD.ColorPriority.none && ColoredDebrisInPreAimRange.Count > 0)
                    PossibleDebrisTargets = ColoredDebrisInPreAimRange;
                else PossibleDebrisTargets = DebrisInPreAimRange;
                isOnlyPreaimTargets = true;
            }
            else
            {
                if (ElemPriority != AlkuTD.ColorPriority.none && ColoredDebrisInPreAimRange.Count > 0)
                    PossibleDebrisTargets = ColoredDebrisInRange;
                else PossibleDebrisTargets = DebrisInRange;
                isOnlyPreaimTargets = false;
            }

            if (PreviousDebrisTargets != null) // Remove targeted status from creatures that fled range
            {
                for (int i = 0; i < PreviousDebrisTargets.Count; i++)
                {
                    if (!PossibleDebrisTargets.Contains(PreviousDebrisTargets[i]))
                    {
                        if (PossibleTargets.Count > EaterArms.Count)
                        {
                            foreach (PseudoPod arm in EaterArms)
                            {
                                if (arm.CurrentTarget == PreviousDebrisTargets[i])
                                {
                                    arm.LeaveTarget();
                                }
                            }
                        }
                    }
                }
            }
            PreviousDebrisTargets = new List<FloatingParticle>(PossibleDebrisTargets);

            for (int i = 0; i < EaterArms.Count; i++)
            {
                PseudoPod arm = EaterArms[i];

                if (arm.State == PseudoPod.ActivityState.Ready || arm.State == PseudoPod.ActivityState.Detracting/*arm.CurrentTarget == null && arm.State != PseudoPod.ActivityState.Pulling && arm.State != PseudoPod.ActivityState.Preparing*/)
                {
                    newTarget = ChooseTarget(arm);
                    if (newTarget != null)
                    {
                        Eat(arm, newTarget);
                    }
                }
                else if (arm.State == PseudoPod.ActivityState.Reaching && arm.CurrentTarget == arm.PrevTarget)
                {
                    if (arm.distanceToTarget > (arm.prevDistanceToTarget + BulletSpeed) /*&& (Vector2.Distance(ScreenLocation, arm.CurrentTarget.Location) > (Range + BulletSpeed))*/)
                    {
                        newTarget = ChooseTarget(arm);
                        if (newTarget != null)
                        {
                            Eat(arm, newTarget);
                        }
                    }
                }
            }
        }

        internal FloatingParticle ChooseTarget(PseudoPod freeArm)
        {
            CurrentDebrisTarget = null;
            //freeArm.CurrentTarget = null;
            //PseudoPod freeArm = EaterArms.Find(arm => arm.State == PseudoPod.ActivityState.Ready || arm.State == PseudoPod.ActivityState.Detracting);
            if (freeArm != null)
            {
                float smallestDistToGoal = float.MaxValue;
                float biggestDistToGoal = 0;
                int biggestBounty = 0;
                int smallestBounty = int.MaxValue;

                FloatingParticle closestToGoalParticle;
                FloatingParticle biggestBountyParticle;

                for (int i = 0; i < PossibleDebrisTargets.Count; i++)
                {
                    if (PossibleDebrisTargets[i].ArmsTargetingThis.Count > 0)
                    {
                        if (Vector2.Distance(PossibleDebrisTargets[i].Location, PossibleDebrisTargets[i].ArmsTargetingThis[0].Position) > Vector2.Distance(PossibleDebrisTargets[i].Location, freeArm.Position))
                        {
                            //PossibleDebrisTargets[i].ArmsTargetingThis[0].LeaveTarget();
                            PossibleDebrisTargets[i].ArmsTargetingThis[0].CurrentTarget = null;
                            CurrentDebrisTarget = PossibleDebrisTargets[i];
                        }
                        else
                            continue;
                    }

                    switch (TargetPriority)
                    {
                        case TargetPriority.last:
                            if (PossibleDebrisTargets[i].DistanceToGoal > biggestDistToGoal)
                            {
                                biggestDistToGoal = PossibleDebrisTargets[i].DistanceToGoal;
                                CurrentDebrisTarget = PossibleDebrisTargets[i];
                            }
                            break;
                        case TargetPriority.tough:
                            if (PossibleDebrisTargets[i].EnergyBounty > biggestBounty)
                            {
                                biggestBounty = PossibleDebrisTargets[i].EnergyBounty;
                                CurrentDebrisTarget = PossibleDebrisTargets[i];
                            }
                            break;
                        case TargetPriority.weak:
                            if (PossibleDebrisTargets[i].EnergyBounty < smallestBounty)
                            {
                                smallestBounty = PossibleDebrisTargets[i].EnergyBounty;
                                CurrentDebrisTarget = PossibleDebrisTargets[i];
                            }
                            break;
                        case TargetPriority.none:
                        case TargetPriority.first:
                        default:
                            if (PossibleDebrisTargets[i].DistanceToGoal < smallestDistToGoal)
                            {
                                smallestDistToGoal = PossibleDebrisTargets[i].DistanceToGoal;
                                //if ()
                                CurrentDebrisTarget = PossibleDebrisTargets[i];
                            }
                            break;
                    }
                }
                return CurrentDebrisTarget;
            }
            else
                return null;
        }

        internal void Eat(PseudoPod arm, FloatingParticle target)
        {
            if (target != null /*&& (arm.State == PseudoPod.ActivityState.Ready || arm.State == PseudoPod.ActivityState.Detracting)*/)
            {
                if (target.ArmsTargetingThis.Count == 0)
                {
                    arm.LeaveTarget();
                    arm.ReachFor(target);
                }
            }
        }

        float oldRange;
        public override void Upgrade()
        {
            base.Upgrade();
            EaterArms.Add(new PseudoPod(this, ArmTexture));
        }

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

                for (int i = 0; i < EaterArms.Count; i++)
                {
                    EaterArms[i].UpdateDistanceToTarget();
                }

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
            byte buildPhaseSixth = (byte)(buildPhase * 6);

            if (buildTimer == 0)
            {
                sb.Draw(Textures[0], ScreenLocation - HexMap.TileWallHeight + Tower.TowerShadowHeight, null, Color.Black * 0.5f, angle + angleOffset, texOrigin, 1, SpriteEffects.None, 0.092f);
                sb.Draw(Textures[0], ScreenLocation - HexMap.TileWallHeight, null, Color.White, angle + angleOffset, texOrigin, 1, SpriteEffects.None, 0.09f);
                
                for (int i = 0; i < EaterArms.Count; i++)
                {
                    if (EaterArms[i].State != PseudoPod.ActivityState.Ready && EaterArms[i].State != PseudoPod.ActivityState.Preparing)
                        EaterArms[i].Draw(sb);
                    //sb.DrawString(CurrentGame.font, EaterArms[i].State.ToString(), ScreenLocation + new Vector2(-58, 11 * i), Color.Coral);
                    //sb.DrawString(CurrentGame.font, EaterArms[i].CurrentTarget != null ? EaterArms[i].CurrentTarget.SourceCreature.Name + $@" ({EaterArms[i].CurrentTarget.ToString()})" : "-", ScreenLocation + new Vector2(10, 11 * i), Color.OrangeRed);
                    //sb.DrawString(CurrentGame.font, EaterArms[i].FirerateCounter.ToString(), ScreenLocation + new Vector2(-80, 11 * i), Color.GreenYellow);

                }
                //sb.DrawString(CurrentGame.font, PossibleDebrisTargets.Count.ToString(), ScreenLocation + new Vector2(0, 35), Color.OrangeRed);
                //if (isOnlyPreaimTargets)
                //    sb.DrawString(CurrentGame.font, "RANGE", ScreenLocation + new Vector2(0, 45), Color.OrangeRed);

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
            else if (IsExample || IsUpgrading || buildTimer > 0)
                sb.Draw(Textures[0], ScreenLocation - HexMap.TileWallHeight, null, Color.White * 0.6f, 0, texOrigin, 1, SpriteEffects.None, 0.1f);

            if (buildFinishedCounter > 0 && buildTimer < BuildTime)
            {
                #region UGLY OUTLINE ANIMATION
                Color lineColor = buildTimer == 0 ? Color.White : Color.GreenYellow;
                Color borderColor = Color.Black * 0.4f;
                lineColor *= buildFinishedCounter / (float)buildFinishedInit;
                borderColor *= buildFinishedCounter / (float)buildFinishedInit;

                //barBackgrounds
				switch (buildPhaseSixth)
				{
					case 5: sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileHalfWidth - 1), (int)ScreenLocation.Y - (int)HexMap.TileWallHeight.Y, (int)Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * (buildPhase - 5/6f) * 6)+2, 4),
							null, borderColor, (float)Math.PI * (5 / 3f), Vector2.Zero, SpriteEffects.None, 0.2f); goto case 4;
					case 4: sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 2), (int)ScreenLocation.Y + ParentMap.TileHalfHeight + 2 - (int)HexMap.TileWallHeight.Y, (int)Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * (buildPhase - 4/6f) * 6) + 2, 4),
							null, borderColor, MathHelper.ToRadians(240.8f), Vector2.Zero, SpriteEffects.None, 0.2f); goto case 3;
					case 3: sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileWidth / 4), (int)ScreenLocation.Y + ParentMap.TileHalfHeight + 2 - (int)HexMap.TileWallHeight.Y, (int)Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * (buildPhase - 3/6f) * 6) + 2, 4),
							null, borderColor, (float)Math.PI, Vector2.Zero, SpriteEffects.None, 0.2f); goto case 2;
					case 2: sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileHalfWidth + 1), (int)ScreenLocation.Y - (int)HexMap.TileWallHeight.Y+1, (int)Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * (buildPhase - 2/6f) * 6)+1, 4),
							null, borderColor, MathHelper.ToRadians(118.2f), Vector2.Zero, SpriteEffects.None, 0.2f); goto case 1;
					case 1: sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileWidth / 4 + 2), (int)ScreenLocation.Y - ParentMap.TileHalfHeight - (int)HexMap.TileWallHeight.Y, (int)Math.Min(ParentMap.TileHalfWidth, (int)(ParentMap.TileHalfWidth * (buildPhase - 1/6f) * 6))+1, 4),
							null, borderColor, MathHelper.ToRadians(60.9f), Vector2.Zero, SpriteEffects.None, 0.2f); goto case 0;
					case 0: sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 1), (int)ScreenLocation.Y - ParentMap.TileHalfHeight - (int)HexMap.TileWallHeight.Y - 1, (int)Math.Min(ParentMap.TileHalfWidth, (int)(ParentMap.TileHalfWidth * buildPhase * 6))+1, 4),
                            null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, 0.2f); break;
				}
				//barBars
				switch (buildPhaseSixth)
				{
					case 5: sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X - ParentMap.TileHalfWidth, ScreenLocation.Y - (int)HexMap.TileWallHeight.Y), null, lineColor, (float)Math.PI * (5 / 3f),
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileHalfWidth - 1, ParentMap.TileHalfWidth * (buildPhase - 5/6f) * 6), 2), SpriteEffects.None, 0.1f); goto case 4;
					case 4: sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X - ParentMap.TileWidth / 4 - 1, ScreenLocation.Y + ParentMap.TileHalfHeight + 1 - (int)HexMap.TileWallHeight.Y), null, lineColor, MathHelper.ToRadians(240.8f),//(float)Math.PI * (4f / 3f)
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * (buildPhase - 4/6f) * 6), 2), SpriteEffects.None, 0.1f); goto case 3;
					case 3: sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 4, ScreenLocation.Y + ParentMap.TileHalfHeight + 1 - (int)HexMap.TileWallHeight.Y), null, lineColor, (float)Math.PI,
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * (buildPhase - 3/6f) * 6), 2), SpriteEffects.None, 0.1f); goto case 2;
					case 2: sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileHalfWidth, ScreenLocation.Y - (int)HexMap.TileWallHeight.Y), null, lineColor, MathHelper.ToRadians(118.2f) /*(float)Math.PI * (1.98f / 3f)*/, //118.95f
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * (buildPhase - 2/6f) * 6), 2), SpriteEffects.None, 0.1f); goto case 1;
					case 1: sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 4 + 1, ScreenLocation.Y - ParentMap.TileHalfHeight - (int)HexMap.TileWallHeight.Y), null, lineColor, MathHelper.ToRadians(60.9f),
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * (buildPhase - 1/6f) * 6), 2), SpriteEffects.None, 0.1f); goto case 0;
					case 0: sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 1), (int)(ScreenLocation.Y - ParentMap.TileHalfHeight - HexMap.TileWallHeight.Y),
							(int)Math.Min(ParentMap.TileHalfWidth, ParentMap.TileHalfWidth * buildPhase * 6), 2), null, lineColor, 0, Vector2.Zero, SpriteEffects.None, 0.1f); break;
				}

                //sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 1), (int)ScreenLocation.Y - ParentMap.TileHeight / 2 - 1, ParentMap.TileWidth / 2, 4),
                //            null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, 0);
                //sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileWidth / 4 + 2), (int)ScreenLocation.Y - ParentMap.TileHeight / 2, ParentMap.TileWidth / 2, 4),
                //        null, borderColor, MathHelper.ToRadians(60.9f), Vector2.Zero, SpriteEffects.None, 0);
                //sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileWidth / 2 + 1), (int)ScreenLocation.Y, ParentMap.TileWidth / 2, 4),
                //        null, borderColor, MathHelper.ToRadians(118.2f), Vector2.Zero, SpriteEffects.None, 0);
                //sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X + ParentMap.TileWidth / 4 + 1), (int)ScreenLocation.Y + ParentMap.TileHeight / 2 + 2, ParentMap.TileWidth / 2 + 1, 4),
                //        null, borderColor, (float)Math.PI, Vector2.Zero, SpriteEffects.None, 0);
                //sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 2), (int)ScreenLocation.Y + ParentMap.TileHeight / 2 + 2, ParentMap.TileWidth / 2 + 1, 4),
                //        null, borderColor, MathHelper.ToRadians(240.8f), Vector2.Zero, SpriteEffects.None, 0);
                //sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 2 - 1), (int)ScreenLocation.Y, ParentMap.TileWidth / 2, 4),
                //        null, borderColor, (float)Math.PI * (5 / 3f), Vector2.Zero, SpriteEffects.None, 0);

                //sb.Draw(CurrentGame.pixel, new Rectangle((int)(ScreenLocation.X - ParentMap.TileWidth / 4 - 1), (int)(ScreenLocation.Y - ParentMap.TileHeight / 2),
                //        (int)Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * buildPhase * 6), 2), lineColor);
                //if (buildPhase >= 1 / 6f)
                //    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 4 + 1, ScreenLocation.Y - ParentMap.TileHeight / 2), null, lineColor, MathHelper.ToRadians(60.9f),
                //            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 1 / 6f) * 6), 2), SpriteEffects.None, 0);
                //if (buildPhase >= 2 / 6f)
                //    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 2, ScreenLocation.Y), null, lineColor, MathHelper.ToRadians(118.2f) /*(float)Math.PI * (1.98f / 3f)*/, //118.95f
                //            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 2 / 6f) * 6), 2), SpriteEffects.None, 0);
                //if (buildPhase >= 3 / 6f)
                //    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 4, ScreenLocation.Y + ParentMap.TileHeight / 2 + 1), null, lineColor, (float)Math.PI,
                //            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 3 / 6f) * 6), 2), SpriteEffects.None, 0);
                //if (buildPhase >= 4 / 6f)
                //    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X - ParentMap.TileWidth / 4 - 1, ScreenLocation.Y + ParentMap.TileHeight / 2 + 1), null, lineColor, MathHelper.ToRadians(240.8f),//(float)Math.PI * (4f / 3f)
                //            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 4 / 6f) * 6), 2), SpriteEffects.None, 0);
                //if (buildPhase >= 5 / 6f)
                //    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X - ParentMap.TileWidth / 2, ScreenLocation.Y), null, lineColor, (float)Math.PI * (5 / 3f),
                //            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2 - 1, ParentMap.TileWidth / 2 * (buildPhase - 5 / 6f) * 6), 2), SpriteEffects.None, 0);
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

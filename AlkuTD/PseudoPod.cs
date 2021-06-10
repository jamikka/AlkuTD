using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class PseudoPod
    {
        public enum ActivityState
        {
            Ready,
            Preparing,
            Reaching,
            Detracting,
            Pulling
        }

        public ActivityState State;
        public ParticleEaterTower ParentTower;
        private FloatingParticle currentTarget;
        public FloatingParticle CurrentTarget { 
            get { return currentTarget; } 
            set { if (currentTarget != null)
                     currentTarget.ArmsTargetingThis.Remove(this);
                  if (value != null)
                     value.ArmsTargetingThis.Add(this);
                currentTarget = value; } }
        public FloatingParticle PrevTarget;
        public Texture2D Texture;
        public Vector2 Position;
        public int FirerateCounter;
        public float Firerate;
        public float prevDistanceToTarget;
        public float distanceToTarget;
        public int armIndex;



        public PseudoPod(ParticleEaterTower parent, Texture2D texture)
        {
            ParentTower = parent;
            Texture = texture;
            Firerate = parent.FireRate;
            FirerateCounter = (int)Math.Round(Firerate, 1, MidpointRounding.AwayFromZero);
            Position = parent.ScreenLocation;
            armIndex = parent.EaterArms.Count;
        }

        public void ReachFor(FloatingParticle target)
        {
            if (State != ActivityState.Preparing && State != ActivityState.Pulling)
            {
                PrevTarget = CurrentTarget;
                CurrentTarget = target;
                distanceToTarget = Vector2.Distance(Position, target.Location);
                prevDistanceToTarget = float.MaxValue;
                State = ActivityState.Reaching;
                accelerationIter = (int)accelerationIters;
            }
        }

        public void LeaveTarget()
        {
            if (CurrentTarget != null)
            {
                PrevTarget = CurrentTarget;
                CurrentTarget = null;
                prevDistanceToTarget = float.MaxValue;
                distanceToTarget = float.MaxValue;
                State = ActivityState.Detracting;
            }
        }

        public void UpdateDistanceToTarget()
        {
            if (CurrentTarget != null)
            {
                if (CurrentTarget == PrevTarget)
                    prevDistanceToTarget = distanceToTarget;
                else
                    prevDistanceToTarget = float.MaxValue;
                distanceToTarget = Vector2.Distance(Position, CurrentTarget.Location);
                PrevTarget = CurrentTarget;
            }
        }

        int transitionIter;
        int accelerationIter;
        const float transitionIters = 30;
        const float accelerationIters = 50;
        public struct SpeedDir 
        {
            public float Speed;
            public Vector2 Direction;
        }
        SpeedDir ParticleMobilityStats;

        //Dictionary<float, Vector2> ParticleMobilityStats;
        Vector2 dir;
        public void Update()
        {
            
            switch (State)
            {
                case ActivityState.Ready:
                    break;
                case ActivityState.Detracting:
                case ActivityState.Pulling:
                    if (Vector2.Distance(Position, ParentTower.ScreenLocation) <= ParentTower.BulletSpeed)
                    {
                        if (State == ActivityState.Pulling)
                        {
                            CurrentTarget.ReleaseYieldAtEater(ParentTower, this);
                            State = ActivityState.Preparing;
                            FirerateCounter = 0;
                        }
                        else
                            State = ActivityState.Ready;
                    }
                    else
                    {
                        dir = Vector2.Normalize(ParentTower.ScreenLocation - Position);
                        float transition = transitionIter / transitionIters;
                        Position += dir * ParentTower.BulletSpeed * (1-transition);
                        if (State == ActivityState.Pulling)
                        {
                            Position += ParticleMobilityStats.Direction * ParticleMobilityStats.Speed * transition;
                            CurrentTarget.Location = Position;
                        }
                        if (transitionIter > 0)
                            transitionIter--;
                    }
                    break;
                case ActivityState.Preparing:
                    if (FirerateCounter < Firerate)
                        FirerateCounter++;
                    else
                        State = ActivityState.Ready;
                    break;
                case ActivityState.Reaching:
                    if (CurrentTarget == null)
                    {
                        State = ActivityState.Detracting;
                    }
                    else if (Vector2.Distance(Position, CurrentTarget.Location) <= ParentTower.BulletSpeed)
                    {
                        State = ActivityState.Pulling;
                        //ParticleMobilityStats = new Dictionary<float, Vector2>() { { CurrentTarget.Speed, CurrentTarget.dir } };
                        ParticleMobilityStats = new SpeedDir() { Speed = CurrentTarget.Speed, Direction = CurrentTarget.dir };
                        transitionIter = (int)transitionIters;
                        CurrentTarget.SuppressUpdate = true;
                    }
                    else
                    {
                        //float distFromTowerToDebris = Vector2.Distance(ParentTower.ScreenLocation, CurrentTarget.Location);
                        dir = Vector2.Normalize(CurrentTarget.Location - Position);
                        Vector2 backDir = Vector2.Normalize(ParentTower.ScreenLocation - Position);
                        float armLength = Vector2.Distance(ParentTower.ScreenLocation, Position);
                        if (armLength >= ParentTower.Range)
                        {
                            //Position = ParentTower.ScreenLocation + Vector2.Normalize(CurrentTarget.Location - ParentTower.ScreenLocation) * ParentTower.Range; //toimii paitsi kun kohde vaihtuu ni warp
                            Position += (backDir * ParentTower.BulletSpeed); 
                        }
                        else /*if (Vector2.Distance(Position, CurrentTarget.Location) <= ParentTower.Range + ParticleEaterTower.preAimRangeBonus)*/
                        {
                            Position += dir * ParentTower.BulletSpeed * (1 - (accelerationIter / accelerationIters));
                            if (accelerationIter > 0)
                                accelerationIter--;
                        }
                    }
                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            Color armColor;
            switch (armIndex)
            {
                case 0: armColor = Color.HotPink; break;
                case 1: armColor = Color.Crimson; break;
                case 2: armColor = Color.MediumVioletRed; break;
                default: armColor = Color.Red; break;
            }
            Rectangle destRect = new Rectangle((int)ParentTower.ScreenLocation.X, (int)ParentTower.ScreenLocation.Y, (int)Vector2.Distance(ParentTower.ScreenLocation, Position), 4);
            Rectangle sourceRect = new Rectangle(0, 0, Texture.Width - 15, Texture.Height -15);
            float rotation = (float)Math.Atan2(Position.Y - ParentTower.ScreenLocation.Y, Position.X - ParentTower.ScreenLocation.X);
            sb.Draw(Texture, destRect, sourceRect, armColor, rotation, Vector2.Zero, SpriteEffects.None, 0);
        }

        public override string ToString()
        {
            return ParentTower.ToString() + $@"({armIndex})";
        }
    }
}
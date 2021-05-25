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
        public FloatingParticle CurrentTarget;
        public Texture2D Texture;
        public Vector2 Position;
        public int FirerateCounter;
        public float Firerate;

        public PseudoPod(ParticleEaterTower parent, Texture2D texture)
        {
            ParentTower = parent;
            Texture = texture;
            Firerate = parent.FireRate;
            FirerateCounter = (int)Math.Round(Firerate, 1, MidpointRounding.AwayFromZero);
            Position = parent.ScreenLocation;
        }

        public void ReachFor(FloatingParticle target)
        {
            if (State != ActivityState.Preparing && State != ActivityState.Pulling)
            {
                CurrentTarget = target;
                State = ActivityState.Reaching;
                accelerationIter = (int)accelerationIters;
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
                            CurrentTarget.ArmsTargetingThis.Remove(this);
                            CurrentTarget.ReleaseYieldAtEater(ParentTower);
                            CurrentTarget = null;
                        }
                        State = ActivityState.Preparing;
                        FirerateCounter = 0;
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
                        dir = Vector2.Normalize(CurrentTarget.Location - Position);
                        Position += dir * ParentTower.BulletSpeed * (1-(accelerationIter / accelerationIters));
                        if (accelerationIter > 0)
                            accelerationIter--;
                    }
                    break;
            }
        }

        public void Draw(SpriteBatch sb)
        {
            Rectangle destRect = new Rectangle((int)ParentTower.ScreenLocation.X, (int)ParentTower.ScreenLocation.Y, (int)Vector2.Distance(ParentTower.ScreenLocation, Position), 4);
            Rectangle sourceRect = new Rectangle(0, 0, Texture.Width - 15, Texture.Height -15);
            float rotation = (float)Math.Atan2(Position.Y - ParentTower.ScreenLocation.Y, Position.X - ParentTower.ScreenLocation.X);
            sb.Draw(Texture, destRect, sourceRect, Color.White, rotation, Vector2.Zero, SpriteEffects.None, 0);
        }
    }
}
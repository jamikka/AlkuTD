using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace AlkuTD
{
	public enum GeneType { None, Red, Green, Blue }

    public class Creature //: ICloneable //---Y iz Clone()?
    {
		public static readonly Vector2 CreatureShadowDistance = new Vector2(2, 7);

		public CurrentGame ParentGame;
        public HexMap ParentMap;
		public SpawnGroup ParentGroup;

        public string Type;
        public string Name;
        public Vector2 Location;
        public float PosX { get { return Location.X; } set { Location.X = value; } }
        public float PosY { get { return Location.Y; } set { Location.Y = value; } }
        public float Angle;
        public float AngleOffset;
        //public float[] AnglesBetweenWaypoints;

        public int SpawnPointIndex;
        public int GoalPointIndex;
        //public int[] GoalPointIndexes; //---------------------------------------------------muista että tuki useammalle maalille on!
        public List<Vector2> OrigPath;
        public List<Vector2> Path;
        public int nextWaypoint;
        //public Team TargetPlayers;
        //public int currWaypoint;

        public float InitHp;
        public float hp;
        public float Hp { get { return (float)Math.Round(hp); } set { hp = value; } }

		public GeneSpecs ElemArmors;

        public byte LifeDmg;
        public float defSpeed;
        public float Speed;
        public int EnergyBounty;

        public bool Born;
        public bool Alive;
        //public bool ShowingPath;

        public Texture2D Spritesheet { get; set; }
        Vector2 Origin;
        public int hpBarWidth;
        public float SpriteScale;
        public int SpritesheetRows { get; set; }
        public int SpritesheetColumns { get; set; }
        public int Width { get { return Spritesheet.Width / SpritesheetColumns; } }
        public int Height { get { return Spritesheet.Height / SpritesheetRows; } }
        int totalFrames;
        int currentFrame;
        int animationCycles;
        public int CurrentFrame { get { return currentFrame; } set { currentFrame = value; } }
        byte AnimationUpdatePhase;

        public ParticleEngine Splatter;
		public ParticleEngine TrailEngine;

        public float spin;
        public float initSpin;

		public float DistanceToGoal; // tää bugaa koska öröt pyöristää reitin kulmia (alempana if (distanceToNextWaypoint < 20) 

		public bool isSlowed;
		int slowedCounter;
		float[] CurrentSlowEffect;

		public List<Tower> TowersTargetingThis;

		public List<KeyValuePair<uint, int>> DmgHeadedThisWay; // < overdueIteration, dmg >

		public Color HpBarColor;

		public float creatureDrawDepth;

        //--------------------------Constructors------------------------------------ //---------------------------------------------------muista että tuki useammalle maalille on!
        public Creature(HexMap map, string creatureType, int spawnPointIndex, int goalPointIndex, Texture2D texture)
        {
            ParentGame = map.ParentGame;
            ParentMap = map;
            OrigPath = new List<Vector2>();
            Type = creatureType;            
            SpawnPointIndex = spawnPointIndex;
            GoalPointIndex = goalPointIndex;
            Location = map.SpawnPoints.Length > 0 ? map.ToScreenLocation(map.SpawnPoints[spawnPointIndex]) : Vector2.Zero;
            nextWaypoint = 1;

            Spritesheet = texture;
            SpriteScale = 1f;
            SpritesheetRows = 1;
            if (texture != null)
                SpritesheetColumns = texture.Width / texture.Height <= 1 ? 1 : texture.Width / texture.Height;
            totalFrames = SpritesheetRows * SpritesheetColumns;
            animationCycles = 30;
            //Angle = (float)Math.Atan2(PosY - Path[nextWaypoint].Y, PosX - Path[nextWaypoint].X);
            //Angle = AnglesBetweenWaypoints[0]; //------------------------------------------------------fix
            //AngleOffset = (float)Math.PI;
            Origin = texture != null ? new Vector2(Width / 2, Height / 2) : Vector2.Zero;
            Alive = false;
            LifeDmg = 1;
            hpBarWidth = 25;
			float rndFloat = (float)(ParentMap.rnd.NextDouble());
			spin = (spin - 0.5f) * 0.08f;
            initSpin = spin;

            TowersTargetingThis = new List<Tower>();
			CurrentSlowEffect = new float[2];
			DmgHeadedThisWay = new List<KeyValuePair<uint, int>>();

			ElemArmors = new GeneSpecs();

			HpBarColor = new Color(0, 255, 0);

			Angle = (float)(ParentMap.rnd.NextDouble());

			creatureDrawDepth = 0.4f - rndFloat * 0.05f;
		}
        public Creature(HexMap map, string creatureType, int spawnPointIndex, int goalPointIndex, Texture2D spriteSheet, int spritesheetRows, int spritesheetColumns)
            : this(map, creatureType, spawnPointIndex, goalPointIndex, spriteSheet)
        {
            SpritesheetRows = spritesheetRows;
            SpritesheetColumns = spritesheetColumns;
            totalFrames = spritesheetRows * spritesheetColumns;
            currentFrame = 0;
            AnimationUpdatePhase = 0;
        }
        public Creature(string creatureType, string name, HexMap map, int spawnPointIndex, int goalPointIndex, Texture2D texture, int initHp, float defaultSpeed)
            : this(map, creatureType, spawnPointIndex, goalPointIndex, texture)
        {
            Name = name;
            InitHp = initHp;
            hp = initHp;
            defSpeed = defaultSpeed;
            Speed = defaultSpeed;
            Splatter = new ParticleEngine(CurrentGame.smallBall, Location, 50 + (int)InitHp, 5, ParentMap.rnd); //---------------------täällä kuoloparticlemäärä----------!!
			TrailEngine = new ParticleEngine(CurrentGame.pixel, Location, 200, 100, ParentMap.rnd); //-----------TRAIL
			TrailEngine.SourceCreature = this; //-----------TRAIL
			TrailEngine.ParticleSpeed = 0; //-----------TRAIL
        }
        public Creature(string creatureType, string name, HexMap map, Texture2D spritesheet, int spawnPointIndex, int goalPointIndex, int initHp, float defaultSpeed, byte lifeDamage, float spriteScale, string textureName)
            : this(creatureType, name, map, spawnPointIndex, goalPointIndex, spritesheet, initHp, defaultSpeed)
        {
            SpriteScale = spriteScale;
            Origin = spritesheet != null ? new Vector2(Width / 2, Height / 2) : Vector2.Zero;
            currentFrame = 0;
            AnimationUpdatePhase = 0;
            LifeDmg = lifeDamage;
            Spritesheet.Name = textureName;
        }
        public Creature(string creatureType, string name, HexMap map, string textureName, int spawnPointIndex, int goalPointIndex, int initHp, float defaultSpeed, GeneSpecs elementWeaknesses, byte lifeDamage, int nrgBounty, float spriteScale)
            : this(creatureType, name, map, spawnPointIndex, goalPointIndex, Array.Find<Texture2D>(CurrentGame.CreatureTextures, tex => tex.Name == textureName), initHp, defaultSpeed)
        {
            EnergyBounty = nrgBounty;
			ElemArmors = elementWeaknesses;
            SpriteScale = spriteScale;
            currentFrame = 0;
            AnimationUpdatePhase = 0;
            LifeDmg = lifeDamage;
            //Spritesheet.Name = textureName;
        }

        //---------------------------Methods----------------------------------------
        public void FindPath()
        {
            Point currentTile = ParentMap.ToMapCoordinate(Location);
            if (ParentMap.GoalPoints.Length > 0 && GoalPointIndex < ParentMap.GoalPoints.Length)
            {
				//ParentMap.Pathfinder.InitializeTiles();
                Path = ParentMap.Pathfinder.FindPath(currentTile, ParentMap.GoalPoints[GoalPointIndex]);
                OrigPath.Clear();
                OrigPath.AddRange(Path);
                if (Path.Count > 1)
                    nextWaypoint = 1;
                else nextWaypoint = 0;
            }
        }

        public void IndividualizePath()
        {
			int rndRange = 6;
			for (int i = 0; i < OrigPath.Count - 1; i++) //Individualize path
			{
				Path[i] = new Vector2(Path[i].X + ParentMap.rnd.Next(-rndRange, rndRange), Path[i].Y + ParentMap.rnd.Next(-rndRange, rndRange));
				if (i == 0)
					Location = Path[0];
			}
        }

       // float showPathFadeCycles = 5;
       // int showPathFade;
       // public void ShowPath(SpriteBatch sb)
       // {
       //     if (ShowingPath)
       //     {
       //         if (showPathFade < showPathFadeCycles)
       //             showPathFade++;
       //         for (int i = 0; i < Path.Count -1; i++)
       //         {
       //             if (i < nextWaypoint -1)
       //                 continue;
       //             //liukukatkoviiva (dashLine -tekstuurilla)
       //             sb.Draw(ParentGame.dashLine, new Rectangle((int)OrigPath[i].X, (int)OrigPath[i].Y, (int)Vector2.Distance(OrigPath[i], OrigPath[i+1]) /*ParentMap.TileHeight +1*/, 2),
       //                     new Rectangle((int)(CurrentGame.gameTimer % 49.5f / 1.5f), 0, 33, 1), //old: ParentGame.GameTime.TotalGameTime.TotalMilliseconds % 825 / 25
       //                     Color.White * 0.3f * (showPathFade / showPathFadeCycles),
       //                     (float)Math.Atan2(OrigPath[i + 1].Y - OrigPath[i].Y, OrigPath[i + 1].X - OrigPath[i].X),
       //                     Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
       //             //sb.Draw(healthbarTexture, new Rectangle((int)OrigPath[i].X, (int)OrigPath[i].Y, ParentMap.TileHeight, 1), null, Color.White * (showPathFade / showPathFadeCycles) * 0.4f, (float)Math.Atan2(OrigPath[i + 1].Y - OrigPath[i].Y, OrigPath[i + 1].X - OrigPath[i].X), Vector2.Zero, SpriteEffects.None, 0f);
       //             //sb.DrawString(ParentMap.ParentGame.font,"" +(int)(ParentGame.GameTime.TotalGameTime.TotalMilliseconds % 600 / 50), new Vector2(1200, 300), Color.Wheat);
       //         }
       //     }
       //     else if (showPathFade > 0)
       //     {
       //         for (int i = 0; i < Path.Count - 1; i++)
       //         {
       //             if (i < nextWaypoint - 1)
       //                 continue;
       //             //liukukatkoviiva (dashLine -tekstuurilla)
       //             sb.Draw(ParentGame.dashLine, new Rectangle((int)OrigPath[i].X, (int)OrigPath[i].Y, ParentMap.TileHeight, 2),
							//new Rectangle((int)(CurrentGame.gameTimer % 49.5f / 1.5f), 0, 33, 1), 
       //                     Color.White * 0.3f * (showPathFade / showPathFadeCycles),
       //                     (float)Math.Atan2(OrigPath[i + 1].Y - OrigPath[i].Y, OrigPath[i + 1].X - OrigPath[i].X),
       //                     Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
       //             //sb.Draw(healthbarTexture, new Rectangle((int)OrigPath[i].X, (int)OrigPath[i].Y, ParentMap.TileHeight, 1), null, Color.White * (showPathFade / showPathFadeCycles) * 0.4f, (float)Math.Atan2(OrigPath[i + 1].Y - OrigPath[i].Y, OrigPath[i + 1].X - OrigPath[i].X), Vector2.Zero, SpriteEffects.None, 0f);
       //         }
       //         showPathFade--;
       //     }
       // }
        
        public void TakeAHit(Bullet bullet)
        {
            if (Alive)
            {
				float hpLoss;
				if (ElemArmors.HasAny)
				{
					GeneType bulletPrimarySpec = bullet.ElemSpecs.GetPrimaryElem();
					GeneType creaturePrimaryArmor = ElemArmors.GetPrimaryElem();
					float bStr = bullet.ElemSpecs.GetPrimaryElemStrength();
					float cArm = ElemArmors.GetPrimaryElemStrength();
					float armorReducedNormalDmg = (bullet.dmg /** (1 - bStr)*/) * (1 - cArm); // normal vs armor = (dmg * (1-spec)) * (1-armor)
					float penetratingDmg = bullet.dmg * bStr; //----------------------- penetration = dmg * specialization

					if (bulletPrimarySpec == creaturePrimaryArmor) // match
						hpLoss = penetratingDmg + armorReducedNormalDmg;
					else // uncompatible bullet
						hpLoss = armorReducedNormalDmg;
				}
				else if (bullet.ElemSpecs.HasAny) // if just the bullet has specialization
				{
					float bStr = bullet.ElemSpecs.GetPrimaryElemStrength();
					hpLoss = bullet.dmg * (1 - bStr);
				}
				else
					hpLoss = bullet.dmg;

				if (hp - hpLoss <= 0) // DEATH OF CREATURE
				{
					hp = 0;
					Alive = false;
					//ParentMap.Players[0].GenePoints[0] += (int)Math.Round(ElemArmors[GeneType.Red] * 10);
					//ParentMap.Players[0].GenePoints[1] += (int)Math.Round(ElemArmors[GeneType.Green] * 10);
					//ParentMap.Players[0].GenePoints[2] += (int)Math.Round(ElemArmors[GeneType.Blue] * 10);
					//CurrentGame.HUD.UpdateGeneBars();
					//ParentMap.Players[0].EnergyPoints += EnergyBounty;
					if (EnergyBounty > 0)
						ParentMap.FloatingParticles.Add(new FloatingParticle(this));

					ParentMap.AliveCreatures.Remove(this);
					BugInfoBox bugBox = HUD.BugBoxes.Find(bb => bb.Target == this);
					if (bugBox != null)
					{
						bugBox.locked = false;
						CurrentGame.HUD.bugHoverCounter = HUD.bugHoverFade;
					}
                    ParentGroup.AliveCreatures.Remove(this);
					Splatter.IsActive = true;
					Splatter.Location = Location;
					Splatter.EmitterAngle = Vector2.Normalize(Location - bullet.originPoint);
					ParentMap.creatureCue = CurrentGame.soundBank.GetCue("narsk");
					ParentMap.creatureCue.Play();
				}
				else
				{
					hp -= hpLoss;
                    ShakeTheCreature(true, hpLoss);

					if (bullet.slow[0] > 0) //if bullet has a slow percentage
					{
						if (!isSlowed)
						{
							//Speed *= bullet.slow[0] * 0.01f; // DONE IN UPDATE TO SMOOTH DOWN THE CHANGE
							CurrentSlowEffect = bullet.slow;
							slowedCounter = (int)bullet.slow[1];
							isSlowed = true;
							justGotSlowed = true;
						}
						else
						{
                            justGotSlowed = false;
                            if (bullet.slow[0] > CurrentSlowEffect[0])
							{
								CurrentSlowEffect = bullet.slow;
                                justGotSlowed = true;
                            }
							//else // if already slowed more... ?
							slowedCounter = (int)bullet.slow[1];
						}
					}
				}
            }
        }

        static int creatureShakeIters = 50;
        static float creatureShakeSpeed = 1.5f;
        int creatureShakeCounter;
        float hpLossToHpRatio;
        void ShakeTheCreature (bool calledFromTakeAHit, float hpLoss)
        {
            if (calledFromTakeAHit)
            {
                creatureShakeCounter = creatureShakeIters;
                hpLossToHpRatio = hpLoss / InitHp;
            }
            else if (creatureShakeCounter >= 0)
            {
                float shakeCounterRatio = (float)creatureShakeCounter / creatureShakeIters;
                spin = initSpin + (float)Math.Pow(shakeCounterRatio, 6) * creatureShakeSpeed * hpLossToHpRatio * Math.Sign(initSpin);
                creatureShakeCounter--;
            }
        }

        void TakeLifePoints(Player[] targetPlayers)
        {
            foreach (Player p in targetPlayers)
            {
                if (p != null)
                {
                    if (p.Alive && p.LifePoints > 0)
                    {
                        if (p.LifePoints - LifeDmg <= 0)
                        {
                            p.LifePoints = 0;
                            p.Alive = false;
                            CurrentGame.gameState = GameState.GameOver;
                            ParentMap.creatureCue = CurrentGame.soundBank.GetCue("loppukumi");
                            ParentMap.creatureCue.Play();
							break;
                        }
						else p.LifePoints -= LifeDmg;

						BugInfoBox bugBox = HUD.BugBoxes.Find(bb => bb.Target == this);
						if (bugBox != null)
						{
							bugBox.locked = false;
							CurrentGame.HUD.bugHoverCounter = HUD.bugHoverFade;
						}

						Alive = false;
						ParentMap.AliveCreatures.Remove(this);
					}
					if (ParentMap.towerCue == null || ParentMap.towerCue.Name != "plurrp0") 
						ParentMap.towerCue = CurrentGame.soundBank.GetCue("plurrp0");
					ParentMap.towerCue.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
					ParentMap.towerCue.Play();
				}
			}
        }

		void CheckDistToGoal()
		{
			float dist = Vector2.Distance(Location, OrigPath[nextWaypoint]);
			for (int i = nextWaypoint; i < OrigPath.Count-1; i++)
				dist += Vector2.Distance(OrigPath[i], OrigPath[i + 1]);
			DistanceToGoal = dist;
		}

		public Vector2 PredictMovement(int iterations, out bool bulletReachesBeforeGoal)
		{
			int tempNextWaypoint = nextWaypoint;
			Vector2 tempLocation = Location;
			Vector2 tempImagPos = imagPos;
			Vector2 tempImagDest = imagDest;
			float tempImagDistOrig = imagDistOrig;
			Vector2 tempDir = dir;
			Vector2 tempDirPrev = dirPrev;
			bool tempTurning = turning;

			for (int i = 0; i < iterations; i++)
			{
				float distanceToNextWaypoint = Vector2.Distance(tempLocation, Path[tempNextWaypoint]);

				if (distanceToNextWaypoint < distToBeginTurn)
				{
					if (tempNextWaypoint >= Path.Count - 1)
					{
						bulletReachesBeforeGoal = false;
						return Vector2.Zero;
					}
					else
					{
						tempDirPrev = Vector2.Normalize(Path[tempNextWaypoint] - tempLocation);
						tempNextWaypoint++;
						tempTurning = true;
						tempImagDest = tempLocation + (tempDirPrev * distanceToNextWaypoint * turnDistModifier); //--------------------täällä mutkan mitta
						tempImagDistOrig = Vector2.Distance(tempLocation, tempImagDest);
						tempImagPos = tempLocation;
					}
				}

				tempDir = Vector2.Normalize(Path[tempNextWaypoint] - tempLocation);

				if (tempTurning)
				{
					tempImagPos += tempDirPrev * Speed;
					imagDist = Vector2.Distance(tempImagPos, tempImagDest);
					vecPos = imagDist / tempImagDistOrig;
					tempLocation += Vector2.Normalize(Vector2.Lerp(tempDir, tempDirPrev, vecPos)) * Speed;
					if (imagDist < Speed)
						tempTurning = false;
				}
				else
					tempLocation += tempDir * Speed; 
			}

			bulletReachesBeforeGoal = true;
			return tempLocation;
		}

		public Vector2 PredictHitLocation(Tower tower, out bool canHitBeforeOutOfRange, out uint hitIteration) // täällä ei ehkä oo uusimmat hidastusLogiikat
		{
			int maxIterations = (int)Math.Round((tower.Range / tower.BulletSpeed)) +1;
			uint startIteration = CurrentGame.gameTimer;
			uint currentIteration = startIteration;

			float[] tempSlowEffect = CurrentSlowEffect;
			int tempSlowedCounter = slowedCounter;
			bool tempIsSlowed = isSlowed;
			bool tempJustGotSlowed = justGotSlowed;
			List<Tower> TargetingSlowTowers = new List<Tower>();
			List<uint> slowedIterations = new List<uint>();
			for (int i = 0; i < TowersTargetingThis.Count; i++)
			{
				if (TowersTargetingThis[i].slow[0] > 0)
				{
					TargetingSlowTowers.Add(TowersTargetingThis[i]);
					slowedIterations.Add(TowersTargetingThis[i].nextHitIteration);
				}
			}

			int tempNextWaypoint = nextWaypoint;
			Vector2 tempLocation = Location;
			float tempSpeed = Speed;
			Vector2 tempImagPos = imagPos;
			Vector2 tempImagDest = imagDest;
			float tempImagDistOrig = imagDistOrig;
			Vector2 tempDir = dir;
			Vector2 tempDirPrev = dirPrev;
			bool tempTurning = turning;

			for (int i = 0; i < maxIterations; i++)
			{
				for (int k = 0; k < slowedIterations.Count; k++)
				{
					if (currentIteration == slowedIterations[k])
					{
						if (!tempIsSlowed)
						{
							tempSlowEffect = TargetingSlowTowers[k].slow;
							tempSlowedCounter = (int)tempSlowEffect[1];
							tempIsSlowed = true;
							tempJustGotSlowed = true;
						}
						else
						{
							tempSpeed = Math.Min(defSpeed * (TargetingSlowTowers[k].slow[0]), tempSpeed); // if already slowed more, keep that
							tempSlowedCounter = (int)TargetingSlowTowers[k].slow[1];
							tempJustGotSlowed = false;
						}
					}
				}

				if (tempIsSlowed)
				{
					if (tempJustGotSlowed && tempSlowedCounter >= tempSlowEffect[1] - 10)
						tempSpeed = defSpeed * (1 - ((tempSlowEffect[1] - tempSlowedCounter) / 10f) * tempSlowEffect[0]);
					if (tempSlowedCounter <= 20)
						tempSpeed = defSpeed * (1 - (tempSlowedCounter / 20f) * tempSlowEffect[0]);
					if (tempSlowedCounter <= 0)
					{
						tempIsSlowed = false;
						tempSlowEffect = new float[2];
						tempSpeed = defSpeed;
					}
					tempSlowedCounter--;
				}

				float distanceToNextWaypoint = Vector2.Distance(tempLocation, Path[tempNextWaypoint]);

				if (distanceToNextWaypoint < distToBeginTurn)
				{
					if (tempNextWaypoint >= Path.Count - 1)
					{
						canHitBeforeOutOfRange = false;
						hitIteration = 0;
						return Vector2.Zero;
					}
					else
					{
						tempDirPrev = Vector2.Normalize(Path[tempNextWaypoint] - tempLocation);
						tempNextWaypoint++;
						tempTurning = true;
						tempImagDest = tempLocation + (tempDirPrev * distanceToNextWaypoint * turnDistModifier); //--------------------täällä mutkan mitta
						tempImagDistOrig = Vector2.Distance(tempLocation, tempImagDest);
						tempImagPos = tempLocation;
					}
				}

				tempDir = Vector2.Normalize(Path[tempNextWaypoint] - tempLocation);
				
				if (tempTurning)
				{
					tempImagPos += tempDirPrev * tempSpeed;
					imagDist = Vector2.Distance(tempImagPos, tempImagDest);
					vecPos = imagDist / tempImagDistOrig;
					tempLocation += Vector2.Normalize(Vector2.Lerp(tempDir, tempDirPrev, vecPos)) * tempSpeed;
					if (imagDist < tempSpeed)
						tempTurning = false;
				}
				else
					tempLocation += tempDir * tempSpeed;

				float creatureDistFromTower = Vector2.Distance(tempLocation, tower.ScreenLocation);
				float bulletDist = tower.BulletSpeed * i;
				if (bulletDist >= creatureDistFromTower)
				{
					canHitBeforeOutOfRange = true;
					hitIteration = currentIteration;
					return tempLocation;
				}
				currentIteration++;
			}

			float finalDistFromTower = Vector2.Distance(tempLocation, tower.ScreenLocation);
			if (finalDistFromTower > tower.Range + tower.SplashRange)
			{
				canHitBeforeOutOfRange = false;
				hitIteration = 0;
				return Vector2.Zero;
			}
			else if (finalDistFromTower > tower.Range)
			{
				Vector2 dirFromTowerToCreature = Vector2.Normalize(tempLocation - tower.ScreenLocation);
				tempLocation = tower.ScreenLocation + dirFromTowerToCreature * tower.Range;
			}

			canHitBeforeOutOfRange = true;
			hitIteration = currentIteration;
			return tempLocation;
		}

		public void ShowPath()
		{
		}

		public Vector2 dir;
		Vector2 dirPrev;
		bool turning;
		const float distToBeginTurn = 20;
		const float turnDistModifier = 1.75f;
		//Vector2 prevLoc;
        //float acc = 0.05f; //0.2f ihan jees
		float imagDist;
		float imagDistOrig;
		Vector2 imagDest;
		float vecPos;
		Vector2 imagPos;
		bool justGotSlowed;
        static int slowTransitionIters = 50;
        public void Update()
        {
            if (Born && Alive)
            {
				//Speed = defSpeed + defSpeed * CurrentGame.mouse.ScrollWheelValue / 5040;
                #region OLDMOVEMENT
                //Speed = defSpeed * 3 - ((AnimationUpdatePhase / (float)animationCycles)) * defSpeed * 3; // JELLYBUG LIMP                
                //float distanceToNextWaypoint = Vector2.Distance(Location, Path[nextWaypoint]);
                /*float rotationSpeed = Speed / 15;

                if (nextWaypoint > currWaypoint)
                {
                    currWaypoint = nextWaypoint;
                    if (nextWaypoint < AnglesBetweenWaypoints.Length)
                    {
                        angleDifference = AnglesBetweenWaypoints[nextWaypoint] - AnglesBetweenWaypoints[nextWaypoint - 1]; //--keep for future changing paths
                        if (angleDifference > Math.PI) angleDifference -= (float)Math.PI * 2;
                        else if (angleDifference < -Math.PI) angleDifference += (float)Math.PI * 2;
                        neededRotationSteps = (int)Math.Round(Math.Abs(angleDifference) / rotationSpeed);
                    }
                    else neededRotationSteps = 0;
                }

                if (distanceToNextWaypoint <= Speed * Math.Max(1, neededRotationSteps / 2) && !turning)
                {
                    if (nextWaypoint == Path.GetUpperBound(0))
                    {
                        TakeLifePoints(LifeDmg, ParentMap.Players);
                        Alive = false;
                        return;
                    }
                    if (neededRotationSteps == 0) nextWaypoint++;
                    else turning = true;
                    rotationStepCounter = neededRotationSteps;
                }
                if (turning)
                {
                    if (rotationStepCounter == 1) Angle = (float)Math.Atan2(-Path[nextWaypoint + 1].Y + PosY, Path[nextWaypoint + 1].X - PosX);
                    else if (angleDifference > 0) Angle += rotationSpeed;
                    else Angle -= rotationSpeed;
                    rotationStepCounter--;
                    if (rotationStepCounter == 0 && nextWaypoint < AnglesBetweenWaypoints.Length)
                    {
                        nextWaypoint++;
                        turning = false;
                    }
                }
                //PosX += Speed * (float)Math.Cos(Angle);
                //PosY -= Speed * (float)Math.Sin(Angle);*/
                #endregion

				#region OLDMOVEMENTRECENT
				//Vector2 dir = Vector2.Normalize(new Vector2(ParentMap.ParentGame.mouse.X, ParentMap.ParentGame.mouse.Y) - Location) * acc; //---Mousefollow:
				//if (distanceToNextWaypoint < 2)
				//{
				//    if (nextWaypoint >= Path.Count - 1)
				//    {
				//        ParentMap.creatureCue = CurrentGame.soundBank.GetCue("plurrp0");
				//        ParentMap.creatureCue.Play();
				//        TakeLifePoints(LifeDmg, ParentMap.Players);
				//        Alive = false;
				//        return;
				//    }
				//    else nextWaypoint++;                    
				//}
                //acc = defSpeed * 0.1f;
				//acc = 0.5f;
				//Vector2 dir = Vector2.Normalize(Path[nextWaypoint] - Location) * acc;
                //vel = vel.Length() > defSpeed ? Vector2.Normalize(vel + dir) * defSpeed : vel + dir;
                //if (Name.Equals("Jellybug")) vel = vel * (1.5f-(AnimationUpdatePhase / (float)animationCycles));
				//Location += vel;
				#endregion

				if (isSlowed)
				{
                    //if (justGotSlowed && slowedCounter >= CurrentSlowEffect[1] - 10)
                    //	Speed = defSpeed * (1 - ((CurrentSlowEffect[1] - slowedCounter) / 10f) * CurrentSlowEffect[0]);
                    if (Speed > defSpeed * (1 - CurrentSlowEffect[0]))
                    {
                        Speed -= defSpeed * (1 - CurrentSlowEffect[0]) * 0.1f; //----------------------29.9.2019
                        Speed = Math.Max(Speed, defSpeed * (1 - CurrentSlowEffect[0]));
                    }
                    if (slowedCounter <= slowTransitionIters)
                    {
                        Speed = defSpeed * (1 - ((float)slowedCounter / slowTransitionIters) * CurrentSlowEffect[0]);
                        TrailEngine.MaxParticles = (int)(TrailEngine.InitMaxParticles * ((float)slowedCounter / slowTransitionIters));
                    }
					if (slowedCounter <= 0)
					{
						isSlowed = false;
						CurrentSlowEffect = new float[2];
						Speed = defSpeed;
					}
					slowedCounter--;
                    TrailEngine.UpdateTrail();
                    TrailEngine.MaxParticles = TrailEngine.InitMaxParticles;
                }

                float distanceToNextWaypoint = Vector2.Distance(Location, Path[nextWaypoint]);

				if (distanceToNextWaypoint < distToBeginTurn)
				{
					if (nextWaypoint >= Path.Count - 1)
					{
						//ParentMap.creatureCue = CurrentGame.soundBank.GetCue("plurrp0");
						//ParentMap.creatureCue.Play();
						TakeLifePoints(ParentMap.Players);
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

				//float actualSpeed = Vector2.Distance(Location, prevLoc);

				CheckDistToGoal();

                //Angle = (float)Math.Atan2(-vel.Y, vel.X)/* + (float)Math.PI * 1.5f*/; //blaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
                ShakeTheCreature(false, 0);
                Angle += spin * (Speed / defSpeed); // Slow spin according to slow effect

                if (totalFrames > 1)
                {                    
                    if (AnimationUpdatePhase == animationCycles) AnimationUpdatePhase = 0;
                    currentFrame = (int)((AnimationUpdatePhase / (float)animationCycles) * totalFrames);
                    AnimationUpdatePhase++;
                }

				if (DmgHeadedThisWay.Count > 0)
				{
					for (int i = 0; i < DmgHeadedThisWay.Count; i++)
					{
						if (CurrentGame.gameTimer > DmgHeadedThisWay[i].Key)
							DmgHeadedThisWay.Remove(DmgHeadedThisWay[i]);
					}
				}
            }
			else if (Splatter.IsActive)
                Splatter.Update();
        }

        public void Draw(SpriteBatch sb)
        {
            //if (Born && Alive)
            //{
            TrailEngine.Draw(sb);

            if (totalFrames > 1)
                {
                    int row = (int)(currentFrame / (float)SpritesheetColumns);
                    int column = currentFrame % SpritesheetColumns;
                    Rectangle sourceRect = new Rectangle(Width * column, Height * row, Width, Height);
                    sb.Draw(Spritesheet, Location, sourceRect, Color.White, -Angle /*+ AngleOffset*/, Origin, 2, SpriteEffects.None, creatureDrawDepth);
                }
            else if (Name == "1") sb.Draw(Spritesheet, Location, null, Color.White, -Angle, Origin, 1, SpriteEffects.None, 0);
            else
			{
				sb.Draw(Spritesheet, Location + CreatureShadowDistance, null, Color.Black * 0.4f, Angle, Origin /*Vector2.One*/, 1, SpriteEffects.None, creatureDrawDepth + 0.01f);
				sb.Draw(Spritesheet, Location, null, Color.White, Angle, Origin /*Vector2.One*/, 1, SpriteEffects.None, creatureDrawDepth);
			}

            //foreach (Vector2 p in Path)
            //	sb.Draw(CurrentGame.pixel, p, null, Color.Red, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            //sb.Draw(CurrentGame.pixel, new Rectangle((int)Location.X, (int)Location.Y, (int)Vector2.Distance(Location, Path[nextWaypoint]), 1), null, Color.CadetBlue, (float)Math.Atan2(Path[nextWaypoint].Y - Location.Y, Path[nextWaypoint].X - Location.X), Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            //sb.Draw(CurrentGame.pixel, new Rectangle((int)Location.X, (int)Location.Y, (int)Vector2.Distance(Location, Path[nextWaypoint-1]), 1), null, Color.Orange, (float)Math.Atan2(Path[nextWaypoint-1].Y - Location.Y, Path[nextWaypoint-1].X - Location.X), Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
            //sb.Draw(Spritesheet, imagPos, null, Color.White * 0.5f, -Angle, Origin, 1, SpriteEffects.None, 0);
            //sb.DrawString(CurrentGame.font, Math.Round(imagDist).ToString(), Location, Color.Wheat);
            //sb.DrawString(CurrentGame.font, vecPos.ToString(), Location, Color.Wheat);
            //sb.Draw(CurrentGame.pixel, imagDest, null, Color.Green, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            //sb.DrawString(CurrentGame.font, Speed.ToString(), Vector2.One, Color.Wheat);


            /*if (hp != InitHp)//-------------SIIRRETTY HUDIIN PIIRTYMÄÄN ÖRÖJEN PÄÄLLE---------------------------------------------------------------------------
            {
                sb.Draw(ParentGame.pixel, new Rectangle((int)Location.X - hpBarWidth / 2, (int)(Location.Y - Height * SpriteScale / 2 - 1), hpBarWidth, 4), Color.Black); //black background
                sb.Draw(ParentGame.pixel, new Rectangle((int)Location.X - hpBarWidth / 2 + 1, (int)(Location.Y - Height * SpriteScale / 2), (int)((hpBarWidth -2) * (hp / InitHp)), 2), new Color(1 - hp / InitHp, hp / InitHp, 0));
            }*/
            //}
            //sb.DrawString(CurrentGame.font, Math.Round(DistanceToGoal).ToString(), Location, Color.Wheat);
        }

        public static Creature Clone(Creature model)
        {
            return new Creature(model.Type, model.Name, model.ParentMap, model.Spritesheet != null ? model.Spritesheet.Name : "", model.SpawnPointIndex, model.GoalPointIndex, (int)model.InitHp, model.defSpeed, model.ElemArmors, model.LifeDmg, model.EnergyBounty, model.SpriteScale);
            //return (Creature)this.MemberwiseClone();
        }

		public override string ToString()
		{
			return Spritesheet.Name + " (" + Math.Round(DistanceToGoal).ToString() + ")";
		}
    }
}

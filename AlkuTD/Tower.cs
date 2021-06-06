using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace AlkuTD
{
	//public enum DmgType { Basic, Splash }
	//public enum UpgLvl { Basic, Advanced, Max }
	//public enum ColorPriority { None, Red, Green, Blue }
	//public enum TargetPriority	{
	//	None = 0,

	//	First = 1,
	//	Last = 2,

	//	Tough = 3,
	//	Weak = 4,

	//	Fast = 5,
	//	Slow = 6,

	//	Mob = 7,
	//	Far = 8
	//}
	
	//public enum TowerSymbol // NOT IN USE YET
	//{
	//	A, E, I, O, U, X,
	//	Ä, Ë, Ï, Ö, Ü, Y,
	//	Â, Ê, Î, Ô, Û, Z
	//}

    public class Tower : ITower
    {     
        //------------Fields-------------------------------
        //CurrentGame ParentGame;
        public HexMap ParentMap;
        public char Symbol;
		public string Name;
        
        public static readonly char[] TowerSymbols = //----------ei enum ku '|' sun muut ei käy literaryiks vai mitä no
        {
            'A', // 0
            'E', // 1
            'I', // 2
            'O', // 3
            'U', // 4
            '|', // 5
            'Ä', // 6
            'Ë', // 7
            'Ï', // 8
            'Ö', // 9
            'Ü', // 10
            '†', // 11
            'Â', // 12
            'Ê', // 13
            'Î', // 14
            'Ô', // 15
            'Û', // 16
            '‡'  // 17
        };
        public int towerTypeIdx;
		public int towerBranch;
        public UpgLvl UpgradeLvl;
        public Player Owner; // ------------- !!
        Point mapCoord;
        public Point MapCoord { get { return mapCoord; } set { mapCoord = value; 
            ScreenLocation = ParentMap == null ? Vector2.Zero : ParentMap.ToScreenLocation(value); } }
        public Vector2 ScreenLocation;
        public float Range;
        public float InitRange;
        public bool ShowRadius;
        public bool Built;
        public bool IsExample;
		public float DPS;
        public float FireRate;
		public float FireRateSec;
        public float BulletSpeed;
        public Texture2D[] Textures;
        internal Vector2 texOrigin;
        internal Texture2D bulletTexture;
        internal Texture2D[] radiusTextures;
        internal List<Bullet> Bullets;
        internal float angle;
        internal float angleOffset;
        public short Dmg;
        public float[] slow;       //[0] = percentage, [1] = duration 
        public int Cost;
        public int BuildTime;
        public int buildTimer;
        public int buildFinishedCounter;
        internal int buildFinishedInit = 50; //------------------------------is this the real life?
        //public Element Element;
		public GeneSpecs GeneSpecs;
		public ColorPriority ElemPriority;
		public TargetPriority TargetPriority;
		internal DmgType DmgType;
		public int SplashRange;

        internal List<Creature> CreaturesInRange;
        internal List<Creature> ColoredInRange;
        internal List<Creature> PossibleTargets;

		public uint nextHitIteration;

        internal Color loadBarColor = Color.White;
        internal int loadBarWidth = 25;

        //--------Constructors------------------------------
        public Tower(char symbol, string name, Point mapCoord, float range, float firerate, Texture2D[] textures, GeneSpecs geneSpecs, Texture2D bulletTexture, float bulletSpeed, short dmg, DmgType dmgType, int splashRange, float[] slow, int cost, int buildTime, bool isExample)
        {
            ParentMap = CurrentGame.currentMap;
			Name = name;
            Symbol = symbol;
            MapCoord = mapCoord;
            //ScreenLocation = map.ToScreenLocation(mapCoord);
            /*this.screenLocation = new Vector2((float)(mapLocation[0] * map.stackedWidth + map.drawPos.X),
                                              (float)(mapLocation[1] * map.TileHeight + mapLocation[0] % 2 * (map.TileHeight / 2)) + map.drawPos.Y);*/
            Range = range;
            InitRange = range;
            FireRate = firerate;
            BulletSpeed = bulletSpeed;
            radiusTextures = new Texture2D[2];
            //this.radiusTextures[0] = radiusTexture;
            MakeRadiusCircle();
            ShowRadius = false;
            Textures = textures;
            //this.angleOffset = angleOffset;
            //angle = (float)Math.PI * 1.5f;
            texOrigin = new Vector2(textures[0].Width / 2, textures[0].Height / 2);
            this.bulletTexture = bulletTexture;
            Dmg = dmg;
            DmgType = dmgType;
            this.slow = slow;
            Cost = cost;
            BuildTime = buildTime;
            buildTimer = buildTime;
            IsExample = isExample;
            if (!IsExample)
                buildFinishedCounter = buildFinishedInit; //---------------------------ist dies der rihl leif?
            Built = false;
            Bullets = new List<Bullet>(10);
            towerTypeIdx = Array.IndexOf(TowerSymbols, symbol);
            UpgradeLvl = (UpgLvl)(towerTypeIdx / 6);
			towerBranch = towerTypeIdx % 6;
			SplashRange = splashRange;

			CreaturesInRange = new List<Creature>();
			ColoredInRange = new List<Creature>();
			PossibleTargets = new List<Creature>();

			GeneSpecs = geneSpecs;
			GeneSpecs.BaseTiers[Math.Max((int)GeneSpecs.GetPrimaryElem() - 1, 0)] = (int)(GeneSpecs.GetPrimaryElemStrength() * 100) / GeneSpecs.TierSize;

			FireRateSec = 1000 / (firerate * (float)ParentMap.ParentGame.TargetElapsedTime.TotalMilliseconds);
			DPS = dmg * FireRateSec;
		}

        //----------Methods-------------------------------

        int oldDiameter;
        public void MakeRadiusCircle() // HEMMETIN RASKAS KREAATIO (hm pitäiskö tehdä heti alkuun yks iso tekstuuri ("canvas"), josta näytetään rangen mukaan vaan osa
        {
            int diameter = (int)Math.Round(Range * 2) % 2 == 0 ? (int)Math.Round(Range * 2) - 1 : (int)Math.Round(Range * 2); //--ensure oddness (center-pixel) (downwards)
            if (diameter == oldDiameter)
                return;
            oldDiameter = diameter;
            Color lineColor = new Color(180, 180, 180, 180); //new Color(0, 0, 0, 150); //vanha 60 30 30
            //float lineThickness = 1 / 2f;
            
            int radius = diameter / 2; //--rounds down due oddness -> radius * 2 = one less than diameter (center-pixel)
            
            radiusTextures[1] = new Texture2D(CurrentGame.graphicsDManager.GraphicsDevice, diameter, diameter);
            Color[] texData = new Color[diameter * diameter];
            double angleStep = Math.PI / (diameter * 1.57); //--arbitrary or abt PI/2, less clutter than in *2 (x & y each its own place(..?))
            //float fillColorBonus = 0;
            //if (Range != initRange)
            //    fillColorBonus = 0.1f;

            #region OLD LOGIC
            //for (int i = 0; i < texData.Length; i++)
             //   texData[i] = Color.White;
            /*                                                                          // texData[radius] = top center pixel
            for (double angle = 0; angle < Math.PI * 2; angle += angleStep)             // texData[radius * 2] = upper right corner 
            {                                                                           // texData[diameter] = first pixel of the second row
                int x = radius + (int)Math.Round(radius * Math.Cos(angle));             // texData[radius * diameter] = left side center pixel
                int y = radius + (int)Math.Round(radius * Math.Sin(angle));             // texData[diameter * diameter - 1] = lower right corner (last pixel)

                texData[x + y * diameter] = lineColor; 
                /*                                     
                if (x < diameter / 2)                  
                    texData[x + y * diameter + 1] = lineColor;
                else texData[x + y * diameter - 1] = lineColor;
                if (y < diameter / 2)
                    texData[x + (y + 1) * diameter] = lineColor;
                else texData[x + (y - 1) * diameter] = lineColor;*/
            //}
            /*int xi = 1;
            int yi = 1;
            do
            {
                while (texData[xi + yi * diameter].Equals(Color.Transparent)) xi++;
                while (texData[xi + yi * diameter].Equals(lineColor)) xi++;
                while (texData[xi + yi * diameter].Equals(Color.Transparent))
                {
                    texData[xi + yi * diameter] = Color.White; //new Color(5, 5, 20, 10); //vanha 0 0 0 20
                    xi++;
                }
                xi = 1;
                yi++;

            } while (yi < diameter - 2);*/
            #endregion
            
            for (int x = 0; x < diameter; x++)
            { for (int y = 0; y < diameter; y++)
                {
                    int xb = x <= radius ? (radius - x) * (radius - x) : (x - radius) * (x - radius); //r-1ykköset
                    int yb = y <= radius ? (radius - y) * (radius - y) : (y - radius) * (y - radius);
                    float distFromCentr = (float)Math.Pow(Math.Sqrt(xb + yb) / radius, 6) * 0.3f; // relative (0-1) distance from center (sqrt(x+y)/r) curved (pow6) and scaled
                    //-------------Border
                    if (xb + yb < radius * radius + radius && xb + yb > radius * radius - radius) // toimii kans jostain syystä... (yhe paksune)
                    //if (xb + yb < (radius+lineThickness) * (radius+lineThickness) && xb + yb > (radius-lineThickness) * (radius-lineThickness))  
                        texData[x + y * diameter] = lineColor;
                    //-------------Fill
                    else if (xb + yb < radius * radius)
                        texData[x + y * diameter] = new Color(distFromCentr, distFromCentr /*+ fillColorBonus*/, distFromCentr + 0.05f, distFromCentr);

                }
            }
            //texData[radius] = Color.Turquoise;
            if (radiusTextures[0] == null)
            {
                radiusTextures[0] = new Texture2D(CurrentGame.graphicsDManager.GraphicsDevice, diameter, diameter);
                radiusTextures[0].SetData(texData);
            }
            radiusTextures[1].SetData(texData);
        }

        #region OLD PREDICATEBULLET
        //Predicate<Bullet> ready(
        /*
            //Shoot at ground
        public void Shoot(Map2D map, Vector2 target)
        {
            if (bullets.Count < 10 && !bullets.Exists(ready))
            {
                bullets.Add(new Bullet(target, bulletSpeed, dmg, dmgType, slow, redMult, greenMult, blueMult,
                                       screenLocation, bulletTexture));
                Debug.WriteLine("Bullet added and shot at {1}! Count is now {0}", bullets.Count, target.ToString());
            }
            else
                bullets.Find(ready).ShootAt(target);
                Debug.WriteLine("Odl bullet fired at {0}!", target.ToString());
        }
        */
        #endregion

		internal void UpdateBullet(Bullet b)
		{
			b.SplashRange = SplashRange;
			b.dmg = Dmg;
			b.slow = slow;
			b.speed = BulletSpeed;
			b.ElemSpecs = GeneSpecs;
		}

        //Shoot at creature
        public virtual void Shoot(Creature targetCreature)
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

			if (freeBullet.DmgType == AlkuTD.DmgType.Basic)
			{
				freeBullet.ShootAt(targetCreature);
				firerateCounter = (int)FireRate;
				targetCreature.DmgHeadedThisWay.Add(new KeyValuePair<uint,int>(CurrentGame.gameTimer + (uint)Math.Round(Vector2.Distance(ScreenLocation, targetCreature.Location) / BulletSpeed + 20), Dmg));
				targetCreature.TowersTargetingThis.Remove(this);
				ParentMap.towerCue = CurrentGame.soundBank.GetCue("kansi"); //-----------------------------randomization implemented in XACT
				ParentMap.towerCue.Play();
			}
			else if (freeBullet.DmgType == AlkuTD.DmgType.Splash)
			{
				bool canHit = false;
				uint hitIteration;
				while (!canHit)
				{
					Vector2 aimLocation = targetCreature.PredictHitLocation(this, out canHit, out hitIteration);
					if (canHit)
					{
						closestIterAim = aimLocation; // DEBUG
						freeBullet.ShootAt(aimLocation);
						firerateCounter = (int)FireRate;
						targetCreature.DmgHeadedThisWay.Add(new KeyValuePair<uint,int>(hitIteration, Dmg));
						targetCreature.TowersTargetingThis.Remove(this);
						ParentMap.towerCue = CurrentGame.soundBank.GetCue("kansi"); //-----------------------------randomization implemented in XACT
						ParentMap.towerCue.Play();

						return;
					}
					else if (PossibleTargets.Count > 1)
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
					else return;
				}
			}
        }

		Vector2 closestIterAim;
		//Vector2 ClosestIterLocation(Creature targetCreature, out bool canHit)
		//{
			//closestIterAim = targetCreature.CheckHitLocation(itersToRangeBoundary, Range, BulletSpeed, ScreenLocation, SplashRange, out canHit);

			// pilviajatus: erotuslaskufunktion tulos on MUUTOS (ei "mitä jää kun 5:stä poistaa 6", niinku ala-asteella opin) -> muutoksia voi lisätä muihin lukuihin
			#region pilvikamaa
			//int itersToHitSameDist = (int)Math.Round(Vector2.Distance(ScreenLocation, targetCreature.Location) / BulletSpeed);
			//Vector2 CreatureLocAfterInitAimTime = targetCreature.PredictMovement(itersToHitSameDist, out canHit);
			//int itersToHitLocAfterInitRange = (int)Math.Round(Vector2.Distance(ScreenLocation, CreatureLocAfterInitAimTime) / BulletSpeed);
			//if (!canHit || itersToHitLocAfterInitRange > itersToRangeBoundary)
			//{
			//    canHit = false;
			//    return Vector2.Zero;
			//}
			//closestIterAim = new Vector2();
			//if (itersToHitSameDist == itersToHitLocAfterInitRange)
			//{
			//    closestIterAim = CreatureLocAfterInitAimTime;
			//    return CreatureLocAfterInitAimTime;
			//}

			//Vector2 middleLoc = targetCreature.PredictMovement(itersToHitLocAfterInitRange, out canHit);
			//int itersToHitMiddle = (int)Math.Round(Vector2.Distance(ScreenLocation, middleLoc) / BulletSpeed);
			//if (!canHit || itersToHitMiddle > itersToRangeBoundary)
			//{
			//    canHit = false;
			//    return Vector2.Zero;
			//}
			//if (itersToHitLocAfterInitRange == itersToHitMiddle)
			//{
			//    closestIterAim = middleLoc;
			//    return middleLoc;
			//}

			//Vector2 FirstQuarterLoc = targetCreature.PredictMovement(itersToHitMiddle, out canHit);
			//int itersToHitFirstQuarter = (int)Math.Round(Vector2.Distance(ScreenLocation, FirstQuarterLoc) / BulletSpeed);
			//if (!canHit || itersToHitFirstQuarter > itersToRangeBoundary)
			//{
			//    canHit = false;
			//    return Vector2.Zero;
			//}
			//if (itersToHitMiddle == itersToHitFirstQuarter)
			//{
			//    closestIterAim = FirstQuarterLoc;
			//    return FirstQuarterLoc;
			//}

			//Vector2 FirstEightLoc = targetCreature.PredictMovement(itersToHitFirstQuarter, out canHit);
			//if (!canHit)
			//    return Vector2.Zero;
			//int itersToHitEightLoc = (int)Math.Round(Vector2.Distance(ScreenLocation, FirstEightLoc) / BulletSpeed);
			//if (itersToHitFirstQuarter == itersToHitEightLoc)
			//{
			//    closestIterAim = FirstEightLoc;
			//    return FirstEightLoc;
			//}

			//Vector2 FirstSixteenthLoc = targetCreature.PredictMovement(itersToHitEightLoc, out canHit);
			//if (!canHit)
			//    return Vector2.Zero;
			//int itersToHitSixteenthLoc = (int)Math.Round(Vector2.Distance(ScreenLocation, FirstSixteenthLoc) / BulletSpeed);

			//Vector2 PinPointedLoc = targetCreature.PredictMovement(itersToHitSixteenthLoc, out canHit);
			//int itersToHitPinpointLoc = (int)Math.Round(Vector2.Distance(ScreenLocation, PinPointedLoc) / BulletSpeed);

			//closestIterAim = PinPointedLoc;

			//closestIterAim = middleLoc;

			//closestIterAim = CreatureLocIfDistStaysSame;
			//float closestDist = Vector2.Distance(CreatureLocIfDistStaysSame, trueCreatureLoc);
			//Vector2 currentIterAim;
			//int closestIter;
			//int currentIter = itersToHitImmobile;

			//for (int i = 0; i < Math.Abs(itersToHitImmobile - iterstoHitCreatureLoc); i++)
			////do
			//{
			//    if (iterstoHitCreatureLoc < itersToHitImmobile)
			//        currentIter--;
			//    else
			//        currentIter++;

			//    currentIterAim = targetCreature.PredictMovement(currentIter);

			//    if (Vector2.Distance(closestIterAim, CreatureLocIfDistStaysSame) < closestDist)
			//    {
			//        closestIterAim = currentIterAim;
			//        closestDist = Vector2.Distance(closestIterAim, CreatureLocIfDistStaysSame);
			//        closestIter = currentIter;
			//    }

			//} //while (currentIter

			//Vector2 predictedAim = targetCreature.PredictMovement(iterstoHitCreatureLoc);
			//int middle = itersToHitImmobile + (iterstoHitCreatureLoc - itersToHitImmobile) / 2;
			//Vector2 middleAim = targetCreature.PredictMovement(middle);
			//int itersFirstQuarter = (int)Math.Round(Vector2.Distance(ScreenLocation, middleAim) / BulletSpeed);
			//aimLocation = targetCreature.PredictMovement(middle);
			#endregion
			//return closestIterAim;
		//}

        public void Upgrade() //-------rahansiirto muissa paikoissa (HUD.Drawissa (!) kahesti) jotta maped-ilmaisuus 
        {
            if (UpgradeLvl != UpgLvl.Max)
            {
                UpgradeLvl++;
                towerTypeIdx += 6;
				Tower exampleTower = HexMap.ExampleTowers[towerTypeIdx];
				Name = exampleTower.Name;
				Textures = exampleTower.Textures;
                BulletSpeed = exampleTower.BulletSpeed;
                bulletTexture = exampleTower.bulletTexture;
                Cost = exampleTower.Cost;
                Dmg = exampleTower.Dmg;
                DmgType = exampleTower.DmgType;
                //Element //---------------------------------------------------------------------------säilyykö vanhat elems?
                //firerateCounter //-------------------------------------------------------------------mielenkiintois. Vrt. Kingdom Rush jossa resetoituu
                FireRate = exampleTower.FireRate;
                InitRange = exampleTower.InitRange;
                radiusTextures = exampleTower.radiusTextures;
                Range = exampleTower.Range;
                slow = exampleTower.slow;
                Symbol = TowerSymbols[towerTypeIdx];
                BuildTime = exampleTower.BuildTime;
				SplashRange = exampleTower.SplashRange;
				//for (int i = 0; i < Bullets.Count; i++)
				//{
				//    Bullets[i].dmg = exampleTower.Dmg;
				//    Bullets[i].speed = exampleTower.BulletSpeed;
				//}
                ParentMap.Layout[MapCoord.Y, MapCoord.X] = Symbol;

				FireRateSec = 1000 / (FireRate * (float)ParentMap.ParentGame.TargetElapsedTime.TotalMilliseconds);
				DPS = Dmg * FireRateSec;
            }
        }

		public bool AddGeneTier(GeneType geneType)
		{
			int geneIdx = (int)geneType - 1;
			if (CurrentGame.players[0].GenePoints[geneIdx] >= GeneSpecs.TierSize)
			{
				if (GeneSpecs.BaseTiers[geneIdx] < 100 / GeneSpecs.TierSize)
				{
					int cost = GeneSpecs.TierSize;
					GeneSpecs.BaseTiers[geneIdx]++;
					if (GeneSpecs.BaseTiers[geneIdx] == 100 / GeneSpecs.TierSize)
						cost = GeneSpecs.TierSize + 1; // 33 33 34 -----!!
					GeneSpecs[geneType] += cost * 0.01f; 
					CurrentGame.players[0].GenePoints[geneIdx] -= cost;
					CurrentGame.HUD.UpdateGeneBars();
					return true;
				}
			}
			return false;
		}
		public bool WithdrawMainGeneTier()
		{
			GeneType mainType = GeneSpecs.GetPrimaryElem();
			int geneIdx = (int)mainType - 1;
			if (GeneSpecs.HasAny)
			{
				int cost = GeneSpecs.TierSize;
				if (GeneSpecs.BaseTiers[geneIdx] == 2)
					cost = GeneSpecs.TierSize +1; // 33 33 34 -----!!
				GeneSpecs.BaseTiers[geneIdx]--;
				GeneSpecs[mainType] -= cost * 0.01f;
				CurrentGame.players[0].GenePoints[geneIdx] += (int)Math.Round(cost * CurrentGame.GeneSellRate);
				CurrentGame.HUD.UpdateGeneBars();
				return true;
			}
			return false;
		}

		public void Disassemble()
		{
			int energyYield = 0;
			if (CurrentGame.gameState != GameState.InitSetup && CurrentGame.gameState != GameState.MapTestInitSetup)
			{
				for (int i = 0; i <= (int)UpgradeLvl; i++)
					energyYield += (int)Math.Round(HexMap.ExampleTowers[towerBranch + i * 6].Cost * CurrentGame.GeneSellRate);
			}
			else
			{
				for (int i = 0; i <= (int)UpgradeLvl; i++)
					energyYield += HexMap.ExampleTowers[towerBranch + i * 6].Cost;
			}

			while (GeneSpecs.HasAny)
				WithdrawMainGeneTier();

			CurrentGame.players[0].EnergyPoints += energyYield;
			CurrentGame.players[0].Towers.Remove(this);
			ParentMap.Layout[mapCoord.Y, mapCoord.X] = '0';
			ParentMap.CurrentLayout[mapCoord] = '0';
		}

		internal int firerateCounter = 0;		
		internal Creature currentTarget;
		internal List<Creature> previousTargets;
		internal Creature prevTarget;

		internal virtual void Hunt(List<Creature> aliveCreatures)
        {
			if (aliveCreatures.Count == 0)
				return;

			CreaturesInRange.Clear();
			ColoredInRange.Clear();
			PossibleTargets.Clear();
			for (int i = 0; i < aliveCreatures.Count; i++)
            {
                if (aliveCreatures[i].Born && Vector2.Distance(aliveCreatures[i].Location, ScreenLocation) <= Range)
                {
					CreaturesInRange.Add(aliveCreatures[i]);
					if (ElemPriority != AlkuTD.ColorPriority.None && aliveCreatures[i].ElemArmors[ElemPriority] > 0)
						ColoredInRange.Add(aliveCreatures[i]);
				}
			}

			if (ElemPriority != AlkuTD.ColorPriority.None && ColoredInRange.Count > 0)
				PossibleTargets = ColoredInRange;
			else PossibleTargets = CreaturesInRange;

			if (previousTargets != null) // Remove targeted status from creatures that fled range
			{
				for (int i = 0; i < previousTargets.Count; i++) 
				{
					if (!PossibleTargets.Contains(previousTargets[i]))
						previousTargets[i].TowersTargetingThis.Remove(this);
				}
			}
			previousTargets = new List<Creature>(PossibleTargets);

			if (CreaturesInRange.Count == 0)
				return;

			currentTarget = ChooseTarget();

			if (slow[0] > 0 && currentTarget != prevTarget) // Share planned target to splash towers for predicting creature location
				nextHitIteration = CurrentGame.gameTimer + (uint)Math.Max(firerateCounter, 0) + (uint)Math.Round(ParentMap.TileHeight / BulletSpeed);

			//angle = (float)Math.Atan2(currentTarget.Location.Y - ScreenLocation.Y, currentTarget.Location.X - ScreenLocation.X);
			if (firerateCounter <= 0)
			{
				Shoot(currentTarget);
			}

			prevTarget = currentTarget;
		}

		internal Creature ChooseTarget()
		{
			//if (DmgType == AlkuTD.DmgType.Basic)
			//{
				switch (TargetPriority)
				{
					case TargetPriority.Last:
							float biggestDistToGoal = 0;
							for (int i = 0; i < PossibleTargets.Count; i++)
							{
								if (PossibleTargets[i].DistanceToGoal > biggestDistToGoal)
								{
									biggestDistToGoal = PossibleTargets[i].DistanceToGoal;
									currentTarget = PossibleTargets[i];
								}
							}
						    break;
					case TargetPriority.Tough:
							int mostHp = 0;
							for (int i = 0; i < PossibleTargets.Count; i++)
							{
								if (PossibleTargets[i].hp > mostHp)
								{
									mostHp = (int)PossibleTargets[i].hp;
									currentTarget = PossibleTargets[i];
								}
							}
						    break;
					case TargetPriority.Weak:
							int leastHp = int.MaxValue;
							for (int i = 0; i < PossibleTargets.Count; i++)
							{
								if (PossibleTargets[i].hp < leastHp)
								{
									leastHp = (int)PossibleTargets[i].hp;
									currentTarget = PossibleTargets[i];
								}
							}
							break;
					case TargetPriority.Fast:
							float fastest = 0;
							for (int i = 0; i < PossibleTargets.Count; i++)
							{
								if (PossibleTargets[i].Speed > fastest)
								{
									fastest = PossibleTargets[i].Speed;
									currentTarget = PossibleTargets[i];
								}
							}
							break;
					case TargetPriority.Slow:
							float slowest = float.MaxValue;
							for (int i = 0; i < PossibleTargets.Count; i++)
							{
								if (PossibleTargets[i].Speed < slowest)
								{
									slowest = PossibleTargets[i].Speed;
									currentTarget = PossibleTargets[i];
								}
							}
							break;
					case TargetPriority.Mob:
							int bestMobCount = 0;
							for (int i = 0; i < PossibleTargets.Count; i++)
							{
								Creature currentCreature = PossibleTargets[i];
								int buddiesInSplashRange = 0;
								for (int k = 0; k < PossibleTargets.Count; k++)
								{
									if (i == k)
										continue;
									if (Vector2.Distance(currentCreature.Location, PossibleTargets[k].Location) <= SplashRange)
										buddiesInSplashRange++;
								}

								if (buddiesInSplashRange > bestMobCount)
								{
									bestMobCount = buddiesInSplashRange;
									currentTarget = PossibleTargets[i];
								}
							}
							if (bestMobCount == 0)
							{
								float smallDistToGoal = float.MaxValue;
								for (int i = 0; i < PossibleTargets.Count; i++)
								{
									if (PossibleTargets[i].DistanceToGoal < smallDistToGoal)
									{
										smallDistToGoal = PossibleTargets[i].DistanceToGoal;
										currentTarget = PossibleTargets[i];
									}
								}
							}
							break;
					case TargetPriority.None:
					case TargetPriority.First:
					default:
							float smallestDistToGoal = float.MaxValue;
							for (int i = 0; i < PossibleTargets.Count; i++)
							{
								if (PossibleTargets[i].DistanceToGoal < smallestDistToGoal)
								{
									smallestDistToGoal = PossibleTargets[i].DistanceToGoal;
									currentTarget = PossibleTargets[i];
								}
							};
							break;
				}
			//}
			//else if (DmgType == AlkuTD.DmgType.Splash)
			//{

			//}
			if (!currentTarget.TowersTargetingThis.Contains(this))
				currentTarget.TowersTargetingThis.Add(this);

			return currentTarget;
		}

        float oldRange;
        public virtual void Update(List<Creature> aliveCreatures)
        {
            if (CurrentGame.gameState == GameState.InitSetup || CurrentGame.gameState == GameState.MapTestInitSetup/*ParentMap.initSetupOn*/) 
				buildTimer = 0;

			if (buildTimer == 0)
            {
                if (firerateCounter > 0)
                    firerateCounter--;

                if (Built == false)
                {
                    Built = true;
                    ParentMap.towerCue = CurrentGame.soundBank.GetCue("pluip2");
                    ParentMap.towerCue.Play();
                }

                if (buildFinishedCounter > 0) buildFinishedCounter--; //---afterglow effect

                Hunt(aliveCreatures);

                for (int i = 0; i < Bullets.Count; i++)
                {
                    if (!Bullets[i].active) continue;
                    Bullets[i].Update();
                }

            }
            else buildTimer--;

            if (Range != oldRange)
                MakeRadiusCircle();
            oldRange = Range;
        }

        public const float radiusFadeCycles = 10; //------------------------------------not cool
        public int radiusFade = 0;
        public virtual void Draw(SpriteBatch sb)
        {
            float buildPhase = (BuildTime - buildTimer) / (float)BuildTime;

            if (buildTimer == 0)
            {
                sb.Draw(Textures[0], ScreenLocation, null, Color.White, angle + angleOffset, texOrigin, 1, SpriteEffects.None, 0);

                //---FirerateLoadBars 

                sb.Draw(CurrentGame.pixel, new Rectangle((int)ScreenLocation.X - loadBarWidth / 2, (int)(ScreenLocation.Y + ParentMap.TileHeight * 0.44f - 1), loadBarWidth, 4), Color.Black); //black background
                sb.Draw(CurrentGame.pixel, new Rectangle((int)ScreenLocation.X - loadBarWidth / 2 + 1, (int)(ScreenLocation.Y + ParentMap.TileHeight * 0.44f), (int)((loadBarWidth - 2) * ((FireRate - firerateCounter) / FireRate)), 2), loadBarColor);

                if (GeneSpecs.HasAny)
				{
					GeneType gt = GeneSpecs.GetPrimaryElem();
					int geneIdx = (int)gt -1;
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
                Color lineColor = buildTimer == 0? Color.White : Color.GreenYellow;
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
                if (buildPhase >= 1/6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 4 + 1, ScreenLocation.Y - ParentMap.TileHeight / 2), null, lineColor, MathHelper.ToRadians(60.9f),
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 1/6f) * 6), 2), SpriteEffects.None, 0);
                if (buildPhase >= 2/6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 2, ScreenLocation.Y), null, lineColor, MathHelper.ToRadians(118.2f) /*(float)Math.PI * (1.98f / 3f)*/, //118.95f
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 2/6f) * 6), 2), SpriteEffects.None, 0);
                if (buildPhase >= 3/6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X + ParentMap.TileWidth / 4, ScreenLocation.Y + ParentMap.TileHeight / 2 + 1), null, lineColor, (float)Math.PI,
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 3/6f) * 6), 2), SpriteEffects.None, 0);
                if (buildPhase >= 4/6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X - ParentMap.TileWidth / 4 - 1, ScreenLocation.Y + ParentMap.TileHeight / 2 + 1), null, lineColor, MathHelper.ToRadians(240.8f),//(float)Math.PI * (4f / 3f)
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2, ParentMap.TileWidth / 2 * (buildPhase - 4/6f) * 6), 2), SpriteEffects.None, 0);
                if (buildPhase >= 5/6f)
                    sb.Draw(CurrentGame.pixel, new Vector2(ScreenLocation.X - ParentMap.TileWidth / 2, ScreenLocation.Y), null, lineColor, (float)Math.PI * (5 / 3f),
                            Vector2.Zero, new Vector2(Math.Min(ParentMap.TileWidth / 2 - 1, ParentMap.TileWidth / 2 * (buildPhase - 5/6f) * 6), 2), SpriteEffects.None, 0);
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

		public void DrawBullets(SpriteBatch sb) // jotta luodit tornien päälle.. EI TARVIIS JOS KÄYTTÄS DRAW-LAYER-DEPTHEJÄ!
		{
			for (int i = 0; i < Bullets.Count; i++)
            {
				if (Bullets[i].Exploding)
					Bullets[i].DrawExplosion(sb);

                if (!Bullets[i].active) continue;

                Bullets[i].Draw(sb);

				//if (closestIterAim != null && DmgType == AlkuTD.DmgType.Splash)
				//{
				//    sb.Draw(CurrentGame.ball, closestIterAim, Color.Orange);
				//    sb.Draw(CurrentGame.pixel, new Rectangle((int)closestIterAim.X, (int)closestIterAim.Y, (int)Vector2.Distance(closestIterAim, currentTarget.Location), 2),
				//            new Rectangle(),
				//            Color.White * 0.3f,
				//            (float)Math.Atan2(currentTarget.Location.Y - closestIterAim.Y, currentTarget.Location.X - closestIterAim.X),
				//            Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
				//}
            }
		}

        public static Tower Clone(Tower t) //--HARD CODED IsExample !!!
        {
            Type checkedType = t.GetType();
            if (checkedType == typeof(SniperTower))
                return new SniperTower(t.MapCoord, t.UpgradeLvl, false);
            else if (checkedType == typeof(ParticleEaterTower))
                return new ParticleEaterTower(t.MapCoord, t.UpgradeLvl, false);
            else
                return new Tower(t.Symbol, t.Name, t.mapCoord, t.InitRange, t.FireRate, t.Textures, new GeneSpecs(t.GeneSpecs[GeneType.Red], t.GeneSpecs[GeneType.Green], t.GeneSpecs[GeneType.Blue]), t.bulletTexture, t.BulletSpeed, t.Dmg, t.DmgType, t.SplashRange, t.slow, t.Cost, t.BuildTime, false);
        }

        Tower ITower.Clone(Tower t)
        {
            return Clone(t);
        }

		public static Tower NewFromModel(Tower t, Point mapCoord)
		{
            Type checkedType = t.GetType();
            Tower tempTower;
            if (checkedType == typeof(SniperTower))
                tempTower = new SniperTower(mapCoord, t.UpgradeLvl, false);
            else if (checkedType == typeof(ParticleEaterTower))
                tempTower = new ParticleEaterTower(mapCoord, t.UpgradeLvl, false);
            else
                tempTower = new Tower(t.Symbol, t.Name, mapCoord, t.InitRange, t.FireRate, t.Textures, new GeneSpecs(t.GeneSpecs[GeneType.Red], t.GeneSpecs[GeneType.Green], t.GeneSpecs[GeneType.Blue]), t.bulletTexture, t.BulletSpeed, t.Dmg, t.DmgType, t.SplashRange, t.slow, t.Cost, t.BuildTime, false);
            tempTower.buildTimer = 0;
            tempTower.buildFinishedCounter = 0; //jottei Tower.Draw pistä valkosia outlinejä valmistumisen kunniaks
            tempTower.Built = true;
			return tempTower;
        }

        public override string ToString()
		{
			return TowerSymbols[towerTypeIdx] + " (" + this.MapCoord.X.ToString() + ";" + this.MapCoord.Y.ToString() + ")";
		}

		
	}
    
}

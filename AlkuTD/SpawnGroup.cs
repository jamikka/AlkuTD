using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class SpawnGroup
    {
        public CurrentGame ParentGame;
        public HexMap ParentMap;
        //public Wave ParentWave;//-----------------------TODO
		public Creature ExampleCreature;
        public Creature[] Creatures;
		public List<Creature> AliveCreatures;
        public Texture2D InfoTexture;
        public Vector2 infoTexOrigin;
        public int SpawnPointIndex;
        public int GoalPointIndex;
        //public int[] GoalPointIndexes;
        float SpawnFrequency;
        public int GroupDuration;
        public int spawnTimer = 0;
		public bool IsWholeGroupBorn;
        private Creature LastCreature;
        public BugInfoBox BugBox;

        public SpawnGroup()
        {

        }

        public SpawnGroup(int numberOfCreatures, Creature creature, int spawnFrequency, int spawnDuration, int groupNumber, int waveNumber)
        {
            ParentGame = creature.ParentGame;
			ParentMap = CurrentGame.currentMap;
			ExampleCreature = creature;
            Creatures = new Creature[numberOfCreatures];
            if (creature.Spritesheet != null && File.Exists(CurrentGame.ContentDir + "Creatures\\Smalls\\NEXTWAVE" + creature.Spritesheet.Name + ".xnb"))
                InfoTexture = ParentGame.Content.Load<Texture2D>("Creatures\\Smalls\\NEXTWAVE" + creature.Spritesheet.Name);
            else InfoTexture = CurrentGame.pixel;

			infoTexOrigin = new Vector2(InfoTexture.Width * 0.5f, InfoTexture.Height * 0.5f);
            //SpawnTimetable = new int[numberOfCreatures];
            SpawnPointIndex = creature.SpawnPointIndex;
            GoalPointIndex = creature.GoalPointIndex;

			AliveCreatures = new List<Creature>();
            Creatures[0] = Creature.Clone(creature);
            Creatures[0].Alive = true;
            Creatures[0].ParentGroup = this;
            Creatures[0].FindPath(); //-----------------------------pitäs kattoo reittivaihtoehdot vain kerran, jakaa niitä rndisti öröille ja sit individualisoida
            AliveCreatures.Add(Creatures[0]);
            for (int i = 1; i < numberOfCreatures; i++)
            {
                //Creatures[i] = creatureType.Clone();     //---------------------------------------YKSILÖT?!-------------
                Creatures[i] = Creature.Clone(creature);
                Creatures[i].Alive = true;
				Creatures[i].ParentGroup = this;

                Creatures[i].Path = new List<Vector2>();
                Creatures[i].Path.AddRange(Creatures[0].Path); //-----------------------------pitäs kattoo reittivaihtoehdot vain kerran, jakaa niitä rndisti öröille ja sit individualisoida
                Creatures[i].OrigPath.Clear();
                Creatures[i].OrigPath.AddRange(Creatures[0].Path);
                if (Creatures[i].Path.Count > 1)
                    Creatures[i].nextWaypoint = 1;
                else Creatures[i].nextWaypoint = 0;

                AliveCreatures.Add(Creatures[i]);
                //SpawnTimetable[i] = spawnFrequency;
            }

            SpawnFrequency = spawnFrequency;
            GroupDuration = spawnDuration;

            LastCreature = Creatures[numberOfCreatures - 1];
            GroupNumberColorMultiplier = 1 - (groupNumber * 0.3f);
            WaveNumberColorMultiplier = Math.Max(1 - (waveNumber * 0.3f), 0.3f);
            BugBox = new BugInfoBox(Vector2.Zero);
            //BugBox = new BugInfoBox(Vector2.Zero, Creatures[0], false, true, null);
        }

        public void FindPath()
        {
            if (Creatures[0].Path == null)
            {
                ParentMap.Pathfinder.InitializeTiles();
                List<Vector2> Path = ParentMap.Pathfinder.FindPath(ParentMap.SpawnPoints[SpawnPointIndex], ParentMap.GoalPoints[GoalPointIndex]);
                foreach (Creature c in Creatures)
                {
                    c.OrigPath.Clear();
                    c.OrigPath = Path;
                    c.Path.Clear();
                    c.Path.AddRange(c.OrigPath);
                    c.IndividualizePath();
                }
            }
            else
            {
                foreach (Creature c in Creatures)
                {
                    c.Path.Clear();
                    c.Path.AddRange(c.OrigPath);
                    c.IndividualizePath();
                }
            }
        }

        public float GroupNumberColorMultiplier { get; }

        private float WaveNumberColorMultiplier;
        public bool ShowingPath;
        public readonly int showPathFadeCycles = 5;
        public int showPathFade;
        public void ShowPath(SpriteBatch sb)
        {
            if (ShowingPath)
            {
                if (showPathFade < showPathFadeCycles)
                    showPathFade++;
                for (int i = 0; i < LastCreature.Path.Count - 1; i++)
                {
                    if (i < LastCreature.nextWaypoint - 1)
                        continue;
                    //liukukatkoviiva (dashLine -tekstuurilla)
                    sb.Draw(ParentGame.dashLine, new Rectangle((int)LastCreature.OrigPath[i].X, (int)LastCreature.OrigPath[i].Y, (int)Vector2.Distance(LastCreature.OrigPath[i], LastCreature.OrigPath[i + 1]) /*ParentMap.TileHeight +1*/, 2),
                            new Rectangle((int)(CurrentGame.gameTimer % 49.5f / 1.5f), 0, 33, 1), //old: ParentGame.GameTime.TotalGameTime.TotalMilliseconds % 825 / 25
                            Color.White * WaveNumberColorMultiplier * (showPathFade / showPathFadeCycles),
                            (float)Math.Atan2(LastCreature.OrigPath[i + 1].Y - LastCreature.OrigPath[i].Y, LastCreature.OrigPath[i + 1].X - LastCreature.OrigPath[i].X),
                            Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
                    //sb.Draw(healthbarTexture, new Rectangle((int)OrigPath[i].X, (int)OrigPath[i].Y, ParentMap.TileHeight, 1), null, Color.White * (showPathFade / showPathFadeCycles) * 0.4f, (float)Math.Atan2(OrigPath[i + 1].Y - OrigPath[i].Y, OrigPath[i + 1].X - OrigPath[i].X), Vector2.Zero, SpriteEffects.None, 0f);
                    //sb.DrawString(ParentMap.ParentGame.font,"" +(int)(ParentGame.GameTime.TotalGameTime.TotalMilliseconds % 600 / 50), new Vector2(1200, 300), Color.Wheat);
                }
            }
            else if (showPathFade > 0)
            {
                for (int i = 0; i < LastCreature.Path.Count - 1; i++)
                {
                    if (i < Creatures[0].nextWaypoint - 1)
                        continue;
                    //liukukatkoviiva (dashLine -tekstuurilla)
                    sb.Draw(ParentGame.dashLine, new Rectangle((int)LastCreature.OrigPath[i].X, (int)LastCreature.OrigPath[i].Y, ParentMap.TileHeight, 2),
                            new Rectangle((int)(CurrentGame.gameTimer % 49.5f / 1.5f), 0, 33, 1),
                            Color.White * WaveNumberColorMultiplier * (showPathFade / showPathFadeCycles),
                            (float)Math.Atan2(LastCreature.OrigPath[i + 1].Y - LastCreature.OrigPath[i].Y, LastCreature.OrigPath[i + 1].X - LastCreature.OrigPath[i].X),
                            Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
                    //sb.Draw(healthbarTexture, new Rectangle((int)OrigPath[i].X, (int)OrigPath[i].Y, ParentMap.TileHeight, 1), null, Color.White * (showPathFade / showPathFadeCycles) * 0.4f, (float)Math.Atan2(OrigPath[i + 1].Y - OrigPath[i].Y, OrigPath[i + 1].X - OrigPath[i].X), Vector2.Zero, SpriteEffects.None, 0f);
                }
                showPathFade--;
            }
        }

        public void Update()
        {
            for (int i = 0; i < Creatures.Length && i <= spawnTimer / SpawnFrequency; i++)
            {
                if (!Creatures[i].Born)
                {
                    Creatures[i].Born = true;
                    //Creatures[i].FindPath();//-----------------------------------------------------------------------------TURH? (map restart?)
                    Creatures[i].Location = Creatures[i].Path[0];
					ParentMap.AliveCreatures.Add(Creatures[i]);
                    //ParentMap.creatureCue = CurrentGame.soundBank.GetCue("geiger1");
					//ParentMap.creatureCue.Play();

					if (i == Creatures.GetUpperBound(0))
						IsWholeGroupBorn = true;
                }
				Creatures[i].Update();
            }
            spawnTimer++;
        }



        public void Draw(SpriteBatch spritebatch)
        {
            for (int i = 0; i < Creatures.Length && (i <= spawnTimer / SpawnFrequency); i++)
            {
                if (Creatures[i].Alive && Creatures[i].Born)
                    Creatures[i].Draw(spritebatch);
            }
        }
    }
}

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

        public SpawnGroup()
        {

        }

        public SpawnGroup(int numberOfCreatures, Creature creature, int spawnFrequency, int spawnDuration)
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
            for (int i = 0; i < numberOfCreatures; i++)
            {
                //Creatures[i] = creatureType.Clone();     //---------------------------------------YKSILÖT?!-------------
                Creatures[i] = Creature.Clone(creature);
                Creatures[i].Alive = true;
				Creatures[i].ParentGroup = this;
                Creatures[i].FindPath(); //-----------------------------pitäs kattoo reittivaihtoehdot vain kerran, jakaa niitä rndisti öröille ja sit individualisoida
				AliveCreatures.Add(Creatures[i]);
                //SpawnTimetable[i] = spawnFrequency;
            }

            SpawnFrequency = spawnFrequency;
            GroupDuration = spawnDuration;
        }

        public void FindPath()
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
                    ParentMap.creatureCue = CurrentGame.soundBank.GetCue("geiger1");
					ParentMap.creatureCue.Play();

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

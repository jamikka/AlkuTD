using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public class Wave
    {
        public CurrentGame ParentGame;
        public HexMap ParentMap;
        public SpawnGroup[] Groups;
        public List<SpawnGroup> TempGroups;
        public int[] GroupsTimetable;
        public int Duration;

        public Wave(HexMap map)
        {
            ParentMap = map;
            ParentGame = map.ParentGame;
            Initialize();
        }
        public Wave(HexMap map, SpawnGroup[] groups)
        {
            ParentMap = map;
            Groups = groups;
            ParentGame = map.ParentGame;

            Initialize();
        }

        /// <summary> Makes a timetable... </summary>
        public void Initialize()
        {
            if (Groups != null)
            {
                GroupsTimetable = new int[Groups.Length];
                for (int i = 1; i < Groups.Length; i++)
                    GroupsTimetable[i] = GroupsTimetable[i -1] + Groups[i - 1].GroupDuration;

                Duration = GroupsTimetable[Groups.GetUpperBound(0)] + Groups[Groups.GetUpperBound(0)].GroupDuration;
            }
            //else throw new NullReferenceException("balabalaNULLGROUP");
        }

        public void Update()
        {
            if (Groups[0].spawnTimer == 0)
            {
                ParentMap.creatureCue = CurrentGame.soundBank.GetCue("plurputus1");
                ParentMap.creatureCue.Play();
            }
            for (int i = 0; i < Groups.Length && Groups[0].spawnTimer >= GroupsTimetable[i]; i++)
                Groups[i].Update();
        }

        public void Draw(SpriteBatch spritebatch)
        {
            for (int g = 0; g < Groups.Length && Groups[0].spawnTimer >= GroupsTimetable[g]; g++)
                Groups[g].Draw(spritebatch);
        }
 
    }
}

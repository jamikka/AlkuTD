using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AlkuTD
{
    public class Player
    {
        public string Name;
        public byte CompletedLevels;
        public int[] HighScores;
        public int Score;
        public int EnergyPoints;
		public int[] GenePoints;
        public byte LifePoints;
        public List<Tower> Towers;
        public bool Alive;

        public Player(string name)
        {
            Name = name;
            CompletedLevels = 0;
            HighScores = new int[3];

            Towers = new List<Tower>();
            Alive = true;

			GenePoints = new int[3];

			CurrentGame.players[0] = this;
		}

        public int UpdateScore()
        {
			//Score = LifePoints * 10 + EnergyPoints / 10 + UpgradePoints * 10; // VANHA
			Score = LifePoints * 10 + EnergyPoints / 10;
            return Score;
        }
    }
}

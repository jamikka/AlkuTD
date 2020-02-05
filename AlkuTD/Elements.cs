using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlkuTD
{
	public class GeneSpecs
	{
		//public float RStrength, GStrength, BStrength;
		public int[] BaseTiers;
		public float RStrength { get { return r; } set { r = value; HasAny = value > 0 || g > 0 || b > 0; } }
		public float GStrength { get { return g; } set { g = value; HasAny = value > 0 || r > 0 || b > 0; } }
		public float BStrength { get { return b; } set { b = value; HasAny = value > 0 || r > 0 || g > 0; } }
		float r, g, b;
		public bool HasAny;

		public GeneSpecs() {	}

		public GeneSpecs(float rStrength, float gStrength, float bStrength)
		{
			RStrength = rStrength;
			GStrength = gStrength;
			BStrength = bStrength;

			HasAny = rStrength > 0 || gStrength > 0 || bStrength > 0;

			BaseTiers = new int[3];
		}

		public float this[GeneType whichElemWeakness] // INDEXER: used to check if tower's element matches creature's weakness
		{
			get
			{
				switch (whichElemWeakness) 
				{
					case GeneType.Red: return r;
					case GeneType.Green: return g;
					case GeneType.Blue: return b;
					default: return 0;
				}
			}
		}

		public float this[ColorPriority whichElemWeakness] // INDEXER: used to check if tower's element matches creature's weakness
		{
			get
			{
				return this[(GeneType)(int)whichElemWeakness];
			}
		}

		public GeneType GetPrimaryElem()
		{
			GeneType strongestElem = GeneType.None;
			float strongest = 0;
			for (int i = 1; i < 4; i++)
			{
				if (this[(ColorPriority)i] > strongest)
				{
					strongest = this[(ColorPriority)i];
					strongestElem = (GeneType)i;
				}
			}
			return strongestElem;
		}

		public float GetPrimaryElemStrength()
		{
			return this[GetPrimaryElem()];
		}
	}
}

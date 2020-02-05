using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlkuTD
{
	public class GeneSpecs
	{
		//public float RStrength, GStrength, BStrength;
		public const int TierSize = 33;
		public int[] BaseTiers;
		public float RStrength { get { return r; } set { r = value; HasAny = value > 0 || g > 0 || b > 0; } }
		public float GStrength { get { return g; } set { g = value; HasAny = value > 0 || r > 0 || b > 0; } }
		public float BStrength { get { return b; } set { b = value; HasAny = value > 0 || r > 0 || g > 0; } }
		float r, g, b;
		public bool HasAny;

		public GeneSpecs() 
		{
			BaseTiers = new int[3];
		}

		public GeneSpecs(float rStrength, float gStrength, float bStrength)
		{
			RStrength = rStrength;
			GStrength = gStrength;
			BStrength = bStrength;

			HasAny = rStrength > 0 || gStrength > 0 || bStrength > 0;

			BaseTiers = new int[3];
		}

		public float this[GeneType whichGeneType] // INDEXER: used to check if tower's element matches creature's weakness
		{
			get
			{
				switch (whichGeneType) 
				{
					case GeneType.Red: return r;
					case GeneType.Green: return g;
					case GeneType.Blue: return b;
					default: return 0;
				}
			}
			set
			{
				switch (whichGeneType)
				{
					case GeneType.Red: r = value; break;
					case GeneType.Green: g = value; break;
					case GeneType.Blue: b = value; break;
				}

				HasAny = r > 0 || g > 0 || b > 0;
			}
		}

		public float this[ColorPriority whichGeneType] // INDEXER: used to check if tower's element matches creature's weakness
		{
			get
			{
				return this[(GeneType)(int)whichGeneType];
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

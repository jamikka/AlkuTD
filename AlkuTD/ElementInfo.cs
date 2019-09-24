using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlkuTD
{
	public class Elements
	{
		//public float RStrength, GStrength, BStrength;
		public float RStrength { get { return RStrength; } set { RStrength = value; HasAny = value > 0 || GStrength > 0 || BStrength > 0; } }
		public float GStrength;
		public float BStrength;
		public bool HasAny;

		public Elements()
		{

		}

		public Elements(float rStrength, float gStrength, float bStrength)
		{
			RStrength = rStrength;
			GStrength = gStrength;
			BStrength = bStrength;

			HasAny = rStrength > 0 || gStrength > 0 || bStrength > 0;
		}
	}
}

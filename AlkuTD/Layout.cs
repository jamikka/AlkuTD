using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace AlkuTD
{
    public class MapLayout
    {
        public enum Adjacent {
            Above,
            UpRight,
            DownRight,
            Below,
            DownLeft,
            UpLeft
        }

        public char[,] Layout;

        public char this[Point mapCoord]
        {
            get
            {
                if (mapCoord.X >= 0 && mapCoord.X <= Layout.GetUpperBound(1) && mapCoord.Y >= 0 && mapCoord.Y <= Layout.GetUpperBound(0))
                    return Layout[mapCoord.Y, mapCoord.X];
                else return '!';
            }
            set
            {
                if (mapCoord.X >= 0 && mapCoord.X <= Layout.GetUpperBound(1) && mapCoord.Y >= 0 && mapCoord.Y <= Layout.GetUpperBound(0))
                    Layout[mapCoord.Y, mapCoord.X] = value;
            }
        }
        public char this[int y, int x, Adjacent direction] {
            get
            {
                switch (direction)
                {
                    case Adjacent.Above: if (y > 0)
                                            return Layout[y - 1, x]; break;
                    case Adjacent.Below: if (y < Layout.GetUpperBound(0))
                                            return Layout[y + 1, x]; break;
                    case Adjacent.UpRight: if (x < Layout.GetUpperBound(1))
                                            {
                                                if (x % 2 == 0 && y > 0)
                                                    return Layout[y-1, x+1];
                                                else 
                                                    return Layout[y, x+1];
                                            } break;
                    case Adjacent.DownRight: if (x < Layout.GetUpperBound(1))
                                            {
                                                if (x % 2 == 0)
                                                    return Layout[y, x+1];
                                                else if (y < Layout.GetUpperBound(0)) 
                                                    return Layout[y+1, x+1];
                                            } break;       

                    case Adjacent.DownLeft: if (x > 0)
                                            {
                                                if (x % 2 == 0)
                                                    return Layout[y, x-1];
                                                else if (y < Layout.GetUpperBound(0))
                                                    return Layout[y+1, x-1];
                                            } break;
                    case Adjacent.UpLeft: if (x > 0)
                                            {
                                                if (x % 2 == 0 && y > 0)
                                                    return Layout[y-1, x-1];
                                                else  
                                                    return Layout[y, x-1];
                                            } break;       
                        
                }
                return '!';
            }
        }

        public char[] NeighborTiles;

        public MapLayout (char[,] layout)
        {
            Layout = layout;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;

namespace AlkuTD
{
    public class Tile
    {
        public Point MapCoord;
        public Tile[] Neighbors;
		public List<Tile> NewNeighbors; // to check if in a tunnel
        public Tile Parent;
        public bool OddLowerCol;
        public bool IsOpen;
        public bool InOpenList;
        public bool Checked;
		public bool LockedWaypoint;
        public float G;
        public float F;

		public bool OnAltPath;

		public Vector3 CubeCoord;

		public override string ToString()
		{
			return this.OnAltPath ? MapCoord.X + "," + MapCoord.Y + " OAP" : MapCoord.X + "," + MapCoord.Y;
		}
    }

    public class Pathfinder
    {
        HexMap HexMap;
        Tile[,] openTiles;
        List<Tile> OpenList = new List<Tile>();
        List<Tile> ResolvedList = new List<Tile>();
        int levelWidth;
        int levelHeight;

        public Pathfinder(HexMap hexMap)
        {
            this.HexMap = hexMap;
            levelWidth = hexMap.Layout.GetLength(1);
            levelHeight = hexMap.Layout.GetLength(0);
            InitializeTiles();
        }

        float HeuristicDistance(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

		float CubeDistance(Tile a, Tile b)
		{
			return (Math.Abs(a.CubeCoord.X - b.CubeCoord.X) + Math.Abs(a.CubeCoord.Y - b.CubeCoord.Y) + Math.Abs(a.CubeCoord.Z - b.CubeCoord.Z)) / 2;
		}

		float StraightDistance(Tile a, Tile b)
		{
			Vector2 A = HexMap.ToScreenLocation(a.MapCoord);
			Vector2 B = HexMap.ToScreenLocation(b.MapCoord);
			float dist = Vector2.Distance(A, B);
			float result = dist / HexMap.TileHeight;
			result = (float)Math.Round(result, 1);
			//float result = Vector2.Distance(A, B) / HexMap.TileHeight;
			return result;
		}

        public void InitializeTiles()
        {
            openTiles = new Tile[levelWidth, levelHeight];

            //-----Initialize open tiles with (map)coords and IsOpen bools, declare 
            //-----neighbor tile arrays for them and then add them to the openTiles table
            for (int x = 0; x < levelWidth; x++)
            { for (int y = 0; y < levelHeight; y++)
                {
                    if (HexMap.Layout[y, x] == '\'' || HexMap.Layout[y, x] == '.' || ((int)HexMap.Layout[y, x] >= 49 && (int)HexMap.Layout[y, x] <= 57) || ((int)HexMap.Layout[y, x] >= 97 && (int)HexMap.Layout[y, x] <= 122)) // if open
                    {
                        Tile tile = new Tile();  // initialize:
                        tile.IsOpen = true;
                        tile.MapCoord = new Point(x, y);
                        tile.Neighbors = new Tile[6];
                        openTiles[x, y] = tile;

						tile.CubeCoord.X = x;
						tile.CubeCoord.Z = y - (x - (x & 1)) / 2;
						tile.CubeCoord.Y = -tile.CubeCoord.X - tile.CubeCoord.Z;
						tile.NewNeighbors = new List<Tile>();
                    }
                }
            }
            //-----Initialize neighbor data for open tiles
            for (int x = 0; x < levelWidth; x++)
            { for (int y = 0; y < levelHeight; y++)
                {
                    if (openTiles[x, y] == null) // esimerkin "|| !tile.Isopen" turha ehkä?
                        continue;
					
                    Point[] neighborCoords;
                    openTiles[x, y].OddLowerCol = x % 2 == 1;

                    if (openTiles[x, y].OddLowerCol)
                        neighborCoords = new Point[] {new Point(x, y-1),   // Up
                                                      new Point(x+1, y),   // Up-R
                                                      new Point(x+1, y+1), // Lo-R
                                                      new Point(x, y+1),   // Down
                                                      new Point(x-1, y+1), // Lo-L
                                                      new Point(x-1, y)};  // Up-L
                    else
                        neighborCoords = new Point[] {new Point(x, y-1),   // Up
                                                      new Point(x+1, y-1), // Up-R
                                                      new Point(x+1, y),   // Lo-R
                                                      new Point(x, y+1),   // Down
                                                      new Point(x-1, y),   // Lo-L
                                                      new Point(x-1, y-1)};// Up-L

                    for (int i = 0; i < neighborCoords.Length; i++) // for each open tile, add its neighbors------------------------------------NAAPURIT LAITARUUDUILTA PUUTTUU!!
                    {
                        if (neighborCoords[i].X < 0 || neighborCoords[i].X >= levelWidth ||  // ignore neighbor tile if
                            neighborCoords[i].Y < 0 || neighborCoords[i].Y >= levelHeight || // out of bounds
                            openTiles[neighborCoords[i].X, neighborCoords[i].Y] == null/* ||  // or if null --------(NOT OPEN..?)
                            !openTiles[neighborCoords[i].X, neighborCoords[i].Y].IsOpen*/)    // or if not open----------?
                            continue;

                        openTiles[x, y].Neighbors[i] = openTiles[neighborCoords[i].X, neighborCoords[i].Y];
						openTiles[x, y].NewNeighbors.Add(openTiles[neighborCoords[i].X, neighborCoords[i].Y]);
                    }
                }
            }
        }

        void ResetTileValues()
        {
            OpenList.Clear();
            ResolvedList.Clear();

            for (int x = 0; x < levelWidth; x++)
            { for (int y = 0; y < levelHeight; y++)
                {
                    if (openTiles[x, y] == null)
                        continue;
                    openTiles[x, y].InOpenList = false;
                    openTiles[x, y].Checked = false;
                    openTiles[x, y].Parent = null;
                    openTiles[x, y].G = float.MaxValue;
                    openTiles[x, y].F = float.MaxValue;
					openTiles[x, y].OnAltPath = false;
                }
            }
        }

        Tile FindBestTile()
        {
            Tile currentTile = OpenList[0];
            float smallestF = float.MaxValue;

            for (int i = 0; i < OpenList.Count; i++)
            {
                if (OpenList[i].F < smallestF)
                {
                    currentTile = OpenList[i];
                    smallestF = currentTile.F;
                }
            }
            return currentTile;
        }
        
		//--------Theta* algorithm with encouragement for turning
        public List<Vector2> FindPath(Point startPoint, Point goalPoint)
        {
            if (startPoint == goalPoint)
                return new List<Vector2>();
            ResetTileValues();

            if (openTiles[startPoint.X, startPoint.Y] == null || openTiles[goalPoint.X, goalPoint.Y] == null)
                return new List<Vector2>();

            Tile startTile = openTiles[startPoint.X, startPoint.Y];
            Tile goalTile = openTiles[goalPoint.X, goalPoint.Y];

            startTile.InOpenList = true;
            startTile.G = 0;
			startTile.F = CubeDistance(startTile, goalTile);
			startTile.Parent = startTile;
            OpenList.Add(startTile);

            while (OpenList.Count > 0)
            {
                Tile currentTile = FindBestTile();
                if (currentTile == null)
                    break;
                if (currentTile == goalTile)
                    return FinalPath(startTile, goalTile);

				//float smallestF = float.MaxValue; //----------OLD TRY AT SIMULT-SMOOTHENING
                for (int i = 0; i < currentTile.Neighbors.Length; i++)
                {
                    Tile neighbor = currentTile.Neighbors[i];
                    if (neighbor == null || !neighbor.IsOpen)
                        continue;

					#region OLD TRY AT SIMULT-SMOOTHENING
					//------------OLD TRY AT SIMULT-SMOOTHENING
					//float newF = newG + CubeDistance(neighbor, goalTile); 
					//if (newF < smallestF)
					//    smallestF = newF;
					//else if (newF == smallestF && !neighbor.LockedWaypoint) // If there are multiple tiles with smallestF, set all of them as OnAltPath
					//{
					//    for (int k = 0; k < i; k++)
					//    {
					//        Tile prevNeighbor = currentTile.Neighbors[k];
					//        if (prevNeighbor != null && !prevNeighbor.Checked) //!checke oli pilvikorjaus
					//        {
					//            if (prevNeighbor.F == smallestF && !prevNeighbor.LockedWaypoint)
					//                prevNeighbor.OnAltPath = true;
					//            else
					//                prevNeighbor.OnAltPath = false;
					//        }
					//    }
					//    neighbor.OnAltPath = true;
					//}
					//if (OpenList.Count == 1)
					//{
					//    currentTile.OnAltPath = false;
					//    currentTile.LockedWaypoint = true;
					//    if (currentTile.MapCoord != startTile.MapCoord && !neighbor.Checked)
					//        neighbor.LockedWaypoint = true;
					//}
					#endregion

					float newG = currentTile.G + 1;
					neighbor.NewNeighbors.Remove(currentTile);

					if (!neighbor.InOpenList && !neighbor.Checked) // a new acquaintance
                    {
						neighbor.InOpenList = true;
                        OpenList.Add(neighbor);

						if (CheckLOS(neighbor.MapCoord, currentTile.Parent.MapCoord))
						{
						    neighbor.Parent = currentTile.Parent;
						    //neighbor.F = newF + CubeDistance(neighbor, currentTile.Parent); // vanh YLLÄTTÄVÄN JEES
							neighbor.G = currentTile.Parent.G + StraightDistance(neighbor, currentTile.Parent); // G = parent's G + straightDist from parent
							neighbor.F = neighbor.G + CubeDistance(neighbor, goalTile); // F = G + dist to goal
							if (currentTile != startTile && currentTile.NewNeighbors.Count > 1 && CheckIfSameDir(currentTile, neighbor)) // IF not in a tunnel and going straight THEN PENALIZE (to encourage looking at different directions)
							{
								neighbor.F += 1.1f; //1.1: U oikein
								//neighbor.G += 1f;
							}
						}
						else
						{
							neighbor.G = newG;
							neighbor.F = newG + CubeDistance(neighbor, goalTile);
							neighbor.Parent = currentTile;
						}
                    }
                    else if (newG < neighbor.G) // an old friend and in a better light than before
                    {
                        neighbor.G = newG;
						neighbor.F = newG + CubeDistance(neighbor, goalTile);
                        neighbor.Parent = currentTile; // omamod
						neighbor.Checked = false; // omamod
						neighbor.InOpenList = true; // omamod
						OpenList.Add(neighbor); // omamod
                    }
					currentTile.NewNeighbors.Remove(neighbor);
                }
                OpenList.Remove(currentTile);
                currentTile.Checked = true;
            }
            return new List<Vector2>(); //--------------------Hmm unreachable
        }
        
        //------Old method for multiple goalpoints
		//public List<Vector2> FindPath(Point startPoint, int[] goalPointIndexes)
		//{
		//    //---------Check which of the creature's goalpoints are open
		//    List<int> openGoalPointIndexes = new List<int>();
		//    for (int k = 0; k < goalPointIndexes.Length; k++)
		//    {
		//        if (HexMap.GoalPointTimetable[goalPointIndexes[k]] <= HexMap.mapTimer)
		//            openGoalPointIndexes.Add(goalPointIndexes[k]);
		//    }
		//    if (openGoalPointIndexes.Count == 0)
		//        return new List<Vector2>{HexMap.ToScreenLocation(startPoint)};

		//    //---------Declare an array of lists of pathpoints
		//    List<Point>[] Paths = new List<Point>[openGoalPointIndexes.Count];
		//    for (int x = 0; x < Paths.Length; x++)
		//        Paths[x] = new List<Point>();
                
		//    List<Vector2> ChosenPath = new List<Vector2>();

		//    //---------The big loop to find paths to open goalpoints
		//    for (int g = 0; g < openGoalPointIndexes.Count; g++)
		//    {
		//        if (startPoint == HexMap.GoalPoints[openGoalPointIndexes[g]])
		//        {
		//            ChosenPath.Add(HexMap.ToScreenLocation(startPoint));
		//            return ChosenPath;
		//        }
		//        ResetTileValues();

		//        Tile startTile = openTiles[startPoint.X, startPoint.Y];
		//        Tile goalTile = openTiles[HexMap.GoalPoints[openGoalPointIndexes[g]].X, HexMap.GoalPoints[openGoalPointIndexes[g]].Y];

		//        startTile.InOpenList = true;
		//        startTile.G = 0;
		//        startTile.F = HeuristicDistance(startPoint, goalTile.MapCoord);
		//        OpenList.Add(startTile);

		//        while (OpenList.Count > 0)
		//        {
		//            Tile currentTile = FindBestTile();
		//            if (currentTile == null)
		//            {
		//                Debug.WriteLine("current tile null!");
		//                break;
		//            }
		//            if (currentTile == goalTile)
		//            {
		//                OpenList.Clear(); //oks hyvä..?
		//                ResolvedList.Add(goalTile);
		//                Tile parentTile = goalTile.Parent;

		//                do
		//                {
		//                    ResolvedList.Add(parentTile);
		//                    parentTile = parentTile.Parent;
		//                } while (parentTile != null);                        

		//                for (int ib = ResolvedList.Count -1; ib >= 0; ib--)
		//                    Paths[g].Add(new Point(ResolvedList[ib].MapCoord.X, ResolvedList[ib].MapCoord.Y)); 

		//                continue;
		//            }

		//            for (int i = 0; i < currentTile.Neighbors.Length; i++)
		//            {
		//                Tile neighbor = currentTile.Neighbors[i];
		//                if (neighbor == null || !neighbor.IsOpen)
		//                    continue;
		//                float newG = currentTile.G + 1;
		//                float newF = newG + HeuristicDistance(neighbor.MapCoord, goalTile.MapCoord); //----- OLI ENNEN "newF = neighbor.G + H"   !!!

		//                if (!neighbor.InOpenList && !neighbor.Checked) // if a new acquaintance
		//                {
		//                    neighbor.G = newG;
		//                    neighbor.F = newF;
		//                    neighbor.Parent = currentTile;
		//                    neighbor.InOpenList = true;
		//                    OpenList.Add(neighbor);
		//                }
		//                else // if an old friend
		//                {
		//                    if (neighbor.G > newG)
		//                    {
		//                        neighbor.G = newG;
		//                        neighbor.F = newF;
		//                        neighbor.Parent = currentTile;
		//                    }
		//                }
		//            }
		//            OpenList.Remove(currentTile);
		//            //Debug.WriteLine(currentTile.MapCoord);
		//            currentTile.Checked = true;
		//        }                
		//    }
            
		//    /*foreach (List<Point> path in Paths)
		//    { foreach (Point pathpoint in path)
		//        {
		//            Debug.WriteLine(pathpoint);
		//        }
		//    }*/
            
		//    int bestPathIndex = 0;
		//    if (Paths.Length > 1)
		//    {
		//        int fewestPoints = int.MaxValue;                
		//        for (int i = 0; i < Paths.Length; i++)
		//        {
		//            if (Paths[i].Count < fewestPoints)
		//            {
		//                fewestPoints = Paths[i].Count;
		//                bestPathIndex = i;
		//            }
		//        }
		//    }
		//    foreach (Point p in Paths[bestPathIndex])
		//        ChosenPath.Add(HexMap.ToScreenLocation(p));

		//    return ChosenPath;
		//}

        List<Vector2> FinalPath(Tile startTile, Tile goalTile)
        {
			List<Vector2> finalPath = new List<Vector2>();
			ResolvedList.Add(goalTile);
            Tile parentTile = goalTile.Parent;

            do
            {
                ResolvedList.Add(parentTile);
                parentTile = parentTile.Parent;
            }
            while (parentTile != startTile);
			ResolvedList.Add(startTile);

			#region OLD TRY AT POST-SMOOTHING
			//List<Point> finPoints = new List<Point>();
			//Point currTile = ResolvedList[0].MapCoord;
			//Point hoppedTile;

			//for (int i = 0; i < ResolvedList.Count - 2; i++)
			//{
			//    hoppedTile = ResolvedList[i + 2].MapCoord;
			//    if (!CheckLOS(currTile, hoppedTile))
			//    {
			//        finalPath.Insert(0, HexMap.ToScreenLocation(currTile));
			//        finPoints.Insert(0, currTile);
			//        if (i == ResolvedList.Count - 3)
			//        {
			//            finalPath.Insert(0, HexMap.ToScreenLocation(ResolvedList[i + 1].MapCoord));
			//            finPoints.Insert(0, ResolvedList[i + 1].MapCoord);
			//        }

			//        currTile = ResolvedList[i + 1].MapCoord;
			//    }
			//    else if (i == ResolvedList.Count - 3)
			//        finalPath.Insert(0, HexMap.ToScreenLocation(currTile));

			//    foreach (Point p in finPoints)
			//        Debug.WriteLine(p.ToString());
			//    Debug.WriteLine("");
			//}
			//finalPath.Insert(0, HexMap.ToScreenLocation(ResolvedList[ResolvedList.Count - 1].MapCoord));
			#endregion

			#region OLD TRY AT SIMULT-SMOOTHING
			//for (int i = ResolvedList.Count - 1; i >= 0; i--)
			//{
				//if (!ResolvedList[i].OnAltPath) //----OLD TRY AT SIMULT-SMOOTHING
				//{
					//finalPath.Add(HexMap.ToScreenLocation(ResolvedList[i].MapCoord.X, ResolvedList[i].MapCoord.Y));
				//}
				//else if (!CheckLOS(currTile, ResolvedList[i].MapCoord))
				//{
				//    finalPath.Add(HexMap.ToScreenLocation(ResolvedList[i].MapCoord.X, ResolvedList[i].MapCoord.Y));
				//}
			//}
			#endregion

			foreach (Tile t in ResolvedList)
				finalPath.Insert(0, HexMap.ToScreenLocation(t.MapCoord));

            return finalPath;
        }

		public bool CheckLOS(Point A, Point B)
		{
			Vector2 a = HexMap.ToScreenLocation(A);
			Vector2 b = HexMap.ToScreenLocation(B);
			Vector2 broadened;
			Vector2 dir = Vector2.Normalize(b - a);
			Vector2 perpDir = new Vector2(dir.Y, -dir.X);
			int clearance = (int)(HexMap.TileHalfWidth * 0.4f); // the width of the ray

			a += (HexMap.TileHalfHeight + 6) * dir; //start raycasting at the tile border + some
			int targetDist = (int)Vector2.Distance(b, a);

			for (int i = 0; i < targetDist; i++)
			{
				broadened = i % 2 == 1 ? a + clearance * perpDir : a - clearance * perpDir; // check collision at clearance distance perpendicular to the ray (+odd, -even)
				Point coord = HexMap.ToMapCoordinate(broadened);
				if (coord.X >= 0 && coord.Y >= 0 && coord.X < HexMap.Layout.GetLength(1) && coord.Y < HexMap.Layout.GetLength(0))
				{
					char tileChar = HexMap.Layout[coord.Y, coord.X];
					if (tileChar != '.' && tileChar != '\'' && !(tileChar >= 'a' && tileChar <= 'i') && !(tileChar >= '1' && tileChar <= '9'))
						return false;
					else a += dir;
				}
				else return false;
			}
			return true;
		}

		public bool CheckIfSameDir(Tile curr, Tile next)
		{
			Vector3 dirCurrNext = Vector3.Normalize(next.CubeCoord - curr.CubeCoord);
			Vector3 dirParentNext = Vector3.Normalize(next.CubeCoord - curr.Parent.CubeCoord);

			if (Math.Round(dirCurrNext.X, 4) == Math.Round(dirParentNext.X,4) && Math.Round(dirCurrNext.Y, 4) == Math.Round(dirParentNext.Y,4) && Math.Round(dirCurrNext.Z, 4) == Math.Round(dirParentNext.Z,4))
				return true;
			else return false;
		}
    }
}

     


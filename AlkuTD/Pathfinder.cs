﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

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
            //return this.OnAltPath ? MapCoord.X + "," + MapCoord.Y + " OAP" : MapCoord.X + "," + MapCoord.Y;
            return MapCoord.X + "," + MapCoord.Y + " G:" + Math.Round(G, 2); //+ ", F:" + F;
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

        List<Tile> pathFindSteps;
        List<Tile> stringpullStepsG;
        List<Tile> stringpullStepsS;


        public Pathfinder(HexMap hexMap)
        {
            this.HexMap = hexMap;
            levelWidth = hexMap.Layout.GetLength(1);
            levelHeight = hexMap.Layout.GetLength(0);
            InitializeTiles();
            pathFindSteps = new List<Tile>();
            stringpullStepsG = new List<Tile>();
            stringpullStepsS = new List<Tile>();
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
			//result = (float)Math.Round(result, 2);
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
                        if (openTiles[x, y].NewNeighbors.Count <= 4)
                            openTiles[x, y].LockedWaypoint = true;
                        else openTiles[x, y].LockedWaypoint = false;
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

        Tile FindBestFTile()
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
        Tile goalTile;
        public List<Vector2> FindPath(Point startPoint, Point goalPoint)
        {
            pathFindSteps.Clear();

            if (startPoint == goalPoint)
                return new List<Vector2>();
            ResetTileValues();

            if (openTiles[startPoint.X, startPoint.Y] == null || openTiles[goalPoint.X, goalPoint.Y] == null) // stop with a empty list if start or goalpoint is missing
                return new List<Vector2>();

            Tile startTile = openTiles[startPoint.X, startPoint.Y];
            goalTile = openTiles[goalPoint.X, goalPoint.Y];

            startTile.InOpenList = true;
            startTile.G = 0;
			startTile.F = CubeDistance(startTile, goalTile); // F as CubeDistance
			startTile.Parent = startTile;
            OpenList.Add(startTile);

            while (OpenList.Count > 0) //------------------------------------ SEARCH AND POPULATE THE OpenList STARTING FROM startTile
            {
                Tile currentTile = FindBestFTile();
                if (currentTile == null) // stop if startTile doesnt exist or return if currTile is the goalTile
                    break;
                if (currentTile == goalTile)
                    return FinalPath(startTile, goalTile);

                for (int i = 0; i < currentTile.Neighbors.Length; i++) //-----GO THROUGH ALL OF OpenList[currentTile]'S OPEN NEIGHBORS CLOCKWISE FROM N TO NW
                {
                    Tile NEighbCheck = currentTile.Neighbors[i];
                    //Tile NEighbCheck = NeighborInGoalDirection(currentTile); //TODO: NeighborInGoalDirection
                    if (NEighbCheck == null || !NEighbCheck.IsOpen)
                        continue;

					#region OLD TRY AT SIMULT-SMOOTHENING
					//------------OLD TRY AT SIMULT-SMOOTHENING
					//float newF = currGPlus1 + CubeDistance(NEighbCheck, goalTile); 
					//if (newF < smallestF)
					//    smallestF = newF;
					//else if (newF == smallestF && !NEighbCheck.LockedWaypoint) // If there are multiple tiles with smallestF, set all of them as OnAltPath
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
					//    NEighbCheck.OnAltPath = true;
					//}
					//if (OpenList.Count == 1)
					//{
					//    currentTile.OnAltPath = false;
					//    currentTile.LockedWaypoint = true;
					//    if (currentTile.MapCoord != startTile.MapCoord && !NEighbCheck.Checked)
					//        NEighbCheck.LockedWaypoint = true;
					//}
					#endregion

					float currGPlus1 = currentTile.G + 1;
					NEighbCheck.NewNeighbors.Remove(currentTile);

					if (!NEighbCheck.InOpenList && !NEighbCheck.Checked) // a new acquaintance in OpenList
                    {
                        OpenList.Add(NEighbCheck);
                        NEighbCheck.InOpenList = true;

                        pathFindSteps.Add(NEighbCheck/*.MapCoord*/);

                        if (!NEighbCheck.LockedWaypoint && CheckLOS(NEighbCheck.MapCoord, currentTile.Parent.MapCoord, true) && !CheckIfStraightHexLine(NEighbCheck, currentTile.Parent)) // we see back to parent from the neighbor (IGNORE IF PARENT IS IN A SRAIGHT HEX LINE)
                        {
                            NEighbCheck.Parent = currentTile.Parent;
                            NEighbCheck.G = currentTile.Parent.G + StraightDistance(NEighbCheck, currentTile.Parent); // G = parent's G + straightDist from parent
                            NEighbCheck.F = NEighbCheck.G + CubeDistance(NEighbCheck, goalTile); // F = G + CubeDist to goal

                            //if (currentTile != startTile && currentTile.NewNeighbors.Count > 1 && CheckIfContinuedDir(currentTile, NEighbCheck))
                            //    NEighbCheck.F += 1.1f;
                        }
                        else // we don't see back to parent from the neigbor, or straight hex line, or LockedWaypoint
                        {
                            NEighbCheck.G = currGPlus1;
                            NEighbCheck.F = currGPlus1 + CubeDistance(NEighbCheck, goalTile);
                            NEighbCheck.Parent = currentTile;

                            // IF 2 NEW NEIGHBORS (not in a tunnel) AND CONTINUED DIR, ENCUMBER F to encourage looking at different directions
                            if (currentTile != startTile && currentTile.NewNeighbors.Count > 1 && CheckIfContinuedDir(currentTile, NEighbCheck))
                                NEighbCheck.F += 0.001f;
                        }
                    }
                    else if (currGPlus1 < NEighbCheck.G) // an old friend in a better light than before
                    {
                        if (!NEighbCheck.LockedWaypoint && CheckLOS(NEighbCheck.MapCoord, currentTile.Parent.MapCoord, true) && !CheckIfStraightHexLine(NEighbCheck, currentTile.Parent)) //Check if we see to current parent from old friend (IGNORE IF PARENT IS IN A SRAIGHT HEX LINE)
                        {
                            NEighbCheck.Parent = currentTile.Parent;
                            NEighbCheck.G = currentTile.Parent.G + StraightDistance(NEighbCheck, currentTile.Parent);
                            NEighbCheck.F = NEighbCheck.G + CubeDistance(NEighbCheck, goalTile);
                        }
                        else //normal step, set currTile as par
                        { 
                            NEighbCheck.G = currGPlus1;
                            NEighbCheck.F = currGPlus1 + CubeDistance(NEighbCheck, goalTile);
                            NEighbCheck.Parent = currentTile; // omamod
                        }
                        NEighbCheck.Checked = false; // omamod
                        NEighbCheck.InOpenList = true; // omamod
                        OpenList.Add(NEighbCheck); // omamod
                    }
                }
                OpenList.Remove(currentTile);
                currentTile.Checked = true;
            }
            return new List<Vector2>(); //--------------------unreachable
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
        //            Tile currentTile = FindBestFTile();
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
        //                Tile NEighbCheck = currentTile.Neighbors[i];
        //                if (NEighbCheck == null || !NEighbCheck.IsOpen)
        //                    continue;
        //                float currGPlus1 = currentTile.G + 1;
        //                float newF = currGPlus1 + HeuristicDistance(NEighbCheck.MapCoord, goalTile.MapCoord); //----- OLI ENNEN "newF = NEighbCheck.G + H"   !!!

        //                if (!NEighbCheck.InOpenList && !NEighbCheck.Checked) // if a new acquaintance
        //                {
        //                    NEighbCheck.G = currGPlus1;
        //                    NEighbCheck.F = newF;
        //                    NEighbCheck.Parent = currentTile;
        //                    NEighbCheck.InOpenList = true;
        //                    OpenList.Add(NEighbCheck);
        //                }
        //                else // if an old friend
        //                {
        //                    if (NEighbCheck.G > currGPlus1)
        //                    {
        //                        NEighbCheck.G = currGPlus1;
        //                        NEighbCheck.F = newF;
        //                        NEighbCheck.Parent = currentTile;
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
        float pathTotalDist;
        List<Vector2> FinalPath(Tile startTile, Tile goalTile)
        {
            stringpullStepsG.Clear();
            stringpullStepsS.Clear();
            Tile tileFromWhichCurrentFurthestLOS = null;
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

            #region POST-STRINGPULLING (jos ennätyspitkällä lopusta alkua kohti nähty vanhempi ei oo samaan suuntaan ku vanha enkka, vaihetaan aiempien vanhemmaksi!
            Tile currTile;
            Tile peekedTile;
            int furthestParentIdx = 0;
            int k = ResolvedList.Count - 1;
            for (int i = 0; i < ResolvedList.Count;)
            {
                currTile = ResolvedList[i]; //from goal (currTile) toward
                peekedTile = ResolvedList[k]; //far start (peekedTile)
                if (!stringpullStepsG.Contains(currTile))
                    stringpullStepsG.Add(currTile);

                if (!stringpullStepsS.Contains(peekedTile))
                    stringpullStepsS.Add(peekedTile);

                if (CubeDistance(currTile, peekedTile) <= 1)
                {
                    //currTile.Parent = peekedTile;

                    if (peekedTile == startTile)
                    {
                        currTile.Parent = startTile;
                        break;
                    }
                    i++;
                    if (k > furthestParentIdx)
                    {
                        furthestParentIdx = k;
                        tileFromWhichCurrentFurthestLOS = ResolvedList[i];
                    }
                    //furthestParentIdx = k;
                    k = ResolvedList.Count - 1;
                }
                else if (CheckLOS(currTile.MapCoord, peekedTile.MapCoord, true)) //we see toward start
                {

                    currTile.Parent = peekedTile;

                    if (k > furthestParentIdx) //peeked tile is furthest we've seen
                    {
                        if (tileFromWhichCurrentFurthestLOS != null) //if path from currTile to furthestSeen (A-B-D) is shorter than prev (A-C-D), set new parents
                        {
                            float oldPathDist = StraightDistance(tileFromWhichCurrentFurthestLOS, ResolvedList[furthestParentIdx]);
                            oldPathDist += StraightDistance(ResolvedList[furthestParentIdx], peekedTile);
                            float newPathDist = StraightDistance(tileFromWhichCurrentFurthestLOS, currTile);
                            newPathDist += StraightDistance(currTile, peekedTile);

                            if (newPathDist < oldPathDist)
                            {
                                for (int j = 0; j < furthestParentIdx; j++)
                                {
                                    if (ResolvedList[j].Parent == ResolvedList[furthestParentIdx])
                                        ResolvedList[j].Parent = currTile;
                                }
                            }
                            //Debug.WriteLine($"newPathDist {newPathDist} < oldPathDist {oldPathDist} = {newPathDist < oldPathDist}");

                            //Vector3 dirCurrToSeen = GetDir(currTile, peekedTile);
                            //Vector3 dirOldToNewParent = GetDir(ResolvedList[furthestParentIdx], peekedTile);
                            //if (dirCurrToSeen != dirOldToNewParent)
                            //{
                            //    for (int j = 0; j < furthestParentIdx; j++)
                            //    {
                            //        if (ResolvedList[j].Parent == ResolvedList[furthestParentIdx])
                            //            ResolvedList[j].Parent = currTile;
                            //    }
                            //}
                        }
                        furthestParentIdx = k;
                        tileFromWhichCurrentFurthestLOS = ResolvedList[i];
                    }

                    if (peekedTile == startTile)
                        break;
                    i++;
                    k = ResolvedList.Count - 1;
                }
                else if (k > 0) //peek closer away from start
                    k--;
            }

            ResolvedList.Clear(); //---- Make the ResolvedList again with stringpulled parents
            ResolvedList.Add(goalTile);
            parentTile = goalTile.Parent;
            do
            {
                ResolvedList.Add(parentTile);
                parentTile = parentTile.Parent;
            }
            while (parentTile != startTile);
            ResolvedList.Add(startTile);

            #endregion

            foreach (Tile t in ResolvedList)
                finalPath.Insert(0, HexMap.ToScreenLocation(t.MapCoord));

            pathTotalDist = 0; //---- TarkistusMatkaLasku
            for (int j = 1; j < finalPath.Count; j++)
            {
                pathTotalDist += StraightDistance(ResolvedList[j], ResolvedList[j - 1]);
            }

            return finalPath;
        }

		public bool CheckLOS(Point A, Point B, bool withClearance)
		{
			Vector2 a = HexMap.ToScreenLocation(A);
			Vector2 b = HexMap.ToScreenLocation(B);
			Vector2 broadened;
			Vector2 dir = Vector2.Normalize(b - a);
			Vector2 perpDir = new Vector2(dir.Y, -dir.X);
            int clearance;
            if (withClearance)
                clearance = (int)(HexMap.TileHalfWidth * 0.363f); // the width of the ray (0.363 AAPFT1(1) menee mut PathfindtestU ei nätti
            else clearance = 0;

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

        public List<Tile> GetTilesInBetween(Point A, Point B, bool withClearance)
        {
            Vector2 a = HexMap.ToScreenLocation(A);
            Vector2 b = HexMap.ToScreenLocation(B);
            Vector2 broadened;
            Vector2 dir = Vector2.Normalize(b - a);
            Vector2 perpDir = new Vector2(dir.Y, -dir.X);
            int clearance;
            if (withClearance)
                clearance = (int)(HexMap.TileHalfWidth * 0.4f); // the width of the ray
            else clearance = 0;

            a += (HexMap.TileHalfHeight + 6) * dir; //start raycasting at the tile border + some
            int targetDist = (int)Vector2.Distance(b - (HexMap.TileHalfHeight+1) * dir, a); // don't include the endpoint tile

            List<Tile> tileList = new List<Tile>();

            for (int i = 0; i < targetDist; i++)
            {
                broadened = i % 2 == 1 ? a + clearance * perpDir : a - clearance * perpDir; // check collision at clearance distance perpendicular to the ray (+odd, -even)
                Point coord = HexMap.ToMapCoordinate(broadened);
                if (coord.X >= 0 && coord.Y >= 0 && coord.X < HexMap.Layout.GetLength(1) && coord.Y < HexMap.Layout.GetLength(0))
                {
                    char tileChar = HexMap.Layout[coord.Y, coord.X];
                    if (tileChar == '.' || tileChar == '\'')
                    {
                        Tile tileUnderRaycastPoint = openTiles[coord.X, coord.Y];
                        if (!tileList.Contains(tileUnderRaycastPoint))
                            tileList.Add(tileUnderRaycastPoint);
                    }
                    a += dir;
                }
            }
            return tileList;
        }

        public bool CheckIfContinuedDir(Tile curr, Tile next)
		{
			Vector3 dirCurrNext = Vector3.Normalize(next.CubeCoord - curr.CubeCoord);
			Vector3 dirParentNext = Vector3.Normalize(next.CubeCoord - curr.Parent.CubeCoord);

			if (Math.Round(dirCurrNext.X, 4) == Math.Round(dirParentNext.X,4) && Math.Round(dirCurrNext.Y, 4) == Math.Round(dirParentNext.Y,4) && Math.Round(dirCurrNext.Z, 4) == Math.Round(dirParentNext.Z,4))
				return true;
			else return false;
		}

        public bool CheckIfStraightHexLine(Tile to, Tile from)
        {
            if (to.CubeCoord.X == from.CubeCoord.X || to.CubeCoord.Y == from.CubeCoord.Y || to.CubeCoord.Z == from.CubeCoord.Z)
                return true;
            else return false;
        }
        Vector3 GetDir(Tile from, Tile to)
        {
            Vector3 retVec = Vector3.Normalize(to.CubeCoord - from.CubeCoord);
            retVec.X = (float)Math.Round(retVec.X, 5);
            retVec.Y = (float)Math.Round(retVec.Y, 5);
            retVec.Z = (float)Math.Round(retVec.Z, 5);
            return retVec;
        }

        public void Draw (SpriteBatch sb) //for debug visualisation of tile check order
        {
            //Vector2 drawPosBonus = new Vector2(0, -18);
            //Vector2 drawPosBonus1 = new Vector2(-8, 0);
            //Vector2 drawPosBonus2 = new Vector2(-8, 12);
            //for (int i = 0; i < pathFindSteps.Count; i++)
            //    sb.DrawString(CurrentGame.font, i.ToString(), HexMap.ToScreenLocation(pathFindSteps[i].MapCoord) + drawPosBonus, pathFindSteps[i].LockedWaypoint ? Color.Red : Color.PeachPuff);
            //for (int i = 0; i < stringpullStepsG.Count; i++)
            //    sb.DrawString(CurrentGame.font, i.ToString(), HexMap.ToScreenLocation(stringpullStepsG[i].MapCoord) + drawPosBonus1, Color.GreenYellow);
            //for (int i = 0; i < stringpullStepsS.Count; i++)
            //    sb.DrawString(CurrentGame.font, i.ToString(), HexMap.ToScreenLocation(stringpullStepsS[i].MapCoord) + drawPosBonus2, Color.Orange);

            //sb.DrawString(CurrentGame.font, "Path total distance: " + Environment.NewLine + Math.Round(pathTotalDist, 1) + Environment.NewLine + Math.Round(ResolvedList[0].F, 1), Vector2.Zero, Color.AliceBlue);
        }
    }
}

     


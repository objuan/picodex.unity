using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace brickgame
{

    public enum MeshFace
    {
        Undefined=0,
        XN,
        XP,
        YN,
        YP,
        ZN,
        ZP
    }
    public enum MeshDirection
    {
        Undefined = 0,
        XN,
        XP,
        YN,
        YP,
        ZN,
        ZP
    }

    public class NavWaypoint
    {
        MeshNavigator nav;
        public MeshChunkCellRef cell;
        public MeshFace face;
        public MeshDirection dir;

        public NavWaypoint(MeshNavigator nav) { this.nav = nav; }

        public NavWaypoint(MeshNavigator nav,MeshChunkCell cell, MeshFace face, MeshDirection dir)
        {
            this.nav = nav;
            this.cell = new MeshChunkCellRef(cell);
            this.face = face;
            this.dir = dir;
        }
        public Vector3 PositionW => nav.GetFacePositionW(cell.Value, face);
        public Vector3 NormalW => nav.GetFaceNormalW( face);
        public Vector3 ForwardW => nav.GetFaceForwardW(dir);

        public bool Equals(NavWaypoint node)
        {
            return cell.Value == node.cell.Value && face == node.face;
        }
    }

    public class NavPath
    {
        public List<NavWaypoint> navPath = new List<NavWaypoint>();
        public int current;

        public bool IsLast => current == navPath.Count-1;
        public bool IsEnded => current == navPath.Count;
        public NavWaypoint Current() => navPath[current];
        public void Avance() => current++;
    }

    #region FIND

    public class Node
    {
        public long Key
        {
            get
            {
                var p = point.cell.worldPosition;
                return p.x + p.y * 100 + p.z * 100 * 100 + (int)point.face * 100 * 100 * 100;
            }
        }

        public Node Parent;
        public NavWaypoint point;
        public Vector3 Position(MeshNavigator nav) => nav.GetFacePositionW(point.cell.Value, point.face);

        public float DistanceToTarget;
        public float Cost;
        public float Weight;
        public float F
        {
            get
            {
                if (DistanceToTarget != -1 && Cost != -1)
                    return DistanceToTarget + Cost;
                else
                    return -1;
            }
        }

        public Node(NavWaypoint point, float weight = 1)
        {
            Parent = null;
            this.point = point;
            DistanceToTarget = -1;
            Cost = 1;
            Weight = weight;
        }

        public bool Equals(Node node)
        {
            return point.Equals(node.point);
        }
    }


    public class NavPathBuilder
    {
        MeshNavigator nav;
        public MeshWorld world;
        public NavWaypoint root;
        public NavWaypoint target;

        public List<NavWaypoint> navPath = new List<NavWaypoint>();

        public List<Node> GetAdjacentNodes(Node n)
        {
            return nav.GetFaceNear(n.point.cell.Value, n.point.face).Select(X => new Node(X, 0) { }).ToList();
        }
        //public List<pathfinder.Node> GetAdjacentNodes(pathfinder.Node n)
        //{
        //    return nav.GetFaceNear(n.point.cell, n.point.face).Select(X => new pathfinder.Node(X, 0) { }).ToList();
        //}

        public NavPath Compute(MeshNavigator nav)
        {
            this.nav = nav;
            //    Node start = new Node(root,0);
            //    Node end = new Node(target,0);

            //    List<Node> path = new List<Node>();
            //    bool findIt = false;
            //    AstarPathFinder.AStarPathFinder(this, start, end, ref path, ref findIt);

            //    var navPath = path.Select(X => X.point).ToList();
            //    return new NavPath() { navPath = navPath };
            //}

            Node start = new Node(root, 0);
            Node end = new Node(target, 0);

            Stack<Node> Path = new Stack<Node>();
            List<Node> OpenList = new List<Node>();
            Dictionary<long, Node> OpenMap = new Dictionary<long, Node>();

            List<Node> ClosedList = new List<Node>();
            Dictionary<long, Node> ClosedMap = new Dictionary<long, Node>();

            List<Node> adjacencies;
            Node current = start;

            // add start node to Open List
            OpenList.Add(start);
            OpenMap.Add(start.Key, start);

            while (OpenList.Count != 0 
                && !ClosedList.Exists(x => x.Equals(end)))
            {
                current = OpenList[0];
                OpenList.Remove(current);
                OpenMap.Remove(current.Key);

                ClosedList.Add(current);
                ClosedMap.Add(current.Key, current);

                adjacencies = GetAdjacentNodes(current);

                foreach (Node n in adjacencies)
                {
                   // if (!ClosedList.Contains(n))
                    if (!ClosedMap.ContainsKey(n.Key))
                    {
                       // if (!OpenList.Contains(n))
                       if (!OpenMap.ContainsKey(n.Key))
                        {
                            n.Parent = current;
                            n.DistanceToTarget = Vector3.Distance(n.Position(nav), end.Position(nav));
                            // n.DistanceToTarget = Math.Abs(n.Position.x - end.Position.x) + Math.Abs(n.Position.y - end.Position.y) + Math.Abs(n.Position.z - end.Position.z);
                            n.Cost = n.Weight + n.Parent.Cost;
                           
                            OpenList.Add(n);
                            OpenMap.Add(n.Key, n);

                            OpenList = OpenList.OrderBy(node => node.F).ToList<Node>();
                        }
                    }
                }
            }

            // construct path, if end was not closed return null
            if (!ClosedList.Exists(x => x.Equals(end)))
            {
                return null;
            }

            // if all good, return path
            Node temp = ClosedList[ClosedList.IndexOf(current)];
            if (temp == null) return null;
            do
            {
                Path.Push(temp);
                temp = temp.Parent;
            } while (temp != start && temp != null);

            // forzo il target 

            navPath = Path.Select(X => X.point).ToList();
            navPath[navPath.Count - 1] = new NavWaypoint(nav, nav.target_cell.Value, nav.target_face, nav.target_dir);

            return new NavPath() { navPath = navPath };

        }

        //public void Compute(MeshNavigator nav, NavStep step)
        //{
        //    NavStep[] near = nav.GetFaceNear(step.pos, step.face);
        //}
    }
    #endregion
    /// <summary>
    /// 
    /// </summary>

    public class MeshNavigator
    {
        static iVector3[] sideOffset = new iVector3[] { new iVector3(0, 0, 0), new iVector3(-1, 0, 0), new iVector3(1, 0, 0), new iVector3(0, -1, 0), new iVector3(0, 1, 0), new iVector3(0, 0, -1), new iVector3(0, 0, 1) }; 
        static Vector3[] sideOffsetW = new Vector3[] { new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1), new Vector3(0, 0, 1) };
        //   static Vector2[] dirOfsetW = new Vector3[] { new Vector3(0, 0, 0), new Vector3(-1, 0, 0), new Vector3(1, 0, 0), new Vector3(0, -1, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1), new Vector3(0, 0, 1) };

        MeshFace[] opposite = new MeshFace[] { MeshFace.Undefined, MeshFace.XP, MeshFace.XN, MeshFace.YP, MeshFace.YN, MeshFace.ZP, MeshFace.ZN };
      
        MeshWorld world;
        public MeshChunkCellRef pos;
        public MeshFace face;
        public MeshDirection dir;

        public MeshChunkCellRef target_cell;
        public MeshFace target_face;
        public MeshDirection target_dir;

        public NavPath navPath;

        public bool IsPathMoving => navPath != null;

        public MeshNavigator(MeshWorld world)
        {
            this.world = world;
        }

        public void ClearPath()
        {
            navPath = null;
            target_cell = null;
        }

        public NavWaypoint[] GetFaceNear()
        {
            if (pos.Value != null)
                return GetFaceNear(pos.Value, face);
            else
                return new NavWaypoint[] { };
        }

        public NavWaypoint[] GetFaceNear(MeshChunkCell cell, MeshFace face)
        {
            List<NavWaypoint> steps = new List<NavWaypoint>();
            var ghostPos = cell.worldPosition + sideOffset[(int)face];

            for (int i = 1; i <= 6; i++)
            {
                if (IsDirectionValid(face, (MeshDirection)i))
                {
                    var p = ghostPos + sideOffset[i];
                    NavWaypoint waypoint = null;

                    if (world.IsValid(p) && world.IsFull(p))
                    {
                        // trovo pieno su ghost
                        var c = world.GetCell(p);
                        if (c.isExisting)
                        // dir = faccia partenza

                        waypoint = new NavWaypoint(this) { dir = (MeshDirection)face, cell = new MeshChunkCellRef(c), face = opposite[i] }; // DIR = prev
                    }
                    else
                    {
                        p = cell.worldPosition + sideOffset[i];
                        if (world.IsValid(p) && world.IsFull(p))
                        {
                            // pieno accanto
                            var c = world.GetCell(p);
                            if (c.isExisting)
                                waypoint =new NavWaypoint(this) { dir = (MeshDirection)i, cell = new MeshChunkCellRef(c), face = face };
                        }
                        else
                        {
                           // giro su me stesso 
                            waypoint = new NavWaypoint(this) { dir = (MeshDirection)opposite[(int)face], cell = new  MeshChunkCellRef(cell), face = (MeshFace)i }; //dir = prev oppusto
                        }
                    }
                    if (waypoint!=null)
                        steps.Add(waypoint);
                }
            }
            return steps.ToArray();
        }
          
        public MeshFace[] GetFacesVisible(MeshChunkCell cell)
        {
            List<MeshFace> list = new List<MeshFace>();
            var pos = cell.worldPosition;
            for (int i = 1; i <= 6; i++)
            {
                if (IsFaceVisible(cell,(MeshFace)i)) 
                    list.Add((MeshFace)i);
            }
            return list.ToArray();
        }

        public bool IsFaceVisible(MeshChunkCell cell, MeshFace face)
        {
            var pos = cell.worldPosition+ sideOffset[(int)face];
            var i = world.IsValid(pos);
            return (!i) ||  (i && !world.IsFull(pos));
        }
    
        public bool IsDirectionValid( MeshFace face, MeshDirection dir)
        {
            if (face == MeshFace.XP || face == MeshFace.XN) return (dir != MeshDirection.XP && dir != MeshDirection.XN) ;
            else if (face == MeshFace.YP || face == MeshFace.YN) return (dir != MeshDirection.YP && dir != MeshDirection.YN);
            else if (face == MeshFace.ZP || face == MeshFace.ZN) return (dir != MeshDirection.ZP && dir != MeshDirection.ZN);
            else return false;
        }

        /// <summary>
        /// return false if not solution found
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool SetPosition(MeshChunkCell cell, MeshFace face, MeshDirection dir)
        {
            if (face == MeshFace.Undefined || !IsFaceVisible(cell,face))
            {
                var faces = GetFacesVisible(cell);
                if (faces.Length==0)
                    return false;
                   
                face = faces[UnityEngine.Random.Range(0, faces.Length)];
            }
            if (dir == MeshDirection.Undefined)
            {
                dir = ((MeshDirection[])Enum.GetValues(typeof(MeshDirection))).Where( X => IsDirectionValid(face,X)).ToArray()[UnityEngine.Random.Range(1, 5)];
            }
            pos = new MeshChunkCellRef(cell);
            this.dir = dir;
            this.face = face;
            return true;
        }

        /// <summary>
        /// alla faccia + vicina
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public bool SetPosition(Vector3 actorPosW,MeshChunkCell cell)
        {
            var faces = GetFacesVisible(cell);
            if (faces.Length == 0)
                return false;

            float r = world.leafSize * 0.5f;
            var posL = world.worldToLocalPoint(actorPosW);
            var O = cell.worldPosition;
            float min_dist = 9999;
            foreach (MeshFace face in faces)
            {
                var C = world.GetCellLocalCenter(cell.worldPosition) + sideOffsetW[(int)face] * r;
                var plane = new Plane(sideOffsetW[(int)face], C);
                var facePosition = plane.ClosestPointOnPlane(posL);

                float dist = Vector3.Distance(facePosition, posL);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    this.face = face;
                }
            }
           
            this.dir = ((MeshDirection[])Enum.GetValues(typeof(MeshDirection))).Where(X => IsDirectionValid(face, X)).ToArray()[UnityEngine.Random.Range(1, 5)];
            pos = new MeshChunkCellRef(cell);
            return true;
        }

        public bool GetFaceRelativePosition(Vector3 posW,ref Vector3 facePositionW,ref bool isUP)
        {
            return GetFaceRelativePosition(posW, pos.Value, face, ref facePositionW, ref isUP);
         //   float r = world.leafSize * 0.5f;

            //   var posL = world.worldToLocalPoint(posW);
            //   // relativa al cubo
            //  // var p = posL - world.GetCellLocalCenter(pos.worldPosition);

            //   var C = world.GetCellLocalCenter(pos.worldPosition) + sideOffsetW[(int)face] * r;
            //   var plane = new Plane( sideOffsetW[(int)face],C);
            //   var facePosition = plane.ClosestPointOnPlane(posL);
            //   isUP = Mathf.Sign(plane.GetDistanceToPoint(posL))==1;

            //   var coord = facePosition - C;
            //   bool inside = Mathf.Abs(coord.x) < r && Mathf.Abs(coord.y) < r && Mathf.Abs(coord.z) < r;

            ////   Debug.Log("local " + inside+" .. " +world.GetCellLocalCenter(pos.worldPosition) + " " + facePosition);

            //   // to world
            //   facePositionW = world.localToWorldPoint(facePosition);
            //   return inside;

        }

        public bool GetFaceRelativePosition(Vector3 posW, MeshChunkCell cell,MeshFace face,  ref Vector3 facePositionW, ref bool isUP)
        {
            float r = world.leafSize * 0.5f;
            var posL = world.worldToLocalPoint(posW);
         
            var C = world.GetCellLocalCenter(cell.worldPosition) + sideOffsetW[(int)face] * r;
            var plane = new Plane(sideOffsetW[(int)face], C);
            var facePosition = plane.ClosestPointOnPlane(posL);
            isUP = Mathf.Sign(plane.GetDistanceToPoint(posL)) == 1;

            var coord = facePosition - C;
            bool inside = Mathf.Abs(coord.x) < r && Mathf.Abs(coord.y) < r && Mathf.Abs(coord.z) < r;

            //   Debug.Log("local " + inside+" .. " +world.GetCellLocalCenter(pos.worldPosition) + " " + facePosition);

            // to world
            facePositionW = world.localToWorldPoint(facePosition);
            return inside;

        }

        //public bool GetCurrentFaceRelativePosition( ref Vector3 facePositionW, ref bool isUP)
        //{
        //    return GetFaceRelativePosition(pos.position, ref facePositionW,ref isUP);
        //}

        // ===============

        public NavWaypoint GetLinkedDir(MeshChunkCell from, MeshChunkCell to)
        {
            var f = from.worldPosition;
            var t = to.worldPosition;

            if (t.x == f.x + 1 && t.y == f.y && t.z == f.z) // dx
            {
                return new NavWaypoint(this, from, MeshFace.XP, MeshDirection.YP);
            }
            else if (t.x == f.x - 1 && t.y == f.y && t.z == f.z) // dx
            {
                return new NavWaypoint(this, from, MeshFace.XN, MeshDirection.YN);
            }
            else if (t.x == f.x && t.y == f.y + 1 && t.z == f.z) // dx
            {
                return new NavWaypoint(this, from, MeshFace.YP, MeshDirection.XP);
            }
            else if (t.x == f.x && t.y == f.y - 1 && t.z == f.z) // dx
            {
                return new NavWaypoint(this, from, MeshFace.YN, MeshDirection.XN);
            }
            else if (t.x == f.x && t.y == f.y && t.z == f.z + 1) // dx
            {
                return new NavWaypoint(this, from, MeshFace.ZP, MeshDirection.YP);
            }
            else if (t.x == f.x && t.y == f.y && t.z == f.z - 1) // dx
            {
                return new NavWaypoint(this, from, MeshFace.ZN, MeshDirection.YN);
            }
            else
                return null;
        }

        public void SetTarget(NavWaypoint wayPoint)
        {
            SetTarget(wayPoint.cell.Value, wayPoint.face, wayPoint.dir);
        }

        public void SetTarget(MeshChunkCell cell, MeshFace face, MeshDirection dir)
        {
            if (face == MeshFace.Undefined || !IsFaceVisible(cell, face))
            {
                var faces = GetFacesVisible(cell);
                if (faces.Length == 0) return;
                    
                face = faces[UnityEngine.Random.Range(0, faces.Length)];
                if (dir == MeshDirection.Undefined)
                {
                    dir = ((MeshDirection[])Enum.GetValues(typeof(MeshDirection))).Where(X => IsDirectionValid(face, X)).ToArray()[UnityEngine.Random.Range(1, 5)];
                }
            }
            target_dir = dir;
            target_cell = new MeshChunkCellRef(cell);
            target_face = face;

            // find shortest path
            var b = new NavPathBuilder() { root = new NavWaypoint(this,this.pos.Value, this.face,this.dir), target = new NavWaypoint(this,target_cell.Value, target_face, dir), world = world };
           navPath = b.Compute(this);

        }

        void EploreNear(MeshChunkCell cell, MeshFace face)
        {
            var faces = GetFacesVisible(cell);
        }

        // =================================

        public Vector3 GetFacePositionW(MeshChunkCell cell, MeshFace face)
        {
            return cell.position + sideOffsetW[(int)face] * world.leafSize * 0.5f;
        }
        public Vector3 GetFaceForwardW( MeshDirection dir)
        {
            return world.localToWorldVector(sideOffsetW[(int)dir]);
        }
        public Vector3 GetFaceNormalW(MeshFace face)
        {
            return world.localToWorldVector(sideOffsetW[(int)face]);
        }
        public Vector3 GetFacePositionW()
        {
            try
            {
                return pos.position + sideOffsetW[(int)face] * world.leafSize * 0.5f;
            }
            catch (Exception e)
            {
                int y = 0;
                return Vector3.zero;
            }
        }
        public Vector3 GetFaceForwardW()
        {
            return world.localToWorldVector(sideOffsetW[(int)dir]);
        }

        public Vector3 GetFaceNormalW()
        {
            return world.localToWorldVector(sideOffsetW[(int)face]) ;
        }

        public Vector3 GetTargetPositionW()
        {
            return target_cell.position + sideOffsetW[(int)target_face] * world.leafSize * 0.5f;
        }

        public Vector3 GetTargetNormalW()
        {
            return world.localToWorldVector(sideOffsetW[(int)target_face]);
        }
        public Vector3 GetTargetForwardW()
        {
            return world.localToWorldVector(sideOffsetW[(int)target_dir]);
        }
    }
}

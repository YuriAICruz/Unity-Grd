using System;
using System.Collections.Generic;
using Graphene.Grid.Presentation;
using Priority_Queue;
using UnityEngine;

namespace Graphene.Grid
{
    public class GridQuad : Grid
    {
        protected int _x, _y;

        protected Transform _root;

        public GridQuad(int x, int y, float size)
        {
            _x = x;
            _y = y;
            _size = new Vector2Int(x, y);
            Size = size;
        }

        public GridQuad()
        {
        }

        public override IGrid SetRoot(Transform root)
        {
            _root = root;

            return this;
        }

        public override IGrid Generate(Vector3 basePos, GridDirection direction)
        {
            _grid = new List<IGridInfo>();

            var offset = basePos + new Vector3(Size / 2, Size / 2, 0);

            for (int x = 0, n = _x; x < n; x++)
            {
                for (int y = 0, m = _y; y < m; y++)
                {
                    var pos = offset + new Vector3(x, y, 0) * Size;
                    _grid.Add(new WorldGridInfo(x, y, pos, _root, _baseColor));
                }
            }

            return this;
        }

        public override IGridInfo GetPos(Vector3 pos)
        {
            var dist = Size / 2;
            return _grid.Find(g => Math.Abs(g.worldPos.x - pos.x) < dist && Math.Abs(g.worldPos.y - pos.y) < dist);
        }

        public override IGridInfo GetMousePos(Vector3 screenMouse, Camera mainCam)
        {
            var dist = (mainCam.WorldToScreenPoint(_grid[0].worldPos) - mainCam.WorldToScreenPoint(_grid[1].worldPos)).magnitude * 0.5f;

            var gr = _grid.Find(g => (mainCam.WorldToScreenPoint(
                                          new Vector3(g.worldPos.x, g.worldPos.y, mainCam.nearClipPlane)) - screenMouse).magnitude < dist);

            return gr;
        }

        public override List<IGridInfo> SelectRegion(IGridInfo gr, int size, bool removeCenter)
        {
            var lst = new List<IGridInfo>() {gr};

            for (int j = 0, m = size; j < m; j++)
            {
                for (int k = 0, o = lst.Count; k < o; k++)
                {
                    for (int i = 0, n = _dirs.Length; i < n; i++)
                    {
                        var tmp = GetPos(lst[k].x + (int) _dirs[i].x, lst[k].y + (int) _dirs[i].y);
                        if (tmp != null && !tmp.isBlocked && !lst.Contains(tmp))
                        {
                            lst.Add(tmp);
                        }
                    }
                }
            }
            if (removeCenter)
            {
                lst.Remove(gr);
            }

            return lst;
        }

        public override List<IGridInfo> GetPath(IGridInfo from, IGridInfo to)
        {
            var origin = from as GridInfo;
            var destination = to as GridInfo;
            var frontier = new FastPriorityQueue<GridInfo>(_grid.Count);

            frontier.Enqueue(origin, 0);

            var weight_so_far = new Dictionary<GridInfo, int>();
            var came_from = new Dictionary<GridInfo, GridInfo>();
            var neighbors = new List<GridInfo>();
            GridInfo current;

            weight_so_far.Add(origin, 0);
            came_from.Add(origin, null);

            while (frontier.Count > 0)
            {
                current = frontier.Dequeue();
                neighbors = SelectRegionInternal(current, 1, false);

                if (current == destination) break;

                for (int i = 0, n = neighbors.Count; i < n; i++)
                {
                    var new_cost = weight_so_far[current] + neighbors[i].weight;

                    if (!weight_so_far.ContainsKey(neighbors[i]) || new_cost < weight_so_far[neighbors[i]])
                    {
                        var priority = new_cost + Heuristic(destination, neighbors[i]);

                        if (weight_so_far.ContainsKey(neighbors[i]))
                        {
                            weight_so_far[neighbors[i]] = new_cost;
                            came_from[neighbors[i]] = current;
                            try
                            {
                                frontier.Enqueue(neighbors[i], priority);
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            weight_so_far.Add(neighbors[i], new_cost);
                            came_from.Add(neighbors[i], current);
                            frontier.Enqueue(neighbors[i], priority);
                        }
                    }
                }
            }

            current = destination;
            var path = new List<IGridInfo>();
            path.Add(current);

            while (current != origin)
            {
                if (!came_from.ContainsKey(current))
                {
                    path = new List<IGridInfo>();
                    break;
                }

                current = came_from[current];
                path.Add(current);
            }

            path.Reverse();

            return path;
        }

        private List<GridInfo> SelectRegionInternal(GridInfo gr, int size, bool removeCenter)
        {
            var lst = new List<GridInfo>() {gr};

            for (int j = 0, m = size; j < m; j++)
            {
                for (int k = 0, o = lst.Count; k < o; k++)
                {
                    for (int i = 0, n = _dirs.Length; i < n; i++)
                    {
                        var tmp = GetPos(lst[k].x + (int) _dirs[i].x, lst[k].y + (int) _dirs[i].y) as GridInfo;
                        if (tmp != null && !tmp.isBlocked && !lst.Contains(tmp))
                        {
                            lst.Add(tmp);
                        }
                    }
                }
            }
            if (removeCenter)
            {
                lst.Remove(gr);
            }

            return lst;
        }
    }

    public class GridInfo : FastPriorityQueueNode, IGridInfo
    {
        public Vector3 worldPos { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool isBlocked { get; set; }
        public int weight { get; set; }
        public float size { get; set; }
        public GridDirection direction { get; set; }

        public GridInfo()
        {
        }

        public GridInfo(int x, int y, Vector3 pos)
        {
            this.x = x;
            this.y = y;
            worldPos = pos;
            isBlocked = false;
        }

        public void Reset()
        {
        }

        public void Draw(Color color)
        {
            Debug.DrawRay(worldPos, Vector3.up * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.down * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.left * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.right * size / 4, color, 5);
        }

        [Obsolete]
        public void Draw(Color color, float size)
        {
            Debug.DrawRay(worldPos, Vector3.up * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.down * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.left * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.right * size / 4, color, 5);
        }

        public Vector3[] GetEdges()
        {
            throw new NotImplementedException();
        }
    }

    public class WorldGridInfo : FastPriorityQueueNode, IGridInfo
    {
        public Vector3 worldPos { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool isBlocked { get; set; }
        public int weight { get; set; }
        public float size { get; set; }
        public GridDirection direction { get; set; }

        protected GridView _worldObject;

        public WorldGridInfo()
        {
        }

        public WorldGridInfo(int x, int y, Vector3 pos, Transform root, Color color)
        {
            this.x = x;
            this.y = y;
            worldPos = pos;
            isBlocked = false;
            var tmp = new GameObject("(" + x.ToString("00") + " ," + y.ToString("00") + ")", new Type[]
            {
                typeof(GridView)
            });

            tmp.transform.SetParent(root);

            tmp.transform.position = pos;

            _worldObject = tmp.GetComponent<GridView>();
            _worldObject.SetBaseColor(color);
        }

        public void Reset()
        {
            _worldObject.Reset();
        }

        public void Draw(Color color)
        {
            _worldObject.Draw(color);
        }

        [Obsolete]
        public void Draw(Color color, float size)
        {
            _worldObject.Draw(color);
        }

        public Vector3[] GetEdges()
        {
            throw new NotImplementedException();
        }
    }

    public static class GridExtensions
    {
        public static int GetDistance(this IGridInfo a, IGridInfo b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }
}
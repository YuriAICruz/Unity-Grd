using System;
using System.Collections.Generic;
using Graphene.Grid.Presentation;
using Priority_Queue;
using UnityEngine;
using VZ.Grid;

namespace Graphene.Grid
{
    public class GridQuad : IGrid
    {
        protected int _x, _y;
        protected float _size;
        protected List<IGridInfo> _grid;

        protected Vector2[] _dirs = new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(0, -1),
            new Vector2(1, 0),
            new Vector2(-1, 0)
        };

        protected Transform _root;
        protected Color _baseColor;

        public GridQuad(int x, int y, float size)
        {
            _x = x;
            _y = y;
            _size = size;
        }

        public GridQuad()
        {
        }

        public IGrid SetRoot(Transform root)
        {
            _root = root;

            return this;
        }
        
        public IGrid SetBaseColor(Color color)
        {
            _baseColor = color;
            return this;
        }

        public virtual IGrid Generate(Vector3 basePos)
        {
            _grid = new List<IGridInfo>();

            var offset = basePos + new Vector3(_size / 2, _size / 2, 0);

            for (int x = 0, n = _x; x < n; x++)
            {
                for (int y = 0, m = _y; y < m; y++)
                {
                    var pos = offset + new Vector3(x, y, 0) * _size;
                    _grid.Add(new WorldGridInfo(x, y, pos, _root, _baseColor));
                }
            }

            return this;
        }

        public IGrid ResetGrid()
        {
            if (_grid == null)
            {
                return this;
            }

            for (int i = 0, n = _grid.Count; i < n; i++)
            {
                _grid[i].Reset();
            }
            return this;
        }

        public IGrid DrawGrid(Color color)
        {
            if (_grid == null)
            {
                return this;
            }

            for (int i = 0, n = _grid.Count; i < n; i++)
            {
                _grid[i].Draw(color, _size);
            }
            return this;
        }

        public void DrawGrid(List<IGridInfo> grids, Color color)
        {
            if (grids == null)
            {
                return;
            }

            for (int i = 0, n = grids.Count; i < n; i++)
            {
                grids[i].Draw(color, _size);
            }
            return;
        }

        public void DrawGrid(IGridInfo gr, Color color)
        {
            gr.Draw(color, _size);
        }

        public List<IGridInfo> GetGrid()
        {
            return _grid;
        }

        public IGridInfo GetPos(int x, int y)
        {
            if (x >= _x || y >= _y || x < 0 || y < 0)
                return null;

            return _grid.Find(g => g.x == x && g.y == y);
        }

        public virtual IGridInfo GetPos(Vector3 pos)
        {
            var dist = _size/2;
            return _grid.Find(g => Math.Abs(g.worldPos.x - pos.x) < dist && Math.Abs(g.worldPos.y - pos.y) < dist);
        }

        public virtual IGridInfo GetMousePos(Vector3 screenMouse, Camera mainCam)
        {
            var dist = (mainCam.WorldToScreenPoint(_grid[0].worldPos) - mainCam.WorldToScreenPoint(_grid[1].worldPos)).magnitude * 0.5f;

            var gr = _grid.Find(g => (mainCam.WorldToScreenPoint(
                                          new Vector3(g.worldPos.x, g.worldPos.y, mainCam.nearClipPlane)) - screenMouse).magnitude < dist);

            return gr;
        }

        public List<IGridInfo> SelectRegion(IGridInfo gr, int size, bool removeCenter)
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

        public virtual List<IGridInfo> GetPath(IGridInfo from, IGridInfo to)
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

        protected int Heuristic(IGridInfo a, IGridInfo b)
        {
            // Manhattan distance on a square grid
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }

    public class GridInfo : FastPriorityQueueNode, IGridInfo
    {
        public Vector3 worldPos { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool isBlocked { get; set; }
        public int weight { get; set; }

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

        public void Draw(Color color, float size)
        {
            Debug.DrawRay(worldPos, Vector3.up * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.down * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.left * size / 4, color, 5);
            Debug.DrawRay(worldPos, Vector3.right * size / 4, color, 5);
        }
    }

    public class WorldGridInfo : FastPriorityQueueNode, IGridInfo
    {
        public Vector3 worldPos { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool isBlocked { get; set; }
        public int weight { get; set; }

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

        public void Draw(Color color, float size)
        {
            _worldObject.Draw(color);
        }
    }

    public static class GridExtensions
    {
        public static int GetDistance(this IGridInfo a, IGridInfo b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }
    }

    public interface IGridInfo
    {
        Vector3 worldPos { get; set; }
        int x { get; set; }
        int y { get; set; }
        bool isBlocked { get; set; }
        int weight { get; set; }

        void Reset();
        void Draw(Color color, float size);
    }
}
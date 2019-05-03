using System;
using System.Collections.Generic;
using Graphene.Grid;
using Priority_Queue;
using UnityEngine;

namespace Graphene.Grid
{
    public class HexGrid : Grid
    {
        public HexGrid()
        {
        }

        public HexGrid(int sizeX, int sizeY, float radius)
        {
            Size = radius;
            _size = new Vector2Int(sizeX, sizeY);

            _dirs = new Vector2Int[]
            {
                //Even
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(-1, -1),
                new Vector2Int(-1, 1),

                //Odd
                new Vector2Int(0, 1),
                new Vector2Int(0, -1),
                new Vector2Int(1, 0),
                new Vector2Int(-1, 0),
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
            };
        }

        public override IGrid Generate(Vector3 basePos, GridDirection direction)
        {
            _grid = new List<IGridInfo>();

            var w = Mathf.Sqrt(3) * Size;
            var h = 2 * Size * 0.75f;

            for (int x = 0, n = _size.x; x < n; x++)
            {
                for (int y = 0, m = _size.y; y < m; y++)
                {
                    Vector3 worldPos;
                    switch (direction)
                    {
                        case GridDirection.XZ:
                            worldPos = basePos + new Vector3(
                                           w * x + w * 0.5f * (y % 2),
                                           0,
                                           h * y
                                       );
                            break;
                        case GridDirection.XY:
                            worldPos = basePos + new Vector3(
                                           w * x + w * 0.5f * (y % 2),
                                           h * y,
                                           0
                                       );
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                    _grid.Add(
                        new HexGridInfo(
                            x,
                            y,
                            worldPos,
                            Size,
                            direction
                        )
                    );
                }
            }

            return this;
        }

        public override IGridInfo GetPos(Vector3 pos)
        {
            return _grid.Find(g => (g.worldPos - pos).magnitude < Size * .5f);
        }

        public override IGridInfo GetPos(Ray ray)
        {
            return _grid.Find(grid => new Bounds(grid.worldPos, Vector3.one * Size).IntersectRay(ray));
            //return _grid.Find(g => (g.worldPos - ray.GetPoint((g.worldPos - ray.origin).magnitude)).magnitude < Size*0.5f);
        }

        public override IGridInfo GetMousePos(Vector3 screenMouse, Camera mainCam)
        {
            var scrray = mainCam.ScreenPointToRay(screenMouse);
            return GetPos(scrray);

            var pos = mainCam.ScreenToWorldPoint(screenMouse + Vector3.forward * mainCam.nearClipPlane);

            var screenref = new Vector2(screenMouse.x / Screen.width, screenMouse.y / Screen.height);
            var aspect = Screen.width / (float) Screen.height;

            screenref = screenref * 2 - Vector2.one;

            var fwd = mainCam.transform.forward;
            fwd = Quaternion.AngleAxis(mainCam.fieldOfView * 0.5f * aspect * screenref.x, mainCam.transform.up) * fwd;
            fwd = Quaternion.AngleAxis(mainCam.fieldOfView * 0.5f * screenref.y * 1.2f, -mainCam.transform.right) * fwd;

            var ray = new Ray(pos, fwd);

            return GetPos(ray);
        }

        public override List<IGridInfo> SelectRegion(IGridInfo gr, int size, bool removeCenter)
        {
            var lst = new List<IGridInfo>() {gr};

            for (int j = 0, m = size; j < m; j++)
            {
                for (int k = 0, o = lst.Count; k < o; k++)
                {
                    for (int i = 0, n = _dirs.Length / 2; i < n; i++)
                    {
                        var tmp = GetPos(lst[k].x + _dirs[i + (lst[k].y % 2) * n].x, lst[k].y + _dirs[i + (lst[k].y % 2) * n].y);
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

        public override List<IGridInfo> GetPath(IGridInfo @from, IGridInfo to)
        {
            var origin = from as HexGridInfo;
            var destination = to as HexGridInfo;
            var frontier = new FastPriorityQueue<HexGridInfo>(_grid.Count);

            frontier.Enqueue(origin, 0);

            var weight_so_far = new Dictionary<HexGridInfo, int>();
            var came_from = new Dictionary<HexGridInfo, HexGridInfo>();
            var neighbors = new List<HexGridInfo>();
            HexGridInfo current;

            weight_so_far.Add(origin, 0);
            came_from.Add(origin, null);

            while (frontier.Count > 0)
            {
                current = frontier.Dequeue();
                neighbors = SelectRegionInternal(current, 1, true);

                if (current == destination) break;

                for (int i = 0, n = neighbors.Count; i < n; i++)
                {
                    var new_cost = weight_so_far[current] + neighbors[i].weight;

                    if (!weight_so_far.ContainsKey(neighbors[i]) || new_cost < weight_so_far[neighbors[i]])
                    {
                        var priority = new_cost + Distance(destination, neighbors[i]);

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

        private int Distance(IGridInfo a, IGridInfo b)
        {
            return Mathf.RoundToInt((a.worldPos - b.worldPos).magnitude);
        }

        private List<HexGridInfo> SelectRegionInternal(HexGridInfo gr, int size, bool removeCenter)
        {
            var lst = new List<HexGridInfo>() {gr};

            for (int j = 0, m = size; j < m; j++)
            {
                for (int k = 0, o = lst.Count; k < o; k++)
                {
                    for (int i = 0, n = _dirs.Length / 2; i < n; i++)
                    {
                        var tmp = GetPos(lst[k].x + _dirs[i + (lst[k].y % 2) * n].x, lst[k].y + _dirs[i + (lst[k].y % 2) * n].y) as HexGridInfo;
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

    public class HexGridInfo : FastPriorityQueueNode, IGridInfo
    {
        private Vector3[] _sides;
        public Vector3 worldPos { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool isBlocked { get; set; }
        public int weight { get; set; }
        public float size { get; set; }
        public GridDirection direction { get; set; }


        public HexGridInfo(int x, int y, Vector3 worldPos, float size, GridDirection direction)
        {
            this.x = x;
            this.y = y;
            this.worldPos = worldPos;
            this.size = size;
            this.direction = direction;

            switch (direction)
            {
                case GridDirection.XZ:
                    _sides = new Vector3[]
                    {
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 0 - 30) * Mathf.PI / 180), worldPos.y, worldPos.z + size * Mathf.Sin((60 * 0 - 30) * Mathf.PI / 180)),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 1 - 30) * Mathf.PI / 180), worldPos.y, worldPos.z + size * Mathf.Sin((60 * 1 - 30) * Mathf.PI / 180)),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 2 - 30) * Mathf.PI / 180), worldPos.y, worldPos.z + size * Mathf.Sin((60 * 2 - 30) * Mathf.PI / 180)),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 3 - 30) * Mathf.PI / 180), worldPos.y, worldPos.z + size * Mathf.Sin((60 * 3 - 30) * Mathf.PI / 180)),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 4 - 30) * Mathf.PI / 180), worldPos.y, worldPos.z + size * Mathf.Sin((60 * 4 - 30) * Mathf.PI / 180)),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 5 - 30) * Mathf.PI / 180), worldPos.y, worldPos.z + size * Mathf.Sin((60 * 5 - 30) * Mathf.PI / 180))
                    };
                    break;
                case GridDirection.XY:
                    _sides = new Vector3[]
                    {
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 0 - 30) * Mathf.PI / 180), worldPos.y + size * Mathf.Sin((60 * 0 - 30) * Mathf.PI / 180), worldPos.z),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 1 - 30) * Mathf.PI / 180), worldPos.y + size * Mathf.Sin((60 * 1 - 30) * Mathf.PI / 180), worldPos.z),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 2 - 30) * Mathf.PI / 180), worldPos.y + size * Mathf.Sin((60 * 2 - 30) * Mathf.PI / 180), worldPos.z),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 3 - 30) * Mathf.PI / 180), worldPos.y + size * Mathf.Sin((60 * 3 - 30) * Mathf.PI / 180), worldPos.z),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 4 - 30) * Mathf.PI / 180), worldPos.y + size * Mathf.Sin((60 * 4 - 30) * Mathf.PI / 180), worldPos.z),
                        new Vector3(worldPos.x + size * Mathf.Cos((60 * 5 - 30) * Mathf.PI / 180), worldPos.y + size * Mathf.Sin((60 * 5 - 30) * Mathf.PI / 180), worldPos.z)
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        public void Reset()
        {
        }

        public void Draw(Color color)
        {
        }

        [Obsolete]
        public void Draw(Color color, float size)
        {
            throw new System.NotImplementedException();
        }

        public Vector3[] GetEdges()
        {
            return _sides;
        }
    }
}
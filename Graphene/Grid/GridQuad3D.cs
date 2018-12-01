using System;
using System.Collections.Generic;
using Graphene.Grid.Presentation;
using Priority_Queue;
using UnityEngine;

namespace Graphene.Grid
{
    public class GridQuad3D : GridQuad
    {
        public GridQuad3D(int x, int y, float size)
        {
            _x = x;
            _y = y;
            _size = new Vector2Int(x, y);
            Size = size;
        }

        public GridQuad3D()
        {
        }

        public override IGrid Generate(Vector3 basePos, GridDirection direction)
        {
            _grid = new List<IGridInfo>();

            var lyr = new LayerMask();
            lyr = ~ LayerMask.NameToLayer("IgnoreGrid") & LayerMask.NameToLayer("Grid");

            var offset = Size / 2;

            for (int x = 0, n = _x; x < n; x++)
            {
                for (int y = 0, m = _y; y < m; y++)
                {
                    Vector3 worldPos;
                    switch (direction)
                    {
                        case GridDirection.XZ:
                            worldPos = basePos + new Vector3(
                                           x * Size + offset,
                                           0,
                                           y * Size + offset
                                       );
                            break;
                        case GridDirection.XY:
                            worldPos = basePos + new Vector3(
                                           x * Size + offset,
                                           y * Size + offset,
                                           0
                                       );
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                    }
                    _grid.Add(
                        new WorldGrid3DInfo(
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
            var dist = Size / 2;
            return _grid.Find(g => Vector3.Distance(g.worldPos, pos) < dist);
        }

        public override IGridInfo GetPos(Ray ray)
        {
            return _grid.Find(g => (g.worldPos - ray.GetPoint((g.worldPos - ray.origin).magnitude)).magnitude < Size*0.5f);
        }

        public override IGridInfo GetMousePos(Vector3 screenMouse, Camera mainCam)
        {
            RaycastHit hit;
            var lyr = LayerMask.GetMask("Grid");
            var ray = mainCam.ScreenPointToRay(screenMouse);

            if (Physics.Raycast(ray, out hit, 200, lyr))
            {
                var pos = hit.point;

                return GetPos(pos);
            }

            return null;
        }

        private List<WorldGrid3DInfo> SelectRegionInternal(WorldGrid3DInfo gr, int size, bool removeCenter)
        {
            var lst = new List<WorldGrid3DInfo>() {gr};

            for (int j = 0, m = size; j < m; j++)
            {
                for (int k = 0, o = lst.Count; k < o; k++)
                {
                    for (int i = 0, n = _dirs.Length; i < n; i++)
                    {
                        var tmp = GetPos(lst[k].x + (int) _dirs[i].x, lst[k].y + (int) _dirs[i].y) as WorldGrid3DInfo;
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
            var origin = from as WorldGrid3DInfo;
            var destination = to as WorldGrid3DInfo;
            var frontier = new FastPriorityQueue<WorldGrid3DInfo>(_grid.Count);

            frontier.Enqueue(origin, 0);

            var weight_so_far = new Dictionary<WorldGrid3DInfo, int>();
            var came_from = new Dictionary<WorldGrid3DInfo, WorldGrid3DInfo>();
            var neighbors = new List<WorldGrid3DInfo>();
            WorldGrid3DInfo current;

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
    }

    public class WorldGrid3DInfo : FastPriorityQueueNode, IGridInfo
    {
        private Vector3[] _sides;
        public Vector3 worldPos { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool isBlocked { get; set; }
        public int weight { get; set; }
        public float size { get; set; }
        public GridDirection direction { get; set; }

        public WorldGrid3DInfo()
        {
        }

        public WorldGrid3DInfo(int x, int y, Vector3 worldPos, float size, GridDirection direction)
        {
            this.x = x;
            this.y = y;
            this.worldPos = worldPos;
            this.size = size;
            this.direction = direction;
            isBlocked = false;

            switch (direction)
            {
                case GridDirection.XZ:
                    _sides = new Vector3[]
                    {
                        new Vector3(worldPos.x + size * 0.5f, worldPos.y, worldPos.z + size * 0.5f),
                        new Vector3(worldPos.x + size * 0.5f, worldPos.y, worldPos.z - size * 0.5f),
                        new Vector3(worldPos.x - size * 0.5f, worldPos.y, worldPos.z - size * 0.5f),
                        new Vector3(worldPos.x - size * 0.5f, worldPos.y, worldPos.z + size * 0.5f)
                    };
                    break;
                case GridDirection.XY:
                    _sides = new Vector3[]
                    {
                        new Vector3(worldPos.x + size * 0.5f, worldPos.y + size * 0.5f, worldPos.z),
                        new Vector3(worldPos.x + size * 0.5f, worldPos.y - size * 0.5f, worldPos.z),
                        new Vector3(worldPos.x - size * 0.5f, worldPos.y - size * 0.5f, worldPos.z),
                        new Vector3(worldPos.x - size * 0.5f, worldPos.y + size * 0.5f, worldPos.z)
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
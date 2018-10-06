using System;
using System.Collections.Generic;
using Graphene.Grid.Presentation;
using Priority_Queue;
using UnityEngine;
using VZ.Grid;

namespace Graphene.Grid
{
    public class GridQuad3D : GridQuad
    {
        public GridQuad3D(int x, int y, float size)
        {
            _x = x;
            _y = y;
            Size = size;
        }

        public GridQuad3D()
        {
        }

        public override IGrid Generate(Vector3 basePos)
        {
            _grid = new List<IGridInfo>();

            var lyr = new LayerMask();
            lyr = ~ LayerMask.NameToLayer("IgnoreGrid") & LayerMask.NameToLayer("Grid");

            var offset = new Vector3(Size / 2, Size / 2, 0);

            var rayDirs = new Vector3[]
            {
                new Vector3(0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f),
                new Vector3(0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f)
            };

            for (int x = 0, n = _x; x < n; x++)
            {
                for (int y = 0, m = _y; y < m; y++)
                {
                    var pos = _root.TransformPoint(offset + new Vector3(x, y, 0) * Size);
                    var block = false;

                    for (int i = 0; i < rayDirs.Length; i++)
                    {
                        var ray = new Ray(_root.TransformDirection(rayDirs[i] * Size) + pos + Vector3.up * 100, Vector3.down);
                        block = Physics.Raycast(ray, 200, lyr);

                        if (block) break;
                    }
                    _grid.Add(new WorldGrid3DInfo(x, y, pos, _root, _baseColor, block, false)); // TODO: all wrong :(
                }
            }

            return this;
        }

        public override IGridInfo GetPos(Vector3 pos)
        {
            var dist = Size / 2;
            return _grid.Find(g => Vector3.Distance(g.worldPos, pos) < dist);
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

    public class WorldGrid3DInfo : WorldGridInfo
    {
        public WorldGrid3DInfo()
        {
        }

        public WorldGrid3DInfo(int x, int y, Vector3 pos, Transform root, Color color, bool blocked, bool createObject = true)
        {
            this.x = x;
            this.y = y;
            worldPos = pos;
            isBlocked = blocked;

            if (!createObject) return;

            var tmp = new GameObject("(" + x.ToString("00") + " ," + y.ToString("00") + ")", new Type[]
            {
                typeof(GridView)
            });


            tmp.transform.SetParent(root);

            tmp.transform.position = pos;

            tmp.transform.localRotation = Quaternion.identity;

            _worldObject = tmp.GetComponent<GridView>();
            _worldObject.SetBaseColor(color);
            _worldObject.gameObject.SetActive(!isBlocked);
            _worldObject.CreateCollider("Grid");
        }
    }
}
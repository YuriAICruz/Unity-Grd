using System;
using System.Collections.Generic;
using System.Xml.Schema;
using Priority_Queue;
using UnityEngine;
using Zenject;

namespace Graphene.Grid
{
    public class InfiniteQuadGrid: Grid
    {
        private Vector3 _basePos;
        private GridDirection _direction;

        public InfiniteQuadGrid()
        {
            CreateDir();
        }

        [Inject]
        public InfiniteQuadGrid(float size, Vector3 basePos, GridDirection direction)
        {
            Size = size;
            _basePos = basePos;
            _direction = direction;
            
            CreateDir();
        }

        private void CreateDir()
        {
            _dirs = new Vector2Int[]
            {
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(0, -1),
                new Vector2Int(-1, 0)
            };
        }

        public override IGridInfo GetPos(int x, int y)
        {
            return new InfiniteQuadGridInfo(_basePos, x, y, Size, _direction);
        }

        public override IGridInfo GetPos(Vector3 pos)
        {
            var p = GetCoordPos(pos);

            return GetPos(p.x, p.y);
        }

        Vector2Int GetCoordPos(Vector3 pos)
        {
            var h = Size; 
            var x = (int) Mathf.Floor((pos.x + h/2) / h);
            var y = (int) Mathf.Floor((pos.z + h/2) / h);

            return new Vector2Int(
                x,
                y
            );
        }
            
        public override IGridInfo GetPos(Ray ray)
        {
            var plane = new Plane(Vector3.up, Vector3.zero);

            float hit;
            
            var p = GetCoordPos(ray.GetPoint(ray.origin.y));
            
            if (plane.Raycast(ray, out hit))
            {
                var h = ray.GetPoint(hit);
                p = GetCoordPos(ray.GetPoint(hit));
            }

            return GetPos(p.x, p.y);
        }
        
        public override IGridInfo GetMousePos(Vector3 screenMouse, Camera mainCam)
        {
            return GetPos(mainCam.ScreenPointToRay(screenMouse));
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
                        var posX = lst[k].x + _dirs[i].x;
                        var posY = lst[k].y + _dirs[i].y;

                        var tmp = GetPos(
                            posX,
                            posY
                        );
                        if (tmp != null && !tmp.isBlocked && !lst.Exists(x => x.x == tmp.x && x.y == tmp.y))
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

    public class InfiniteQuadGridInfo : FastPriorityQueueNode, IGridInfo
    {
        private Vector3[] _sides;
        public Vector3 worldPos { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool isBlocked { get; set; }
        public int weight { get; set; }
        public float size { get; set; }
        public GridDirection direction { get; set; }

        public InfiniteQuadGridInfo(Vector3 basePos, int x, int y, float size, GridDirection direction)
        {
            this.x = x;
            this.y = y;
            this.direction = direction;

            var xTemp = size * x;
            var zTemp = size * y;
            this.worldPos = basePos + new Vector3(
                                xTemp,
                                0,
                                zTemp
                            );

            this.size = size;

            var list = new List<Vector3>();

            var dir = new Vector2Int[]
            {
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, -1),
                new Vector2Int(-1, 1),
            };
            for (int i = 0; i < dir.Length; i++)
            {
                var xPos = worldPos.x + size*0.5f * dir[i].x;
                var zPos = worldPos.z + size*0.5f * dir[i].y;
                switch (direction)
                {
                    case GridDirection.XZ:
                        list.Add(new Vector3(xPos, 0, zPos));
                        break;
                    case GridDirection.XY:
                        list.Add(new Vector3(xPos, zPos, 0));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
                }
            }

            _sides = list.ToArray();
        }

        public void Reset()
        {
        }

        public void Draw(Color color)
        {
            for (int i = 0, n = _sides.Length; i < n; i++)
            {
                Debug.DrawLine(_sides[i], _sides[(i+1) % n], color);   
            }
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
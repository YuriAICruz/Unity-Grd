using System;
using System.CodeDom;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;

namespace Graphene.Grid
{
    public class InfiniteHexGrid : Grid
    {
        private Vector3 _basePos;

        public delegate float FloatFunc(Vector3 pos);

        public InfiniteHexGrid()
        {
        }

        public InfiniteHexGrid(float radius, Vector3 basePos)
        {
            Size = radius;
            _basePos = basePos;

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

        public float YGraph(Vector3 pos)
        {
            return Mathf.Sin(pos.x * 0.05f) * 2.6f + Mathf.Cos(pos.z * 0.1f) * 1.2f;
        }

        public override IGridInfo GetPos(int x, int y)
        {
            return new InfinityHexGridInfo(_basePos, x, y, Size, YGraph);
        }

        public override IGridInfo GetPos(Vector3 pos)
        {
            var p = GetCoordPos(pos);

            return GetPos(p.x, p.y);
        }

        public override IGridInfo GetPos(Ray ray)
        {
            var p = GetCoordPos(ray.GetPoint(ray.origin.y - YGraph(ray.origin)));

            return GetPos(p.x, p.y);
        }

        Vector2Int GetCoordPos(Vector3 pos)
        {
            var w = Mathf.Sqrt(3) * Size;
            var h = 2 * Size * 0.75f;
            var y = (int) (pos.z / h);

            return new Vector2Int(
                (int) ((pos.x - w * 0.5f * (y % 2)) / w),
                y
            );
        }

        public override IGridInfo GetMousePos(Vector3 screenMouse, Camera mainCam)
        {
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
                        var posX = lst[k].x + _dirs[i + (Math.Abs(lst[k].y) % 2) * n].x;
                        var posY = lst[k].y + _dirs[i + (Math.Abs(lst[k].y) % 2) * n].y;

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

    public class InfinityHexGridInfo : FastPriorityQueueNode, IGridInfo
    {
        private Vector3[] _sides;
        public Vector3 worldPos { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool isBlocked { get; set; }
        public int weight { get; set; }
        public float size { get; set; }

        public InfinityHexGridInfo(Vector3 basePos, int x, int y, float size, InfiniteHexGrid.FloatFunc YPos)
        {
            this.x = x;
            this.y = y;

            var w = Mathf.Sqrt(3) * size;
            var h = 2 * size * 0.75f;

            var xTemp = w * x + w * 0.5f * (y % 2);
            var zTemp = h * y;
            this.worldPos = basePos + new Vector3(
                                xTemp,
                                YPos(new Vector3(xTemp, 0, zTemp)),
                                zTemp
                            );

            this.size = size;

            var list = new List<Vector3>();

            for (int i = 0; i < 6; i++)
            {
                var xPos = worldPos.x + size * Mathf.Cos((60 * i - 30) * Mathf.PI / 180);
                var zPos = worldPos.z + size * Mathf.Sin((60 * i - 30) * Mathf.PI / 180);
                list.Add(new Vector3(xPos, YPos(new Vector3(xPos, 0, zPos)), zPos));
            }

            _sides = list.ToArray();
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
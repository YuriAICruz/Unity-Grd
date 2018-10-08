using System;
using System.Collections.Generic;
using Graphene.Grid;
using UnityEngine;

namespace Graphene.Grid
{
    public abstract class Grid : IGrid 
    {
        protected Vector2Int _size;
        protected List<IGridInfo> _grid;
        protected Color _baseColor;
        protected float Size;
        
        protected Vector2Int[] _dirs = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
        
        [Obsolete]
        public virtual IGrid SetRoot(Transform root)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual IGrid Generate(Vector3 basePos, GridDirection direction)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual IGridInfo GetPos(Vector3 pos)
        {
            throw new System.NotImplementedException();
        }

        public virtual IGridInfo GetPos(Ray ray)
        {
            throw new NotImplementedException();
        }

        public virtual IGridInfo GetMousePos(Vector3 screenMouse, Camera mainCam)
        {
            throw new System.NotImplementedException();
        }
        
        public virtual List<IGridInfo> SelectRegion(IGridInfo gr, int size, bool removeCenter)
        {
            throw new System.NotImplementedException();
        }

        public virtual List<IGridInfo> GetPath(IGridInfo @from, IGridInfo to)
        {
            throw new System.NotImplementedException();
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
                _grid[i].Draw(color);
            }
            return this;
        }

        public IGrid SetBaseColor(Color color)
        {
            _baseColor = color;
            return this;
        }

        public List<IGridInfo> GetGrid()
        {
            return _grid;
        }

        public virtual IGridInfo GetPos(int x, int y)
        {
            if (x >= _size.x || y >= _size.y || x < 0 || y < 0)
                return null;

            return _grid.Find(g => g.x == x && g.y == y);
        }

        public void DrawGrid(List<IGridInfo> grids, Color color)
        {
            if (grids == null)
            {
                return;
            }

            for (int i = 0, n = grids.Count; i < n; i++)
            {
                grids[i].Draw(color);
            }
            return;
        }

        public void DrawGrid(IGridInfo gr, Color color)
        {
            gr.Draw(color);
        }
        
        protected int Heuristic(IGridInfo a, IGridInfo b)
        {
            // Manhattan distance on a square grid
            return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Graphene.Grid {

    public enum GridDirection
    {
        XZ,
        XY
    }
    
    public interface IGrid {
        IGrid Generate(Vector3 basePos, GridDirection direction);
        
        IGrid ResetGrid();

        IGrid DrawGrid(Color color);
        
        [Obsolete]
        IGrid SetRoot(Transform root);
        
        IGrid SetBaseColor(Color color);

        List<IGridInfo> GetGrid();
        
        IGridInfo GetPos(int x, int y);
        IGridInfo GetPos(Vector3 pos);
        IGridInfo GetPos(Ray ray);
        IGridInfo GetMousePos(Vector3 screenMouse, Camera mainCam);
        
        void DrawGrid(IGridInfo gr, Color red);
        void DrawGrid (List<IGridInfo> grids, Color color);
        
        List<IGridInfo> SelectRegion(IGridInfo gr, int size, bool removeCenter);
        List<IGridInfo> GetPath(IGridInfo from, IGridInfo to);
    }
}
using System;
using UnityEngine;

namespace Graphene.Grid
{
    public interface IGridInfo
    {
        Vector3 worldPos { get; set; }
        int x { get; set; }
        int y { get; set; }
        bool isBlocked { get; set; }
        int weight { get; set; }
        float size { get; set; }
        GridDirection direction { get; set; }

        void Reset();
        void Draw(Color color);

        [Obsolete]
        void Draw(Color color, float size);

        Vector3[] GetEdges();
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Graphene.Grid.FogOfWar {
    [RequireComponent(typeof(MeshFilter))]
    public class FogOfWarManager: MonoBehaviour {
        private MeshFilter _meshFilter;

        public List<PaintVertexColor> Painters;
        public float Range;

        List<IGridInfo> _unfoged;

        void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _unfoged = new List<IGridInfo>();
        }

        List<Vector3> GetPositions(List<IGridInfo> gridList){
            var res = new List<Vector3>();

            for (int i = 0, n = gridList.Count; i < n; i++)
            {
                res.Add(gridList[i].worldPos);
            }

            return res;
        }

        public void Unfog(List<IGridInfo> gridList)
        {
            _unfoged.AddRange(gridList);
            for (int i = 0, n = Painters.Count; i < n; i++)
            {
                Painters[i].PaintArea(GetPositions(gridList), Range);
            }
        }
    }
}
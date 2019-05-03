using System.Collections.Generic;
using System.Runtime.InteropServices;
using Graphene.Grid;
using UnityEngine;

namespace Graphene.Grid
{
    public enum GridType
    {
        Quad,
        Hex,
        InfHex
    }

    public class GridSystem : MonoBehaviour
    {
        [HideInInspector] public Vector3 Offset;
        [HideInInspector] public Vector2Int Size = new Vector2Int(10, 10);
        [HideInInspector] public float Widith = 1;

        [HideInInspector] public GridType GridType;

        [HideInInspector] public GridDirection Direction;
        
        [HideInInspector] public IGridInfo SelectedGrid;

        public IGrid Grid;
        private MeshGeneration _meshGenerator;

        private void Start()
        {
            _meshGenerator = FindObjectOfType<MeshGeneration>();
        }

        public void GenHexGrid()
        {
            Grid = new HexGrid(Size.x, Size.y, Widith).Generate(Offset, Direction);
        }

        public void GenInfHexGrid()
        {
            Grid = new InfiniteHexGrid(Widith, Offset, Direction);
        }

        public void GenMesh(List<IGridInfo> grids)
        {
            if (_meshGenerator == null)
                _meshGenerator = FindObjectOfType<MeshGeneration>();

            _meshGenerator.GenerateMesh(grids);
        }
    }
}
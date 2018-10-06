using System.Collections.Generic;
using Graphene.Grid;
using UnityEngine;

namespace Packages.Grid.Graphene.Grid
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class MeshGeneration : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private Renderer _renderer;
        public GridSystem GridSystem;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _meshFilter = GetComponent<MeshFilter>();
            
            if (GridSystem == null)
                GridSystem = FindObjectOfType<GridSystem>();

            if(GridSystem.Grid == null)
                GridSystem.GenHexGrid();
            
            GenerateMesh();
        }

        private void GenerateMesh()
        {
            var cells = GridSystem.Grid.GetGrid();
            
            var mesh = new Mesh();
            var triangles = new List<int>();
            var vertices = new List<Vector3>();

            foreach (var cell in cells)
            {
                var points = cell.GetEdges();

                var center = vertices.Count;
                vertices.Add(cell.worldPos);

                for (int i = 0; i < points.Length; i++)
                {
                    if (i % 2 == 1)
                    {
                        triangles.Add(center);
                        triangles.Add(center+i-1);
                        triangles.Add(center+i);
                    }
                    vertices.Add(points[i]);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            _meshFilter.mesh = mesh;
        }
    }
}
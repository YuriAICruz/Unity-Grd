using System.Collections.Generic;
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

            if (GridSystem.Grid == null)
                GridSystem.GenHexGrid();

            GenerateMesh();
        }

        private void GenerateMesh()
        {
            var cells = GridSystem.Grid.GetGrid();

            var mesh = new Mesh();
            var triangles = new List<int>();
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();

            foreach (var cell in cells)
            {
                var points = cell.GetEdges();

                var center = vertices.Count;
                normals.Add(Vector3.up);
                vertices.Add(cell.worldPos);

                for (int i = 0; i < points.Length; i++)
                {
                    if (i == 0)
                    {
                        triangles.Add(center + 1);
                        triangles.Add(center + points.Length);
                        triangles.Add(center);
                    }
                    else
                    {
                        triangles.Add(center + i + 1);
                        triangles.Add(center + i);
                        triangles.Add(center);
                    }

                    normals.Add(Vector3.up);
                    
                    vertices.Add(points[i]);
                }
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.normals = normals.ToArray();
            _meshFilter.mesh = mesh;
        }
    }
}
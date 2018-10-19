using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Graphene.Grid;
using UnityEngine;

namespace Graphene.Grid
{
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class MeshGeneration : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private Renderer _renderer;
        public GridSystem GridSystem;

        public Queue<Action> _setStack = new Queue<Action>();
        private Thread _trd;


        private Mesh _mesh;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _meshFilter = GetComponent<MeshFilter>();

            if (GridSystem == null)
                GridSystem = FindObjectOfType<GridSystem>();
        }

        private void Update()
        {
            var n = _setStack.Count;

            for (int i = 0; i < n; i++)
            {
                _setStack.Dequeue()();
            }
        }

        public void GenerateMesh(List<IGridInfo> cells)
        {
            if (cells == null) return;
            if (_renderer == null)
                _renderer = GetComponent<Renderer>();
            if (_meshFilter == null)
                _meshFilter = GetComponent<MeshFilter>();

            _trd?.Abort();

            _trd = new Thread(() =>
            {
                var triangles = new List<int>();
                var vertices = new List<Vector3>();

                foreach (var cell in cells)
                {
                    var points = cell.GetEdges();

                    var center = vertices.Count;

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

                        vertices.Add(points[i]);
                    }
                }

                _setStack.Enqueue(() => SetMesh(vertices, triangles));
            });

            _trd.Start();
        }


        private void SetMesh(List<Vector3> vertices, List<int> triangles)
        {
            _mesh = new Mesh();

            _mesh.vertices = vertices.ToArray();
            _mesh.triangles = triangles.ToArray();
            _mesh.RecalculateNormals();

            _meshFilter.mesh = _mesh;
        }

        private void GenerateMesh()
        {
            var cells = GridSystem.Grid.GetGrid();
            if (cells == null) return;

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
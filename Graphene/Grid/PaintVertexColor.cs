using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VZ.Grid.FogOfWar {
    [RequireComponent(typeof(MeshFilter))]
    public class PaintVertexColor: MonoBehaviour {
        public bool PaintNow = false;
        private MeshFilter _meshFilter;

        private List<Vector3> _vertices;
        private List<Vector3> _worldVertices;
        private List<Color> _colors;

        private void Awake () {
            _meshFilter = GetComponent<MeshFilter>();
            _colors = new Color[_meshFilter.mesh.vertexCount].ToList();
            _vertices = _meshFilter.mesh.vertices.ToList();
            _worldVertices = new List<Vector3>();

            for (int i = 0, n = _vertices.Count; i < n; i++)
            {
                _worldVertices.Add(transform.TransformPoint(_vertices[i]));
            }
        }

        private void Start () {
            PaintInBlack();
        }

        public void PaintArea(List<Vector3> verts, float range){
            for (int i = 0, n =verts.Count; i < n; i++)
            {
                var index = _worldVertices.FindIndex(v => Vector3.Distance(verts[i], v) <= range );

                if(index>=0){
                    Paint(index, Color.black);
                }
            }
            
            ApplyColor();
        }
        void Paint(int index, Color color){
            _colors[index] = color;
        }
        void PaintInBlack(){
            for (int i = 0, n = _colors.Count; i < n; i++)
            {
                _colors[i] = new Color(1,1,1,1);
            }

            ApplyColor();
            print("all black");
        }

        void ApplyColor(){
            _meshFilter.sharedMesh.colors = _colors.ToArray();
        }

        private void Update () {
        }
    }
}
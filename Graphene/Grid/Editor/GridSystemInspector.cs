using System;
using Graphene.Grid;
using UnityEditor;
using UnityEngine;

namespace Packages.Grid.Graphene.Grid
{
    [CustomEditor(typeof(GridSystem))]
    public class GridSystemInspector : Editor
    {
        private GridSystem _self;

        private void Awake()
        {
            _self = target as GridSystem;
        }

        private void OnSceneGUI()
        {
            DrawGrid();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var widith = EditorGUILayout.FloatField("Widith", _self.Widith);
            if (widith != _self.Widith)
            {
                _self.Widith = widith;
                ClearGrid();
            }

            var offset = EditorGUILayout.Vector3Field("Offset", _self.Offset);
            if (offset != _self.Offset)
            {
                _self.Offset = offset;
                ClearGrid();
            }

            var size = EditorGUILayout.Vector2IntField("Size", _self.Size);
            if (size != _self.Size)
            {
                _self.Size = size;
                ClearGrid();
            }

            EditorGUILayout.Space();

            var type = (GridType) EditorGUILayout.EnumPopup("Type", _self.GridType);
            if (type != _self.GridType)
            {
                _self.GridType = type;
                ClearGrid();
            }
        }

        private void ClearGrid()
        {
            _self.Grid = null;
        }

        private void DrawGrid()
        {
            switch (_self.GridType)
            {
                case GridType.Quad:
                    DrawQuadGrid();
                    break;
                case GridType.Hex:
                    DrawHexGrid();
                    break;
            }
        }

        private void DrawHexGrid()
        {
            if (_self.Grid == null)
            {
                _self.Grid = new HexGrid(_self.Size.x, _self.Size.y, _self.Widith).Generate(_self.Offset);
            }

            if (_self.Grid == null) return;

            var color = Handles.color;

            Handles.color = Color.green;
            foreach (var cell in _self.Grid.GetGrid())
            {
                var sqr = cell.GetEdges();

                for (int i = 0, n = sqr.Length; i < n; i++)
                {
                    Handles.DrawLine(sqr[i], sqr[(i + 1) % n]);
                }
            }
            Handles.color = color;

            var cam = Camera.current;
            if (cam == null) return;

            var mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            mouse.y = -mouse.y + Screen.height + 60;
            var gr = _self.Grid.GetMousePos(mouse, cam);

            if (gr == null) return;
            {
                 var path = _self.Grid.GetPath(_self.Grid.GetPos(_self.Size.x/2, _self.Size.y/2), gr);
//                var path = _self.Grid.SelectRegion(gr, 6, true);

                Handles.color = Color.red;

                foreach (var cell in path)
                {
                    var sqr = cell.GetEdges();

                    for (int i = 0, n = sqr.Length; i < n; i++)
                    {
                        Handles.DrawLine(sqr[i], sqr[(i + 1) % n]);
                    }
                }

                Handles.color = color;
            }
        }

        private void DrawQuadGrid()
        {
            if (_self.Grid == null)
            {
                _self.Grid = new GridQuad3D(_self.Size.x, _self.Size.y, _self.Widith).SetRoot(_self.transform).Generate(_self.Offset);
            }

            if (_self.Grid == null) return;

            var color = Handles.color;

            Handles.color = Color.green;
            foreach (var cell in _self.Grid.GetGrid())
            {
                var side = _self.Widith / 2;
                var sqr = new Vector3[]
                {
                    new Vector3(cell.worldPos.x - side, cell.worldPos.y - side, cell.worldPos.z),
                    new Vector3(cell.worldPos.x + side, cell.worldPos.y - side, cell.worldPos.z),
                    new Vector3(cell.worldPos.x + side, cell.worldPos.y + side, cell.worldPos.z),
                    new Vector3(cell.worldPos.x - side, cell.worldPos.y + side, cell.worldPos.z),
                };

                for (int i = 0, n = sqr.Length; i < n; i++)
                {
                    Handles.DrawLine(sqr[i], sqr[(i + 1) % n]);
                }
            }
            Handles.color = color;
        }
    }
}
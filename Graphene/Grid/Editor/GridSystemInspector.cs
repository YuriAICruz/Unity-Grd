using System;
using System.Collections.Generic;
using Graphene.Grid;
using UnityEditor;
using UnityEngine;

namespace Graphene.Grid
{
    [CustomEditor(typeof(GridSystem))]
    public class GridSystemInspector : Editor
    {
        private GridSystem _self;
        private IGridInfo _selectedGrid;
        private bool _mouseDown;

        private void Awake()
        {
            _self = target as GridSystem;
        }

        private void OnEnable()
        {
            EditorApplication.update += EditorUpdate;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            DrawGrid();

            DebugMouse();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var direction = (GridDirection) EditorGUILayout.EnumPopup("Direction", _self.Direction);
            if (direction != _self.Direction)
            {
                _self.Direction = direction;
                ClearGrid();
            }

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
            if (_self.Size.x * _self.Size.y > 1000) return;

            switch (_self.GridType)
            {
                case GridType.Quad:
                    DrawQuadGrid();
                    break;
                case GridType.Hex:
                    DrawHexGrid();
                    break;
                case GridType.InfHex:
                    DrawInfinityHexGrid();
                    break;
            }
        }

        private void DrawInfinityHexGrid()
        {
            if (_self.Grid == null)
            {
                _self.Grid = new InfiniteHexGrid(_self.Widith, _self.Offset, _self.Direction);
            }

            if (_self.Grid == null) return;

            var color = Handles.color;

            Handles.color = Color.green;
            for (int x = 0; x < _self.Size.x; x++)
            {
                for (int y = 0; y < _self.Size.y; y++)
                {
                    var sqr = _self.Grid.GetPos(x, y).GetEdges();

                    for (int i = 0, n = sqr.Length; i < n; i++)
                    {
                        Handles.DrawLine(sqr[i], sqr[(i + 1) % n]);
                    }
                }
            }
            Handles.color = color;
        }

        private void DrawHexGrid()
        {
            if (_self.Grid == null)
            {
                _self.Grid = new HexGrid(_self.Size.x, _self.Size.y, _self.Widith).Generate(_self.Offset, _self.Direction);
            }

            DrawGridOnViewport();
        }

        private void DrawGridOnViewport()
        {
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
        }

        private void DrawQuadGrid()
        {
            if (_self.Grid == null)
            {
                _self.Grid = new GridQuad3D(_self.Size.x, _self.Size.y, _self.Widith).SetRoot(_self.transform).Generate(_self.Offset, _self.Direction);
            }

            if (_self.Grid == null) return;

            var color = Handles.color;

            Handles.color = Color.green;
            var gr = _self.Grid.GetGrid();

            if (gr == null)
            {
                ClearGrid();
                return;
            }

            foreach (var cell in gr)
            {
                var side = _self.Widith / 2;
                var sqr = cell.GetEdges();

                for (int i = 0, n = sqr.Length; i < n; i++)
                {
                    Handles.DrawLine(sqr[i], sqr[(i + 1) % n]);
                }
            }
            Handles.color = color;
        }

        private void DebugMouse()
        {
            var cam = Camera.current;

            if (cam == null || Event.current == null) return;

            Vector2 mouse, test;
            mouse = test = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            mouse.y = -mouse.y + Screen.height + 60;
            var gr = _self.Grid.GetMousePos(mouse, cam);

            if (_selectedGrid != null)
            {
                DrawGrid(_selectedGrid, Color.blue);
            }
            
            if (gr == null) return;
            
            if(Event.current.type == EventType.MouseDown && Event.current.button == 0) {
                _selectedGrid = gr;
                _mouseDown  = true;
            }
            else if (_mouseDown && Event.current.type == EventType.MouseUp && Event.current.button == 0) {
                _mouseDown  = false;
            }
            
//                var path = _self.Grid.GetPath(_self.Grid.GetPos(_self.Size.x / 2, _self.Size.y / 2), gr);
            DrawGrid(gr, Color.red);
        }

        private void DrawGrid(IGridInfo gr, Color color)
        {
            var cl = Handles.color;
            Handles.color = color;

            var sqr = gr.GetEdges();

            for (int i = 0, n = sqr.Length; i < n; i++)
            {
                Handles.DrawLine(sqr[i], sqr[(i + 1) % n]);
            }

            Handles.color = cl;
        }
    }
}
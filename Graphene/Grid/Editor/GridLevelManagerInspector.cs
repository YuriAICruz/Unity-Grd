using Graphene.Grid;
using Graphene.Grid.LevelBuilder;
using UnityEditor;
using UnityEngine;

namespace Packages.Grid.Graphene.Grid
{
    [CustomEditor(typeof(GridLevelManager))]
    public class GridLevelManagerInspector : Editor
    {
        private GridLevelManager _self;
        private GridSystem _grid;

        private bool _mouseDown;

        private void Awake()
        {
            _self = target as GridLevelManager;
            _grid = _self.GetComponent<GridSystem>();
        }

        private void OnSceneGUI()
        {
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {
                AddItem();
                _mouseDown = true;
            }
            else if (_mouseDown && Event.current.type == EventType.MouseUp && Event.current.button == 1)
            {
                _mouseDown = false;
            }
        }

        private void AddItem()
        {
            if (_grid.SelectedGrid == null) return;

            Instantiate(_self.GridData[0].Prefab, _grid.SelectedGrid.worldPos, Quaternion.identity);
        }
    }
}
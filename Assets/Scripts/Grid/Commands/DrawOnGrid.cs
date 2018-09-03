using System.Collections;
using System.Collections.Generic;
using TurnBased.Command;
using UnityEngine;

namespace Grid.Commands
{
    public class DrawOnGrid : ICommand
    {
        private readonly IGrid _grid;
        private readonly Color _color;
        private Color _originalColor;
        private readonly List<IGridInfo> _grds;

        public DrawOnGrid(IGrid grid, List<IGridInfo> grds, Color color, Color originalColor)
        {
            _grid = grid;
            _color = color;
            _originalColor = originalColor;
            _grds = grds;
        }

        public IEnumerator Execute()
        {
            _grid.DrawGrid(_grds, _color);
            yield return null;
        }

        public IEnumerator UnExecute()
        {
            _grid.DrawGrid(_grds, _originalColor);
            yield return null;
        }
    }
}
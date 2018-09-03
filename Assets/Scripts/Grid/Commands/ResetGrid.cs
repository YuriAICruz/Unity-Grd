using System.Collections;
using TurnBased.Command;
using UnityEngine;

namespace Grid.Commands
{
    public class ResetGrid : ICommand
    {
        private readonly IGrid _grid;

        public ResetGrid(IGrid grid)
        {
            _grid = grid;
        }
        
        public IEnumerator Execute()
        {
            _grid.ResetGrid();
            yield return null;
        }

        public IEnumerator UnExecute()
        {
            Debug.Log("unreseting grid\nNothing to do here");
            yield return null;
        }
    }
}
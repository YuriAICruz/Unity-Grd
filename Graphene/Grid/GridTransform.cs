using System.Collections;
using Graphene.Grid;
using UnityEngine;

namespace Graphene.Grid
{
    public class GridTransform
    {
        public Transform transform;
        private readonly float _speed;
        
        private Animator _animator;

        public IGridInfo GridPos;
        private GridManager _gridManager;

        public GridTransform(Transform transform, float speed, Animator animator)
        {
            this.transform = transform;
            _speed = speed;
            _gridManager = Object.FindObjectOfType<GridManager>();
            _animator = animator;
        }

        public void SetPosition(IGridInfo grid)
        {
            GridPos = grid;
            transform.position = grid.worldPos;
        }

        public IEnumerator SetPositionAsync(IGridInfo grid)
        {
            var path = _gridManager.GetPath(GridPos, grid);
            
            _animator.SetFloat("LinearSpeed", _speed);
            foreach (var pos in path)
            {
                var dir = (pos.worldPos - transform.position);
                transform.LookAt(pos.worldPos + dir);
            
                while (Vector3.Distance(transform.position, pos.worldPos) > 0.2f)
                {
                    transform.position += dir * Time.deltaTime * _speed;
                    yield return null;
                }
            }
            
            transform.position = grid.worldPos;
            
            _animator.SetFloat("LinearSpeed", 0);
            
            GridPos = grid;
        }

        public void LookTo(IGridInfo pos)
        {
            transform.LookAt(pos.worldPos);
        }
    }
}
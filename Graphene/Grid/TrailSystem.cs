using Graphene.Grid;
using UnityEngine;

namespace Graphene.Grid
{
    public class TrailSystem : MonoBehaviour
    {
        public float Div = 50;
        public float Step = 1;
        public float MinDistance = 0;
        public GridSystem GridSystem;
        private InfiniteHexGrid _infGrid;

        private void Start()
        {
            if (GridSystem == null)
                GridSystem = GetComponent<GridSystem>();

            _infGrid = (InfiniteHexGrid) GridSystem.Grid;
        }


        public Vector3[] TrailMath(Vector3 p)
        {
            var z = Mathf.Floor(p.z / Step) * Step;
            var s = Step / 2;
            var division = Mathf.Clamp(Mathf.Pow(Mathf.Sin((p.x - Div * 2) / Div), 3) * s, 0, s);

            var res = new Vector3[]
            {
                new Vector3(p.x, 0, z + division - (division <= 0 ? 0 : MinDistance)),
                new Vector3(p.x, 0, z - division + (division <= 0 ? 0 : MinDistance)),
            };

            //res += p;

            return res;

//            var v = Mathf.Round(Mathf.Pow(Mathf.Sin(x), 2)) * Mathf.Round(Mathf.Pow(Mathf.Cos(z * 0.1f), 2));
//            return v;
        }
    }
}
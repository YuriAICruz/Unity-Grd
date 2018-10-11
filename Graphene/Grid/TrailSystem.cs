using Graphene.Grid;
using UnityEngine;

namespace Graphene.Grid
{
    public class TrailSystem : MonoBehaviour
    {
        public float Div = 50;
        public float Step = 1;
        public GridSystem GridSystem;
        private InfiniteHexGrid _infGrid;

        private void Start()
        {
            if (GridSystem == null)
                GridSystem = GetComponent<GridSystem>();

            _infGrid = (InfiniteHexGrid) GridSystem.Grid;
        }


        public Vector3[] CoinMath(Vector3 p)
        {
            var l = Mathf.Pow(Mathf.Sin(p.z / Step), 2);

            //l = Mathf.Floor(l * (1+1/_step));

            var z = Mathf.Floor(p.z / Step) * Step;
            var s = Step / 2;
            var naturalMotion = 0;//Mathf.Pow(Mathf.Sin(p.x * 0.01f), 3)* 50;
            var division = Mathf.Clamp(Mathf.Pow(Mathf.Sin((p.x - Div * 2)/ Div), 3) * s, 0, s);
            
            var res = new Vector3[]
            {
                new Vector3(p.x, 0, z + division + naturalMotion),
                new Vector3(p.x, 0, z - division + naturalMotion),
            };

            //res += p;

            return res;


//            var v = Mathf.Round(Mathf.Pow(Mathf.Sin(x), 2)) * Mathf.Round(Mathf.Pow(Mathf.Cos(z * 0.1f), 2));
//            return v;
        }
    }
}
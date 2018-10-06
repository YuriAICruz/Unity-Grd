using Graphene.Grid;
using UnityEngine;

namespace Packages.Grid.Graphene.Grid
{
    public enum GridType
    {
        Quad,
        Hex
    }
    
    public class GridSystem : MonoBehaviour
    {
        [HideInInspector]
        public Vector3 Offset;
        [HideInInspector]
        public Vector2Int Size = new Vector2Int(10, 10);
        [HideInInspector]
        public float Widith = 1;
        
        [HideInInspector]
        public GridType GridType;
        
        public IGrid Grid;

        public void GenHexGrid()
        {
            Grid = new HexGrid(Size.x,Size.y, Widith).Generate(Offset);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Graphene.Grid
{
    public class GridManager : MonoBehaviour
    {
        public Transform GridRootPosition;
        private static GridManager _instance;
        private Camera _mainCam;

        private GameObject[] _arrows;
        private LineRenderer _pathDrawer;

        private IGrid _grid;

        [SerializeField] private Vector2 _gridCellSize = new Vector2(10, 10);
        [SerializeField] private float _squareWidith = 1;
        public Color BaseColor = Color.white;
        // private TurnManagerBase _manager;

        public static GridManager GetInstance()
        {
            return _instance;
        }

        private void GenerateSingleton()
        {
            if (GridManager.GetInstance() == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }

//        private Queue<ICommand> _history = new Queue<ICommand>();
//
//        public ExecutionResult ScheduleCommand(ICommand command)
//        {
//            StartCoroutine(command.Execute());
//            _history.Enqueue(command);
//            Timeline.SaveToTimeline(command);
//
//            return ExecutionResult.Success;
//        }
//
//        public ExecutionResult UndoLastCommand()
//        {
//            throw new System.NotImplementedException();
//        }

        private void Awake()
        {
            GenerateSingleton();

            GenerateArrows();

            GenerateDrawer();

            _mainCam = Camera.main;

            _grid = new GridQuad3D((int) _gridCellSize.x, (int) _gridCellSize.y, _squareWidith).SetRoot(GridRootPosition).SetBaseColor(BaseColor).Generate(GridRootPosition.position).DrawGrid(BaseColor);
        }

        private void GenerateDrawer()
        {
            _pathDrawer = Instantiate(Resources.Load<GameObject>("UI/PathRenderer"), transform).GetComponent<LineRenderer>();
        }

        private void GenerateArrows()
        {
            _arrows = new GameObject[4];
            var arr = Resources.Load<GameObject>("UI/Arrow");
            for (int i = 0; i < _arrows.Length; i++)
            {
                _arrows[i] = Instantiate(arr, transform);
                _arrows[i].SetActive(false);
            }
        }

        void HideArrows()
        {
            for (int i = 0; i < _arrows.Length; i++)
            {
                _arrows[i].SetActive(false);
            }
        }

        public void ResetArrows()
        {
            HideArrows();
        }

        public void ShowArrow(IGridInfo grid, Vector3 dir)
        {
            HideArrows();

            _arrows[0].transform.position = grid.worldPos + dir;
            _arrows[0].transform.LookAt(grid.worldPos + dir * 2, Vector3.up);
            _arrows[0].SetActive(true);
        }


        private void Start()
        {
//            _manager = TurnManagerBase.GetInstance();
//
//            if (_manager != null)
//                _manager.InitFirstTurn();
//
//            _manager.OnChangeState += StateChanged;
        }

        public float GetSquareWidith()
        {
            return _squareWidith;
        }

        public IGridInfo GetPos(int x, int y)
        {
            return _grid.GetPos(x, y);
        }

        public IGridInfo GetPos(GridTransform gridTransform)
        {
            if (gridTransform.GridPos == null)
            {
                gridTransform.GridPos = _grid.GetPos(gridTransform.transform.position);
            }

            return gridTransform.GridPos;
        }

        public List<IGridInfo> SelectRegion(IGridInfo gr, int count, bool removeCenter = false)
        {
            return _grid.SelectRegion(gr, count, removeCenter);
        }

        public List<IGridInfo> GetPath(IGridInfo from, IGridInfo to)
        {
            return _grid.GetPath(from, to);
        }


        public void Clear()
        {
            ClearPath();
            HideArrows();

            _grid.ResetGrid();
            // ScheduleCommand(new ResetGrid(_grid));
        }
        public void ClearPath()
        {
            _pathDrawer.positionCount = 0;
        }

        public void DrawGrid(List<IGridInfo> grs, Color color, bool reset = true)
        {
//            if (reset)
//                ScheduleCommand(new ResetGrid(_grid));
//
//            ScheduleCommand(new DrawOnGrid(_grid, grs, color, BaseColor));

            if (reset)
                Clear();
            
            _grid.DrawGrid(grs, color);
        }

        public IGridInfo GetMousePos(Vector2 ScreenMouse)
        {
            var gr = _grid.GetMousePos(ScreenMouse, _mainCam);
            return gr;
        }

        public void DrawPath(IGridInfo start, IGridInfo end)
        {
            var path = GetPath(start, end);
            _pathDrawer.positionCount = path.Count;

            for (int i = 0; i < path.Count; i++)
            {
                _pathDrawer.SetPosition(i, path[i].worldPos + Vector3.up*0.5f);
            }
        }
    }
}
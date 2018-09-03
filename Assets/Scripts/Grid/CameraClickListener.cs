using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Graphene.Grid
{
    [RequireComponent(typeof(Camera))]
    public class CameraClickListener : MonoBehaviour
    {
        private GridManager _grid;
        
        private Camera _cam;

        private EventSystem _eventSys;
        private Vector3 _lastPos;
        [SerializeField] private float _speed;
        private float _timer, _timeLimit = 0.4f;
        private IGridInfo _lastGridPos;

        public event Action<Vector3, Vector3> MousePan;
        public event Action<IGridInfo, bool> GridPop;

        private void Start()
        {
            _eventSys = GameObject.FindObjectOfType<EventSystem>();
            _grid = GridManager.GetInstance();
            _cam = GetComponent<Camera>();
        }

        private void Update()
        {
            if (GridInput.BlockInput) return;

            if (Input.GetMouseButtonDown(0))
            {
                _timer = Time.time;
                GetMousePos();
            }
            else if (Input.GetMouseButton(0))
            {
                Pan();
            }

            if (Input.GetMouseButtonUp(0) && Time.time - _timer < _timeLimit)
            {
                GetMousePos();
                GetPos();
            }
            else if (!Input.GetMouseButton(0))
            {
                GetMousePos();
                GetPos(true);
            }
        }

        private void Pan()
        {
            if (_eventSys.IsPointerOverGameObject())
            {
                return;
            }

            var mouse = Input.mousePosition;
            var center = _cam.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, _cam.nearClipPlane));
            if (MousePan != null) MousePan(center, (_lastPos - mouse) * _speed);
            _lastPos = mouse;
        }

        private void GetPos(bool over = false)
        {
            if (_eventSys.IsPointerOverGameObject())
            {
                return;
            }

            var mouse = Input.mousePosition;
            _lastPos = mouse;

            mouse.z = _cam.nearClipPlane;
            var pos = _grid.GetMousePos(mouse);

            if (pos == _lastGridPos && over)
            {
                return;
            }

            _lastGridPos = pos;

            if (_lastGridPos != null)
            {
                if (GridPop != null) GridPop(_lastGridPos, over);
                Debug.LogError("Turn Manager, needs to receive this response");
                // _turnManager.GridSetInput(_lastGridPos, over);
            }
        }

        private void GetMousePos()
        {
            if (_eventSys.IsPointerOverGameObject())
            {
                return;
            }

            var mouse = Input.mousePosition;
            _lastPos = mouse;
        }
    }
}
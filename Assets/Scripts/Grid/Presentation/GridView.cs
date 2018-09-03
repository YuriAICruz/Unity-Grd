using UnityEngine;
using UnityEngine.UI;

namespace Grid.Presentation
{
    public class GridView : MonoBehaviour
    {
        private SpriteRenderer _renderer;

        private float _alpha = 0.6f;
        
        Color _baseColor = Color.white;
        private BoxCollider _collider;

        private void Awake()
        {
            _renderer = gameObject.AddComponent<SpriteRenderer>();
            _renderer.sprite = Resources.Load<Sprite>("SpriteBase");

            _baseColor.a = _alpha;

        }

        public void CreateCollider(string layerName)
        {
            _collider = gameObject.AddComponent<BoxCollider>();
            _collider.size = new Vector3(1,1,0.1f);
            gameObject.layer = LayerMask.NameToLayer(layerName);
        }
        
        public void SetBaseColor(Color color)
        {
            _alpha = color.a;
            _baseColor = color;
        }

        public void Draw(Color color)
        {
            color.a = _alpha;
            _renderer.color = color;
        }

        public void Reset()
        {
            _renderer.color = _baseColor;
        }
    }
}
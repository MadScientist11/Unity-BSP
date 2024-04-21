using UnityEngine;


namespace DungeonGeneration
{
    public class DungeonGenerator : MonoBehaviour
    {
        private int _mapWidth = 50;
        private int _mapDepth = 50;

        private int _scale = 2;

        private Leaf _root;


        private void Start()
        {
            _root = new Leaf(Vector2Int.zero, _mapWidth, _mapDepth, _scale);
            BSP(_root, 3);
        }

        private void BSP(Leaf parent, int splitDepth)
        {
            if (parent == null)
                return;

            if (splitDepth <= 0)
            {
                parent.Draw(0);
                return;
            }
     
            if (parent.Split())
            {
                splitDepth--;
                BSP(parent.LeftChild, splitDepth);
                BSP(parent.RightChild, splitDepth);
            }
            else
            {
                parent.Draw(0);
            }
        }
    }
    
    public class Leaf
    {
        public Leaf LeftChild => _leftChild;
        public Leaf RightChild => _rightChild;
        
        private Vector2Int _position;
        private int _width;
        private int _depth;
        private int _scale;

        private int _minRoomSize = 5;

        private Leaf _leftChild;
        private Leaf _rightChild;

        public Leaf(Vector2Int position, int width, int depth, int scale)
        {
            _position = position;
            _scale = scale;
            _depth = depth;
            _width = width;
        }

        public bool Split()
        {
            if (_width <= _minRoomSize || _depth <= _minRoomSize) 
                return false;

            bool splitHorizontal = Random.value > 0.5;

            if (_width > _depth && (float)_width / _depth >= 1.2f)
            {
                splitHorizontal = false;
            }
            else if (_depth > _width && (float)_depth / _width >= 1.2f)
            {
                splitHorizontal = true;
            }

            int max = (splitHorizontal ? _depth : _width) - _minRoomSize;
            if (max <= _minRoomSize)
                return false;
            
            
            if (splitHorizontal)
            {
                int l1Depth = Random.Range(_minRoomSize, max);

                _leftChild = new(_position, _width, l1Depth, _scale);
            
                _rightChild = new(new Vector2Int(_position.x,
                        _position.y + l1Depth), 
                    _width, _depth - l1Depth, _scale);
            }
            else
            {
                int l1Width = Random.Range(_minRoomSize, max);

                _leftChild = new(_position, l1Width, _depth, _scale);
            
                _rightChild = new(new Vector2Int(_position.x + l1Width,
                        _position.y), _width - l1Width, _depth, _scale);
            }
            
            return true;
        }

        public void Draw(int level)
        {
            Color randomColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            for (int x = _position.x; x < _width + _position.x; x++)
            {
                for (int y = _position.y; y < _depth + _position.y; y++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = new Vector3(x * _scale, level * 3, y * _scale);
                    cube.transform.localScale = Vector3.one * _scale;
                    cube.GetComponent<Renderer>().material.SetColor("_BaseColor", randomColor);
                }
            }
        }
    }
}
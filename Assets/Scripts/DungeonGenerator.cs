using System.Collections.Generic;
using UnityEngine;


namespace DungeonGeneration
{
    public class DungeonGenerator : MonoBehaviour
    {
        private int _mapWidth = 50;
        private int _mapDepth = 50;

        private int _scale = 2;

        private Leaf _root;

        private byte[,] map;

        private List<Vector2Int> corridors = new();
        

        private void Start()
        {
            map = new byte[_mapWidth, _mapDepth];
            for (int z = 0; z < _mapDepth; z++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    map[x, z] = 1;
                }
            }
            
            
            _root = new Leaf(Vector2Int.zero, _mapWidth, _mapDepth, _scale);
            BSP(_root, 5);
            AddCorridors();
            DrawMap();
        }

        private void BSP(Leaf parent, int splitDepth)
        {
            if (parent == null)
                return;

            if (splitDepth <= 0)
            {
                parent.Draw(map);
                corridors.Add(
                    new Vector2Int(
                        parent._position.x + parent._width / 2,
                        parent._position.y + parent._depth / 2)
                    );
                return;
            }
     
            if (parent.TrySplit())
            {
                splitDepth--;
                BSP(parent.LeftChild, splitDepth);
                BSP(parent.RightChild, splitDepth);
            }
            else
            {
                parent.Draw(map);
                corridors.Add(
                    new Vector2Int(
                        parent._position.x + parent._width / 2,
                        parent._position.y + parent._depth / 2)
                );
            }
        }

        private void AddCorridors()
        {
            for (int i = 1; i < corridors.Count; i++)
            {
                if (corridors[i].x - corridors[i - 1].x != 0 && corridors[i].y - corridors[i - 1].y != 0)
                {
                    int newX = corridors[i].x + (corridors[i - 1].x - corridors[i].x);
                    int newY = corridors[i - 1].y +(corridors[i].y - corridors[i - 1].y);
                    
                    line(corridors[i].x, (int)corridors[i].y, newX, newY);
                    line(corridors[i-1].x, (int)corridors[i-1].y, newX, newY);
                    continue;
                }
                
                line((int)corridors[i].x, (int)corridors[i].y,
                    (int)corridors[i - 1].x, (int)corridors[i - 1].y);
            }
        }

        private void DrawMap()
        {
            for (int z = 0; z < _mapDepth; z++)
            {
                for (int x = 0; x < _mapWidth; x++)
                {
                    if (map[x, z] == 1)
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = 
                            new Vector3(x * _scale, 10, z * _scale);
                        cube.transform.localScale = Vector3.one * _scale;
                    }
                    else if (map[x, z] == 1)
                    {
                        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        cube.transform.position = 
                            new Vector3(x * _scale, 10, z * _scale);
                        cube.transform.localScale = Vector3.one * _scale;
                        cube.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.magenta);

                    }
                }
            }

        }
        
        //Adapted Bresenham's line algorithm
        //https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm
        public void line(int x, int y, int x2, int y2)
        {
            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Mathf.Abs(w);
            int shortest = Mathf.Abs(h);
            if (!(longest > shortest))
            {
                longest = Mathf.Abs(h);
                shortest = Mathf.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                map[x,y] = 2;
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }
        }
    }
    
    public class Leaf
    {
        public Leaf LeftChild => _leftChild;
        public Leaf RightChild => _rightChild;
        
        public Vector2Int _position;
        public int _width;
        public int _depth;
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

        public bool TrySplit()
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

        public void Draw(byte[,] map)
        {
            Color randomColor = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
            for (int x = _position.x; x < _width + _position.x; x++)
            {
                for (int y = _position.y; y < _depth + _position.y; y++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = new Vector3(x * _scale, 0, y * _scale);
                    cube.transform.localScale = Vector3.one * _scale;
                    cube.GetComponent<Renderer>().material.SetColor("_BaseColor", randomColor);
                }
            }
            
            for (int x = _position.x + 1; x < _width + _position.x - 1; x++)
            {
                for (int y = _position.y + 1; y < _depth + _position.y  -1; y++)
                {
                    map[x, y] = 0;
                }
            }
            
            
        }
        
      
    }
}
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Line_of_Sight
{
    public class DynamicOcclusion : MonoBehaviour
    {
        public Camera BaseCamera;
        public OccludingPoolManager OccludingPool;
        public WallsInView Walls;
        public Transform TargetTransform;
        public InputManager Input;
        public float ScanInterval = 0.01f;

        private Vector3 _currentPosition;
        private int _halfWidth, _halfHeight;

        private void Start()
        {
            _currentPosition = TargetTransform.position;

            _halfHeight = (int) BaseCamera.orthographicSize;
            _halfWidth = (int) (_halfHeight * BaseCamera.aspect);

            // Reduction in scanning rate as speed increases.
            ScanInterval *= Input.Speed;

            /*for (var row = 0; row <= _halfHeight * 2; row++)
            {
                var floatPosition = bottomLeft + Vector3Int.up * row;
                var position = new Vector3Int(floatPosition.x, floatPosition.y, 0);
                DebugGenerateDots(position, ArrayDirection.X);
            }*/
        }

        private void Update()
        {
            var newPosition = TargetTransform.position;
            var targetPosition = _currentPosition;

            if ((Vector3Int.FloorToInt(newPosition) - Vector3Int.FloorToInt(_currentPosition)).sqrMagnitude != 0)
            {
                Walls.UpdateAll();
                _currentPosition = newPosition;
            }

            if ((newPosition - targetPosition).sqrMagnitude <= ScanInterval)
                return;

            var wallVisibility = new NativeArray<bool>((_halfHeight * 2 + 1) * (_halfWidth * 2 + 1), Allocator.TempJob);

            var bottomLeft = Vector3Int.FloorToInt(newPosition - new Vector3(_halfWidth, _halfHeight, 0));
            new GetUnobstructedTiles
            {
                BottomLeft = new float2(bottomLeft.x, bottomLeft.y),
                Origin = new float2(newPosition.x, newPosition.y),
                CameraWidth = _halfWidth * 2 + 1,
                WallsInView = Walls.Collection,
                WallVisibility = wallVisibility
            }.Run(wallVisibility.Length);

            OccludingPool.ClearAll();

            for (var index = 0; index < wallVisibility.Length; index++)
            {
                var position = new float2(bottomLeft.x, bottomLeft.y) +
                               new int2(index % (_halfWidth * 2 + 1), index / (_halfWidth * 2 + 1));
                var target = OccludingPool.GetNextOccluder(new Vector3(position.x + 0.5f, position.y + 0.5f, -0.1f));
                target.GetComponent<SpriteRenderer>().color = wallVisibility[index] ? Color.green : Color.red;
            }

            wallVisibility.Dispose();

            _currentPosition = newPosition;
        }

        /*
        private void Update()
        {
            var newPosition = DebugMap.WorldToCell(VisualPlayerTransform.position);
            if ((newPosition - _currentPosition).sqrMagnitude == 0)
                return;
            
            if (_currentPosition.x != newPosition.x && _currentPosition.y != newPosition.y)
            {
                // Diagonals don't work very well. Don't know why. Spent a couple hours trying to fix it. This is the fix.
                // If diagonal, temporarily displaces new position along the x axis so it moves only one tile instead of two.
                newPosition.y = _currentPosition.y;
            }

            var dataPackage = UpdateData(newPosition);

            DeleteDistantCells(dataPackage);
            AddDistantCells(dataPackage);
            
            _currentPosition = newPosition;
        }

        private (Vector3Int, Vector3Int) UpdateData(Vector3Int newPosition)
        {
            var cornerRelative = new Vector3Int(_halfWidth, _halfHeight, 0);
            
            var delta = _currentPosition - newPosition;

            return (cornerRelative, delta);
        }

        private void DeleteDistantCells((Vector3Int, Vector3Int) dataPackage)
        {
            var (cornerRelative, delta) = dataPackage;

            // I can probably make this into one line, probably.
            if (delta.x < 0) // Right Movement
                OccludingPool.RemoveOccluder(_currentPosition - cornerRelative,
                    ArrayDirection.Y, _halfHeight * 2, 1);
            else if (delta.x > 0) // Left
                OccludingPool.RemoveOccluder(_currentPosition + cornerRelative,
                    ArrayDirection.Y, _halfHeight * 2, -1);
            else if (delta.y < 0) // Up
                OccludingPool.RemoveOccluder(_currentPosition - cornerRelative,
                    ArrayDirection.X, _halfWidth * 2, 1);
            else if (delta.y > 0) // Down
                OccludingPool.RemoveOccluder(_currentPosition + cornerRelative,
                    ArrayDirection.X, _halfWidth * 2, -1);
        }

        private void AddDistantCells((Vector3Int, Vector3Int) dataPackage)
        {
            var (cornerRelative, delta) = dataPackage;
            
            // I can probably make this into one line, probably.
            // Note the positive and negative signs
            if (delta.x < 0)
                DebugGenerateDots(_currentPosition + new Vector3Int(cornerRelative.x + 1,-cornerRelative.y,0),
                    ArrayDirection.Y);
            else if (delta.x > 0)
                DebugGenerateDots(_currentPosition - new Vector3Int(cornerRelative.x + 1,cornerRelative.y,0),
                    ArrayDirection.Y);
            else if (delta.y < 0)
                DebugGenerateDots(_currentPosition + new Vector3Int(-cornerRelative.x,cornerRelative.y + 1,0),
                    ArrayDirection.X);
            else if (delta.y > 0)
                DebugGenerateDots(_currentPosition - new Vector3Int(cornerRelative.x,cornerRelative.y + 1,0),
                    ArrayDirection.X);
        }

        private void DebugGenerateDots(Vector3Int startingPosition, ArrayDirection direction)
        {
            var incremental = direction switch
            {
                ArrayDirection.X => Vector3Int.right,
                ArrayDirection.Y => Vector3Int.up,
                _ => throw new Exception("ERROR")
            };

            var wallsInView = GetAllWallsAlongAxis(startingPosition, direction);

            var targetPosition = startingPosition;

            foreach (var wallBool in wallsInView)
            {
                var position = DebugMap.CellToWorld(targetPosition);
                
                var debugSpot = OccludingPool.GetNextOccluder(position - new Vector3(0, 0, 0.1f));
                debugSpot.GetComponent<SpriteRenderer>().color = wallBool ? Color.red : Color.green;
                
                targetPosition += incremental;
            }
        }*/
    }
}
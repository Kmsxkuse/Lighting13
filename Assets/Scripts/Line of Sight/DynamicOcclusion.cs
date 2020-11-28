using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Line_of_Sight
{
    public class DynamicOcclusion : MonoBehaviour
    {
        public Camera BaseCamera;
        public OccludingPoolManager OccludingPool;
        public Transform VisualPlayerTransform;
        public Tilemap DebugMap;
        public List<TileBase> WallTiles;

        private Vector3Int _currentPosition;
        private int _halfWidth, _halfHeight;

        private void Start()
        {
            _currentPosition = DebugMap.WorldToCell(VisualPlayerTransform.position);
            
            _halfHeight = (int) BaseCamera.orthographicSize;
            _halfWidth = (int) (_halfHeight * BaseCamera.aspect);

            var bottomLeft = _currentPosition - new Vector3Int(_halfWidth,_halfHeight,0);

            for (var row = 0; row <= _halfHeight * 2; row++)
            {
                var floatPosition = bottomLeft + Vector3Int.up * row;
                var position = new Vector3Int(floatPosition.x, floatPosition.y, 0);
                DebugGenerateDots(position, ArrayDirection.X);
            }
        }

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

        private IEnumerable<bool> GetAllWallsAlongAxis(Vector3 position, ArrayDirection direction)
        {
            var currentTilePosition = DebugMap.WorldToCell(position);

            var range = direction == ArrayDirection.X 
                ? Vector3Int.right * (_halfWidth * 2)
                : Vector3Int.up * (_halfHeight * 2);
            
            var bounds = new BoundsInt(currentTilePosition,range + Vector3Int.one);
            
            return DebugMap.GetTilesBlock(bounds).Select(tile => WallTiles.Contains(tile));
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
        }
    }
}

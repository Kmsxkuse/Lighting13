using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Line_of_Sight
{
    public class WallsInView : MonoBehaviour
    {
        public Camera BaseCamera;
        public Tilemap BaseMap;
        public Transform TargetTransform;
        public List<TileBase> WallTiles;

        private int _halfWidth, _halfHeight;

        [NonSerialized] public NativeHashSet<int2> Collection;

        private void Start()
        {
            _halfHeight = (int) BaseCamera.orthographicSize;
            _halfWidth = (int) (_halfHeight * BaseCamera.aspect);

            Collection = new NativeHashSet<int2>(_halfWidth * _halfHeight, Allocator.Persistent);
        }

        private void OnDestroy()
        {
            Collection.Dispose();
        }

        public void UpdateAll()
        {
            Collection.Clear();

            var bottomLeft = BaseMap.WorldToCell(TargetTransform.position) - new Vector3Int(_halfWidth, _halfHeight, 0);

            for (var row = 0; row <= _halfHeight * 2; row++)
            {
                var currentPosition = bottomLeft + Vector3Int.up * row;
                UpdateAlongAxis(currentPosition, ArrayDirection.X);
            }
        }

        private void UpdateAlongAxis(Vector3Int position, ArrayDirection direction)
        {
            var currentTilePosition = BaseMap.WorldToCell(position);

            var range = direction == ArrayDirection.X
                ? Vector3Int.right * (_halfWidth * 2)
                : Vector3Int.up * (_halfHeight * 2);

            var bounds = new BoundsInt(currentTilePosition, range + Vector3Int.one);

            var tileBlock = BaseMap.GetTilesBlock(bounds);
            for (var index = 0; index < tileBlock.Length; index++)
            {
                // Checking for null will turn non existent tiles to walls.
                if (!WallTiles.Contains(tileBlock[index]))
                    continue;

                var targetCoordinates = direction == ArrayDirection.X
                    ? new int2(index + position.x, position.y)
                    : new int2(position.x, index + position.y);
                Collection.Add(targetCoordinates);
            }
        }

        /*public void UpdateDelta(Vector3 newPosition, Vector3 delta)
        {
            var cellPosition = Vector3Int.FloorToInt(newPosition);
            
            // I don't know why this doesn't work in diagonals. Fix maybe some time?
            if (delta.x > 0) // Right Movement
            {
                RemoveAlongAxis(cellPosition - new Vector3Int(_halfWidth, _halfHeight, 0),
                    ArrayDirection.Y);
                UpdateAlongAxis(cellPosition + new Vector3Int(_halfWidth + 1, -_halfHeight, 0),
                    ArrayDirection.Y);
            }
            else if (delta.x < 0) // Left
            {
                RemoveAlongAxis(cellPosition + new Vector3Int(_halfWidth, -_halfHeight, 0),
                    ArrayDirection.Y);
                UpdateAlongAxis(cellPosition - new Vector3Int(_halfWidth + 1, _halfHeight, 0),
                    ArrayDirection.Y);
            }
            else if (delta.y > 0) // Up
            {
                RemoveAlongAxis(cellPosition - new Vector3Int(_halfWidth, _halfHeight, 0),
                    ArrayDirection.X);
                UpdateAlongAxis(cellPosition + new Vector3Int(-_halfWidth, _halfHeight + 1, 0),
                    ArrayDirection.X);
            }
            else if (delta.y < 0) // Down
            {
                RemoveAlongAxis(cellPosition + new Vector3Int(-_halfWidth, _halfHeight, 0),
                    ArrayDirection.X);
                UpdateAlongAxis(cellPosition - new Vector3Int(_halfWidth, _halfHeight + 1, 0),
                    ArrayDirection.X);
            }
        }

        private void RemoveAlongAxis(Vector3Int position, ArrayDirection direction)
        {
            var positionMono = BaseMap.WorldToCell(position);
            var currentTilePosition = new int2(positionMono.x, positionMono.y);

            var increment = direction == ArrayDirection.X
                ? new int2(1,0)
                : new int2(0,1);

            var range = direction == ArrayDirection.X
                ? _halfWidth * 2
                : _halfHeight * 2;

            for (var index = 0; index <= range; index++)
            {
                Collection.Remove(currentTilePosition);
                currentTilePosition += increment;
            }
        }*/
    }
}
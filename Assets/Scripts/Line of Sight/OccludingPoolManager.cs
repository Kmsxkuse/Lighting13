using System;
using System.Collections.Generic;
using UnityEngine;

namespace Line_of_Sight
{
    public enum ArrayDirection
    {
        X, Y
    }
    
    public class OccludingPoolManager : MonoBehaviour
    {
        public GameObject OccluderPrefab;
        public Transform CompositeTransform;

        private Dictionary<Vector2, (OccluderType occluderType, GameObject gameObject)> _activeObjects;
        private Stack<GameObject> _regularPool, _compositePool;

        private void Awake()
        {
            _regularPool = new Stack<GameObject>();
            _compositePool = new Stack<GameObject>();
            _activeObjects = new Dictionary<Vector2, (OccluderType, GameObject)>();
        }

        public enum OccluderType
        {
            Regular, Composite
        }
        
        public GameObject GetNextOccluder(Vector3 position, OccluderType occluderType = OccluderType.Regular)
        {
            var isComposite = occluderType == OccluderType.Composite;
            var targetPool= isComposite ? _compositePool : _regularPool;
            var targetTransform = isComposite ? CompositeTransform : transform;

            if (targetPool.Count == 0)
            {
                var newTarget = Instantiate(OccluderPrefab, position, Quaternion.identity, targetTransform);
                _activeObjects[position] = (occluderType, newTarget);
                return newTarget;
            }

            var oldTarget = targetPool.Pop();
            oldTarget.SetActive(true);
            oldTarget.transform.position = position;
            _activeObjects[position] = (occluderType, oldTarget);
            return oldTarget;
        }

        public void RemoveOccluder(Vector3 startPosition, ArrayDirection direction, int range, int increment)
        {
            var incremental = direction switch
            {
                ArrayDirection.X => Vector2.right * increment,
                ArrayDirection.Y => Vector2.up * increment,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };

            // PlusMinus one increment in order to select just one outside window.
            var currentPosition = (Vector2) startPosition;
            
            for (var cursor = 0; cursor <= range; cursor++)
            {
                if (_activeObjects.TryGetValue(currentPosition, out var value))
                {
                    ref var targetPool= ref value.occluderType == OccluderType.Composite 
                        ? ref _compositePool : ref _regularPool;
                
                    value.gameObject.SetActive(false);
                    targetPool.Push(value.gameObject);
                    _activeObjects.Remove(currentPosition);
                }
                
                currentPosition += incremental;
            }
        }
    }
}

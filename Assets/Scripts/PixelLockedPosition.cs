using UnityEngine;

public class PixelLockedPosition : MonoBehaviour
{
    public Transform BaseCameraTransform;

    private float _height;

    private void Start()
    {
        _height = transform.position.z;
    }

    private void Update()
    {
        var targetPosition = BaseCameraTransform.position;
        targetPosition.z = _height;
        transform.position = targetPosition;
    }
}
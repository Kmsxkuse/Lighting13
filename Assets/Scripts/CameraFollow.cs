using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;

    private float _height;

    private void Start()
    {
        _height = transform.position.z - Target.position.z;
    }

    private void LateUpdate()
    {
        transform.position = Target.position + new Vector3(0, 0, _height);
    }
}

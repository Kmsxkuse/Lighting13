using UnityEngine;

public class InputManager : MonoBehaviour
{
    public float Speed = 5;
    private Vector3 _inputAxis;

    private Rigidbody2D _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Get the keyboard input data
        _inputAxis = Vector3.zero; //new float2();
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            _inputAxis.y = 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            _inputAxis.y = -1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _inputAxis.x = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _inputAxis.x = 1;

        //_inputAxis = math.normalizesafe(_inputAxis);

        _inputAxis = _inputAxis.normalized * Speed;

        //transform.Translate(_inputAxis / Speed);
    }

    private void FixedUpdate()
    {
        // Get the touch input data
        // if (input.TouchCount() > 0 && input.GetTouch(0).phase == TouchState.Moved)
        // {
        //     var touchDelta = new float2(input.GetTouch(0).deltaX, input.GetTouch(0).deltaY);
        //     inputAxis = math.normalizesafe(touchDelta);
        // }

        _rigidbody.velocity = _inputAxis;

        //_camTransform.position = transform.position + Vector3.back;

        // var playerPosition = new float3();
        // Entities.ForEach((ref PhysicsVelocity physicsVelocity, ref Translation translation, ref Rotation rotation,
        //     in PlayerInfo playerInfo, in PhysicsMass physicsMass) =>
        // {
        //     // Applying key movement.
        //     physicsVelocity.ApplyLinearImpulse(physicsMass, inputAxis * playerInfo.Speed);
        //     // Preventing any drifting of entity up or down or rotating due to friction.
        //     translation.Value *= new float3(1, 1, 0);
        //     rotation.Value = quaternion.identity;
        //     // Communicating position to camera for following.
        //     playerPosition = translation.Value;
        // }).Run();
        //
        // // -1 z to make sure it stays above the player and not rendering through it.
        // // It's orthographic so it doesn't matter the exact Z level.
        // _camTransform.position = playerPosition + new float3(0, 0, -1);
    }
}
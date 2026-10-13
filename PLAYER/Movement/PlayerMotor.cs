using Godot;

// Camera-relative X/Z movement; physical motion belongs here, not in Player.cs.
public static class PlayerMotor
{
    public static void Tick(CharacterBody3D body, Camera3D camera, PlayerConfig config, double delta)
    {
        Vector2 input = PlayerInput.ReadMovement();
        Vector3 cameraRight = camera.GlobalTransform.Basis.X;
        Vector3 cameraBack = camera.GlobalTransform.Basis.Z;
        cameraRight.Y = 0;
        cameraBack.Y = 0;
        cameraRight = cameraRight.Normalized();
        cameraBack = cameraBack.Normalized();
        Vector3 desired = (cameraRight * input.X + cameraBack * input.Y).Normalized() * config.WalkSpeed;
        Vector3 velocity = body.Velocity;
        float rate = input.LengthSquared() > 0f ? config.Acceleration : config.Deceleration;
        velocity.X = Mathf.MoveToward(velocity.X, desired.X, rate * (float)delta);
        velocity.Z = Mathf.MoveToward(velocity.Z, desired.Z, rate * (float)delta);
        velocity.Y = body.IsOnFloor() ? -0.1f : velocity.Y - config.Gravity * (float)delta;
        body.Velocity = velocity;
        body.MoveAndSlide();
    }
}

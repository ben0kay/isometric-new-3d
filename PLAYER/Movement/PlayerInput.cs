using Godot;

// Reads controls only; it does not move or rotate the body.
public static class PlayerInput
{
    public static Vector2 ReadMovement() =>
        Input.GetVector("move_left", "move_right", "move_forward", "move_back");
}

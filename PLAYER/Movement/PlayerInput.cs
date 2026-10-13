using Godot;

// The authoritative movement bindings are stored in PLAYER/PlayerConfig.tres.
// Rebind on startup so legacy WASD bindings in project.godot cannot compete.
public static class PlayerInput
{
    public const string Left = "move_left";
    public const string Right = "move_right";
    public const string Forward = "move_forward";
    public const string Back = "move_back";

    public static void Configure(PlayerConfig config)
    {
        SetKey(Left, config.MoveLeftKey);
        SetKey(Right, config.MoveRightKey);
        SetKey(Forward, config.MoveForwardKey);
        SetKey(Back, config.MoveBackKey);
    }

    private static void SetKey(string action, Key key)
    {
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action, 0.2f);
        InputMap.ActionEraseEvents(action);
        if (key != Key.None)
            InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    public static Vector2 ReadMovement() => Input.GetVector(Left, Right, Forward, Back);
}

using Godot;

// The authoritative prototype controls are in PlayerConfig.tres.
public static class PlayerInput
{
    public const string Left = "move_left";
    public const string Right = "move_right";
    public const string Forward = "move_forward";
    public const string Back = "move_back";
    public const string Sprint = "player_sprint";
    public const string Jump = "player_jump";
    public const string AnimationToggle = "player_animation_toggle";

    public static void Configure(PlayerConfig config)
    {
        SetKey(Left, config.MoveLeftKey);
        SetKey(Right, config.MoveRightKey);
        SetKey(Forward, config.MoveForwardKey);
        SetKey(Back, config.MoveBackKey);
        SetKey(Sprint, config.SprintKey);
        SetKey(AnimationToggle, config.AnimationToggleKey);
        SetMouse(Jump, config.JumpMouseButton);
    }

    private static void SetKey(string action, Key key)
    {
        EnsureAction(action);
        InputMap.ActionEraseEvents(action);
        if (key != Key.None)
            InputMap.ActionAddEvent(action, new InputEventKey { PhysicalKeycode = key });
    }

    private static void SetMouse(string action, MouseButton button)
    {
        EnsureAction(action);
        InputMap.ActionEraseEvents(action);
        if (button != MouseButton.None)
            InputMap.ActionAddEvent(action, new InputEventMouseButton { ButtonIndex = button });
    }

    private static void EnsureAction(string action)
    {
        if (!InputMap.HasAction(action))
            InputMap.AddAction(action, 0.2f);
    }

    public static Vector2 ReadMovement() => Input.GetVector(Left, Right, Forward, Back);
    public static bool SprintHeld => Input.IsActionPressed(Sprint);
    public static bool JumpPressed => Input.IsActionJustPressed(Jump);
    public static bool AnimationTogglePressed => Input.IsActionJustPressed(AnimationToggle);
}

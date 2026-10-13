using Godot;
using System.Collections.Generic;

// Select imported skeletal clips from actual movement; never move the physics body.
public partial class PlayerAnimator : Node3D
{
    private AnimationPlayer _animations;
    private PlayerConfig _config;
    private readonly Dictionary<string, string> _clips = new();
    private string _current = "";
    private bool _wasGrounded, _initialized;
    private float _jumpStartRemaining, _landingRemaining;

    // Resolve imported names once; library prefixes are allowed.
    // =========================================================
    public override void _Ready()
    {
        _animations = FindAnimationPlayer(this);
        if (_animations == null)
        {
            GD.PushError("PlayerExplorer has no imported AnimationPlayer.");
            return;
        }
        foreach (string name in _animations.GetAnimationList())
        {
            string clip = name.Substring(name.LastIndexOf('/') + 1);
            _clips[clip] = name;
            bool loop = clip == "Idle" || clip.StartsWith("Walk") ||
                clip.StartsWith("Run") || clip == "JumpAir" || clip == "Fall";
            _animations.GetAnimation(name).LoopMode = loop ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
        }
        foreach (string required in new[] { "Idle", "Walk", "Run", "JumpStart", "JumpAir", "Fall", "Land" })
            if (!_clips.ContainsKey(required)) GD.PushError($"PlayerExplorer is missing animation: {required}");
    }

    // Apply player-owned visual settings without touching the collision shape.
    // =========================================================
    public void Configure(PlayerConfig config)
    {
        _config = config;
        var model = GetNode<Node3D>("Model");
        model.Scale = Vector3.One * config.ModelScale;
        model.Position = new Vector3(0, config.ModelOffsetY, 0);
        Play("Idle");
    }

    // Called by Player after movement and cursor-facing have been updated.
    // =========================================================
    public void Tick(CharacterBody3D body, double delta)
    {
        if (_animations == null || _config == null) return;
        float dt = (float)delta;
        bool grounded = body.IsOnFloor();
        bool landed = _initialized && !_wasGrounded && grounded;
        bool tookOff = _initialized && _wasGrounded && !grounded && body.Velocity.Y > 0.1f;
        _initialized = true;
        _wasGrounded = grounded;
        if (tookOff)
        {
            _jumpStartRemaining = ClipLength("JumpStart");
            _landingRemaining = 0;
        }
        if (landed)
        {
            _landingRemaining = ClipLength("Land");
            _jumpStartRemaining = 0;
        }
        if (!grounded)
        {
            if (_jumpStartRemaining > 0)
            {
                Play("JumpStart");
                _jumpStartRemaining = Mathf.Max(0, _jumpStartRemaining - dt);
            }
            else Play(body.Velocity.Y > 0 ? "JumpAir" : "Fall");
            return;
        }
        if (_landingRemaining > 0)
        {
            Play("Land");
            _landingRemaining = Mathf.Max(0, _landingRemaining - dt);
            return;
        }
        Vector3 horizontal = new(body.Velocity.X, 0, body.Velocity.Z);
        float speed = horizontal.Length();
        if (speed < _config.AnimationMoveThreshold)
        {
            Play("Idle");
            return;
        }
        bool running = speed > _config.WalkSpeed * 1.08f;
        string gait = running ? "Run" : "Walk";
        Vector3 localTravel = GlobalTransform.Basis.Inverse() * horizontal;
        // Pick travel relative to the cursor-facing model, not camera input.
        if (Mathf.Abs(localTravel.X) > Mathf.Abs(localTravel.Z) * 1.15f)
            gait += localTravel.X < 0 ? "Left" : "Right";
        else if (localTravel.Z > 0) gait += "Back";
        float reference = running ? _config.AnimationRunReferenceSpeed : _config.AnimationWalkReferenceSpeed;
        Play(gait, Mathf.Clamp(speed / Mathf.Max(0.1f, reference), 0.35f, 4f));
    }

    // Blend on changes; retain playback phase while the same clip continues.
    // =========================================================
    private void Play(string clip, float speed = 1f)
    {
        if (_animations == null || !_clips.TryGetValue(clip, out string name)) return;
        _animations.SpeedScale = speed;
        if (_current == clip) return;
        _animations.Play(name, _config?.AnimationBlendSeconds ?? 0.12f);
        _current = clip;
    }

    // Read one-shot durations from the imported asset rather than duplicating them.
    // =========================================================
    private float ClipLength(string clip) => _clips.TryGetValue(clip, out string name)
        ? (float)_animations.GetAnimation(name).Length : 0f;

    // The importer owns its internal scene tree; avoid brittle skeleton paths.
    // =========================================================
    private static AnimationPlayer FindAnimationPlayer(Node node)
    {
        if (node is AnimationPlayer animations) return animations;
        foreach (Node child in node.GetChildren())
        {
            AnimationPlayer found = FindAnimationPlayer(child);
            if (found != null) return found;
        }
        return null;
    }
}

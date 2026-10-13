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
    private bool _moving, _running;
    private bool _animationEnabled;
    private int _travelDirection;
    private float _playbackSpeed = 1f;
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
        ApplyAnimationToggle(true);
    }

    // Called by Player after movement and cursor-facing have been updated.
    // =========================================================
    public void Tick(CharacterBody3D body, double delta)
    {
        if (_animations == null || _config == null) return;
        ApplyAnimationToggle();
        if (!_animationEnabled) return;
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
        float moveThreshold = _config.AnimationMoveThreshold * (_moving ? 0.6f : 1f);
        _moving = speed >= moveThreshold;
        if (!_moving)
        {
            _running = false;
            _playbackSpeed = 1f;
            Play("Idle");
            return;
        }
        _running = speed > _config.WalkSpeed * (_running ? 1.02f : 1.08f);
        string gait = _running ? "Run" : "Walk";
        Vector3 localTravel = GlobalTransform.Basis.Inverse() * horizontal;
        gait += SelectDirection(localTravel);
        float reference = _running ? _config.AnimationRunReferenceSpeed : _config.AnimationWalkReferenceSpeed;
        float targetSpeed = Mathf.Clamp(speed / Mathf.Max(0.1f, reference), 0.35f, 4f);
        float weight = 1f - Mathf.Exp(-_config.AnimationSpeedSmoothness * dt);
        _playbackSpeed = Mathf.Lerp(_playbackSpeed, targetSpeed, weight);
        Play(gait, _playbackSpeed);
    }

    // Freeze in a neutral idle pose for movement diagnosis, and resume cleanly.
    // =========================================================
    private void ApplyAnimationToggle(bool force = false)
    {
        if (_animations == null || _config == null) return;
        if (!force && _animationEnabled == _config.AnimationEnabled) return;
        _animationEnabled = _config.AnimationEnabled;
        _initialized = false;
        _moving = _running = false;
        _jumpStartRemaining = _landingRemaining = 0;
        _playbackSpeed = 1f;
        _current = "";
        _animations.Stop();
        _animations.SpeedScale = 1f;
        if (_animationEnabled) Play("Idle");
        else if (_clips.TryGetValue("Idle", out string idle))
        {
            _animations.Play(idle, 0);
            _animations.Seek(0, true);
            _animations.Pause();
        }
    }

    // Hold a direction beyond its boundary so tiny aiming changes cannot chatter.
    // =========================================================
    private string SelectDirection(Vector3 localTravel)
    {
        float angle = Mathf.Atan2(localTravel.X, -localTravel.Z);
        float center = _travelDirection * Mathf.Pi * 0.5f;
        float distance = Mathf.Abs(Mathf.Wrap(angle - center, -Mathf.Pi, Mathf.Pi));
        float boundary = Mathf.Pi * 0.25f + Mathf.DegToRad(_config.AnimationDirectionMarginDegrees);
        if (!IsGait(_current) || distance > boundary)
        {
            int nearest = Mathf.RoundToInt(angle / (Mathf.Pi * 0.5f));
            _travelDirection = (nearest + 4) % 4;
        }
        return _travelDirection switch { 1 => "Right", 2 => "Back", 3 => "Left", _ => "" };
    }

    // Blend gait changes at the same step phase instead of restarting the legs.
    // =========================================================
    private void Play(string clip, float speed = 1f)
    {
        if (_animations == null || !_clips.TryGetValue(clip, out string name)) return;
        _animations.SpeedScale = speed;
        if (_current == clip) return;
        bool keepPhase = IsGait(_current) && IsGait(clip);
        double phase = 0;
        if (keepPhase)
        {
            double length = ClipLength(_current);
            if (length > 0) phase = (_animations.CurrentAnimationPosition / length) % 1.0;
        }
        _animations.Play(name, _config?.AnimationBlendSeconds ?? 0.12f);
        // Defer pose evaluation to normal animation processing to retain blending.
        if (keepPhase) _animations.Seek(phase * ClipLength(clip), false);
        _current = clip;
    }

    // Only looping locomotion clips share a step phase; jumps start at frame zero.
    // =========================================================
    private static bool IsGait(string clip) => clip.StartsWith("Walk") || clip.StartsWith("Run");

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

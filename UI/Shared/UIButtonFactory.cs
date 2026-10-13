using Godot;
using System;

// Adapted from the 2D UI/Shared factory: a single science-fiction menu style.
public static class UIButtonFactory
{
    public static readonly Color Accent = new("#68dce2");
    public static readonly Color Ink = new("#09171f");
    public static readonly Color TextColour = new("#d7edf0");

    // =========================================================
    // Build a keyboard-accessible button with consistent hover and focus feedback.
    public static Button Create(string text, Action pressed, float height = 48f)
    {
        var button = new Button {
            Text = text, CustomMinimumSize = new Vector2(0, height),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand
        };
        Apply(button);
        button.Pressed += pressed;
        return button;
    }

    // =========================================================
    // Apply the existing 2D game's cyan-on-ink visual language.
    public static void Apply(Button button)
    {
        button.AddThemeFontSizeOverride("font_size", 20);
        button.AddThemeColorOverride("font_color", TextColour);
        button.AddThemeColorOverride("font_hover_color", Colors.White);
        button.AddThemeColorOverride("font_focus_color", Colors.White);
        button.AddThemeColorOverride("font_disabled_color", new Color("#61717a"));
        button.AddThemeStyleboxOverride("normal", Box(Ink, new Color("#34545e")));
        button.AddThemeStyleboxOverride("hover", Box(new Color("#143039"), Accent));
        button.AddThemeStyleboxOverride("pressed", Box(new Color("#204752"), Accent));
        button.AddThemeStyleboxOverride("disabled", Box(new Color("#0b141a"), new Color("#24343d")));
        var focus = Box(Colors.Transparent, Accent);
        focus.DrawCenter = false;
        button.AddThemeStyleboxOverride("focus", focus);
    }

    // =========================================================
    // Use the two-part right-column button treatment from the old main menu.
    public static Button CreateMenu(string text, Action pressed, float height)
    {
        var button = Create(text + "   >", pressed, height);
        button.Alignment = HorizontalAlignment.Right;
        var strip = new Panel { MouseFilter = Control.MouseFilterEnum.Ignore };
        button.AddChild(strip);
        strip.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.LeftWide);
        strip.AnchorRight = 0.32f;
        strip.OffsetLeft = 5;
        strip.OffsetRight = 0;
        strip.OffsetTop = 5;
        strip.OffsetBottom = -5;
        strip.AddThemeStyleboxOverride("panel", Box(new Color("#16343e"), new Color("#34545e")));
        var marker = Label("/ / /", 20);
        marker.HorizontalAlignment = HorizontalAlignment.Center;
        marker.VerticalAlignment = VerticalAlignment.Center;
        strip.AddChild(marker);
        marker.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        return button;
    }

    // =========================================================
    // Keep styling resources lightweight and code-driven, matching the 2D version.
    public static StyleBoxFlat Box(Color fill, Color edge) => new() {
        BgColor = fill, BorderColor = edge,
        BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
        ContentMarginLeft = 18, ContentMarginRight = 18,
        ContentMarginTop = 10, ContentMarginBottom = 10,
        CornerRadiusTopLeft = 3, CornerRadiusTopRight = 3,
        CornerRadiusBottomLeft = 3, CornerRadiusBottomRight = 3
    };

    // =========================================================
    // Build matching headers and body labels without per-menu styling duplication.
    public static Label Label(string text, int size = 18)
    {
        var label = new Label { Text = text, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", TextColour);
        return label;
    }
}

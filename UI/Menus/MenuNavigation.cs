using Godot;
using System;

// Shared boot/front-end scene transitions; the campaign submenu stays in the same 3D scene.
public static class MenuNavigation
{
    public const string BootScene = "res://UI/Menus/Boot/BootScreen.tscn";
    public const string FrontEndScene = "res://UI/Menus/FrontEnd/FrontEnd.tscn";
    public const string WorldScene = "res://WORLD/World.tscn";

    // =========================================================
    // Never leave a previous paused scene freezing its destination.
    public static void Open(Node context, string scene)
    {
        var tree = context.GetTree();
        bool paused = tree.Paused;
        tree.Paused = false;
        var error = tree.ChangeSceneToFile(scene);
        if (error == Error.Ok) return;
        tree.Paused = paused;
        throw new InvalidOperationException($"Unable to open {scene}: {error}");
    }
}

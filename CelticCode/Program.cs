namespace CelticCode;

using System;

using RaylibSharp;

public class Program
{
    [STAThread]
    static void Main()
    {
        Raylib.SetConfigFlags(WindowFlag.Resizable | WindowFlag.VsyncHint);

        Raylib.InitWindow(800, 600, "CelticCode");
        Raylib.SetExitKey(0);

        // Debug move to left monitor and maximize for hot reload
        Raylib.SetWindowPosition(-1920 + (1920 / 2), 31);
        Raylib.MaximizeWindow();

        Raylib.SetTargetFPS(60);

        using Application application = new();
        while (!Raylib.WindowShouldClose())
        {
            double dt = Raylib.GetFrameTime();
            application.Update(dt);
            application.Render(dt);
        }

        Raylib.CloseWindow();
    }
}

namespace CelticCode;

using System;

using RaylibSharp;

public class Program
{
    static readonly double[] Framerates = new double[30];
    static int frame = 0;

    [STAThread]
    static void Main()
    {
        Raylib.SetConfigFlags(WindowFlag.Resizable);

        Raylib.InitWindow(800, 600, "CelticCode");
        Raylib.SetExitKey(0);

        // Debug move to left monitor and maximize for hot reload
        Raylib.SetWindowPosition(-1920 + (1920 / 2) - 400, 151);
        // Raylib.MaximizeWindow();

        Raylib.SetTargetFPS(60);

        using Application application = new("../test.txt");
        while (!Raylib.WindowShouldClose())
        {
            double dt = Raylib.GetFrameTime();
            frame++;
            Framerates[frame % 30] = dt;
            if (frame % 30 == 0)
            {
                Raylib.SetWindowTitle($"CelticCode - FPS: {1.0 / (Framerates.Sum() / 30.0):0.0}");
            }
            application.Update(dt);
            application.Render(dt);
        }

        Raylib.CloseWindow();
    }
}

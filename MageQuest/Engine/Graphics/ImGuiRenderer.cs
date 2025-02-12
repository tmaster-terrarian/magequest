using ImGuiNET;
using Microsoft.Xna.Framework;
using MonoGame.ImGuiNet;

namespace MageQuest.Graphics;

public static class ImGuiRenderer
{
    private static MonoGame.ImGuiNet.ImGuiRenderer _renderer;

    internal static void Initialize(Main game)
    {
        _renderer = new(game);
        _renderer.RebuildFontAtlas();
    }

    internal static void Draw(GameTime gameTime)
    {
        _renderer.BeginLayout(gameTime);

        if(PlayerData.Config.Debug.ShowMenu)
        {
            if(ImGui.BeginMainMenuBar())
            {
                if(ImGui.BeginMenu("test!"))
                {
                    for(int i = 0; i < 3; i++)
                    {
                        ImGui.MenuItem($"item {i}");
                    }

                    ImGui.EndMenu();
                }

                ImGui.SetCursorPosX(ImGui.GetContentRegionMax().X - 7);

                if(ImGui.Button("x"))
                {
                    PlayerData.Config.Debug.ShowMenu = false;
                }

                ImGui.EndMainMenuBar();
            }
        }

        _renderer.EndLayout();
    }
}

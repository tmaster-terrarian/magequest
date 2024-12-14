using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using MonoGameReload.Assets;
using MonoGameReload.Files;

namespace MageQuest;

public static class ContentReloader
{
    public static FileWatcher? FileWatcher { get; private set; }

    public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice, TargetPlatform target)
    {
        FileWatcher = new FileWatcher(content, content.RootDirectory);
        AssetReloader.Initialize(FileWatcher.RootPath, target, graphicsDevice);
    }

    public static void Ignore(params AssetType[] types)
    {
        if (types.Length != 0 && FileWatcher != null)
        {
            AssetType assetType = types[0];
            for (int i = 1; i < types.Length; i++)
            {
                assetType |= types[i];
            }

            FileWatcher.IgnoreType = assetType;
        }
    }

    public static void OnUpdate(string asset, EventHandler<FileSystemEventArgs> callback)
    {
        if (FileWatcher != null)
        {
            FileProperties fileProperties = FileWatcher.FilesTree.Find(asset);
            if (fileProperties != null)
            {
                fileProperties.Updated += callback;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGameReload.Assets;

namespace MageQuest;

public static class ContentLoader
{
    private static ContentManager _content;

    private static readonly Dictionary<string, Texture2D> loadedTextures = [];
    private static readonly List<string> pathsThatDontWork = [];

    public static void Initialize(ContentManager content)
    {
        _content = content;
    }

    public static T? Load<T>(string assetName) where T : class
    {
        #if DEBUG
        if(typeof(T).IsAssignableFrom(typeof(Texture2D)))
        {
            return (AssetsManager.Textures.TryGetValue(assetName, out Texture2D value) ? value : default) as T;
        }
        #endif

        if(typeof(T).IsAssignableFrom(typeof(Texture2D)) && loadedTextures.TryGetValue(assetName, out Texture2D tex))
        {
            return tex as T;
        }

        if(pathsThatDontWork.Contains(assetName)) return default;

        try
        {
            var asset = _content.Load<T>(assetName);
            if(asset is Texture2D texture)
                loadedTextures.TryAdd(assetName, texture);

            return asset;
        }
        catch(Exception e)
        {
            Console.Error.WriteLine(e.GetType().FullName + $": The content file \"{assetName}\" was not found.");
            pathsThatDontWork.Add(assetName);
            return default;
        }
    }

    public static void ClearTextures()
    {
        loadedTextures.Clear();
    }
}

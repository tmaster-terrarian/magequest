using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

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
        if(pathsThatDontWork.Contains(assetName)) return default;

        try
        {
            return _content.Load<T>(assetName);
        }
        catch(Exception e)
        {
            Console.Error.WriteLine(e.GetType().FullName + $": The content file \"{assetName}\" was not found.");
            pathsThatDontWork.Add(assetName);
            return default;
        }
    }
}

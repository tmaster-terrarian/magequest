using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LDtk;

using MageQuest.Actors;

namespace MageQuest.IO;

public static class LevelLoader
{
    private static readonly string levelsPath = Path.Combine(FileLocations.Data, "levels", "base");

    public static string ActiveLevel { get; private set; }

    public static int CurrentEntrance { get; private set; }

    public static FPoint[] Entrances { get; private set; } = [];

    public static void Load(string id, int playerEntrance = 0)
    {
        Unload();

        LoadLevelData(id);

        ActiveLevel = id;

        if(Entrances.Length != 0 && playerEntrance >= 0 && playerEntrance < Entrances.Length)
        {
            CurrentEntrance = playerEntrance;

            Actor.Initialize(new Player {
                Position = Entrances[playerEntrance] + new FPoint(-4, -14),
            });
        }
        else
        {
            CurrentEntrance = -1;
        }

        Actor.DoStart();
    }

    public static void Unload()
    {
        Actor.Cleanup();
        ActiveLevel = null;
        CurrentEntrance = 0;
        Entrances = [];
    }

    private static void LoadLevelData(string id)
    {
        var path = Path.Combine(levelsPath, $"{id}.ldtkl");
        var bgPath = Path.Combine(levelsPath, "png", $"{id}_bg");

        var level = LDtkLevel.FromFile(path);

        Main.Camera.Bounds = new(0, 0, level.PixelWidth, level.PixelHeight);

        var entityLayer = level.LayerInstances.First(static l => l.Identifier == "entities");
        var specialEntityLayer = level.LayerInstances.First(static l => l.Identifier == "special_entities");

        List<EntityInstance> entrances = [];

        foreach(var entity in specialEntityLayer.EntityInstances)
        {
            if(entity.Identifier == "entrance")
                entrances.Add(entity);
        }

        Entrances = new FPoint[entrances.Count];
        foreach(var entrance in entrances)
        {
            Entrances[((JsonElement)entrance.FieldInstances[0].Value).GetInt32()] = entrance.PixelCoord.ToFPoint();
        }

        foreach(var entity in entityLayer.EntityInstances)
        {
            // if(entity._Identifier == "Ledge")
            // {
            //     scene.Entities.Add(new() {
            //         Position = entity.Px,
            //         Enabled = true,
            //         Visible = true,
            //         Components = [
            //             new Solid {
            //                 DefaultBehavior = false,
            //                 Width = entity.Width,
            //                 Height = entity.Height,
            //             }
            //         ]
            //     });
            // }

            // if(entity._Identifier == "JumpThrough")
            //     scene.Collisions.JumpThroughs.Add(new(entity.Px, new(entity.Width, MathHelper.Max(entity.Height - 1, 1))));

            // if(entity._Identifier.EndsWith("Slope"))
            // {
            //     Point point1 = entity.Px;
            //     Point point2 = new Point(entity.FieldInstances[0]._Value[0].GetProperty("cx").GetInt32() * Consts.OneTile, entity.FieldInstances[0]._Value[0].GetProperty("cy").GetInt32() * Consts.OneTile);

            //     if(entity._Identifier == "JumpThrough_Slope")
            //         scene.Collisions.JumpThroughSlopes.Add(new(point1, point2, 2));
            //     if(entity._Identifier == "Slope")
            //         scene.Collisions.Slopes.Add(new(point1, point2, 2));
            // }
        }

        var collisionLayer = level.GetIntGrid("collisions");
        Actor.Initialize(
            new Tilemap(collisionLayer.GridSize.X, collisionLayer.GridSize.Y, collisionLayer.Values) {
                Tag = Tag.Add(Tag.Empty, (uint)(
                    ActorTags.PlayerCollidable |
                    ActorTags.EnemyCollidable
                )),
            }
        );
    }
}

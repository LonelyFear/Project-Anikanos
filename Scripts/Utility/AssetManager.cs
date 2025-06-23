using Godot;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text.Json;
public static class AssetManager
{
    // Saved Stuff
    public const string modsFolderPath = "Mods/";
    public static List<string> loadedModIds;
    public static List<string> foundModPaths;
    public static Dictionary<string, Biome> biomes = new Dictionary<string, Biome>();
    // public static Dictionary<string, Crop> crops = new Dictionary<string, Crop>();
    // public static Dictionary<string, BaseResource> resources = new Dictionary<string, BaseResource>();
    public static void LoadBiomes(string modPath)
    {
        string biomesPath = modPath + "/Biomes/biomes.json";
        FileAccess bio = FileAccess.Open(biomesPath, FileAccess.ModeFlags.Read);
        if (bio != null)
        {
            string biomeData = bio.GetAsText();

            Biome[] biomeArray = JsonSerializer.Deserialize<Biome[]>(biomeData);
            foreach (Biome biome in biomeArray)
            {
                biomes.Add(biome.id, biome);
            }
            GD.Print("Loaded " + biomeArray.Length + " biomes");
        }
        else
        {
            GD.PushError("Biomes.json not found at path '" + biomesPath + "'");
        }
        
    }
    public static void GetLoadedMods()
    {
        GD.Print("Mod Loading Start");
        foundModPaths = new List<string>();
        DirAccess modsDir = DirAccess.Open(modsFolderPath);

        if (modsDir != null)
        {
            foreach (string localModPath in modsDir.GetDirectories())
            {
                string modPath = modsFolderPath + localModPath;
                if (DirAccess.Open(modPath).GetFiles().Contains("mod.json"))
                {
                    foreach (string dataPath in DirAccess.Open(modPath).GetFiles())
                    {
                        if (dataPath == "mod.json")
                        {
                            string modInfoJson = FileAccess.Open(modPath + "/" + dataPath, FileAccess.ModeFlags.Read).GetAsText();
                            Dictionary<string, string> modData = JsonSerializer.Deserialize<Dictionary<string, string>>(modInfoJson);

                            if (modData.ContainsKey("name") && modData.ContainsKey("description") && modData.ContainsKey("author") && modData.ContainsKey("version"))
                            {
                                GD.Print("Found Mod '" + modData["name"] + "' by " + modData["author"]);
                                foundModPaths.Add(modPath);
                            }
                            else
                            {
                                GD.Print("Mod at path '" + modPath + "' mod.json lacks information. Mod loading skipped");
                            }
                            break;
                        }
                    }
                }
            }
        }

        GD.Print(foundModPaths.Count + " Mod(s) Found");
    }
    public static void LoadMods()
    {
        biomes = new Dictionary<string, Biome>();
        // crops = new Dictionary<string, Crop>();
        // resources = new Dictionary<string, BaseResource>();

        GetLoadedMods();
        foreach (string modPath in foundModPaths)
        {
            //LoadFood(modPath);
            //LoadResources(modPath);
        }
        foreach (string modPath in foundModPaths)
        {
            //LoadCrops(modPath);
            LoadBiomes(modPath);
        }          
        
        
    }
    /*
    public static void LoadFood(string modPath)
    {
        string foodDirPath = modPath + "/Food/";

        DirAccess foodDir = DirAccess.Open(foodDirPath);
        if (foodDir != null)
        {
            foreach (string localFoodPath in foodDir.GetFiles())
            {
                string foodPath = foodDirPath + localFoodPath;

                string foodData = FileAccess.Open(foodPath, FileAccess.ModeFlags.Read).GetAsText();
                FoodResouce food = JsonSerializer.Deserialize<FoodResouce>(foodData);

                resources.Add(food.id, food);
            }

        }
    }
    public static void LoadResources(string modPath)
    {
        string resourcesPath = modPath + "/Resources/";

        DirAccess resourcesDir = DirAccess.Open(resourcesPath);
        if (resourcesDir != null)
        {
            foreach (string resourcesFile in resourcesDir.GetFiles())
            {
                string path = resourcesPath + resourcesFile;

                string resourceData = FileAccess.Open(path, FileAccess.ModeFlags.Read).GetAsText();
                BaseResource resource = JsonSerializer.Deserialize<BaseResource>(resourceData);

                resources.Add(resource.id, resource);
            }

        }
    }
    public static void LoadCrops(string modPath)
    {
        string cropsDirPath = modPath + "/Crops/";

        DirAccess cropsDir = DirAccess.Open(cropsDirPath);
        if (cropsDir != null)
        {
            foreach (string localCropPath in cropsDir.GetFiles())
            {
                string cropPath = cropsDirPath + localCropPath;

                string cropData = FileAccess.Open(cropPath, FileAccess.ModeFlags.Read).GetAsText();
                Crop crop = JsonSerializer.Deserialize<Crop>(cropData);

                string yieldIDDict = JsonSerializer.Deserialize<Dictionary<string, object>>(cropData)["yield"].ToString();
                Dictionary<string, float> cropYieldIds = JsonSerializer.Deserialize<Dictionary<string, float>>(yieldIDDict);

                crop.yields = new Dictionary<BaseResource, float>();
                foreach (string id in cropYieldIds.Keys)
                {
                    if (GetResource(id) != null)
                    {
                        crop.yields.Add(GetResource(id), cropYieldIds[id]);
                    }
                }
                crops.Add(crop.id, crop);
            }

        }
    }
    public static BaseResource GetResource(string id)
    {
        if (resources.ContainsKey(id))
        {
            return resources[id];
        }
        else
        {
            GD.PushError("Resource not found with ID '" + id + "'");
            return null;
        }
    }
    public static Crop GetCrop(string id)
    {
        if (crops.ContainsKey(id))
        {
            return crops[id];
        }
        else
        {
            GD.PushError("Resource not found with ID '" + id + "'");
            return null;
        }
    }
    */
    public static Biome GetBiome(string id)
    {
        return biomes[id];
    }
}
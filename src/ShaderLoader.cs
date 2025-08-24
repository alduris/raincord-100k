using System;
using RWCustom;
using UnityEngine;

namespace Raincord100k;

public static class ShaderLoader
{
    public static FShader Reflection { get; set; } = null!;
    public static int ReflectionMirrorY { get; } = Shader.PropertyToID("_ReflectionMirrorY");
    public static int ReflectionFadeStrength { get; } = Shader.PropertyToID("_ReflectionFadeStrength");
    public static int ReflectionBlurStrength { get; } = Shader.PropertyToID("_ReflectionBlurStrength");

    public static void LoadShaders()
    {
        var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assetbundles/reflection"));

        Reflection = bundle.LoadShader(nameof(Reflection), "Assets/Shaders/reflection.shader");
    }

    public static FShader LoadShader(this AssetBundle bundle, string shaderName, string shaderPath)
    {
        try
        {
            var shader = bundle.LoadAsset<Shader>(shaderPath);
            var fShader = FShader.CreateShader(shaderName, shader);

            Custom.rainWorld.Shaders[shaderName] = fShader;
            return fShader;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Exception loading shader: {(shaderPath == "" ? "(missing shader path)" : shaderPath)}\n{e}");
        }

        return null!;
    }
}

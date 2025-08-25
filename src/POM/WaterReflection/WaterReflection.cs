using JetBrains.Annotations;
using UnityEngine;

namespace Raincord100k;

[UsedImplicitly]
public class WaterReflection(Room room, PlacedObject placedObject) : Reflection(room, placedObject)
{
    protected override FShader ReflectionShader => ShaderLoader.WaterReflection;

    public override void Update(bool eu)
    {
        base.Update(eu);

        var data = (WaterReflectionData)Data;

        Shader.SetGlobalFloat(ShaderLoader.ReflectionRippleSpeed, data.rippleSpeed);
        Shader.SetGlobalFloat(ShaderLoader.ReflectionRippleAspect, data.rippleAspect);
        Shader.SetGlobalFloat(ShaderLoader.ReflectionRippleFrequency, data.rippleFrequency);
        Shader.SetGlobalFloat(ShaderLoader.ReflectionRippleStrength, data.rippleStrength);
    }
}

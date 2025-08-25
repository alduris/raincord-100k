using JetBrains.Annotations;
using UnityEngine;

namespace Raincord100k;

[UsedImplicitly]
public class ReflectionData(PlacedObject owner) : Pom.Pom.ManagedData(owner, null)
{
    [Pom.Pom.Vector2Field("size", 400.0f, 100.0f, Pom.Pom.Vector2Field.VectorReprType.rect, "Size")]
    public Vector2 size;

    [Pom.Pom.FloatField("mirrorY", 0.0f, 1.0f, 0.1f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Mirror Y")]
    public float mirrorY;

    [Pom.Pom.FloatField("fadeStrength", 0.0f, 1.0f, 0.5f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Fade Strength")]
    public float fadeStrength;

    [Pom.Pom.FloatField("blurStrength", 0.0f, 1.0f, 0.5f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Blur Strength")]
    public float blurStrength;
}

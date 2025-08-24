using JetBrains.Annotations;

namespace Raincord100k;

[UsedImplicitly]
public class WaterReflectionData(PlacedObject owner) : ReflectionData(owner)
{
    [Pom.Pom.FloatField("rippleSpeed", 0.0f, 1.0f, 0.8f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Ripple Speed")]
    public float rippleSpeed;

    [Pom.Pom.FloatField("rippleAspect", 0.0f, 2500f, 100.0f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Ripple Aspect")]
    public float rippleAspect;

    [Pom.Pom.FloatField("rippleFrequency", 0.0f, 5.0f, 1.0f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Ripple Frequency")]
    public float rippleFrequency;

    [Pom.Pom.FloatField("rippleStrength", 0.0f, 0.05f, 0.01f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Ripple Strength")]
    public float rippleStrength;
}

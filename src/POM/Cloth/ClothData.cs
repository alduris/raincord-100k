using JetBrains.Annotations;
using UnityEngine;

namespace Raincord100k;

[UsedImplicitly]
public sealed class ClothData(PlacedObject owner) : Pom.Pom.ManagedData(owner, null)
{
    [Pom.Pom.Vector2Field("polePos", 0.0f, 0.0f, Pom.Pom.Vector2Field.VectorReprType.line, "Pole Pos")]
    public Vector2 polePos;

    [Pom.Pom.ColorField("clothColor", 1.0f, 1.0f, 1.0f, 1.0f, Pom.Pom.ManagedFieldWithPanel.ControlType.button, "Cloth Color")]
    public Color clothColor;

    [Pom.Pom.IntegerField("clothLength", 6, 100, 20, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Cloth Length")]
    public int clothLength;

    [Pom.Pom.IntegerField("clothWidth", 6, 100, 20, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Cloth Width")]
    public int clothWidth;

    [Pom.Pom.BooleanField("hasPole", true, Pom.Pom.ManagedFieldWithPanel.ControlType.button, "Has Pole?")]
    public bool hasPole;

    [Pom.Pom.BooleanField("hasCollisions", true, Pom.Pom.ManagedFieldWithPanel.ControlType.button, "Has Collisions?")]
    public bool hasCollisions;

    [Pom.Pom.BooleanField("hasWind", false, Pom.Pom.ManagedFieldWithPanel.ControlType.button, "Has Wind?")]
    public bool hasWind;

    [Pom.Pom.FloatField("windDir", 0.0f, 360.0f, 90.0f, 1.0f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Wind Dir")]
    public float windDir;

    [Pom.Pom.FloatField("windSpeed", 0.0f, 5.0f, 2.0f, 0.1f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Wind Speed")]
    public float windSpeed;

    [Pom.Pom.FloatField("windWavelength", 0.0f, 1.0f, 0.25f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Wind Wavelength")]
    public float windWavelength;

    [Pom.Pom.FloatField("windFrequency", 0.0f, 0.1f, 0.02f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Wind Frequency")]
    public float windFrequency;

    [Pom.Pom.FloatField("windAmplitude", 0.0f, 2.0f, 0.5f, 0.01f, Pom.Pom.ManagedFieldWithPanel.ControlType.slider, "Wind Amplitude")]
    public float windAmplitude;
}

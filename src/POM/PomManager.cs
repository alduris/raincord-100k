namespace Raincord100k;

// Placed Objects Manager Manager :D
public static class PomManager
{
    public static string Category => "Raincord100k";

    public static void RegisterPlacedObjects()
    {
        // Cloth with wind simulation
        Pom.Pom.RegisterManagedObject<Cloth, ClothData, Pom.Pom.ManagedRepresentation>("100kCloth", Category);

        // Reflective floor
        Pom.Pom.RegisterManagedObject<Reflection, ReflectionData, Pom.Pom.ManagedRepresentation>("100kReflection", Category, true);

        // Reflective floor with rain droplets
        Pom.Pom.RegisterManagedObject<WaterReflection, WaterReflectionData, Pom.Pom.ManagedRepresentation>("100kWaterReflection", Category, true);
    }
}

using System;
using DevInterface;
using Raincord100k.Pearls;

namespace Raincord100k.Hooks
{
    internal static class PlacedObjectRegistration
    {
        // important note: pearl spot not having a non-default placed object rep (despite using consumable object data) is intentional

        internal static void ApplyHooks()
        {
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;

            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;

            On.Room.Loaded += Room_Loaded;
        }

        private static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
        {
            if (type == Constants.PearlSpot100k)
            {
                return ObjectsPage.DevObjectCategories.Gameplay;
            }
            return orig(self, type);
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            if (self.type == Constants.PearlSpot100k)
            {
                self.data = new PlacedObject.ConsumableObjectData(self)
                {
                    minRegen = 0
                };
            }
            orig(self);
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            bool firstTimeRealized = self.abstractRoom.firstTimeRealized;
            orig(self);
            if (self.game == null || !self.game.IsStorySession || self.world == null || self.world.name.ToLowerInvariant() != "100k") return;

            for (int i = 0; i < self.roomSettings.placedObjects.Count; i++)
            {
                PlacedObject po = self.roomSettings.placedObjects[i];
                bool consumed = self.game.IsStorySession && self.game.GetStorySession.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i);
                if (po.type == Constants.PearlSpot100k && firstTimeRealized && !consumed)
                {
                    if (PearlSpotRegistry.TryGetPearl(self.game.GetStorySession.saveState.seed, self.abstractRoom.name, i, out var pearlType))
                    {
                        var abstrPearl = new DataPearl.AbstractDataPearl(self.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, self.GetWorldCoordinate(po.pos), self.game.GetNewID(), self.abstractRoom.index, i, po.data as PlacedObject.ConsumableObjectData, pearlType)
                        {
                            isConsumed = false,
                            placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(self.abstractRoom.name, i)
                        };
                        self.abstractRoom.entities.Add(abstrPearl);
                    }
                    Plugin.Logger.LogDebug($"Pearl spot in room {self.abstractRoom.name} at index {i} type: {pearlType?.value ?? "[NULL]"}");
                }
            }
        }
    }
}

using DevInterface;
using Raincord100k.Pearls;
using Raincord100k.SpawnSpots;
using RWCustom;
using UnityEngine;

namespace Raincord100k.Hooks
{
    internal static class PlacedObjectRegistration
    {
        // important note: pearl spot not having a non-default placed object rep (despite using consumable object data) is intentional

        internal static void ApplyHooks()
        {
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += ObjectsPage_DevObjectGetCategoryFromPlacedType;
            On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRep;

            On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyData;

            On.Room.Loaded += Room_Loaded;
        }

        private static ObjectsPage.DevObjectCategories ObjectsPage_DevObjectGetCategoryFromPlacedType(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
        {
            if (type == Constants.PearlSpot || type == Constants.SpawnSpot || type == Constants.ShelterNoSaveZone)
            {
                return ObjectsPage.DevObjectCategories.Gameplay;
            }
            if (type == Constants.ShelterLanternMouse || type == Constants.ShelterJetfish)
            {
                return ObjectsPage.DevObjectCategories.Creatures;
            }
            return orig(self, type);
        }

        private static void ObjectsPage_CreateObjRep(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            PlacedObjectRepresentation rep = null;

            if (tp == Constants.SpawnSpot)
            {
                NullCheckPObj();
                rep = new SpawnSpotRepresentation(self.owner, tp.value + "_Rep", self, pObj, "Spawn Spot");
            }
            else if (tp == Constants.ShelterNoSaveZone)
            {
                NullCheckPObj();
                rep = new ResizeableObjectRepresentation(self.owner, tp.value + "_Rep", self, pObj, "Shelter No Save Zone", true);
            }

            if (rep != null)
            {
                self.tempNodes.Add(rep);
                self.subNodes.Add(rep);
            }
            else
            {
                orig(self, tp, pObj);
            }

            void NullCheckPObj()
            {
                if (pObj == null)
                {
                    var camPos = self.owner.room.game.cameras[0].pos;
                    pObj = new PlacedObject(tp, null)
                    {
                        pos = camPos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(Random.value * 360f) * 0.2f
                    };
                    self.RoomSettings.placedObjects.Add(pObj);
                }
            }
        }

        private static void PlacedObject_GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            if (self.type == Constants.PearlSpot)
            {
                self.data = new PlacedObject.ConsumableObjectData(self)
                {
                    minRegen = 0
                };
            }
            else if (self.type == Constants.SpawnSpot)
            {
                self.data = new SpawnSpotData(self);
            }
            else if (self.type == Constants.ShelterNoSaveZone)
            {
                self.data = new PlacedObject.ResizableObjectData(self);
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
                if (po.type == Constants.PearlSpot && firstTimeRealized && !consumed)
                {
                    if (PearlSpotRegistry.TryGetPearl(self.game.GetStorySession.saveState.seed, self.abstractRoom.name, i, out var pearlType) && !SaveData.HasBeenRead(pearlType))
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
                else if (po.type == Constants.SpawnSpot && firstTimeRealized && self.game.GetStorySession.saveState.cycleNumber == 0)
                {
                    self.AddObject(new SpawnSpotScript(self));
                }
                else if (po.type == Constants.ShelterLanternMouse)
                {
                    if (Random.value < 0.5f)
                    {
                        var crit = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.LanternMouse), null, self.GetWorldCoordinate(po.pos), new EntityID(-1, self.abstractRoom.index * 100 + i))
                        {
                            destroyOnAbstraction = true,
                            saveCreature = false
                        };
                        ShelterHooks.RegisterCreatureWithShelter(crit);
                        crit.pos.abstractNode = 1;
                        self.abstractRoom.AddEntity(crit);
                        self.AssignOriginAndIteration(crit, i);
                    }
                }
                else if (po.type == Constants.ShelterJetfish)
                {
                    if (Random.value < 0.5f)
                    {
                        var crit = new AbstractCreature(self.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.JetFish), null, self.GetWorldCoordinate(po.pos), new EntityID(-1, self.abstractRoom.index * 100 + i))
                        {
                            destroyOnAbstraction = true,
                            saveCreature = false
                        };
                        ShelterHooks.RegisterCreatureWithShelter(crit);
                        crit.pos.abstractNode = 1;
                        self.abstractRoom.AddEntity(crit);
                        self.AssignOriginAndIteration(crit, i);
                    }
                }
            }
        }
    }
}

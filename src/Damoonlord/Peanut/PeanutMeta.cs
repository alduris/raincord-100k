using DevInterface;
using Fisobs.Core;
using RWCustom;
using UnityEngine;

namespace Raincord100k.Damoonlord.Peanut
{
    internal class PeanutMeta
    {
        public static void EnableHooks()
        {
            Content.Register(new PeanutFisob());
            RegisterValues();
            On.Room.Loaded += LoadPeanutToRoom;
            On.DevInterface.ObjectsPage.DevObjectGetCategoryFromPlacedType += PlaceInCategory;
            On.PlacedObject.GenerateEmptyData += GenerateEmptyData;
            On.DevInterface.ObjectsPage.CreateObjRep += CreateObjRepresentation;
            On.AImap.TileAccessibleToCreature_IntVector2_CreatureTemplate += PeanutPathFinding;
        }

        public static void Disable()
        {
            UnregisterValues();
        }

        public static SoundID Peanut_Jump;
        public static PlacedObject.Type PlacedPeanut;

        public static void RegisterValues()
        {
            Peanut_Jump = new SoundID("Peanut_Jump", true);
            PlacedPeanut = new PlacedObject.Type(nameof(Peanut), true);
        }

        public static void UnregisterValues()
        {
            if (Peanut_Jump != null) { Peanut_Jump.Unregister(); Peanut_Jump = null; }
            if (PlacedPeanut != null) { PlacedPeanut.Unregister(); PlacedPeanut = null; }
        }

        internal static ObjectsPage.DevObjectCategories PlaceInCategory(On.DevInterface.ObjectsPage.orig_DevObjectGetCategoryFromPlacedType orig, ObjectsPage self, PlacedObject.Type type)
        {
            if (type == PlacedPeanut)
            {
                return ObjectsPage.DevObjectCategories.Consumable;
            }

            return orig(self, type);
        }

        internal static void GenerateEmptyData(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
        {
            if (self.type == PlacedPeanut)
            {
                self.data = new PlacedPeanutData(self);
            }
            else
            {
                orig(self);
            }
        }

        internal static void CreateObjRepresentation(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
        {
            if (tp == PlacedPeanut)
            {
                if (pObj is null)
                    self.RoomSettings.placedObjects.Add(pObj = new(tp, null)
                    {
                        pos = self.owner.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new(-683f, 384f), .25f) + Custom.DegToVec(Random.value * 360f) * .2f
                    });
                var pObjRep = new PlacedPeanutRepresentation(self.owner, self, pObj);
                self.tempNodes.Add(pObjRep);
                self.subNodes.Add(pObjRep);
            }
            else
            {
                orig(self, tp, pObj);
            }
        }

        internal static void LoadPeanutToRoom(On.Room.orig_Loaded orig, Room self)
        {
            if (self.game == null)
            {
                orig(self);
                return;
            }

            if (self.abstractRoom.firstTimeRealized)
            {
                for (int num = 1; num <= 2; num++)
                {
                    if (num == 2 && self.warpPoints.Count > 0)
                    {
                        continue;
                    }

                    for (int num2 = 0; num2 < self.roomSettings.placedObjects.Count; num2++)
                    {
                        if ((num2 == 1 && self.roomSettings.placedObjects[num2].deactivatedByWarpFilter) || (num == 2 && !self.roomSettings.placedObjects[num2].deactivatedByWarpFilter) || !self.roomSettings.placedObjects[num2].active || self.CheckForWarpedObjects(num2))
                        {
                            continue;
                        }

                        if (self.roomSettings.placedObjects[num2].type == PlacedPeanut)
                        {
                            PlacedPeanutData PlacedPeanutData = (self.roomSettings.placedObjects[num2].data as PlacedPeanutData);
                            Vector2 pos = self.roomSettings.placedObjects[num2].pos;

                            if (!(self.game.session is StoryGameSession) || !(self.game.session as StoryGameSession).saveState.ItemConsumed(self.world, karmaFlower: false, self.abstractRoom.index, num2))
                            {
                                int x = self.GetTilePosition(pos).x;
                                int foundTileY = self.GetTilePosition(pos).y;
                                Vector2 peanutPos = new Vector2(0, 0);
                                bool valid = true;
                                bool flag = PlacedPeanutData.Grounded;

                                while (foundTileY >= 0)
                                {
                                    bool invalidtile =
                                        self.GetTile(x, foundTileY).AnyWater ||
                                        self.GetTile(x, foundTileY).hive ||
                                        self.GetTile(x, foundTileY).wormGrass ||
                                        self.GetTile(x, foundTileY).Terrain == Room.Tile.TerrainType.Slope ||
                                        self.GetTile(x, foundTileY).Terrain == Room.Tile.TerrainType.ShortcutEntrance;

                                    if (!self.GetTile(x, foundTileY).Solid && self.GetTile(x, foundTileY - 1).Solid)
                                    {
                                        if (invalidtile)
                                        {
                                            valid = false;
                                            flag = false;
                                        }

                                        peanutPos = (flag ? self.MiddleOfTile(x, foundTileY) : new Vector2(pos.x, foundTileY * 20)) - (flag ? new Vector2(Mathf.Lerp(-5f, 5f, self.game.SeededRandom((int)pos.x)), 0f) : new Vector2(0f, 0f));
                                        break;
                                    }

                                    foundTileY--;
                                }

                                if (valid && peanutPos != new Vector2(0f, 0f))
                                {
                                    AbstractPhysicalObject abstractPhysicalObject = new AbstractPeanut(self.world, self.GetWorldCoordinate(peanutPos), self.game.GetNewID(), self.abstractRoom.index, num2, self.roomSettings.placedObjects[num2].data as PlacedObject.ConsumableObjectData, (self.roomSettings.placedObjects[num2].data as PlacedPeanutData).SuperPeanutChance, (self.roomSettings.placedObjects[num2].data as PlacedPeanutData).Grounded);
                                    (abstractPhysicalObject as AbstractConsumable).isConsumed = false;
                                    self.abstractRoom.entities.Add(abstractPhysicalObject);
                                    abstractPhysicalObject.placedObjectOrigin = self.SetAbstractRoomAndPlacedObjectNumber(self.abstractRoom.name, num2);
                                }
                                else
                                {
                                    Debug.Log("Position was invalid for a PlacedPeanut in room: " + self.abstractRoom.name + "!");
                                }
                            }
                        }
                    }
                }
            }

            orig(self);
        }

        internal static bool PeanutPathFinding(On.AImap.orig_TileAccessibleToCreature_IntVector2_CreatureTemplate orig, AImap self, IntVector2 pos, CreatureTemplate crit)
        {
            for (int i = 0; i < self.room.physicalObjects.Length; i++)
            {
                for (int i2 = 0; i2 < self.room.physicalObjects[i].Count; i2++)
                {
                    if (self.room.physicalObjects[i][i2] is Peanut peanut && peanut.Abstract.SuperPeanut && peanut.BitesLeft == 3 && peanut.grabbedBy.Count == 0 && Custom.InsideRect(pos, new IntRect(self.room.GetTilePosition(peanut.firstChunk.pos).x - 1, self.room.GetTilePosition(peanut.firstChunk.pos).y - 1, self.room.GetTilePosition(peanut.firstChunk.pos).x + 1, self.room.GetTilePosition(peanut.firstChunk.pos).y + 1)))
                    {
                        return false;
                    }
                }
            }

            return orig(self, pos, crit);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace Raincord100k.Hooks
{
    internal static class ShelterHooks
    {
        private static readonly ConditionalWeakTable<AbstractCreature, object> shelterCreatureCWT = new();
        public static void RegisterCreatureWithShelter(AbstractCreature creature)
        {
            shelterCreatureCWT.GetOrCreateValue(creature);
        }

        internal static void Apply()
        {
            try
            {
                On.ShelterDoor.DoorClosed += ShelterDoor_DoorClosed;
                IL.ShelterDoor.Update += ShelterDoor_Update;
                IL.AbstractCreature.RealizeInRoom += AbstractCreature_RealizeInRoom;
                IL.Player.UpdateMSC += Player_UpdateMSC;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogFatal(e);
            }
        }

        private static void Player_UpdateMSC(ILContext il)
        {
            // Goal: don't move items
            var c = new ILCursor(il);
            c.GotoNext(x => x.MatchLdsfld<AbstractPhysicalObject.AbstractObjectType>(nameof(AbstractPhysicalObject.AbstractObjectType.Creature)));
            c.GotoNext(MoveType.AfterLabel, x => x.MatchBrfalse(out _));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, 3);
            c.Emit(OpCodes.Ldloc, 4);
            c.EmitDelegate((bool notCreature, Player self, int i, int j) => notCreature && !(self.room.physicalObjects[i][j].abstractPhysicalObject is AbstractConsumable ac && ac.placedObjectIndex > -1 && !ac.isConsumed));
        }

        private static void AbstractCreature_RealizeInRoom(ILContext il)
        {
            // Goal: don't spawn in the middle of the shelter, spawn where we need to spawn at
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(AbstractRoom).GetProperty(nameof(AbstractRoom.shelter)).GetGetMethod()));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((bool isShelter, AbstractCreature self) =>
            {
                return isShelter && !shelterCreatureCWT.TryGetValue(self, out _);
            });
        }

        private static void ShelterDoor_Update(ILContext il)
        {
            // Goal: don't stun on start
            var c = new ILCursor(il);
            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<Creature>(nameof(Creature.Stun)));
            ILLabel label = c.MarkLabel();
            c.GotoPrev(MoveType.AfterLabel, x => x.MatchLdarg(0), x => x.MatchLdfld<UpdatableAndDeletable>(nameof(UpdatableAndDeletable.room)));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, 11);
            c.EmitDelegate((ShelterDoor self, int i) => shelterCreatureCWT.TryGetValue(self.room.abstractRoom.creatures[i], out _));
            c.Emit(OpCodes.Brtrue, label);
        }

        private static void ShelterDoor_DoorClosed(On.ShelterDoor.orig_DoorClosed orig, ShelterDoor self)
        {
            try
            {
                List<PhysicalObject> objectsToDestroy = [];
                foreach (var po in self.room.roomSettings.placedObjects)
                {
                    if (po.type == Constants.ShelterNoSaveZone && po.data is PlacedObject.ResizableObjectData resizableData)
                    {
                        foreach (var obj in self.room.physicalObjects.SelectMany(x => x))
                        {
                            if (obj.bodyChunks.Any(x => Vector2.Distance(x.pos, po.pos) < resizableData.Rad))
                            {
                                objectsToDestroy.Add(obj);
                            }
                        }
                    }
                }
                foreach (var obj in objectsToDestroy)
                {
                    obj.Destroy();
                    obj.abstractPhysicalObject.Destroy();
                    self.room.abstractRoom.entities.Remove(obj.abstractPhysicalObject);
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError(e);
            }
            orig(self);
        }
    }
}

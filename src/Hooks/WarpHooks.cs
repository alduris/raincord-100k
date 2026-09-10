using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Watcher;

namespace Raincord100k.Hooks
{
    internal static class WarpHooks
    {
        internal static void Apply()
        {
            On.Room.ForceSpawnWarpPoint += Room_ForceSpawnWarpPoint;
            On.Region.HasWarpFatigueResistance += Region_HasWarpFatigueResistance;
            IL.Watcher.WarpPoint.SuckInCreatures += WarpPoint_SuckInCreatures;
        }

        private static WarpPoint Room_ForceSpawnWarpPoint(On.Room.orig_ForceSpawnWarpPoint orig, Room self, PlacedObject po, bool saveInRegionState)
        {
            saveInRegionState &= self.world?.name?.ToUpperInvariant() != "100K";
            var warpPoint = orig(self, po, saveInRegionState);
            return warpPoint;
        }

        private static bool Region_HasWarpFatigueResistance(On.Region.orig_HasWarpFatigueResistance orig, string name)
        {
            if (name.ToUpperInvariant() == "100K") return true;
            return orig(name);
        }

        private static void WarpPoint_SuckInCreatures(ILContext il)
        {
            // Prevent neuron flies from being sucked in but only in 100K

            var c = new ILCursor(il);

            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt(typeof(List<AbstractPhysicalObject.AbstractObjectType>).GetMethod("Contains")));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, 5);
            c.EmitDelegate((bool blacklisted, WarpPoint self, PhysicalObject pObj) =>
            {
                return blacklisted || (self.room.world.name.ToUpperInvariant() == "100K" && pObj is SSOracleSwarmer);
            });
        }
    }
}

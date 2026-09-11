using Mono.Cecil.Cil;
using MonoMod.Cil;
using Raincord100k.Damoonlord.Peanut;
using UnityEngine;

namespace Raincord100k.Hooks
{
    internal static class ScavengerHooks
    {
        internal static void Apply()
        {
            IL.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
        }

        private static void ScavengerAbstractAI_InitGearUp(ILContext il)
        {
            var c = new ILCursor(il);

            c.GotoNext(x => x.MatchLdstr("SB"));
            c.GotoPrev(MoveType.Before, x => x.MatchCallOrCallvirt(typeof(ModManager).GetProperty(nameof(ModManager.DLCShared)).GetGetMethod()));

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloca, 0);
            c.Emit(OpCodes.Ldloc, 7);
            c.EmitDelegate((ScavengerAbstractAI self, ref int i, string regionName) =>
            {
                if (regionName == "100K")
                {
                    if (i >= 0 && Random.value < 0.2f)
                    {
                        var abstractLantern = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.Lantern, null, self.parent.pos, self.world.game.GetNewID());
                        self.parent.Room.AddEntity(abstractLantern);
                        new AbstractPhysicalObject.CreatureGripStick(self.parent, abstractLantern, i, true);
                        i--;
                    }
                    if (i >= 0 && Random.value < 0.4f)
                    {
                        var abstractPeanut = new AbstractPeanut(self.world, self.parent.pos, self.world.game.GetNewID(), -1, -1, null, 0.2f, false, false);
                        self.parent.Room.AddEntity(abstractPeanut);
                        new AbstractPhysicalObject.CreatureGripStick(self.parent, abstractPeanut, i, true);
                        i--;
                    }
                }
            });
        }
    }
}

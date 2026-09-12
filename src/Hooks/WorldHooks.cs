using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Raincord100k.Hooks
{
    internal static class WorldHooks
    {
        internal static void Apply()
        {
            IL.RainWorldGame.Update += RainWorldGame_Update;
            On.World.ctor += World_ctor;
        }

        private static void World_ctor(On.World.orig_ctor orig, World self, RainWorldGame game, Region region, string name, bool singleRoomWorld)
        {
            orig(self, game, region, name, singleRoomWorld);
            if (self.name == "100K")
            {
                self.AddWorldProcess(new DebugScavengerMapper(self));
            }
        }

        private static void RainWorldGame_Update(ILContext il)
        {
            // Goal: increase update rate if 100K

            const int numUpdates = 2;

            var c = new ILCursor(il);

            c.GotoNext(MoveType.After, x => x.MatchCallOrCallvirt<AbstractRoom>(nameof(AbstractRoom.Update)));
            ILLabel brTo = c.MarkLabel();

            c.GotoPrev(x => x.MatchCallOrCallvirt(typeof(RainWorldGame).GetProperty(nameof(RainWorldGame.IsStorySession)).GetGetMethod()));
            c.GotoNext(MoveType.After, x => x.MatchBrfalse(out _));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((RainWorldGame self) =>
            {
                if (self.world.name == "100K")
                {
                    int updateBy = self.world.NumberOfRooms / 4;
                    for (int i = 0; i < numUpdates; i++)
                    {
                        self.updateAbstractRoom++;
                        if (self.updateAbstractRoom >= self.world.NumberOfRooms)
                        {
                            self.updateAbstractRoom = 0;
                        }
                        self.world.GetAbstractRoom(self.updateAbstractRoom + self.world.firstRoomIndex).Update(updateBy);
                    }
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, brTo);
        }
    }
}

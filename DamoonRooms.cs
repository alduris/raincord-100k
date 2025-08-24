using UnityEngine;

namespace Raincord100k
{
    public static class DamoonRooms
    {
        public static void OESphereFix(On.MoreSlugcats.OEsphere.orig_AddToContainer orig, MoreSlugcats.OEsphere self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
            rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[1]);
            rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[2]);
            sLeaser.sprites[1].MoveInFrontOfOtherNode(sLeaser.sprites[0]);
            sLeaser.sprites[0].MoveToBack();
        }
		
		private void ShelterHUDHide(On.HUD.HUD.orig_Update orig, HUD.HUD self)
        {
            orig(self);

            bool flag = false;
            if (self.owner.GetOwnerType() == HUD.HUD.OwnerType.Player && (self.owner as Player).room != null)
            {
                flag = (self.owner as Player).room.abstractRoom.name == "TREETOP_SHELTER";
            }

            if (flag && !ValidGateLocation(self.owner as Player))
            {
                self.showKarmaFoodRain = false;
            }
        }
		
		private void ShelterHUDInit(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig(self, cam);

            bool flag = cam.room.abstractRoom.name == "TREETOP_SHELTER";

            if (flag && !ValidGateLocation(self.owner as Player))
            {
                self.karmaMeter.fade = 0f;
                self.rainMeter.fade = 0f;
                self.foodMeter.fade = 0f;
            }
        }

        private IntVector2? ShelterEntrance(On.ShelterDoor.orig_ShelterEntranceOverrides orig, ShelterDoor self)
        {
            if (self.room.abstractRoom.name == "TREETOP_SHELTER")
            {
                self.dir = new Vector2(-1f, 0f);
                return new IntVector2(15, 22);
            }

            return orig(self);
        }
		
		static bool ValidGateLocationFoodHUD(HUD.FoodMeter.MeterCircle self)
        {
            Player player = self.meter.hud.owner as Player;
            return ValidGateLocation(player);
        }

        private void OEWallShelterHUD(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);

                c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchCallOrCallvirt(typeof(HUD.FoodMeter).GetMethod("get_PupHasDied"))
                );

                c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchLdfld(typeof(HUD.FoodMeter).GetField("timeCounter"))
                );

                c.TryGotoPrev(
                    MoveType.After,
                    x => x.MatchLdcI4(3)
                );

                ILLabel label = (ILLabel)c.Next.Operand;

                c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchBge(label)
                );

                c.MoveAfterLabels();

                c.Emit(OpCodes.Ldarg_0);

                c.EmitDelegate<Func<HUD.FoodMeter.MeterCircle, bool>>(ValidGateLocationFoodHUD);

                c.Emit(OpCodes.Brfalse, label);

            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
		
        static bool ValidGateLocation(Player self)
        {
            if (self.room.abstractRoom.name == "TREETOP_SHELTER")
            {
                return
                    self.room.GetTile(self.mainBodyChunk.pos).Y < 19 &&
                    self.room.GetTile(self.mainBodyChunk.pos).Y > 12 &&
                    self.room.GetTile(self.mainBodyChunk.pos).X < 8 &&
                    self.room.GetTile(self.mainBodyChunk.pos).X > 2;
            }

            return true;
        }

        private void ShelterCheck(ILContext il)
        {
            try
            {
                ILCursor c = new ILCursor(il);

                c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchCallOrCallvirt(typeof(ShelterDoor).GetMethod("Close"))
                );

                for (int i = 0; i < 2; i ++)
                {
                    c.TryGotoPrev(
                        MoveType.After,
                        x => x.MatchLdsfld(typeof(ModManager).GetField("MMF"))
                    );
                }

                c.TryGotoPrev(
                    MoveType.After,
                    x => x.MatchLdcI4(6)
                );

                ILLabel label = (ILLabel)c.Next.Operand;

                c.TryGotoNext(
                    MoveType.After,
                    x => x.MatchBle(label)
                );

                c.MoveAfterLabels();

                c.Emit(OpCodes.Ldarg_0);

                c.EmitDelegate<Func<Player, bool>>(ValidGateLocation);

                c.Emit(OpCodes.Brfalse, label);

            }
            catch (Exception e)
            {
                Logger.LogError(e);
            }
        }
    }

}

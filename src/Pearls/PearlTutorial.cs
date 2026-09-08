namespace Raincord100k.Pearls
{
    internal class PearlTutorial : UpdatableAndDeletable
    {
        public PearlTutorial(Room room)
        {
            this.room = room;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Hold SPECIAL to select a pearl reading"), 40, 160, false, true);
            room.game.cameras[0].hud.textPrompt.AddMessage(room.game.manager.rainWorld.inGameTranslator.Translate("Double tap SPECIAL while a reading is playing to stop it"), 40, 160, false, true);
            Destroy();
        }
    }
}

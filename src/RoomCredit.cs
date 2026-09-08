using System.Linq;
using HUD;
using RWCustom;

namespace Raincord100k
{
    public class RoomCredit : UpdatableAndDeletable, Conversation.IOwnAConversation
    {
        private readonly string creditName;
        public Conversation conversation;
        private bool hasCreatedConversation = false;

        public RoomCredit(Room room)
        {
            this.room = room;
            string[] split = room.abstractRoom.name.Split('_');
            if (split.Length > 2)
            {
                creditName = CreditsRegistry.GetActualName(split[2]);
            }
            else
            {
                creditName = "???";
            }
            Plugin.Logger.LogDebug($"Room created by {creditName}");

            if (room.updateList.OfType<RoomCredit>().Any())
            {
                Destroy();
            }
        }

        public static string Translate(string s)
        {
            return Custom.rainWorld.inGameTranslator.Translate(s);
        }

        public string ReplaceParts(string s)
        {
            return s.Replace("<CREATOR>", creditName);
        }

        public void SpecialEvent(string eventName)
        {
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            if (slatedForDeletetion) return;

            if (!hasCreatedConversation)
            {
                hasCreatedConversation = true;
                if (room.game.cameras[0].hud.dialogBox == null)
                {
                    room.game.cameras[0].hud.InitDialogBox();
                }
                conversation = new RoomCreditConversation(this, room.game.cameras[0].hud.dialogBox);
            }
            else if (conversation != null)
            {
                conversation.Update();
                if (conversation.slatedForDeletion)
                {
                    Destroy();
                }
            }
        }

        public class RoomCreditConversation : Conversation
        {
            public RoomCreditConversation(IOwnAConversation interfaceOwner, DialogBox dialogBox) : base(interfaceOwner, Constants.RoomCredit, dialogBox)
            {
                AddEvents();
            }

            public override void AddEvents()
            {
                base.AddEvents();
                events.Add(new TextEvent(this, 20, Translate("[ Room created by <CREATOR> ]"), 20));
            }
        }
    }
}

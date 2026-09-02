using System.Linq;
using UnityEngine;

namespace Raincord100k.SpawnSpots
{
    internal class SpawnSpotScript : UpdatableAndDeletable
    {
        internal static bool hasRunScript = false;
        private Vector2 spawnPoint;
        private bool foundSpawnPoint;
        private bool movedPlayers;

        public SpawnSpotScript(Room room)
        {
            this.room = room;

            if (hasRunScript)
            {
                Destroy();
                return;
            }

            foreach (var po in room.roomSettings.placedObjects)
            {
                if (po.type == Constants.SpawnSpot)
                {
                    foundSpawnPoint = true;
                    spawnPoint = po.pos;
                    return;
                }
            }
            Destroy();
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            if (slatedForDeletetion) return;
            
            if (foundSpawnPoint)
            {
                int playerNum = 0;
                foreach (var obj in room.physicalObjects.SelectMany(x => x))
                {
                    if (obj is Player player && !player.isNPC)
                    {
                        movedPlayers = true;
                        player.SuperHardSetPosition(spawnPoint + new Vector2((playerNum + 1) / 2 * (playerNum % 2 == 0 ? -1 : 1), 0f) * 20f);
                        playerNum++;
                    }
                }
            }

            if (movedPlayers)
            {
                hasRunScript = true;
                Destroy();
            }
        }
    }
}

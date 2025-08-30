using Fisobs.Core;
using UnityEngine;

namespace Raincord100k.Damoonlord.Peanut
{
    sealed class AbstractPeanut : AbstractConsumable
    {
        public AbstractPeanut(World world, WorldCoordinate pos, EntityID ID, int origRoom, int PlacedObjectindex, PlacedObject.ConsumableObjectData placedObjectData, float chance = 0f, bool grounded = false, bool golden = false) : base(world, PeanutFisob.AbstractPeanut, null, pos, ID, origRoom, PlacedObjectindex, placedObjectData)
        {
            //this.Golden = golden ? golden : Random.value < 0.000001; // i think this is one in a million??? idk

            if (!Golden)
            {
                SuperPeanut = world.game.SeededRandom(ID.number) < chance;
            }

            if (SuperPeanut)
            {
                Hue = Mathf.Lerp(0.20f, 1.00f, world.game.SeededRandom(ID.number));
                Sat = 1f;
                Lit = 0.7f;
            }
            else
            {
                Hue = Mathf.Lerp(0.05f, 0.10f, world.game.SeededRandom(ID.number));
                Sat = Mathf.Lerp(0.18f, 0.46f, world.game.SeededRandom(ID.number));
                Lit = Mathf.Lerp(0.60f, 0.80f, world.game.SeededRandom(ID.number));
            }

            Grounded = grounded;
        }
        public override void Realize()
        {
            base.Realize();
            if (realizedObject == null)
            {
                realizedObject = new Peanut(this, Room.realizedRoom.MiddleOfTile(pos.Tile), Vector2.zero);
            }
        }

        public float Hue;
        public float Sat;
        public float Lit;
        public bool SuperPeanut;
        public bool Grounded;
        public bool Golden; // Scratch that, its an echo peanut

        public override string ToString()
        {
            return this.SaveToString($"{originRoom};{placedObjectIndex};{Hue};{Sat};{Lit};{SuperPeanut};{Grounded};{Golden}");
        }
    }
}

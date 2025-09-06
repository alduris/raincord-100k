using Fisobs.Properties;

namespace Raincord100k.Damoonlord.Peanut
{
    sealed class PeanutProperties : ItemProperties
    {
        public Peanut Peanut;

        public PeanutProperties(Peanut peanut)
        {
            this.Peanut = peanut;
        }

        public override void Throwable(Player player, ref bool throwable) => throwable = true;

        public override void ScavCollectScore(Scavenger scavenger, ref int score)
        {
            if (Peanut.Abstract.Grounded)
            {
                score = 0;
                return;
            }

            if (!Peanut.Abstract.Golden)
            {
                score = Peanut.BitesLeft == 1 ? 0 : 1;

                if (Peanut.BitesLeft == 3)
                {
                    score = 4;

                    if (Peanut.Abstract.SuperPeanut)
                    {
                        score = 5;
                    }
                }
            }
            else
            {
                score = Peanut.BitesLeft == 3 ? int.MaxValue : 0;

            }
        }

        public override void ScavWeaponPickupScore(Scavenger scav, ref int score)
        {
            score = 0;

            if (Peanut.Abstract.Grounded) { return; }

            if (Peanut.BitesLeft == 3 && !Peanut.Abstract.Golden)
            {
                score = 1;

                if (Peanut.Abstract.SuperPeanut)
                {
                    score = 3;
                }
            }
        }

        public override void ScavWeaponUseScore(Scavenger scav, ref int score)
        {
            score = 0;

            if (Peanut.Abstract.Grounded) { return; }

            if (Peanut.BitesLeft == 3 && !Peanut.Abstract.Golden)
            {
                score = 2;

                if (Peanut.Abstract.SuperPeanut)
                {
                    score = scav.AI.currentViolenceType == ScavengerAI.ViolenceType.Lethal ? 3 : 0;
                }
                else if (scav.AI.currentViolenceType == ScavengerAI.ViolenceType.NonLethal)
                {
                    for (int i = 0; i < scav.AI.scavenger.grasps.Length; i++)
                    {
                        if (scav.AI.scavenger.grasps[0] == null)
                        {
                            score = 4;
                        }
                    }
                }
            }
        }

        public override void Nourishment(Player player, ref int quarterPips)
        {
            if (!Peanut.Abstract.SuperPeanut)
            {
                quarterPips = 4 * Peanut.FoodPoints;
            }
            else if (Peanut.Abstract.SuperPeanut)
            {
                quarterPips = 4 * Peanut.FoodPoints + 4;
            }
            else if (Peanut.Abstract.Golden)
            {
                quarterPips = (player.slugcatStats.maxFood - player.FoodInStomach) * 4;
            }
        }

        public override void Grabability(Player player, ref Player.ObjectGrabability grabability)
        {
            if (Peanut.Grounded)
            {
                grabability = Player.ObjectGrabability.Drag;
            }
            else
            {
                grabability = Player.ObjectGrabability.OneHand;
            }
        }
    }
}

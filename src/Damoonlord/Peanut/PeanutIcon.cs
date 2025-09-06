using Fisobs.Core;
using RWCustom;
using UnityEngine;

namespace Raincord100k.Damoonlord.Peanut
{
    sealed class PeanutIcon : Icon
    {
        public override int Data(AbstractPhysicalObject AbsObject)
        {
            if (AbsObject is AbstractPeanut peanut)
            {
                int h = (int)(peanut.Hue * 360f);
                int s = (int)(peanut.Sat * 100f) * 360;
                int l = (int)(peanut.Lit * 100f) * 36000;

                return h + s + l;
            }

            return 0;
        }

        public override Color SpriteColor(int data)
        {
            float h = (data % 360) / 360f;
            float s = ((data / 360) % 100) / 100f;
            float l = (data / 36000) / 100f;

            if (data == 1)
            {
                h = 0.20f;
                s = 1f;
                l = 0.7f;
            }
            else if (data == 0)
            {
                h = 0.04f;
                s = 0.28f;
                l = 0.70f;
            }

            return Custom.HSL2RGB(h, s, l);
        }

        public override string SpriteName(int data)
        {
            return "Icon_Peanut";
        }
    }
}

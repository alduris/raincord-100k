using Fisobs.Core;
using UnityEngine;

namespace Raincord100k.Damoonlord.Peanut
{
    sealed class PeanutIcon : Icon
    {
        public override int Data(AbstractPhysicalObject AbsObject)
        {
            if (AbsObject is AbstractPeanut peanut)
            {
                int h = (int)(peanut.Hue * 1000f);
                int s = (int)(peanut.Sat * 10000000f);
                int l = (int)(peanut.Lit * 100000000000f);

                return h + s + l;
            }

            return 0;
        }

        public override Color SpriteColor(int data)
        {
            float h = (data - Mathf.Floor(data / 10000) * 10000) / 1000;
            float s = Mathf.Floor((data - Mathf.Floor(data / 100000000) * 100000000) / 100000) / 100;
            float l = Mathf.Floor(data / 1000000000000) * 1000000000000 / 100000000000;


            if (data == 1)
            {
                h = Mathf.Lerp(0.20f, 1.00f, Random.value);
                s = 1f;
                l = 0.7f;
            }
            else if (data == 0)
            {
                h = 0.04f;
                s = 0.28f;
                l = 0.70f;
            }

            return RWCustom.Custom.HSL2RGB(h, s, l);
        }

        public override string SpriteName(int data)
        {
            return "Icon_Peanut";
        }
    }
}

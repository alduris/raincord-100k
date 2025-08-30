using Fisobs.Core;
using Fisobs.Items;
using Fisobs.Properties;
using Fisobs.Sandbox;
using UnityEngine;

namespace Raincord100k.Damoonlord.Peanut
{
    sealed class PeanutFisob : Fisob
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType AbstractPeanut = new("AbstractPeanut", true);

        public static readonly MultiplayerUnlocks.SandboxUnlockID Peanut = new("Peanut", true);
        public static readonly MultiplayerUnlocks.SandboxUnlockID SuperPeanut = new("SuperPeanut", true);

        public PeanutFisob() : base(AbstractPeanut)
        {
            Icon = new PeanutIcon();

            SandboxPerformanceCost = new(linear: 0.30f, exponential: 0f);


            RegisterUnlock(Peanut, MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 0);
            RegisterUnlock(SuperPeanut, MultiplayerUnlocks.SandboxUnlockID.Slugcat, data: 1);
        }

        public override AbstractPhysicalObject Parse(World world, EntitySaveData saveData, SandboxUnlock? unlock)
        {
            string[] p = saveData.CustomData.Split(';');

            if (p.Length < 8)
            {
                p = new string[8];
            }

            var result = new AbstractPeanut(world, saveData.Pos, saveData.ID, 0, 0, null, 0, false, false)
            {
                originRoom = int.TryParse(p[0], out var or) ? or : 0,
                placedObjectIndex = int.TryParse(p[1], out var ri) ? ri : 0,
                Hue = float.TryParse(p[2], out var h) ? h : 0,
                Sat = float.TryParse(p[3], out var s) ? s : 0,
                Lit = float.TryParse(p[4], out var l) ? l : 0,
                SuperPeanut = bool.TryParse(p[5], out var sp) ? sp : false,
                Grounded = bool.TryParse(p[6], out var gr) ? gr : false,
            };

            if (unlock is SandboxUnlock u)
            {
                result.Hue = Mathf.Lerp(0.05f, 0.10f, Random.value);
                result.Sat = Mathf.Lerp(0.18f, 0.46f, Random.value);
                result.Lit = Mathf.Lerp(0.60f, 0.80f, Random.value);

                if (u.Data == 1)
                {
                    result.SuperPeanut = true;
                    result.Hue = Mathf.Lerp(0.20f, 1.00f, Random.value);
                    result.Sat = 1f;
                    result.Lit = 0.7f;
                }

                result.Grounded = false;
            }

            return result;
        }

        public override ItemProperties Properties(PhysicalObject forObject)
        {
            if (forObject is Peanut)
            {
                return new PeanutProperties(forObject as Peanut);
            }

            return null;
        }
    }
}

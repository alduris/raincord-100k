using UnityEngine;

namespace Raincord100k.Pearls
{
    public class PearlPing : UpdatableAndDeletable, IDrawable
    {
        private readonly Vector2 pos;
        private readonly int totalLifetime;
        private int lifetime;

        private float Life(float timeStacker) => Mathf.InverseLerp(0, totalLifetime, lifetime + timeStacker);

        public PearlPing(Vector2 pos)
        {
            this.pos = pos;
            totalLifetime = 30;
        }

        public override void Update(bool eu)
        {
            base.Update(eu);
            lifetime++;
            if (lifetime > totalLifetime)
            {
                Destroy();
            }
        }

        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites =
            [
                new FSprite("Futile_White")
                {
                    shader = rCam.game.rainWorld.Shaders["VectorCircleFadable"]
                }
            ];
            AddToContainer(sLeaser, rCam, null);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            newContatiner ??= rCam.ReturnFContainer("WarpPoint");
            foreach (var sprite in sLeaser.sprites)
            {
                sprite.RemoveFromContainer();
                newContatiner.AddChild(sprite);
            }
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            float life = Life(timeStacker);
            float scale = 6f + 20f * life;
            var sprite = sLeaser.sprites[0];
            sprite.SetPosition(pos - camPos);
            sprite.color = new Color(0f, 0f, 1f - life, 1f / scale);
            sprite.scale = scale / 16f;
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }
    }
}

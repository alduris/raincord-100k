using JetBrains.Annotations;
using UnityEngine;

namespace Raincord100k;

[UsedImplicitly]
public class Reflection : UpdatableAndDeletable, IDrawable
{
    public PlacedObject PlacedObject { get; }
    public ReflectionData Data { get; }

    protected virtual FShader ReflectionShader => ShaderLoader.Reflection;

    public Reflection(Room room, PlacedObject placedObject)
    {
        this.room = room;

        PlacedObject = placedObject;
        Data = (ReflectionData)placedObject.data;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        Shader.SetGlobalFloat(ShaderLoader.ReflectionMirrorY, Data.mirrorY);
        Shader.SetGlobalFloat(ShaderLoader.ReflectionFadeStrength, Data.fadeStrength);
        Shader.SetGlobalFloat(ShaderLoader.ReflectionBlurStrength, Data.blurStrength);
    }

    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var sprites = new Sprites(sLeaser);

        sprites.Initialize();

        AddToContainer(sLeaser, rCam, null!);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        sLeaser.RemoveAllSpritesFromContainer();

        var container = rCam.ReturnFContainer("GrabShaders");

        foreach (var sprite in sLeaser.sprites)
        {
            container.AddChild(sprite);
        }
    }

    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        var sprites = new Sprites(sLeaser);
        var main = sprites.Main;

        var pos = PlacedObject.pos + (Data.size / 2.0f) - camPos;
        main.SetPosition(pos);

        main.scaleX = Data.size.x;
        main.scaleY = Data.size.y;

        main.shader = ReflectionShader;
    }

    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette) { }

    public class Sprites(RoomCamera.SpriteLeaser sLeaser)
    {
        private RoomCamera.SpriteLeaser SLeaser { get; } = sLeaser;

        // Order represents actual layering of container, from back to front
        public enum Sprite
        {
            Main,

            Count,
        }

        public FSprite Main => GetSprite<FSprite>(Sprite.Main);

        public void Initialize()
        {
            SLeaser.sprites = new FSprite[(int)Sprite.Count];

            SetSprite(Sprite.Main, new FSprite("pixel"));
        }

        private T GetSprite<T>(Sprite sprite)
            where T : FSprite
        {
            return (T)SLeaser.sprites[(int)sprite];
        }

        private void SetSprite<T>(Sprite spriteId, T sprite)
            where T : FSprite
        {
            SLeaser.sprites[(int)spriteId] = sprite;
        }
    }
}

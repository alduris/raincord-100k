using JetBrains.Annotations;
using RWCustom;
using UnityEngine;

namespace Raincord100k;

[UsedImplicitly]
public sealed class Cloth : UpdatableAndDeletable, IDrawable
{
    public PlacedObject PlacedObject { get; }
    public ClothData Data { get; }

    public int ClothSprite { get; set; }
    public int PoleClothSprite { get; set; }
    public int PoleSprite { get; set; }

    public Vector2[,] ClothVertices { get; set; }
    public Vector2[] PoleVertices { get; }

    public SharedPhysics.TerrainCollisionData ClothCollisionData { get; } = new();

    public Vector2 PoleStartPos => PlacedObject.pos;
    public Vector2 PoleEndPos => PlacedObject.pos + Data.polePos;
    public Vector2 ClothPos => PoleEndPos + Custom.DirVec(PoleEndPos, PoleStartPos) * 15.0f;
    public Vector2 PoleClothPos => PoleEndPos + Custom.DirVec(PoleEndPos, PoleStartPos) * 30.0f;

    public float WindDirStacker { get; set; }

    public Cloth(Room room, PlacedObject placedObject)
    {
        this.room = room;

        PlacedObject = placedObject;
        Data = (ClothData)placedObject.data;

        ClothVertices = new Vector2[Data.clothLength, Data.clothWidth];
        PoleVertices = new Vector2[(int)Mathf.Clamp(Data.polePos.magnitude / 11.0f, 3.0f, 30.0f)];

        var state = Random.state;
        Random.InitState((int)PlacedObject.pos.x);

        for (var i = 0; i < Data.clothLength; i++)
        {
            for (var j = 0; j < Data.clothWidth; j++)
            {
                ClothVertices[i, j] = ClothPos;
            }
        }

        for (var i = 0; i < 120; i++)
        {
            UpdateCloth();
        }

        for (var i = 0; i < PoleVertices.Length; i++)
        {
            PoleVertices[i] = Custom.RNV() * Random.value;
        }

        Random.state = state;
    }

    public override void Update(bool eu)
    {
        base.Update(eu);

        UpdateCloth();

        if (Data.windFrequency == 0)
        {
            WindDirStacker = 0;
        }
        else
        {
            WindDirStacker += Data.windFrequency;
        }
    }

    public void UpdateCloth()
    {
        var clothVertices = ClothVertices;
        var collisionData = ClothCollisionData;

        if (clothVertices.GetLength(1) <= 5)
        {
            return;
        }

        var rot = Vector2.one;
        var conRad = 7.0f;

        for (var i = 0; i < clothVertices.GetLength(0); i++)
        {
            var t = i / (float)(clothVertices.GetLength(0) - 1);

            clothVertices[i, 1] = clothVertices[i, 0];
            clothVertices[i, 0] += clothVertices[i, 2];
            clothVertices[i, 2] -= rot * Mathf.InverseLerp(1f, 0f, i) * 0.8f;
            clothVertices[i, 4] = clothVertices[i, 3];
            clothVertices[i, 3] = (clothVertices[i, 3] + clothVertices[i, 5] * Custom.LerpMap(Vector2.Distance(clothVertices[i, 0], clothVertices[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
            clothVertices[i, 5] = (clothVertices[i, 5] + Custom.RNV() * Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(clothVertices[i, 0], clothVertices[i, 1])), 0.3f)).normalized;

            if (room.PointSubmerged(clothVertices[i, 0]))
            {
                clothVertices[i, 2] *= Custom.LerpMap(clothVertices[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                clothVertices[i, 2].y += 0.05f;
                clothVertices[i, 2] += Custom.RNV() * 0.1f;
            }
            else
            {
                clothVertices[i, 2] *= Custom.LerpMap(Vector2.Distance(clothVertices[i, 0], clothVertices[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                clothVertices[i, 2].y -= room.gravity * Custom.LerpMap(Vector2.Distance(clothVertices[i, 0], clothVertices[i, 1]), 1f, 6f, 0.6f, 0f);

                if (i == 0)
                {
                    clothVertices[i, 2] = Vector2.zero;
                }
                else if (Data.hasWind)
                {
                    var windDir = Custom.DegToVec(Data.windDir);

                    clothVertices[i, 2] += windDir * Data.windSpeed;
                    clothVertices[i, 2] += Custom.PerpendicularVector(windDir) * Mathf.Sin(WindDirStacker + i * Data.windWavelength) * Data.windAmplitude;
                }

                if (i % 3 != 2 && i != clothVertices.GetLength(0) - 1)
                {
                    continue;
                }

                if (!Data.hasCollisions)
                {
                    continue;
                }

                var terrainCollisionData = collisionData.Set(clothVertices[i, 0], clothVertices[i, 1], clothVertices[i, 2], 1f, new IntVector2(0, 0), false);

                terrainCollisionData = SharedPhysics.HorizontalCollision(room, terrainCollisionData);
                terrainCollisionData = SharedPhysics.VerticalCollision(room, terrainCollisionData);
                terrainCollisionData = SharedPhysics.SlopesVertically(room, terrainCollisionData);

                clothVertices[i, 0] = terrainCollisionData.pos;
                clothVertices[i, 2] = terrainCollisionData.vel;

                if (terrainCollisionData.contactPoint.x != 0)
                {
                    clothVertices[i, 2].y *= 0.6f;
                }

                if (terrainCollisionData.contactPoint.y != 0)
                {
                    clothVertices[i, 2].x *= 0.6f;
                }
            }
        }

        for (var i = 0; i < clothVertices.GetLength(0); i++)
        {
            if (i > 2)
            {
                var normalized = (clothVertices[i, 0] - clothVertices[i - 1, 0]).normalized;
                var num = Vector2.Distance(clothVertices[i, 0], clothVertices[i - 1, 0]);
                var d = (num > conRad) ? 0.5f : 0.25f;
                clothVertices[i, 0] += normalized * (conRad - num) * d;
                clothVertices[i, 2] += normalized * (conRad - num) * d;
                clothVertices[i - 1, 0] -= normalized * (conRad - num) * d;
                clothVertices[i - 1, 2] -= normalized * (conRad - num) * d;

                if (i > 3)
                {
                    normalized = (clothVertices[i, 0] - clothVertices[i - 2, 0]).normalized;
                    clothVertices[i, 2] += normalized * 0.2f;
                    clothVertices[i - 2, 2] -= normalized * 0.2f;
                }

                if (i < clothVertices.GetLength(0) - 1)
                {
                    clothVertices[i, 3] = Vector3.Slerp(clothVertices[i, 3], (clothVertices[i - 1, 3] * 2f + clothVertices[i + 1, 3]) / 3f, 0.1f);
                    clothVertices[i, 5] = Vector3.Slerp(clothVertices[i, 5], (clothVertices[i - 1, 5] * 2f + clothVertices[i + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(clothVertices[i, 1], clothVertices[i, 0]), 1f, 8f, 0.05f, 0.5f));
                }
            }
            else
            {
                clothVertices[i, 0] = ClothPos;
                clothVertices[i, 2] *= 0f;
            }
        }
    }


    public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        var spriteIndex = 0;

        PoleSprite = spriteIndex++;
        ClothSprite = spriteIndex++;
        PoleClothSprite = spriteIndex++;

        sLeaser.sprites = new FSprite[spriteIndex];

        sLeaser.sprites[PoleClothSprite] = new FSprite("SpearRag");

        sLeaser.sprites[PoleSprite] = TriangleMesh.MakeLongMesh(PoleVertices.GetLength(0), false, false);

        sLeaser.sprites[ClothSprite] = TriangleMesh.MakeLongMesh(ClothVertices.GetLength(0), false, false);
        // sLeaser.sprites[ClothSprite].shader = Utils.RainWorld.Shaders["JaggedSquare"];

        AddToContainer(sLeaser, rCam, null);
    }

    public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer? newContainer)
    {
        newContainer ??= rCam.ReturnFContainer("Midground");

        foreach (var sprite in sLeaser.sprites)
        {
            newContainer.AddChild(sprite);
        }
    }


    public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        if (slatedForDeletetion || room != rCam.room)
        {
            sLeaser.CleanSpritesAndRemove();
            return;
        }

        if (Data.clothLength != ClothVertices.GetLength(0) || Data.clothWidth != ClothVertices.GetLength(1))
        {
            ClothVertices = new Vector2[Data.clothLength, Data.clothWidth];
            sLeaser.RemoveAllSpritesFromContainer();
            InitiateSprites(sLeaser, rCam);
        }

        DrawPole(sLeaser, camPos);
        DrawCloth(sLeaser, timeStacker, camPos);
    }

    private void DrawPole(RoomCamera.SpriteLeaser sLeaser, Vector2 camPos)
    {
        var poleSprite = (TriangleMesh)sLeaser.sprites[PoleSprite];
        var poleClothSprite = sLeaser.sprites[PoleClothSprite];

        if (!Data.hasPole)
        {
            poleSprite.isVisible = false;
            poleClothSprite.isVisible = false;
            return;
        }

        poleSprite.isVisible = true;
        poleClothSprite.isVisible = true;

        var endPos = PoleEndPos;
        var startPos = PoleStartPos;
        var clothpos = ClothPos;

        // Stolen from LanternStick, thanks Joar
        var num = 1f;

        for (var i = PoleVertices.Length - 1; i >= 0; i--)
        {
            var perc = 1.0f - (i / (float)(PoleVertices.Length - 1));
            var num3 = Mathf.Lerp(1f + Mathf.Min(Data.polePos.magnitude / 190f, 3f), 0.5f, perc);
            var vector6 = Vector2.Lerp(endPos, startPos, perc) + PoleVertices[i] * Mathf.Lerp(num3 * 0.6f, 1f, perc);
            var normalized = (clothpos - vector6).normalized;
            var vector7 = Custom.PerpendicularVector(normalized);
            var num4 = Vector2.Distance(clothpos, vector6) / 5f;

            poleSprite.MoveVertice(i * 4, clothpos - normalized * num4 - vector7 * (num3 + num) * 0.5f - camPos);
            poleSprite.MoveVertice(i * 4 + 1, clothpos - normalized * num4 + vector7 * (num3 + num) * 0.5f - camPos);
            poleSprite.MoveVertice(i * 4 + 2, vector6 + normalized * num4 - vector7 * num3 - camPos);
            poleSprite.MoveVertice(i * 4 + 3, vector6 + normalized * num4 + vector7 * num3 - camPos);
            clothpos = vector6;
            num = num3;
        }

        poleClothSprite.SetPosition(PoleClothPos - camPos);
        poleClothSprite.rotation = Custom.VecToDeg(Custom.DirVec(PoleStartPos, PoleEndPos));
        poleClothSprite.color = Data.clothColor;

    }

    private void DrawCloth(RoomCamera.SpriteLeaser sLeaser, float timeStacker, Vector2 camPos)
    {
        var clothPos = ClothPos;

        var clothSprite = (TriangleMesh)sLeaser.sprites[ClothSprite];
        var clothVertices = ClothVertices;

        clothSprite.color = Data.clothColor;

        if (clothVertices.GetLength(1) <= 5)
        {
            return;
        }

        var prevRot = 0.0f;

        for (var i = 0; i < clothVertices.GetLength(0); i++)
        {
            var index = (float)i / (clothVertices.GetLength(0) - 1);
            var pos = Vector2.Lerp(clothVertices[i, 1], clothVertices[i, 0], timeStacker);

            var rot = (2f + 2f * Mathf.Sin(Mathf.Pow(index, 2f) * Mathf.PI)) * Vector3.Slerp(clothVertices[i, 4], clothVertices[i, 3], timeStacker).x;

            var normalized = (clothPos - pos).normalized;
            var perp = Custom.PerpendicularVector(normalized);

            var dist = Vector2.Distance(clothPos, pos) / 5f;

            clothSprite.MoveVertice(i * 4, clothPos - normalized * dist - perp * (rot + prevRot) * 0.5f - camPos);
            clothSprite.MoveVertice(i * 4 + 1, clothPos - normalized * dist + perp * (rot + prevRot) * 0.5f - camPos);
            clothSprite.MoveVertice(i * 4 + 2, pos + normalized * dist - perp * rot - camPos);
            clothSprite.MoveVertice(i * 4 + 3, pos + normalized * dist + perp * rot - camPos);

            prevRot = rot;
            clothPos = pos;
        }

        // Color & UV Map
        if (clothSprite.verticeColors == null || clothSprite.verticeColors.Length != clothSprite.vertices.Length)
        {
            clothSprite.verticeColors = new Color[clothSprite.vertices.Length];
        }

        for (var i = clothSprite.verticeColors.Length - 1; i >= 0; i--)
        {
            var halfIndex = i / 2;
            var perc = halfIndex / (clothSprite.verticeColors.Length / 2.0f);

            Vector2 uvInterpolation;

            // Last Vertex
            if (i == clothSprite.verticeColors.Length - 1)
            {
                uvInterpolation = new Vector2(1.0f, 0.0f);
            }
            // Even Vertices
            else if (i % 2 == 0)
            {
                uvInterpolation = new Vector2(perc, 0.0f);
            }
            // Odd Vertices
            else
            {
                uvInterpolation = new Vector2(perc, 1.0f);
            }

            var x = Mathf.Lerp(clothSprite.element.uvBottomLeft.x, clothSprite.element.uvTopRight.x, uvInterpolation.x);
            var y = Mathf.Lerp(clothSprite.element.uvBottomLeft.y, clothSprite.element.uvTopRight.y, uvInterpolation.y);

            clothSprite.UVvertices[i] = new(x, y);
        }
    }


    public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
    {
        var poleSprite = sLeaser.sprites[PoleSprite];

        poleSprite.color = palette.blackColor;
    }
}

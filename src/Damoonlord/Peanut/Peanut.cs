using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Raincord100k.Damoonlord.Peanut
{
    sealed class Peanut : Weapon, IPlayerEdible
    {
        public class PeanutFragment : CosmeticSprite
        {
            public float rotation;

            public float lastRotation;

            public float rotVel;

            public Color color;

            public PeanutFragment(Vector2 pos, Vector2 vel, Color col)
            {
                color = col;
                base.pos = pos + vel * 2f;
                lastPos = pos;
                base.vel = vel;
                rotation = Random.value * 360f;
                lastRotation = rotation;
                rotVel = Mathf.Lerp(-26f, 26f, Random.value);
            }

            public override void Update(bool eu)
            {
                vel *= 0.998f;
                vel.y -= room.gravity * 0.9f;
                lastRotation = rotation;
                rotation += rotVel * vel.magnitude;
                if (Vector2.Distance(lastPos, pos) > 18f && room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
                {
                    IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
                    FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
                    pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
                    bool flag = false;
                    if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
                    {
                        vel.x = Mathf.Abs(vel.x) * Mathf.Lerp(0.15f, 0.7f, Random.value);
                        flag = true;
                    }
                    else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
                    {
                        vel.x = (0f - Mathf.Abs(vel.x)) * Mathf.Lerp(0.15f, 0.7f, Random.value);
                        flag = true;
                    }
                    else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
                    {
                        vel.y = Mathf.Abs(vel.y) * Mathf.Lerp(0.15f, 0.7f, Random.value);
                        flag = true;
                    }
                    else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
                    {
                        vel.y = (0f - Mathf.Abs(vel.y)) * Mathf.Lerp(0.15f, 0.7f, Random.value);
                        flag = true;
                    }

                    if (flag)
                    {
                        rotVel *= 0.8f;
                        rotVel += Mathf.Lerp(-1f, 1f, Random.value) * 4f * Random.value;
                    }
                }

                if (room.GetTile(pos).Solid && room.GetTile(lastPos).Solid || pos.y < -100f)
                {
                    Destroy();
                }

                base.Update(eu);
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[1];
                sLeaser.sprites[0] = new FSprite("PeanutBit");
                AddToContainer(sLeaser, rCam, null);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
                sLeaser.sprites[0].x = vector.x - camPos.x;
                sLeaser.sprites[0].y = vector.y - camPos.y;
                sLeaser.sprites[0].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
            }

            public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
            {
                sLeaser.sprites[0].color = Color.Lerp(color, Color.Lerp(palette.fogColor, palette.blackColor, 0.5f), 0.2f);
            }

            public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
                base.AddToContainer(sLeaser, rCam, newContatiner);
            }
        }

        public AbstractPeanut Abstract { get; }

        public int bites = 3;
        public int BitesLeft => bites;

        public int ExplodeCooldown = 100;
        public int ThrowCooldown;
        public bool Grounded
        {
            get
            {
                return Abstract.Grounded;
            }
            set
            {
                Abstract.Grounded = value;
            }
        }

        public Vector2 GroundedPos;
        public int PullCounter = 300;

        public int SuperPulseCounter;
        public bool SuperPulseCounterFlip = false;

        private int GoldenShift;
        private bool GoldenShiftFlip = false;

        public int FoodPoints => 1;
        public bool Edible => !Grounded;
        public bool AutomaticPickUp => false;

        new public float rotation;
        new public float lastRotation;

        public Smoke.BombSmoke smoke;
        public StaticSoundLoop pullingSoundLoop;
        private StaticSoundLoop GoldenPeanutSong;
        public Color explodeColor
        {
            get
            {
                return Custom.HSL2RGB(Abstract.Hue, Abstract.Sat, Abstract.Lit * 0.8f); 
            }
        }

        public float rotVel;
        public float lastDarkness = -1f;

        public Peanut(AbstractPeanut abstractpeanut, Vector2 pos, Vector2 vel) : base(abstractpeanut, abstractpeanut.world)
        {
            Abstract = abstractpeanut;

            ExplodeCooldown = 100;

            bodyChunks = new[] { new BodyChunk(this, 0, pos + vel, 5, 0.35f) { goThroughFloors = true } };
            bodyChunks[0].lastPos = bodyChunks[0].pos;
            bodyChunks[0].vel = vel;

            bodyChunkConnections = new BodyChunkConnection[0];
            airFriction = 0.999f;
            gravity = Abstract.Golden ? 0.6f : 0.9f;
            bounce = 0.4f;
            surfaceFriction = 0.4f;
            collisionLayer = 2;
            waterFriction = 0.98f;
            buoyancy = 0.4f;
            firstChunk.loudness = 9f;

            rotation = Random.value * 360f;
            lastRotation = rotation;

            Grounded = Abstract.Grounded;

            ResetVel(vel.magnitude);
        }
        public override void PlaceInRoom(Room placeRoom)
        {
            base.PlaceInRoom(placeRoom);

            if (Abstract.Grounded)
            {
                GroundedPos = new Vector2(firstChunk.pos.x, firstChunk.pos.y - 7.5f);
            }
        }

        public void GroundedUpdate()
        {
            if (!Grounded && !Abstract.isConsumed)
            {
                Abstract.Consume();
            }

            if (pullingSoundLoop == null && Grounded)
            {
                pullingSoundLoop = new StaticSoundLoop(SoundID.Big_Needle_Worm_Attack_Charge_Wings_LOOP, GroundedPos, room, 0f, 1.6f);
            }

            if (pullingSoundLoop != null) { pullingSoundLoop.Update(); }

            if (Grounded)
            {
                firstChunk.pos = GroundedPos;

                if (grabbedBy.Count == 0)
                {
                    if (PullCounter > 0)
                    {
                        firstChunk.vel = Vector2.zero;
                        PullCounter = 300;
                    }
                }
                else
                {
                    if (PullCounter > 0)
                    {
                        firstChunk.vel += firstChunk.vel * 0.2f;

                        if (Mathf.Abs(firstChunk.vel.x) > 3f)
                        {
                            if (grabbedBy[0].grabber is Player player && player.input[0].jmp && !player.input[1].jmp)
                            {
                                PullCounter -= 50;
                            }
                            else
                            {
                                PullCounter--;
                            }
                        }
                        else if (PullCounter < 300)
                        {
                            PullCounter++;
                        }
                    }
                    else if (Mathf.Abs(firstChunk.vel.x) > 3f)
                    {
                        pullingSoundLoop.volume = 0f;
                        Grounded = false;
                        room.PlaySound(SoundID.Snail_Pop, firstChunk, false, 0.4f, 0.7f);
                    }
                }
            }

            if (pullingSoundLoop != null)
            {
                if (PullCounter > 0f)
                {
                    pullingSoundLoop.volume = Mathf.Lerp(pullingSoundLoop.volume, Mathf.Clamp01(Mathf.Abs(firstChunk.vel.x) / 6f), 0.2f);
                }
                else
                {
                    pullingSoundLoop.volume = Mathf.Lerp(pullingSoundLoop.volume, 0f, 0.02f);
                }
            }

            firstChunk.vel = Grounded ? Vector2.zero : firstChunk.vel;
        }

        private void GoldenUpdate()
        {
            if (!Abstract.Golden) { return; }

            if (GoldenPeanutSong == null)
            {
                GoldenPeanutSong = new StaticSoundLoop(SoundID.Void_Sea_Worms_Bkg_LOOP, firstChunk.pos, room, 0f, 1f);
            }

            if (GoldenPeanutSong != null)
            {
                GoldenPeanutSong.Update();

                Player closetPlayer = room.PlayersInRoom[0]; // closet player :leditoroverload:

                for (int i = 1; i < room.PlayersInRoom.Count; i++)
                {
                    if (Custom.VectorIsCloser(closetPlayer.mainBodyChunk.pos, room.PlayersInRoom[i].mainBodyChunk.pos, firstChunk.pos))
                    {
                        closetPlayer = room.PlayersInRoom[i];
                    }
                }

                if (Custom.DistLess(closetPlayer.mainBodyChunk.pos, firstChunk.pos, 600f))
                {
                    GoldenPeanutSong.volume = Mathf.Lerp(0.2f, 0f, Custom.Dist(firstChunk.pos, closetPlayer.mainBodyChunk.pos) / 600f);
                }

            }
        }

        public override void Update(bool eu)
        {
            int l = Abstract.SuperPeanut ? 1 : 2;
            l = ThrowCooldown == 0 ? l : 2;
            ChangeCollisionLayer(grabbedBy.Count == 0 ? l : 1);
            firstChunk.collideWithObjects = !Grounded;
            firstChunk.collideWithTerrain = grabbedBy.Count == 0;
            firstChunk.collideWithSlopes = grabbedBy.Count == 0;

            base.Update(eu);

            var chunk = firstChunk;

            lastRotation = rotation;
            rotation += rotVel * Vector2.Distance(chunk.lastPos, chunk.pos);

            rotation %= 360;

            if (grabbedBy.Count == 0)
            {
                if (ExplodeCooldown > 0)
                {
                    ExplodeCooldown--;
                }

                if (ThrowCooldown > 0)
                {
                    ThrowCooldown--;
                }

                if (firstChunk.lastPos == firstChunk.pos)
                {
                    rotVel *= 0.9f;
                }
                else if (Mathf.Abs(rotVel) <= 0.01f)
                {
                    ResetVel((firstChunk.lastPos - firstChunk.pos).magnitude);
                }
            }
            else
            {
                var grabberChunk = grabbedBy[0].grabber.mainBodyChunk;
                rotVel *= 0.9f;
                rotation = Mathf.Lerp(rotation, grabberChunk.Rotation.GetAngle(), 0.1f);
            }

            GroundedUpdate();
            GoldenUpdate();

            if (soundLoop != null)
            {
                soundLoop.sound = SoundID.None;
                if (firstChunk.vel.magnitude > 5f)
                {
                    if (firstChunk.ContactPoint.y < 0)
                    {
                        soundLoop.sound = SoundID.Rock_Skidding_On_Ground_LOOP;
                    }
                    else
                    {
                        soundLoop.sound = SoundID.Rock_Through_Air_LOOP;
                    }

                    soundLoop.Volume = Mathf.InverseLerp(5f, 15f, firstChunk.vel.magnitude);
                }

                soundLoop.Update();
            }

            if (firstChunk.ContactPoint.y != 0)
            {
                rotationSpeed = (rotationSpeed + firstChunk.vel.x * 5f) / 3f;
            }
        }

        public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
        {
            base.Collide(otherObject, myChunk, otherChunk);

            if (ExplodeCooldown == 0 && Abstract.SuperPeanut && BitesLeft == 3 && otherObject is Creature && grabbedBy.Count == 0)
            {
                Explode(otherObject.bodyChunks[otherChunk]);
            }
        }

        public override void Grabbed(Creature.Grasp grasp)
        {
            base.Grabbed(grasp);
            ExplodeCooldown = 100;
        }
        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            if (Grounded)
            {
                return;
            }

            ExplodeCooldown = 0;
            ThrowCooldown = 5;
            base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);

            if (throwDir.x != 0)
            {
                firstChunk.vel.x *= Mathf.Lerp(0.2f, 1f, BitesLeft / 3);
            }
            else
            {
                firstChunk.vel.y *= Mathf.Lerp(0.2f, 0.8f, BitesLeft / 3);

                if (throwDir.y == -1)
                {
                    for (int b = 0; b < thrownBy.bodyChunks.Length; b++)
                    {
                        thrownBy.bodyChunks[b].vel.y += (BitesLeft == 3 ? 1f : 0.5f) * frc * (Abstract.SuperPeanut ? 30f : 17.5f);
                        room.PlaySound(PeanutMeta.Peanut_Jump, firstChunk, false, 1f, Abstract.SuperPeanut ? 1.6f : (Abstract.Golden ? 4f : 1f));
                    }
                }
            }
        }

        public void BitByPlayer(Creature.Grasp grasp, bool eu)
        {
            if (bites > 1)
            {
                room.PlaySound(DLCSharedEnums.SharedSoundID.Duck_Pop, grasp.grabber.mainBodyChunk, loop: false, 0.4f, 0.4f + Random.value * 0.5f);
                for (int i = 0; i < bites - 1; i++)
                {
                    room.AddObject(new PeanutFragment(firstChunk.pos, Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(2f, 6f, Random.value), Custom.HSL2RGB(Abstract.Hue, Abstract.Sat, Abstract.Lit)));
                }
            }

            bites--;
            if (bites == 0)
            {
                room.PlaySound(SoundID.Slugcat_Eat_Dangle_Fruit, firstChunk);
            }
            firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
            if (bites < 1)
            {
                (grasp.grabber as Player).ObjectEaten(this);
                grasp.Release();
                Destroy();
            }
        }

        public void ThrowByPlayer()
        {
        }

        public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
        {
            if (result.obj == null)
            {
                return false;
            }

            if (result.obj.abstractPhysicalObject.rippleLayer != abstractPhysicalObject.rippleLayer && !result.obj.abstractPhysicalObject.rippleBothSides && !abstractPhysicalObject.rippleBothSides)
            {
                return false;
            }

            if (BitesLeft == 0)
            {
                return false;
            }

            if (thrownBy is Scavenger && (thrownBy as Scavenger).AI != null)
            {
                (thrownBy as Scavenger).AI.HitAnObjectWithWeapon(this, result.obj);
            }

            vibrate = 20;
            if (result.obj is Creature)
            {
                float stunBonus = 45f;
                if (ModManager.MMF && MMF.cfgIncreaseStuns.Value && (result.obj is Cicada || result.obj is LanternMouse || ModManager.MSC && result.obj is Yeek))
                {
                    stunBonus = BitesLeft == 3 ? 90f : 40f;
                }

                if (ModManager.MSC && room.game.IsArenaSession && room.game.GetArenaGameSession.chMeta != null)
                {
                    stunBonus = BitesLeft == 3 ? 90f : 40f;
                }

                (result.obj as Creature).Violence(firstChunk, firstChunk.vel * firstChunk.mass * (BitesLeft / 3), result.chunk, result.onAppendagePos, Creature.DamageType.Blunt, 0.1f * BitesLeft, stunBonus);
                if (ExplodeCooldown == 0 && Abstract.SuperPeanut && BitesLeft == 3)
                {
                    Explode(result.chunk);
                }
            }
            else if (result.chunk != null)
            {
                result.chunk.vel += firstChunk.vel * firstChunk.mass / result.chunk.mass * (BitesLeft / 3);
            }
            else if (result.onAppendagePos != null)
            {
                (result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, firstChunk.vel * firstChunk.mass * (BitesLeft / 3));
            }

            ChangeMode(Mode.Free);
            firstChunk.vel = firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * firstChunk.vel.magnitude;
            if (result.obj is Creature)
            {
                room.PlaySound(SoundID.Rock_Hit_Creature, firstChunk);
            }
            if (result.chunk != null)
            {
                room.AddObject(new ExplosionSpikes(room, result.chunk.pos + Custom.DirVec(result.chunk.pos, result.collisionPoint) * result.chunk.rad, 5, 2f, 4f, 4.5f, 30f, new Color(1f, 1f, 1f, 0.5f)));
            }

            SetRandomSpin();
            return true;
        }

        public void Explode(BodyChunk hitChunk)
        {
            if (slatedForDeletetion)
            {
                return;
            }

            Vector2 vector = Vector2.Lerp(firstChunk.pos, firstChunk.lastPos, 0.35f);
            room.AddObject(new SootMark(room, vector, 80f, bigSprite: true));
            room.AddObject(new Explosion(room, this, vector, 7, 250f, 6.2f, 2f, 280f, 0.25f, thrownBy, 0.7f, 160f, 1f));

            room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
            room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            room.AddObject(new ExplosionSpikes(room, vector, 14, 30f, 9f, 7f, 170f, explodeColor));
            room.AddObject(new ShockWave(vector, 330f, 0.045f, 5));
            for (int i = 0; i < 25; i++)
            {
                Vector2 vector2 = Custom.RNV();
                if (room.GetTile(vector + vector2 * 20f).Solid)
                {
                    if (!room.GetTile(vector - vector2 * 20f).Solid)
                    {
                        vector2 *= -1f;
                    }
                    else
                    {
                        vector2 = Custom.RNV();
                    }
                }

                for (int j = 0; j < 3; j++)
                {
                    room.AddObject(new Spark(vector + vector2 * Mathf.Lerp(30f, 60f, Random.value), vector2 * Mathf.Lerp(7f, 38f, Random.value) + Custom.RNV() * 20f * Random.value, Color.Lerp(explodeColor, new Color(1f, 1f, 1f), Random.value), null, 11, 28));
                }

                room.AddObject(new Explosion.FlashingSmoke(vector + vector2 * 40f * Random.value, vector2 * Mathf.Lerp(4f, 20f, Mathf.Pow(Random.value, 2f)), 1f + 0.05f * Random.value, new Color(1f, 1f, 1f), explodeColor, Random.Range(3, 11)));
            }

            if (smoke != null)
            {
                for (int k = 0; k < 8; k++)
                {
                    smoke.EmitWithMyLifeTime(vector + Custom.RNV(), Custom.RNV() * Random.value * 17f);
                }
            }

            for (int l = 0; l < 5; l++)
            {
                room.AddObject(new PeanutFragment(vector, Custom.DegToVec((l + Random.value) / 6f * 360f) * Mathf.Lerp(18f, 38f, Random.value), Custom.HSL2RGB(Abstract.Hue, Abstract.Sat, Abstract.Lit)));
            }

            room.ScreenMovement(vector, default, 1.3f);
            for (int m = 0; m < abstractPhysicalObject.stuckObjects.Count; m++)
            {
                abstractPhysicalObject.stuckObjects[m].Deactivate();
            }

            room.PlaySound(SoundID.Bomb_Explode, vector, abstractPhysicalObject);
            room.InGameNoise(new Noise.InGameNoise(vector, 9000f, this, 1f));
            bool flag = hitChunk != null;
            for (int n = 0; n < 5; n++)
            {
                if (room.GetTile(vector + Custom.fourDirectionsAndZero[n].ToVector2() * 20f).Solid)
                {
                    flag = true;
                    break;
                }
            }

            if (flag)
            {
                if (smoke == null)
                {
                    smoke = new Smoke.BombSmoke(room, vector, null, explodeColor);
                    room.AddObject(smoke);
                }

                if (hitChunk != null)
                {
                    smoke.chunk = hitChunk;
                }
                else
                {
                    smoke.chunk = null;
                    smoke.fadeIn = 1f;
                }

                smoke.pos = vector;
                smoke.stationary = true;
                smoke.DisconnectSmoke();
            }
            else if (smoke != null)
            {
                smoke.Destroy();
            }

            Destroy();
        }

        private void ResetVel(float speed)
        {
            rotVel = Mathf.Lerp(-1f, 1f, Random.value) * Custom.LerpMap(speed, 0f, 18f, 5f, 26f);
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[Abstract.Golden ? 6 : 3];
            sLeaser.sprites[0] = new FSprite("PeanutShellA");
            sLeaser.sprites[1] = new FSprite("PeanutShellB");
            sLeaser.sprites[2] = new FSprite("PeanutBit");

            if (Abstract.Golden)
            {
                sLeaser.sprites[3] = new FSprite("Futile_White");
                sLeaser.sprites[3].shader = rCam.game.rainWorld.Shaders["LocalBloom"];

                sLeaser.sprites[4] = new FSprite("Futile_White");
                sLeaser.sprites[4].shader = rCam.game.rainWorld.Shaders["WarpTear"];

                sLeaser.sprites[5] = new FSprite("Futile_White");
                sLeaser.sprites[5].shader = rCam.game.rainWorld.Shaders["RippleHybrid"];
            }

            sLeaser.sprites[2].scale = 1.2f;

            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 pos = Vector2.Lerp(firstChunk.lastPos, firstChunk.pos, timeStacker);

            for (int i = 0; i < sLeaser.sprites.Length; i++)
            {
                sLeaser.sprites[i].x = pos.x - camPos.x;
                sLeaser.sprites[i].y = pos.y - camPos.y;
                if (!Abstract.Golden || i != sLeaser.sprites.Length - 2)
                {
                    sLeaser.sprites[i].rotation = Mathf.Lerp(lastRotation, rotation, timeStacker);
                }
            }

            sLeaser.sprites[0].isVisible = BitesLeft > 2;
            sLeaser.sprites[1].isVisible = BitesLeft > 1;
            sLeaser.sprites[2].isVisible = BitesLeft > 0 && BitesLeft < 3;

            if (Abstract.Golden)
            {
                sLeaser.sprites[3].scale = Mathf.Lerp(2.2f, 2.4f, Mathf.Clamp01(BitesLeft));
                sLeaser.sprites[4].scale = Mathf.Lerp(2f, 3.2f, Mathf.Clamp01(BitesLeft));
                sLeaser.sprites[5].scale = Mathf.Lerp(2f, 3.2f, Mathf.Clamp01(BitesLeft));
                sLeaser.sprites[4].color = new Color(1f, 1f, 0.25f);
            }

            if (ExplodeCooldown == 0 && BitesLeft == 3 && Abstract.SuperPeanut)
            {
                if (SuperPulseCounter == 0)
                {
                    SuperPulseCounterFlip = true;
                }
                else if (SuperPulseCounter == 5)
                {
                    SuperPulseCounterFlip = false;
                }

                if (SuperPulseCounterFlip)
                {
                    SuperPulseCounter += (int)Unity.Mathematics.math.round(Mathf.Lerp(1, 3, UnityEngine.Random.value));
                }
                else
                {
                    SuperPulseCounter -= (int)Unity.Mathematics.math.round(Mathf.Lerp(1, 3, UnityEngine.Random.value));
                }

                SuperPulseCounter = Unity.Mathematics.math.clamp(SuperPulseCounter, 0, 5);

                for (int i = 0; i < sLeaser.sprites.Length - 1; i++)
                {
                    sLeaser.sprites[i].scale = Mathf.Lerp(1f, 1.1f, SuperPulseCounter / 5f);
                }
            }

            if (Grounded)
            {
                float dir = 1f;

                if (grabbedBy.Count > 0)
                {
                    dir = Custom.AimFromOneVectorToAnother(grabbedBy[0].grabber.mainBodyChunk.pos, firstChunk.pos);
                }

                sLeaser.sprites[0].rotation = -45f + dir;
                sLeaser.sprites[0].y -= 2f;
                sLeaser.sprites[1].rotation = -45f + dir;
                sLeaser.sprites[1].y -= 2f;

                sLeaser.sprites[2].isVisible = false;
            }

            if (slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            float hue = Abstract.Hue;
            float sat = Abstract.Sat;

            if (Abstract.Golden)
            {
                if (GoldenShift == 0)
                {
                    GoldenShiftFlip = true;
                }
                else if (GoldenShift == 100)
                {
                    GoldenShiftFlip = false;
                }

                if (GoldenShiftFlip)
                {
                    GoldenShift++;
                }
                else
                {
                    GoldenShift--;
                }

                GoldenShift = Unity.Mathematics.math.clamp(GoldenShift, 0, 5);

                hue = Mathf.Lerp(0.05f, 0.10f, GoldenShift / 100f);
                sat = Mathf.Lerp(0.68f, 0.73f, GoldenShift / 100f);
            }

            for (int i = 0; i < sLeaser.sprites.Length - (Abstract.Golden ? 4 : 1); i++)
            {
                sLeaser.sprites[i].color = Color.Lerp(Custom.HSL2RGB(Abstract.Hue, Abstract.Sat, Abstract.Lit), Color.Lerp(palette.fogColor, palette.blackColor, 0.5f), 0.2f);
            }

            sLeaser.sprites[2].color = Color.Lerp(Custom.HSL2RGB(Abstract.Hue, Abstract.Sat * 2f, Abstract.Lit * (Abstract.SuperPeanut ? 1.6f : 1.2f)), Color.Lerp(palette.fogColor, palette.blackColor, 0.5f), 0.2f);
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContainer)
        {
            newContainer ??= rCam.ReturnFContainer("Items");

            if (!Abstract.Golden)
            {
                foreach (FSprite fsprite in sLeaser.sprites)
                {
                    fsprite.RemoveFromContainer();
                    newContainer.AddChild(fsprite);
                }
            }
            else
            {
                for (int i = 0; i < sLeaser.sprites.Length - 3; i++)
                {
                    sLeaser.sprites[i].RemoveFromContainer();
                    newContainer.AddChild(sLeaser.sprites[i]);
                }

                newContainer ??= rCam.ReturnFContainer("Foreground");
                for (int i = 3; i < sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i].RemoveFromContainer();
                    rCam.room.game.cameras[0].ReturnFContainer("Foreground").AddChild(sLeaser.sprites[i]);
                }
            }
        }
    }
}

using Menu.Remix.MixedUI;
using Raincord100k.Pearls;
using UnityEngine;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace Raincord100k.Remix
{
    public class OpPearlButton : OpSimpleButton
    {
        public FSprite pearlSprite;
        public FSprite readerSprite;

        public PearlType pearlType;
        public PearlHologram.Reader reader;
        public Color pearlColor;
        public Color readerColor;

        public OpPearlButton(Vector2 pos, PearlType pearlType, PearlHologram.Reader? reader) : base(pos, new Vector2(reader != null ? 70f : 30f, 30f), "")
        {
            pearlSprite = new FSprite(Futile.atlasManager.GetElementWithName("Symbol_Pearl"), true);
            myContainer.AddChild(pearlSprite);
            pearlSprite.SetAnchor(0.5f, 0.5f);
            if (reader == null)
            {
                pearlSprite.SetPosition(size.x / 2f, size.y / 2f);
            }
            else
            {
                pearlSprite.SetPosition(size.x / 4f, size.y / 2f);

                readerSprite = new FSprite(PearlHologram.SpriteForReader(reader.Value));
                myContainer.AddChild(readerSprite);
                readerSprite.SetAnchor(0.5f, 0.5f);
                readerSprite.SetPosition(size.x * 3f / 4f, size.y / 2f);
            }

            this.pearlType = pearlType;
            this.reader = reader ?? PearlHologram.Reader.MoonPostCollapse;

            pearlColor = DataPearl.UniquePearlMainColor(this.pearlType);
            readerColor = PearlHologram.ColorForReader(this.reader);
        }

        public override void Change()
        {
            base.Change();
            if (readerSprite == null)
            {
                pearlSprite.SetPosition(size.x / 2f, size.y / 2f);
            }
            else
            {
                pearlSprite.SetPosition(size.x * 1f / 4f, size.y / 2f);
                readerSprite.SetPosition(size.x * 3f / 4f, size.y / 2f);
            }
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            pearlSprite.color = bumpBehav.GetColor(pearlColor);
            if (readerSprite != null)
            {
                readerSprite.color = bumpBehav.GetColor(readerColor);
            }
        }
    }
}

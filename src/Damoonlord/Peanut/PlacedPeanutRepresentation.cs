using DevInterface;
using UnityEngine;

namespace Raincord100k.Damoonlord.Peanut
{
    internal class PlacedPeanutRepresentation : ConsumableRepresentation
    {
        public class PlacedPeanutControlPanel : ConsumableControlPanel, IDevUISignals
        {
            public class PeanutSlider : Slider
            {
                new Vector2 pos;
                public PeanutSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
                    : base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
                {
                    this.pos = pos;
                }

                public override void Refresh()
                {
                    base.Refresh();

                    (base.subNodes[0] as DevUILabel).fSprites[0].width = 90f;

                    (base.subNodes[1] as DevUILabel).fSprites[0].width = 35f;
                    (base.subNodes[1] as DevUILabel).fSprites[0].x = absPos.x + 100f;
                    (base.subNodes[1] as DevUILabel).fLabels[0].x = absPos.x + 102.5f;

                    PlacedPeanutData peanutdata = ((parentNode.parentNode as PlacedPeanutRepresentation)!.pObj.data as PlacedPeanutData)!;
                    float nubPos = 0;

                    if (IDstring == "PeanutChanceSlider")
                    {
                        nubPos = peanutdata.PeanutChance;
                        base.NumberText = Mathf.Round(peanutdata.PeanutChance * 100f).ToString() + "%";
                    }
                    else if (IDstring == "SuperPeanutChanceSlider")
                    {
                        nubPos = peanutdata.SuperPeanutChance;
                        base.NumberText = Mathf.Round(peanutdata.SuperPeanutChance * 100f).ToString() + "%";
                    }

                    RefreshNubPos(nubPos);
                }

                public override void NubDragged(float nubPos)
                {
                    if (IDstring == "PeanutChanceSlider")
                    {
                        ((parentNode.parentNode as PlacedPeanutRepresentation)!.pObj.data as PlacedPeanutData)!.PeanutChance = nubPos;
                        ((parentNode.parentNode as PlacedPeanutRepresentation)!.pObj.data as PlacedPeanutData)!.SuperPeanutChance = Mathf.Abs(nubPos - 1);
                    }
                    else if (IDstring == "SuperPeanutChanceSlider")
                    {
                        ((parentNode.parentNode as PlacedPeanutRepresentation)!.pObj.data as PlacedPeanutData)!.SuperPeanutChance = nubPos;
                        ((parentNode.parentNode as PlacedPeanutRepresentation)!.pObj.data as PlacedPeanutData)!.PeanutChance = Mathf.Abs(nubPos - 1);
                    }

                    parentNode.parentNode.Refresh();
                    Refresh();
                }
            }

            public Button GroundedButton;
            public DevUILabel GroundedLabel;
            public PeanutSlider PeanutChanceSlider;
            public PeanutSlider SuperPeanutChanceSlider;

            public FSprite HidePreviewButton;
            public bool HidePreviewLabelOn = false;
            public Vector2 HidePreviewButtonPos => nonCollapsedAbsPos + size + new Vector2(-70f, 10f);

            public PlacedPeanutControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string name) : base(owner, IDstring, parentNode, pos, name)
            {
                size.y = 111f;

                subNodes.Add(GroundedLabel = new DevUILabel(owner, IDstring, this, new Vector2(5f, 51), 90f, "Start Buried:"));

                subNodes.Add(GroundedButton = new Button(owner, "GroundedButton", this, new Vector2(105f, 51f), 140f, ((parentNode as PlacedPeanutRepresentation).pObj.data as PlacedPeanutData).Grounded.ToString()));

                subNodes.Add(SuperPeanutChanceSlider = new PeanutSlider(owner, "SuperPeanutChanceSlider", this, new Vector2(5f, 71f), "Super Chance:")); ;

                subNodes.Add(PeanutChanceSlider = new PeanutSlider(owner, "PeanutChanceSlider", this, new Vector2(5f, 91f), "Peanut Chance:"));

                subNodes.Add(new HorizontalDivider(owner, "Breakline", this, 46f));

                fSprites.Add(HidePreviewButton = new FSprite("Icon_Peanut"));
                HidePreviewButton.scale = 0.75f;
                HidePreviewButton.alpha = 0.75f;
                Futile.stage.AddChild(HidePreviewButton);
            }
            public override void Update()
            {
                base.Update();

                HidePreviewButton.color = new Color(1f, 1f, 1f);
                if (owner != null && new Rect(HidePreviewButtonPos - new Vector2(5f, 5f), new Vector2(10f, 10f)).Contains(owner.mousePos))
                {
                    HidePreviewButton.color = (owner.mouseDown ? new Color(0f, 0f, 1f) : new Color(1f, 0f, 0f));
                    if (owner.mouseClick)
                    {
                        HidePreviewLabelOn = HidePreviewLabelOn ? false : true;
                        HidePreviewButton.alpha = HidePreviewLabelOn ? 0.25f : 0.75f;
                    }
                }
            }
            public override void Refresh()
            {
                base.Refresh();
                if (shiftLabel != -1)
                {
                    MoveLabel(shiftLabel, ShiftLabelPos + new Vector2(-10,0f));
                }

                HidePreviewButton.SetPosition(HidePreviewButtonPos);
                GroundedButton.Text = ((parentNode as PlacedPeanutRepresentation)!.pObj.data as PlacedPeanutData)!.Grounded.ToString();
            }

            public virtual void Signal(DevUISignalType type, DevUINode sender, string message)
            {
                if (sender.IDstring == "GroundedButton")
                {
                    var data = ((parentNode as PlacedPeanutRepresentation)!.pObj.data as PlacedPeanutData)!;
                    data.Grounded = !data.Grounded;
                }

                Refresh();
            }
        }
        public FSprite PeanutPositioner;
        public Vector2 lastpos;

        public PlacedPeanutRepresentation(DevUI owner, DevUINode parentNode, PlacedObject pObj) : base(owner, "PlacedPeanutRepresentation", parentNode, pObj, "Peanut")
        {
            controlPanel.size = default;
            controlPanel.Title = string.Empty;
            controlPanel.ToggleCollapse();
            controlPanel.ClearSprites();
            subNodes.RemoveAt(subNodes.Count - 1);
            subNodes.Add(controlPanel = new PlacedPeanutControlPanel(owner, "PlacedPeanutPanel", this, new(0f, 100f), "Consumable: Peanut") { pos = (pObj.data as PlacedPeanutData).panelPos });

            fSprites.Add(PeanutPositioner = new FSprite("PeanutShellA"));
            PeanutPositioner.rotation = -45f;
            PeanutPositioner.alpha = 0.25f;
            owner.placedObjectsContainer.RemoveChild(fSprites[fSprites.Count - 2]);
            owner.placedObjectsContainer.AddChild(PeanutPositioner);

            fSprites.Add(new FSprite("pixel"));
            owner.placedObjectsContainer.AddChild(fSprites[fSprites.Count - 1]);
            fSprites[fSprites.Count - 1].anchorY = 0f;
        }

        public override void Update()
        {
            base.Update();

            bool valid = true;
            Vector2 peanutPos = new Vector2(0, 0);
            Room room = owner.room;

            bool flag = (pObj.data as PlacedPeanutData).Grounded;

            if (!(subNodes[subNodes.Count - 1] as PlacedPeanutControlPanel).HidePreviewLabelOn || room != null || pos != lastpos || PeanutPositioner.element.name != (flag ? "PeanutShellA" : "Icon_Peanut"))
            {
                int x = room.GetTilePosition(pObj.pos).x;
                int foundTileY = room.GetTilePosition(pObj.pos).y;

                while (foundTileY >= 0)
                {
                    bool invalidtile =
                        room.GetTile(x, foundTileY).AnyWater ||
                        room.GetTile(x, foundTileY).hive ||
                        room.GetTile(x, foundTileY).wormGrass ||
                        room.GetTile(x, foundTileY).Terrain == Room.Tile.TerrainType.Slope ||
                        room.GetTile(x, foundTileY).Terrain == Room.Tile.TerrainType.ShortcutEntrance;

                    if (!room.GetTile(x, foundTileY).Solid && room.GetTile(x, foundTileY - 1).Solid)
                    {
                        if (invalidtile)
                        {
                            valid = false;
                            flag = false;
                        }

                        peanutPos = (flag ? room.MiddleOfTile(x, foundTileY) : new Vector2(pObj.pos.x, foundTileY * 20)) - room.cameraPositions[room.game.cameras[0].currentCameraPosition] - (flag ? new Vector2(16f + Mathf.Lerp(-5f, 5f, room.game.SeededRandom((int)pObj.pos.x)), 28f) : new Vector2(16f, 8f));
                        break;
                    }

                    foundTileY--;
                }

                if (PeanutPositioner.element.name != (flag ? "PeanutShellA" : "Icon_Peanut"))
                {
                    PeanutPositioner.rotation = flag ? -45f : 0f;
                    PeanutPositioner.SetElementByName(flag ? "PeanutShellA" : "Icon_Peanut");
                }

                PeanutPositioner.SetPosition(peanutPos);
                    
                PeanutPositioner.color = new Color(1f, valid ? 1f : 0f, valid ? 1f : 0f);
            }

            PeanutPositioner.isVisible = !((subNodes[subNodes.Count - 1] as PlacedPeanutControlPanel).HidePreviewLabelOn || peanutPos == new Vector2(0f, 0f));

            lastpos = pos;
        } 
    }
}

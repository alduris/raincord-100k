using DevInterface;
using UnityEngine;

namespace Raincord100k.SpawnSpots
{
    internal class SpawnSpotRepresentation : PlacedObjectRepresentation
    {
        public SpawnSpotData Data => pObj.data as SpawnSpotData;

        public SpawnSpotRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name) : base(owner, IDstring, parentNode, pObj, name)
        {
            string[] roomNameParts = owner.room.abstractRoom.name.Split('_');
            if (roomNameParts.Length > 1)
            {
                string regionPart = roomNameParts[1];
                var region = new SpawnSpotData.SpawnRegion(regionPart);
                if (region.Index > -1)
                {
                    Data.SetIfDefault(region);
                }
            }
            subNodes.Add(new Panel(owner, "Panel", this, new Vector2(0f, 10f)));
        }

        private class Panel : DevInterface.Panel, IDevUISignals
        {
            private SpawnSpotRepresentation Rep => parentNode as SpawnSpotRepresentation;
            private readonly ButtonWithSelectPanel selectPanel;

            public Panel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(120f, 25f), "Spawn spot")
            {
                subNodes.Add(selectPanel = new ButtonWithSelectPanel(owner, "Button", this, new Vector2(5f, 5f), size.x - 10f, "", MakeSelectPanel));
                SetText();
            }

            public void Signal(DevUISignalType type, DevUINode sender, string message)
            {
                if (sender == selectPanel)
                {
                    Rep.Data.Region = new SpawnSpotData.SpawnRegion(message);
                    SetText();
                }
            }

            private void SetText()
            {
                selectPanel.Text = $"Region: {Rep.Data.Region}";
            }

            private static SelectPanel MakeSelectPanel(ButtonWithSelectPanel button)
            {
                return new SelectPanel(button.owner, "Select", button, new Vector2(10f, 10f) - button.absPos, new Vector2(305f, 420f), "Select region", [.. SpawnSpotData.SpawnRegion.values.entries]);
            }
        }
    }
}

using System.Globalization;
using System.Text;

namespace Raincord100k.Damoonlord.Peanut
{
    public class PlacedPeanutData : PlacedObject.ConsumableObjectData
    {
        public PlacedPeanutData(PlacedObject obj) : base(obj) { }

        public bool Grounded = true;

        public float PeanutChance = 0.95f;
        public float SuperPeanutChance = 0.05f;

        public override void FromString(string s)
        {
            var array = s.Split('~');
            if (array.Length >= 7)
            {
                float.TryParse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture, out panelPos.x);
                float.TryParse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture, out panelPos.y);
                int.TryParse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture, out minRegen);
                int.TryParse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture, out maxRegen);

                bool.TryParse(array[4], out Grounded);
                float.TryParse(array[5], NumberStyles.Float, CultureInfo.InvariantCulture, out PeanutChance);
                float.TryParse(array[6], NumberStyles.Float, CultureInfo.InvariantCulture, out SuperPeanutChance);
                unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 6);
            }
        }

        public override string ToString()
        {
            StringBuilder data = new StringBuilder();
            data.Append(panelPos.x.ToString(CultureInfo.InvariantCulture));
            data.Append('~');
            data.Append(panelPos.y.ToString(CultureInfo.InvariantCulture));
            data.Append('~');
            data.Append(minRegen.ToString(CultureInfo.InvariantCulture));
            data.Append('~');
            data.Append(maxRegen.ToString(CultureInfo.InvariantCulture));
            data.Append('~');
            data.Append(Grounded.ToString(CultureInfo.InvariantCulture));
            data.Append('~');
            data.Append(PeanutChance.ToString(CultureInfo.InvariantCulture));
            data.Append('~');
            data.Append(SuperPeanutChance.ToString(CultureInfo.InvariantCulture));

            return SaveUtils.AppendUnrecognizedStringAttrs(data.ToString(), "~", unrecognizedAttributes);
        }
    }
}

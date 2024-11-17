using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BundleVersion
{
    public string Name { get; set; }
    public string Hash { get; set; }
    public List<string> ChildDependencies { get; set; } = new List<string>();
    [JsonIgnore]
    public List<string> ParentDependencies { get; set; } = new List<string>();

    private uint[] _hashInts = null;
    public uint[] GetHashInts()
    {
        if (_hashInts != null)
        {
            return _hashInts;
        }

        _hashInts = new uint[4];

        if (string.IsNullOrEmpty(Hash) || Hash.Length != 32)
        {
            return _hashInts;
        }

        for (int i = 0; i < 4; i++)
        {
            string currentHashHex = Hash.Substring(i * 8, 8);
            uint newVal = 0;
            UInt32.TryParse(currentHashHex, System.Globalization.NumberStyles.HexNumber, null, out newVal);
            _hashInts[i] = newVal;
        }

        return _hashInts;

    }
}

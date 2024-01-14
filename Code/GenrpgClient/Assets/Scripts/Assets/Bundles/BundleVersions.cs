using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BundleVersions
{
    public BundleUpdateInfo UpdateInfo { get; set; }
    public Dictionary<string, BundleVersion> Versions { get; set; } = new Dictionary<string, BundleVersion>();
}
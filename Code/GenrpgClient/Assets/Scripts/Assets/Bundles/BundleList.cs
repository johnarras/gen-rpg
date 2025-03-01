using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Assets.Bundles
{
    public class BundleList
    {
        public List<BundleListItem> Items { get; set; } = new List<BundleListItem>();
    }

    public class BundleListItem
    {
        public string BundleName { get; set; }
        public bool IsLocal { get; set; }
    }
}

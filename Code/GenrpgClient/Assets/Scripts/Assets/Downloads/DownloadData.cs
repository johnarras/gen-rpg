using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

public class DownloadData
{
    public bool IsImage { get; set; }

    public bool ForceDownload { get; set; }

    public bool IsText { get; set; }

    public OnDownloadHandler Handler { get; set; }

    public object Data { get; set; }

    public byte[] StartBytes { get; set; }

    public byte[] UncompressedBytes { get; set; }

    public string URLPrefixOverride { get; set; }


}

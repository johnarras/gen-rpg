using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Constants;

public class FileUploadData
{
    public string GamePrefix;
    public string Env;
    public string LocalPath;
    public string RemotePath;
    public bool IsWorldData;
    public bool WaitForComplete = false;
}

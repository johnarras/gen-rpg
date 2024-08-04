using System;
using System.Collections.Generic;
using System.Text;


// This must match the ConfigResponse class in the GenrpgClient project
public class ConfigResponse
{
    public string ServerURL { get; set; }
    public string ContentRoot { get; set; }
    public string AssetEnv { get; set; }
    public string MaintenanceMessage { get; set; }
}
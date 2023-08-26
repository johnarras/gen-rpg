using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Genrpg.Shared.Core.Entities;

using Services;
using System.Diagnostics;
using Genrpg.Shared.Constants;

public class FileUploader
{


    public static void UploadFile(FileUploadData fdata)
    {

        // Call out to the FileUploader program in Assets../FileUploader/FileUploader.exe

        ProcessStartInfo psi = new ProcessStartInfo();

        string filePath = Application.dataPath.Replace("Assets", "../FileUploader/Output/FileUploader.exe");

        psi.FileName = filePath;

        string env = fdata.Env;
        
        if (env == EnvNames.Test)
        {
            env = EnvNames.Dev;
        }

        fdata.RemotePath = fdata.GamePrefix.ToLower() + env.ToLower() + "/" + fdata.RemotePath;

        fdata.RemotePath = "\"" + fdata.RemotePath + "\"";
        fdata.LocalPath = "\"" + fdata.LocalPath + "\"";


        psi.Arguments = fdata.LocalPath + " " + fdata.RemotePath;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.CreateNoWindow = true;
        Process process = Process.Start(psi);
        process.WaitForExit();

        return;
    }
}

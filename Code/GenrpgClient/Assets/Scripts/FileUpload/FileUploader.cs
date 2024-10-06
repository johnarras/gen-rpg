
using System.Diagnostics;
using Genrpg.Shared.Constants;

public class FileUploader
{
    public static void UploadFile(FileUploadData fdata)
    {
        // Call out to the FileUploader program in Assets../FileUploader/FileUploader.exe

        ProcessStartInfo psi = new ProcessStartInfo();

        string filePath = fdata.LocalDataPath.Replace("Assets", "../FileUploader/Output/FileUploader.exe");

        psi.FileName = filePath;

        string env = fdata.Env;
        
        fdata.RemotePath = fdata.GamePrefix.ToLower() + env.ToLower() + "/" + fdata.RemotePath;

        fdata.RemotePath = "\"" + fdata.RemotePath + "\"";
        fdata.LocalPath = "\"" + fdata.LocalPath + "\"";

        psi.Arguments = fdata.IsWorldData.ToString() + " " + fdata.LocalPath + " " + fdata.RemotePath;
        psi.WindowStyle = ProcessWindowStyle.Hidden;
        psi.CreateNoWindow = true;
        Process process = Process.Start(psi);
        if (fdata.WaitForComplete)
        {
            process.WaitForExit();
        }
        return;
    }
}

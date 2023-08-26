using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Genrpg.Shared.Utils
{
    public static class FileUtils
    {
        public static string ReadFileInExeFolder(string filename)
        {
            string strExeFilePath = Assembly.GetExecutingAssembly().Location;
            //This will strip just the working path name:
            //C:\Program Files\MyApplication
            string strWorkPath = Path.GetDirectoryName(strExeFilePath);

            string fullPath = Path.Combine(strWorkPath, filename);

            return File.ReadAllText(fullPath);
        }
    }
}

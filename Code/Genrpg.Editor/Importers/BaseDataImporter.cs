using Genrpg.Editor.Constants;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.UI;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace Genrpg.Editor.Importers
{
    public abstract class BaseDataImporter : IDataImporter
    {

        protected ILogService _logService;
        protected IRepositoryService _repoService;

        public abstract string ImportDataFilename { get; }

        public abstract EImportTypes GetKey();

        protected abstract Task<bool> ParseInputFromLines(Window window, EditorGameState gs, string[] lines);



        string dataFolderOffsetPath = "\\..\\..\\..\\..\\..\\..\\..\\ImportData\\";
        protected string[] ReadImportDataLines(string importFilename)
        {
            string strExeFilePath = Assembly.GetExecutingAssembly().Location;

            int lastSlashIndex = strExeFilePath.LastIndexOf("\\");

            strExeFilePath = strExeFilePath.Substring(0, lastSlashIndex);

            string fullFilePath = strExeFilePath + dataFolderOffsetPath + importFilename;

            string text = File.ReadAllText(fullFilePath);

            string[] lines = text.Split('\n');

            for (int l = 0; l < lines.Length; l++)
            {
                lines[l] = lines[l].Trim();
            }

            return lines;
        }

        public async Task<bool> ImportData(Window window, EditorGameState gs)
        {
            try
            {
                string[] lines = ReadImportDataLines(ImportDataFilename);

                if (lines.Length < 1)
                {
                    return false;
                }

                if (!await ParseInputFromLines(window, gs, lines))
                {
                    gs.LookedAtObjects = new List<object>();
             
                }

            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "Import Data From: " + ImportDataFilename);
                return false;

            }


            await Task.CompletedTask;
            return true;
        }


        protected void ShowErrorDialog(EditorGameState gs, string message)
        {
            MessageDialog dialog = new MessageDialog(message, "Aborted");
            dialog.Commands.Add(new UICommand("Ok"));

            dialog.ShowAsync().GetAwaiter().GetResult();

            gs.LookedAtObjects.Clear();
        }
    }
}

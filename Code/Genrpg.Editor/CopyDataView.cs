using GameEditor;
using Genrpg.Editor.Entities.Core;
using Genrpg.Editor.Services.Setup;
using Genrpg.ServerShared.Config;
using Genrpg.ServerShared.GameSettings.Services;
using Genrpg.ServerShared.MapSpawns;
using Genrpg.ServerShared.Setup;
using Genrpg.Shared.Constants;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Genrpg.Editor
{
    public partial class CopyDataView : UserControl
    {
        private EditorGameState _gs;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private DataWindow win;
        private IMapSpawnService _mapSpawnService;
        public CopyDataView(EditorGameState gs, DataWindow winIn)
        {
            _gs = gs; 
            win = winIn;
            if (win != null)
            {
                Size = win.Size;
                win.Controls.Clear();
                win.Controls.Add(this);
                win.ViewStack.Add(this);
            }
            gs.loc.Resolve(this);
            InitializeComponent();
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            _ = Task.Run(() => CopyDataAsync(_cts.Token));
        }

        private async Task CopyDataAsync(CancellationToken token)
        {
            ServerConfig fromConfig = await ConfigUtils.SetupServerConfig(token, "Editor");
            fromConfig.Env = EnvNames.Dev;
            EditorGameState fromGS = await SetupUtils.SetupFromConfig<EditorGameState>(this, "Editor", new EditorSetupService(), token, fromConfig);

            ServerConfig toConfig = SerializationUtils.SafeMakeCopy(fromConfig);
            toConfig.Env = EnvNames.Test;

            EditorGameState toGS = await SetupUtils.SetupFromConfig<EditorGameState>(this, "Editor", new EditorSetupService(), token, toConfig);

            IGameDataService gds = toGS.loc.Get<IGameDataService>();

            await gds.SaveGameData(fromGS.data, toGS.repo);
        }
    }
}

using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Networking.Messages;
using Genrpg.Shared.Utils;
using System.Text;
using System.Threading;

namespace Assets.Scripts.UI
{
    public class NetworkStatusUI : BaseBehaviour
    {

        private IMapTerrainManager _terrainManager;
        protected IMapProvider _mapProvider;
        public GText Text;

        private CancellationToken _token;
        public void Init(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddEvent<ServerMessageCounts>(this, OnServerMessageCounts);
        }

        protected void OnServerMessageCounts (ServerMessageCounts data)
        {
            if (Text == null)
            {
                return;
            }           

            StringBuilder sb = new StringBuilder();

            sb.Append("Server: (" + data.QueueCount + " Queues) - ");
            sb.Append("Uptime: " + DateUtils.PrintTime(data.Seconds) + " - ");
            sb.Append("TotMsg: " + StrUtils.PrintCommaValue(data.TotalMessages) + "\n");
            sb.Append("PerSec: " + StrUtils.PrintCommaValue(data.MessagesPerSecond) + " -");
            sb.Append(" Hour: " + StrUtils.PrintCommaValue(3600*data.TotalMessages / data.Seconds) + " - ");
            sb.Append(" Day: " + StrUtils.PrintCommaValue(3600*24 * data.TotalMessages / data.Seconds) + "\n");
            sb.Append("MsgMin/Max: " + StrUtils.PrintCommaValue(data.MinMessages) + "/" 
                + StrUtils.PrintCommaValue(data.MaxMessages) + "\n");
            sb.Append("Spl: " + StrUtils.PrintCommaValue(data.TotalSpells) + " PerSec: " + (data.TotalSpells / data.Seconds) + " - ");
            sb.Append("Upd: " + StrUtils.PrintCommaValue(data.TotalUpdates) + " PerSec: " + (data.TotalUpdates / data.Seconds) + "\n");

            if (data.MapCounts != null)
            {               
                MapObjectCounts mapCounts = data.MapCounts;
                if (mapCounts.TotalGridLocks > 0)
                {
                    sb.Append("Objs: " + StrUtils.PrintCommaValue(mapCounts.CurrentObjectCount) + "/" + StrUtils.PrintCommaValue(mapCounts.ObjectTotal) + " - ");
                    sb.Append("Units: " + StrUtils.PrintCommaValue(mapCounts.CurrentUnitCount) + "/" + StrUtils.PrintCommaValue(mapCounts.UnitTotal) + "\n");
                    sb.Append("Units Add: " + StrUtils.PrintCommaValue(mapCounts.UnitsAdded) + " Rem: " + StrUtils.PrintCommaValue(mapCounts.UnitsRemoved) + " - ");
                    sb.Append(" PerSec +  " + StrUtils.PrintCommaValue(mapCounts.UnitsAdded / data.Seconds) + " : " + StrUtils.PrintCommaValue(mapCounts.UnitsRemoved / data.Seconds) + "\n");
                    sb.Append("AreaQuer: " + StrUtils.PrintCommaValue(mapCounts.TotalQueries) + " PerSec -: " + StrUtils.PrintCommaValue((mapCounts.TotalQueries / data.Seconds)) + " - ");
                    sb.Append("Grid R/W " + StrUtils.PrintCommaValue(mapCounts.TotalGridReads) + " PerSec: " + StrUtils.PrintCommaValue((mapCounts.TotalGridReads / data.Seconds)) + "\n");
                    sb.Append("Grid L/W " + StrUtils.PrintCommaValue(mapCounts.TotalGridLocks) + " PerSec: " + StrUtils.PrintCommaValue((mapCounts.TotalGridLocks / data.Seconds)) + " - ");
                    sb.Append("GridR/GridW " + StrUtils.PrintCommaValue(mapCounts.TotalGridReads / mapCounts.TotalGridLocks) + "\n");
                }
                else
                {
                    sb.Append("Units: " + StrUtils.PrintCommaValue(mapCounts.UnitTotal) + " Objs: " + StrUtils.PrintCommaValue(mapCounts.ObjectTotal) + "\n");
                }
            }

            ConnMessageCounts clientCounts = _networkService.GetMessageCounts();
            long size = _mapProvider.GetMap().BlockCount * (MapConstants.TerrainPatchSize - 1);
            sb.Append("Client Size: " + size + "x" + size + " units\n");
            sb.Append("Uptime: " + DateUtils.PrintTime(clientCounts.Seconds) + " - ");

            sb.Append("Msg In & Out: " + clientCounts.MessagesReceived + " & " + clientCounts.MessagesSent);
            if (clientCounts.Seconds > 0)
            {
                sb.Append(" - Msg PerSec: " + (clientCounts.MessagesReceived / clientCounts.Seconds) + " & " + (clientCounts.MessagesSent / clientCounts.Seconds));
            }
            sb.Append("\n");

            sb.Append("Bytes In  & Out: " + StrUtils.PrintCommaValue(clientCounts.BytesReceived) + " & " + StrUtils.PrintCommaValue(clientCounts.BytesSent));

            if (clientCounts.Seconds > 0)
            {
                sb.Append("Bytes PerSec: " + (clientCounts.BytesReceived / clientCounts.Seconds) + " & " + (clientCounts.BytesSent / clientCounts.Seconds));
            }
            sb.Append("\n");

            if (data.MessagesPerSecond > 0 && clientCounts.MessagesReceived > 0 && clientCounts.Seconds > 0)
            {
                long messageRatio = data.MessagesPerSecond / (clientCounts.MessagesReceived / clientCounts.Seconds);

                sb.Append("Server/Client Msg Ratio: " + messageRatio + "\n");
            }

            ClientAssetCounts assetCounts = _assetService.GetAssetCounts();

            sb.Append("\nClientAssets:\n");
            ShowClientVals(sb, "Bundles", assetCounts.BundlesLoaded, assetCounts.BundlesUnloaded, clientCounts.Seconds);
            ShowClientVals(sb, "TerrainPatches", _terrainManager.GetPatchesAdded(), _terrainManager.GetPatchesRemoved(), clientCounts.Seconds);
            ShowClientVals(sb, "Objects", assetCounts.ObjectsLoaded, assetCounts.ObjectsUnloaded, clientCounts.Seconds);
            _uIInitializable.SetText(Text, sb.ToString());

            return;
        }

        protected void ShowClientVals(StringBuilder sb, string prefix, long added, long removed, long seconds)
        {
            long curr = added - removed;
            float perSecond = 1.0f*curr / seconds;
            string perSecondText = "";
            if (perSecond > 10)
            {
                perSecondText = ((int)perSecond).ToString();
            }
            else
            {
                perSecondText = perSecond.ToString("F2");
            }
            sb.Append(prefix 
                + " Loaded: " + added 
                + " Unloaded: " + removed 
                + " Curr: " + curr
                + " PerSec: " + perSecondText + "\n");
        }

    }
}

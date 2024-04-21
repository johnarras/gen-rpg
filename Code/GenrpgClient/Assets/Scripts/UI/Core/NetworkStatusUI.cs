using Genrpg.Shared.MapServer.Messages;
using Genrpg.Shared.Networking.Messages;
using Genrpg.Shared.Utils;
using System.Text;
using System.Threading;

namespace Assets.Scripts.UI
{
    public class NetworkStatusUI : BaseBehaviour
    {

        private IMapTerrainManager _terrainManager;
        public GText Text;

        private CancellationToken _token;
        public void Init(CancellationToken token)
        {
            _token = token;
            _dispatcher.AddEvent<ServerMessageCounts>(this, OnServerMessageCounts);
        }

        protected ServerMessageCounts OnServerMessageCounts (UnityGameState gs, ServerMessageCounts data)
        {
            if (Text == null)
            {
                return null;
            }           

            StringBuilder sb = new StringBuilder();

            sb.Append("Server: (" + data.QueueCount + " Queues) -- ");
            sb.Append("Uptime: " + DateUtils.PrintTime(data.Seconds) + "\n");
            sb.Append("TotalMsg: " + StrUtils.PrintCommaValue(data.TotalMessages) + " -- ");
            sb.Append("MsgPerSec: " + StrUtils.PrintCommaValue(data.MessagesPerSecond) + "\n");
            sb.Append("MsgPerHour: " + StrUtils.PrintCommaValue(3600*data.TotalMessages / data.Seconds) + " -- ");
            sb.Append("MsgPerDay: " + StrUtils.PrintCommaValue(3600*24 * data.TotalMessages / data.Seconds) + "\n");
            sb.Append("MsgMin/Max: " + StrUtils.PrintCommaValue(data.MinMessages) + "/" 
                + StrUtils.PrintCommaValue(data.MaxMessages) + "\n");
            sb.Append("Spells: " + StrUtils.PrintCommaValue(data.TotalSpells) + " PerSec: " + (data.TotalSpells / data.Seconds) + "\n");
          
            if (data.MapCounts != null)
            {               
                MapObjectCounts mapCounts = data.MapCounts;
                if (mapCounts.TotalGridLocks > 0)
                {
                    sb.Append("Objs: " + StrUtils.PrintCommaValue(mapCounts.CurrentObjectCount) + "/" + StrUtils.PrintCommaValue(mapCounts.ObjectTotal) + "\n");
                    sb.Append("Units: " + StrUtils.PrintCommaValue(mapCounts.CurrentUnitCount) + "/" + StrUtils.PrintCommaValue(mapCounts.UnitTotal) + "\n");
                    sb.Append("Units Created: " + StrUtils.PrintCommaValue(mapCounts.UnitsAdded) + " Removed: " + StrUtils.PrintCommaValue(mapCounts.UnitsRemoved) + "\n");
                    sb.Append("Units/Sec Created: " + StrUtils.PrintCommaValue(mapCounts.UnitsAdded / data.Seconds) + " Removed: " + StrUtils.PrintCommaValue(mapCounts.UnitsRemoved / data.Seconds) + "\n");
                    sb.Append("AreaQueries: " + StrUtils.PrintCommaValue(mapCounts.TotalQueries) + " PerSec: " + StrUtils.PrintCommaValue((mapCounts.TotalQueries / data.Seconds)) + "\n");
                    sb.Append("GridReads/Writes " + StrUtils.PrintCommaValue(mapCounts.TotalGridReads) + " PerSec: " + StrUtils.PrintCommaValue((mapCounts.TotalGridReads / data.Seconds)) + "\n");
                    sb.Append("GridLocks/Writes " + StrUtils.PrintCommaValue(mapCounts.TotalGridLocks) + " PerSec: " + StrUtils.PrintCommaValue((mapCounts.TotalGridLocks / data.Seconds)) + "\n");
                    sb.Append("GridRead/GridLock " + StrUtils.PrintCommaValue(mapCounts.TotalGridReads / mapCounts.TotalGridLocks) + "\n");
                }
                else
                {
                    sb.Append("Units: " + StrUtils.PrintCommaValue(mapCounts.UnitTotal) + " Objs: " + StrUtils.PrintCommaValue(mapCounts.ObjectTotal) + "\n");
                }
            }

            ConnMessageCounts clientCounts = _networkService.GetMessageCounts();
            long size = gs.map.BlockCount * (MapConstants.TerrainPatchSize - 1);
            sb.Append("Client Size: " + size + "x" + size + " units\n");
            sb.Append("Uptime: " + DateUtils.PrintTime(clientCounts.Seconds) + "\n");

            sb.Append("Msg In & Out: " + clientCounts.MessagesReceived + " & " + clientCounts.MessagesSent + "\n");
            if (clientCounts.Seconds > 0)
            {
                sb.Append("Msg PerSec: " + (clientCounts.MessagesReceived / clientCounts.Seconds) + " & " + (clientCounts.MessagesSent / clientCounts.Seconds) + "\n");
            }

            sb.Append("Bytes In  & Out: " + StrUtils.PrintCommaValue(clientCounts.BytesReceived) + " & " + StrUtils.PrintCommaValue(clientCounts.BytesSent) + "\n");

            if (clientCounts.Seconds > 0)
            {
                sb.Append("Bytes PerSec: " + (clientCounts.BytesReceived / clientCounts.Seconds) + " & " + (clientCounts.BytesSent / clientCounts.Seconds) + "\n");
            }

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

            return null;
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

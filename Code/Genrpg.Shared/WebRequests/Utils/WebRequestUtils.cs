using MessagePack;
using Genrpg.Shared.Core.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Shared.WebRequests.Utils
{
    [MessagePackObject]
    public class WebRequestUtils
    {
        public static async Task<byte[]> DownloadBytes(GameState gs, string fullURL)
        {
            WebClient webClient = new WebClient();

            byte[] buffer = new byte[4096];

            WebRequest request = WebRequest.Create(fullURL);

            using (WebResponse response = request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        int count = 0;
                        do
                        {
                            count = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                            await memoryStream.WriteAsync(buffer, 0, count);

                        } while (count != 0);

                        return memoryStream.ToArray();
                    }
                }
            }
        }
    }
}

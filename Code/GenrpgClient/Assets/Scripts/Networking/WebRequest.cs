using UnityEngine;
using UnityEngine.Networking;
using System;
using Cysharp.Threading.Tasks;
using Genrpg.Shared.Utils;
using System.Threading;
using Genrpg.Shared.Login.Messages;

public class WebRequest
{
	private UnityGameState _gs;
	private string _uri;
    private string _postData;
    private WebResultsHandler _handler = null;

    const int MaxTimes = 3;
	public async UniTask GetData (UnityGameState gs, string uri, string postData, WebResultsHandler handler, CancellationToken token)
    {
        _gs = gs;
        _uri = uri;
        _postData = postData;
        _handler = handler;

        WWWForm form = new WWWForm();
        form.AddField("Data", _postData);
        for (int times = 0; times < MaxTimes; times++)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(_uri, form))
            {
                await request.SendWebRequest();

                while (!request.isDone && string.IsNullOrEmpty(request.error))
                {
                    await UniTask.NextFrame(token);
                }

                if (!String.IsNullOrEmpty(request.error))
                {
                    _gs.logger.Info("HTTP Post Error: " + request.error + " URI: " + _uri);
                    await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
                    continue;
                }

                string text = request.downloadHandler.text;
                request.Dispose();

                if (!string.IsNullOrEmpty(text))
                {
                    if (handler != null)
                    {
                        handler(gs, text, token);
                    }
                }
                else
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.0f), cancellationToken: token);
                    continue;
                }
                break;
            }
        }
	}
}
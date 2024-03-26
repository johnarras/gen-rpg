using UnityEngine.Networking;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using Genrpg.Shared.Logging.Interfaces;
using System.Runtime.InteropServices; // Needed

public class ClientWebRequest
{
	private UnityGameState _gs;
	private string _uri;
    private string _postData;
    private WebResultsHandler _handler = null;
    private ILogService _logService = null;
    const int MaxTimes = 3;
	public async UniTask SendRequest (UnityGameState gs, ILogService logService, string uri, string postData, WebResultsHandler handler, CancellationToken token)
    {
        _logService = logService;
        _uri = uri;
        _postData = postData;
        _handler = handler;
        WWWForm form = new WWWForm();
        form.AddField("Data", _postData);
        for (int times = 0; times < MaxTimes; times++)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(_uri, form))
            {
                UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
                while (!asyncOp.isDone)
                {
                    try
                    {
                        await UniTask.NextFrame(cancellationToken: token);
                    }
                    catch (OperationCanceledException ce)
                    {
                        _logService.Info("Op was cancelled " + ce.Message);
                        break;
                    }
                }

                if (!String.IsNullOrEmpty(request.error))
                {
                    _logService.Info("HTTP Post Error: " + request.error + " URI: " + _uri);
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
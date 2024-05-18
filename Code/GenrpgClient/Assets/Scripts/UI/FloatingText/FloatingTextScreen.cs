using System;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using Genrpg.Shared.DataStores.Entities;
using Cysharp.Threading.Tasks;
using System.Threading;

public enum EFloatingTextArt
{
    Message = 0,
    Error = 1,
}

public class FloatingTextQueuedItem
{
    public string Message;
    public string ArtName;
}

public class FloatingTextScreen : BaseScreen
{
    private IInputService _inputService;
    
    public GEntity _textAnchor;

    private string MessageArt = "FloatingMessageText";
    private string ErrorArt = "FloatingErrorText";

    
    public float _TimeBetweenMessages = 3.0f;

    private List<FloatingTextItem> _currentItems;
    private List<FloatingTextQueuedItem> _messageQueue;

    protected override void OnEnable()
    {
        _messageQueue = new List<FloatingTextQueuedItem>();
        _currentItems = new List<FloatingTextItem>();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        _dispatcher.AddEvent<ShowFloatingText>(this, OnReceiveMessage);
        await UniTask.CompletedTask;
    }

    private ShowFloatingText OnReceiveMessage(UnityGameState gs, ShowFloatingText message)
    {
        ShowMessage(message.Text, message.Art);
        return null;
    }

    DateTime _lastShowTime = DateTime.UtcNow.AddDays(-1);
    float delta = 0;
    List<FloatingTextItem> removeList = new List<FloatingTextItem>();

    protected override void ScreenUpdate()
    {

        if (_messageQueue.Count > 0 && (DateTime.UtcNow-_lastShowTime).TotalSeconds >= _TimeBetweenMessages)
        {
            FloatingTextQueuedItem firstItem = _messageQueue[0];
            _messageQueue.RemoveAt(0);

            _assetService.LoadAssetInto(_gs, _textAnchor, AssetCategoryNames.UI, firstItem.ArtName, OnLoadText, firstItem.Message, _token, Subdirectory);

        }


        delta = _inputService.GetDeltaTime();

        removeList.Clear();

        foreach (FloatingTextItem ft in _currentItems)
        {
            ft.transform().localPosition += GVector3.Create(0, delta * ft.PixelsPerSecond, 0);
            ft.ElapsedSeconds += delta;

            if (ft.ElapsedSeconds > ft.DurationSeconds)
            {
                removeList.Add(ft);
            }
        }
        foreach (FloatingTextItem item in removeList)
        {
            if (_currentItems.Contains(item))
            {
                _currentItems.Remove(item);
            }
            GEntityUtils.Destroy(item.entity());
        }
        
    }

    private void ShowMessage(string msg, EFloatingTextArt art)
    {
        string prefabName = MessageArt;
        if (art == EFloatingTextArt.Error)
        {
            prefabName = ErrorArt;
        }


        if (_textAnchor == null || string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(prefabName))
        {
            return;
        }

        FloatingTextQueuedItem queuedItem = new FloatingTextQueuedItem()
        {
            Message = msg,
            ArtName = prefabName,
        };

        _messageQueue.Add(queuedItem);
    }

    private void OnLoadText(UnityGameState gs, object obj, object data, CancellationToken token)
    {
        GEntity go = obj as GEntity;
        if (go == null)
        {
            return;
        }
        string txt = data as String;
        if (string.IsNullOrEmpty(txt))
        {
            GEntityUtils.Destroy(go);
            return;
        }
        FloatingTextItem ft = go.GetComponent<FloatingTextItem>();
        if (ft == null || ft.TextString == null)
        {
            GEntityUtils.Destroy(go);
            return;
        }
        _uIInitializable.SetText(ft.TextString, txt);
        if (_currentItems == null)
        {
            _currentItems = new List<FloatingTextItem>();
        }

        _currentItems.Add(ft);
        ft.ElapsedSeconds = 0;
    }
}


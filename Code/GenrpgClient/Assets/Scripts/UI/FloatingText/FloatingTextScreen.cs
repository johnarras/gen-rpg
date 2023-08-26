using System;
using System.Collections.Generic;
using UnityEngine;
using Genrpg.Shared.DataStores.Entities;
using Cysharp.Threading.Tasks;
using System.Threading;

public class FloatingTextQueuedItem
{
    public string Message;
    public string ArtName;
}

public class FloatingTextScreen : BaseScreen
{
    [SerializeField]
    private GameObject _textAnchor;

    private string MessageArt = "FloatingMessageText";
    private string ErrorArt = "FloatingErrorText";

    [SerializeField]
    private float _TimeBetweenMessages = 3.0f;

    private List<FloatingTextItem> _currentItems;
    private List<FloatingTextQueuedItem> _messageQueue;

    public static FloatingTextScreen Instance = null;

    protected override void OnEnable()
    {
        _messageQueue = new List<FloatingTextQueuedItem>();
        _currentItems = new List<FloatingTextItem>();
        Instance = this;
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        Instance = null;
        base.OnDisable();
    }

    protected override async UniTask OnStartOpen(object data, CancellationToken token)
    {
        await UniTask.CompletedTask;
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

            _assetService.LoadAssetInto(_gs, _textAnchor, AssetCategory.UI, firstItem.ArtName, OnLoadText, firstItem.Message, _token);

        }


        delta = InputService.Instance.GetDeltaTime();

        removeList.Clear();

        foreach (FloatingTextItem ft in _currentItems)
        {
            ft.transform.localPosition += new Vector3(0, delta * ft.PixelsPerSecond, 0);
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
            GameObject.Destroy(item.gameObject);
        }
        
    }

    public void ShowMessage(string msg)
    {
        AddFloatingText(msg, MessageArt);
    }

    public void ShowError(string msg)
    {
        AddFloatingText(msg, ErrorArt);
    }

    private void AddFloatingText(string msg, string prefab)
    {
        if (_textAnchor == null || string.IsNullOrEmpty(msg) || string.IsNullOrEmpty(prefab))
        {
            return;
        }

        FloatingTextQueuedItem queuedItem = new FloatingTextQueuedItem()
        {
            Message = msg,
            ArtName = prefab,
        };

        _messageQueue.Add(queuedItem);
    }

    private void OnLoadText(UnityGameState gs, string url, object obj, object data, CancellationToken token)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            return;
        }
        string txt = data as String;
        if (string.IsNullOrEmpty(txt))
        {
            GameObject.Destroy(go);
            return;
        }
        FloatingTextItem ft = go.GetComponent<FloatingTextItem>();
        if (ft == null || ft.TextString == null)
        {
            GameObject.Destroy(go);
            return;
        }
        ft.TextString.text = txt;
        if (_currentItems == null)
        {
            _currentItems = new List<FloatingTextItem>();
        }

        _currentItems.Add(ft);
        ft.ElapsedSeconds = 0;
    }
}


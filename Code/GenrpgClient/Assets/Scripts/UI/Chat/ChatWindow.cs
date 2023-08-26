using Assets.Scripts.UI.Chat;
using Genrpg.Shared.Chat.Entities;
using Genrpg.Shared.Chat.Messages;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ChatWindow : BaseBehaviour
    {
        [SerializeField]    
        private GameObject _chatParent;
        [SerializeField]
        private InputField _chatInput;

        [SerializeField]
        private Text _chatTextPrefix;

        [SerializeField]
        private Image _inputBackground;

        [SerializeField]
        private ChatRow _row;

        [SerializeField]
        private int _maxRows = 20;

        private List<OnChatMessage> _messages = new List<OnChatMessage>();

        private List<ChatRow> _rows = new List<ChatRow>();

        private static ChatWindow _instance;

        public static ChatWindow Instance => _instance;

        private ChatType _currentChatType = null;

        private string _chatPrefix = "";


        public override void Initialize(UnityGameState gs)
        {
            base.Initialize(gs);
            _gs.AddEvent<OnChatMessage>(this, OnChatMessageHandler);
            _instance = this;
        }

        private bool _editing = false;
        public void SetEditing(bool editing)
        {
            _editing = editing;
            if (_chatInput != null)
            {
                if (_editing)
                {
                    EventSystem.current.SetSelectedGameObject(_chatInput.gameObject);
                    if (_inputBackground != null)
                    {
                        _inputBackground.color = Color.white;
                    }
                    if (_currentChatType == null)
                    {
                        _currentChatType = _gs.data.GetGameData<ChatSettings>().GetChatType(ChatType.Say);
                    }
                    ShowChatInputPrefix();
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    if (_inputBackground != null)
                    {
                        _inputBackground.color = Color.gray;
                    }
                    UIHelper.SetText(_chatTextPrefix, "");
                }
            }
        }

        public void SendChat()
        {
            if (_chatInput == null)
            {
                return;
            }
            string text = _chatInput.text;
            _chatInput.text = "";

            if (text.Length > 0 && text[0] == '/')
            {
                text = text.Substring(1);

                if (text.Length > 0)
                {
                    List<ChatType> chatTypes = _gs.data.GetList<ChatType>();

                    string firstWord = text.Substring(0, text.IndexOf(" "));
                    firstWord = firstWord.ToLower();

                    bool didChangeChat = false;
                    foreach (ChatType chatType in chatTypes)
                    {
                        if (chatType.Name.ToLower().IndexOf(firstWord) == 0)
                        {
                            _currentChatType = chatType;
                            ShowChatInputPrefix();
                            didChangeChat = true;
                            break;
                        }
                    }

                    text = text.Substring(firstWord.Length);

                    if (didChangeChat)
                    {
                        _gs.logger.Debug("Change chat type to: " + _currentChatType.Name);
                        if (text.Length < 1)
                        {
                            InputService.Instance.ToggleChat();
                            return;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                string targetName = "";
                if (_currentChatType.IdKey == ChatType.Tell)
                {
                    targetName = text.Substring(0, text.IndexOf(" "));
                    text = text.Substring(targetName.Length);    
                    
                    if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(text))
                    {
                        InputService.Instance.ToggleChat();
                        return;
                    }
                }

                _gs.logger.Debug("Sending chat: " + _currentChatType.Name + " To: " + targetName + ": " + text);
                InputService.Instance.ToggleChat();

                SendChatMessage sendMessage = new SendChatMessage()
                {
                    Text = text,
                    ChatTypeId = _currentChatType.IdKey,
                    ToId = targetName,
                };
                _networkService.SendMapMessage(sendMessage);
            }
            else
            {
                if (_chatInput != null)
                {
                    _chatInput.text = "";
                }
            }
        }

        private void ShowChatInputPrefix()
        {
            if (_currentChatType != null && _chatInput != null)
            {
                _chatPrefix = "[" + _currentChatType.Name + "]: ";
                UIHelper.SetText(_chatTextPrefix, _chatPrefix);
            }
        }

        private OnChatMessage OnChatMessageHandler(UnityGameState gs, OnChatMessage message)
        {
            if (_chatParent == null || _row == null)
            {
                return null;
            }

            _messages.Add(message);
            ChatRow newRow = GameObjectUtils.FullInstantiate(gs,_row.gameObject).GetComponent<ChatRow>();
            newRow.gameObject.SetActive(true);
            GameObjectUtils.AddToParent(newRow.gameObject, _chatParent);
            _rows.Add(newRow);
            newRow.Init(message);
            while (_rows.Count > _maxRows)
            {
                GameObject.Destroy(_rows[0].gameObject);
                _rows.RemoveAt(0);
            }
            return null;
        }
    }
}

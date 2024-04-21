using Assets.Scripts.UI.Chat;
using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.WhoList.Entities;
using Genrpg.Shared.WhoList.Messages;
using System.Collections.Generic;
using GEntity = UnityEngine.GameObject;
using UnityEngine.EventSystems; // Needed
using Genrpg.Shared.Chat.Constants;
using Genrpg.Shared.Chat.Settings;
using UnityEngine;

namespace UI
{
    public class ChatWindow : BaseBehaviour
    {
            
        public GEntity ChatParent;
        public GInputField ChatInput;
        public GText ChatTextPrefix;
        public GImage InputBackground;
        public ChatRow Row;
        
        public int _maxRows = 20;

        private List<ChatRow> _rows = new List<ChatRow>();

        private ChatType _currentChatType = null;

        private string _chatPrefix = "";


        public override void Initialize(UnityGameState gs)
        {
            base.Initialize(gs);
            _dispatcher.AddEvent<OnChatMessage>(this, OnChatMessageHandler);
            _dispatcher.AddEvent<OnGetWhoList>(this, OnGetWhoListHandler);
            _updateService.AddUpdate(this, UpdateChat, UpdateType.Regular);
        }

        private void UpdateChat()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SetEditing(!_editing);
            }
        }

        private bool _editing = false;
        private void SetEditing(bool editing)
        {
            _editing = editing;
            _logService.Info("Set editing val " + _editing);
            if (ChatInput != null)
            {
                if (_editing)
                {
                    _logService.Info("Edit Now!");
                    EventSystem.current.SetSelectedGameObject(ChatInput.entity());
                    if (InputBackground != null)
                    {
                        InputBackground.color = GColor.white;
                    }
                    if (_currentChatType == null)
                    {
                        _currentChatType = _gameData.Get<ChatSettings>(_gs.ch).Get(ChatTypes.Say);
                    }
                    ShowChatInputPrefix();
                }
                else
                {
                    SendChat();
                    _logService.Info("Stop edit now!");
                    EventSystem.current.SetSelectedGameObject(null);
                    if (InputBackground != null)
                    {
                        InputBackground.color = GColor.gray;
                    }
                    _uIInitializable.SetText(ChatTextPrefix, "");
                }
            }
        }

        public void SendChat()
        {
            if (ChatInput == null)
            {
                return;
            }
            string text = ChatInput.Text;
            ChatInput.Text = "";

            if (text.Length > 0 && text[0] == '/')
            {
                text = text.Substring(1);

                if (text.Length > 0)
                {
                    IReadOnlyList<ChatType> chatTypes = _gameData.Get<ChatSettings>(_gs.ch).GetData();

                    int firstSpaceIndex = text.IndexOf(" ");

                    string firstWord = text;

                    if (firstSpaceIndex > 0)
                    {
                        firstWord = text.Substring(0, text.IndexOf(" "));
                    }
                    firstWord = firstWord.ToLower();

                    if (firstWord == "who")
                    {
                        string args = "";

                        if (text.Length > firstWord.Length)
                        {
                            args = text.Substring(firstWord.Length + 1);
                        }
                        _networkService.SendMapMessage(new GetWhoList() { Args = args });
                        SetEditing(false);
                        return;
                    }

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
                        _logService.Debug("Change chat type to: " + _currentChatType.Name);
                        if (text.Length < 1)
                        {
                            SetEditing(false);
                            return;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                string targetName = "";
                if (_currentChatType.IdKey == ChatTypes.Tell)
                {
                    targetName = text.Substring(0, text.IndexOf(" "));
                    text = text.Substring(targetName.Length);    
                    
                    if (string.IsNullOrEmpty(targetName) || string.IsNullOrEmpty(text))
                    {
                        SetEditing(false);
                        return;
                    }
                }

                _logService.Debug("Sending chat: " + _currentChatType.Name + " To: " + targetName + ": " + text);

                SetEditing(false);

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
                if (ChatInput != null)
                {
                    ChatInput.text = "";
                }
            }
        }

        private void ShowChatInputPrefix()
        {
            if (_currentChatType != null && ChatInput != null)
            {
                _chatPrefix = "[" + _currentChatType.Name + "]: ";
                _uIInitializable.SetText(ChatTextPrefix, _chatPrefix);
            }
        }

        private OnChatMessage OnChatMessageHandler(UnityGameState gs, OnChatMessage message)
        {
            if (ChatParent == null || Row == null)
            {
                return null;
            }
            AddChatRow(gs, message);
            return null;
        }


        private void AddChatRow(UnityGameState gs, OnChatMessage message)
        { 
            ChatRow newRow = GEntityUtils.FullInstantiate(gs,Row.entity()).GetComponent<ChatRow>();
            newRow.entity().SetActive(true);
            GEntityUtils.AddToParent(newRow.entity(), ChatParent);
            _rows.Add(newRow);
            newRow.Init(message);
            while (_rows.Count > _maxRows)
            {
                GEntityUtils.Destroy(_rows[0].entity());
                _rows.RemoveAt(0);
            }
        }

        private OnGetWhoList OnGetWhoListHandler(UnityGameState gs, OnGetWhoList message)
        {
            foreach (WhoListItem item in message.Items)
            {

                ChatRow newRow = GEntityUtils.FullInstantiate(gs, Row.entity()).GetComponent<ChatRow>();
                newRow.entity().SetActive(true);
                GEntityUtils.AddToParent(newRow.entity(), ChatParent);
                _rows.Add(newRow);
                newRow.InitTextOnly(item.Name + " :" + item.Level);
                while (_rows.Count > _maxRows)
                {
                    GEntityUtils.Destroy(_rows[0].entity());
                    _rows.RemoveAt(0);
                }
            }
            return null;
        }
    }
}

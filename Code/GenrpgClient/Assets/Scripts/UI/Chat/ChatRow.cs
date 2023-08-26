using Genrpg.Shared.Chat.Entities;
using Genrpg.Shared.Chat.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Chat
{
    public class ChatRow : BaseBehaviour
    {
        [SerializeField]
        private Text _text;

        private OnChatMessage _message;

        public void Init(OnChatMessage message)
        {
            _message = message;

            ChatType chatType = _gs.data.GetGameData<ChatSettings>().GetChatType(message.ChatTypeId);
            
            

            UIHelper.SetText(_text, "[" + chatType?.Name + "] " + message.SenderName + ": " + message.Message);
        }
    }
}

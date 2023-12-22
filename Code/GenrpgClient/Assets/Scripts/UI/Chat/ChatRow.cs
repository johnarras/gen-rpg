using Genrpg.Shared.Chat.Messages;
using Genrpg.Shared.Chat.Settings;

namespace Assets.Scripts.UI.Chat
{
    public class ChatRow : BaseBehaviour
    {
        public GText Text;
        public GImage Background;

        private OnChatMessage _message;

        public void Init(OnChatMessage message)
        {
            _message = message;

            ChatType chatType = _gs.data.GetGameData<ChatSettings>(_gs.ch).GetChatType(message.ChatTypeId);

            UIHelper.SetText(Text, "[" + chatType?.Name + "] " + message.SenderName + ": " + message.Message);
        }

        public void InitTextOnly(string text)
        {
            UIHelper.SetText(Text, text);
        }
    }
}

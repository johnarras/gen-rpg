using Genrpg.Shared.DataStores.PlayerData;
using Genrpg.Shared.Utils;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Genrpg.Shared.Input.Entities
{
    [MessagePackObject]
    public class KeyCommData : OwnerObjectList<KeyComm>
    {
        [Key(0)] public override string Id { get; set; }

        private List<KeyComm> _data { get; set; } = new List<KeyComm>();

        public override void SetData(List<KeyComm> data)
        {
            _data = data;
        }

        public override List<KeyComm> GetData()
        {
            return _data;
        }
        public KeyComm GetKeyComm(string keyComm)
        {
            return _data.FirstOrDefault(x => x.KeyCommand == keyComm);
        }

        public void AddKeyComm(string keyCommand, string keyPress)
        {
            KeyComm keyComm = new KeyComm()
            { 
                KeyCommand = keyCommand, 
                KeyPress = keyPress,
                OwnerId = Id,
                Id = HashUtils.NewGuid(),
            };
            _data.Add(keyComm);
        }
    }

}

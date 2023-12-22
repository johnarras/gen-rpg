using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.SpellCrafting
{
    public class OnDeleteSpellHandler : BaseClientMapMessageHandler<OnDeleteSpell>
    {
        protected override void InnerProcess(UnityGameState gs, OnDeleteSpell msg, CancellationToken token)
        {
            gs.ch.Get<SpellData>().Remove(msg.SpellId);
            gs.Dispatch(msg);
        }
    }
}

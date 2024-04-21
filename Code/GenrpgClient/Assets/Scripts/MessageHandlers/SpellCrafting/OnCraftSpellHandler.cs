using Genrpg.Shared.SpellCrafting.Messages;
using Genrpg.Shared.Spells.PlayerData.Spells;
using System.Threading;

namespace Assets.Scripts.MessageHandlers.SpellCrafting
{
    public class OnCraftSpellHandler : BaseClientMapMessageHandler<OnCraftSpell>
    {
        protected override void InnerProcess(UnityGameState gs, OnCraftSpell msg, CancellationToken token)
        {

            gs.ch.Get<SpellData>().Remove(msg.CraftedSpell.IdKey);
            gs.ch.Get<SpellData>().Add(msg.CraftedSpell);
            _dispatcher.Dispatch(gs,msg);
        }
    }
}

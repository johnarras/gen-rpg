using Genrpg.Shared.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
namespace Genrpg.Shared.UserAbilities.Services
{
    public interface IUserAbilityService : IInitializable
    {
        long GetAbilityTotal(IFilteredObject obj, long userAbilityId, long upgradeLevel);
    }
}


using System.Collections.Generic;
using System.Threading.Tasks;
using Genrpg.Shared.MapObjects.Entities;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Spawns.Entities;
using Genrpg.Shared.Core.Entities;
using System.Threading;
using Genrpg.Shared.Spawns.Interfaces;
using Genrpg.Shared.Entities.Interfaces;
using Genrpg.Shared.PlayerFiltering.Interfaces;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Utils;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.ProcGen.Settings.Names;
using System;
using System.Linq;
using Genrpg.Shared.GameSettings.Loaders;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.Crawler.Info.Constants;

namespace Genrpg.Shared.Entities.Services
{
    public interface IEntityService : IInjectable
    {
        IEntityHelper GetEntityHelper(long entityTypeId);
        IIdName Find(IFilteredObject obj, long entityType, long entityId);
        List<IIdName> GetChildList(IFilteredObject obj, long entityTypeId);
        List<IIdName> GetChildList(IFilteredObject obj, string tableName);
    }

    public class EntityService : IEntityService
    {
        private SetupDictionaryContainer<Type,IGameSettingsLoader> _loaders = new SetupDictionaryContainer<Type, IGameSettingsLoader> ();
        private SetupDictionaryContainer<long, IEntityHelper> _entityHelpers = new SetupDictionaryContainer<long, IEntityHelper>();
        protected IGameData _gameData;
    
        public IEntityHelper GetEntityHelper(long entityTypeId)
        {
            if (_entityHelpers.TryGetValue(entityTypeId, out IEntityHelper helper))
            {
                return helper;
            }
            return null;
        }
        public IIdName Find( IFilteredObject obj, long entityType, long entityId)
        {
            IEntityHelper helper = GetEntityHelper(entityType);

            if (helper == null)
            {
                return null;
            }

            return helper.Find(obj, entityId);

        }
        public List<IIdName> GetChildList(IFilteredObject obj, long entityTypeId)
        {
            IEntityHelper helper = GetEntityHelper(entityTypeId);
            if (helper != null)
            {
                return helper.GetChildList(obj);
            }

            return new List<IIdName>();
        }

        public List<IIdName> GetChildList(IFilteredObject obj, string tableName)
        {
            Dictionary<long, IEntityHelper> helpers = _entityHelpers.GetDict();

            IEntityHelper helper = helpers.Values.FirstOrDefault(x=>x.GetEditorPropertyName() == tableName);

            if (helper != null)
            {
                return helper.GetChildList(obj).OrderBy(x=>x.IdKey).ToList();
            }

            IGameSettingsLoader loader = _loaders.GetDict().Values.FirstOrDefault(x => x.GetChildType().Name == tableName);

            if (loader != null)
            {
                List<ITopLevelSettings> levelSettings = _gameData.AllSettings();

                ITopLevelSettings matchingSettings = levelSettings.FirstOrDefault(x => x.GetType() == loader.GetKey());

                if (matchingSettings != null)
                {
                    return matchingSettings.GetChildren().Cast<IIdName>().ToList();
                }
            }

            return new List<IIdName>();
        }


    }
}

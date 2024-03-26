using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.DataStores.DbQueues.Actions
{
    public class UpdateAction<T> : IDbAction where T : class, IStringId
    {
        private string _docId;
        private Dictionary<string, object> _fieldUpdates = new Dictionary<string, object>();
        private IRepositoryService _repoSystem { get; set; }
        private Action<T> _updateAction;

        public UpdateAction(string docId, Dictionary<string,object> fieldUpdates, IRepositoryService repoSystem)
        {
            _docId = docId;
            _fieldUpdates = fieldUpdates;
            _repoSystem = repoSystem;
        }
        public UpdateAction(string docId, Action<T> action, IRepositoryService repoSystem)
        {
            _docId = docId;
            _updateAction = action;
            _repoSystem = repoSystem;
        }

        public async Task<bool> Execute()
        {
            if (_fieldUpdates != null)
            {
                return await _repoSystem.UpdateDict<T>(_docId, _fieldUpdates).ConfigureAwait(false);
            }
            else
            {
                return await _repoSystem.UpdateAction<T>(_docId, _updateAction).ConfigureAwait(false);
            }
        }
    }
}

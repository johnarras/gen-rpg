using Genrpg.Editor.Entities.Core;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Genrpg.Editor.Services.Reflection
{
    public interface IEditorReflectionService : IInjectable
    {
        bool MemberIsMultiType(MemberInfo mem);
        bool IsMultiType(Type type);
        bool MemberIsGenericList(MemberInfo mem);
        bool IsGenericList(Type mtype);
        object GetObjectValue(object obj, string name);
        void InitializeObjectData(object data);
        bool MemberIsPrimitive(MemberInfo mem);
        List<MemberInfo> GetMembers(object obj);
        Type GetMemberType(MemberInfo mem);
        List<IIdName> CreateDataList(EditorGameState gs, string listName);
        string GetMemberName(object parent, object child);
        List<IIdName> GetDropdownList(EditorGameState gs, MemberInfo mem, object obj);
        void SetObjectValue(object obj, string name, object val);
        object AddItems(object obj, object parent, IRepositoryService repoSystem, out List<object> newItems,
            object copyFrom = null, int maxCount = 0);
        object GetItemWithId(object list, int id, string idMemberName = GameDataConstants.IdKey);
        object GetLastItem(object list);
        IList CreateGenericList(Type type);
        MemberInfo GetMemberInfo(object obj, string name);
        string GetIdString(string txt);
        Type GetUnderlyingType(object obj);
        object DeleteItem(object obj, object parent, object item);
        IEnumerable SortOnParameter(IEnumerable elist, bool ascending = true);
        void ReplaceIndexedItems(EditorGameState gs, object list, List<IIdName> newList);
        object GetItemWithIndex(object list, int index);
        object GetObjectValue(object obj, MemberInfo mem);
        void SetObjectValue(object obj, MemberInfo mem, object val);
        List<T> OrderOn<T>(List<T> list, string memberName, bool numeric = false, bool descending = false);
    }

}

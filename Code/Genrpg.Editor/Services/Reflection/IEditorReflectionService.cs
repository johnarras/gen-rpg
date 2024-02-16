using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.DataStores.Entities;
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
    public interface IEditorReflectionService : IService
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
        List<NameValue> CreateDataList(GameState gs, string listName);
        string GetMemberName(object parent, object child);
        List<NameValue> GetDropdownList(GameState gs, MemberInfo mem, object obj);
        void SetObjectValue(object obj, string name, object val);
        object AddItems(object obj, object parent, IRepositorySystem repoSystem, out List<object> newItems,
            object copyFrom = null, int maxCount = 0);
        object GetItemWithId(object list, int id, string idMemberName = GameDataConstants.IdKey);
        object GetLastItem(object list);
        IList CreateGenericList(Type type);
        MemberInfo GetMemberInfo(object obj, string name);
        string GetIdString(string txt);
        Type GetUnderlyingType(object obj);
        string GetOnClickDropdownName(GameState gs, object obj, MemberInfo mem);
        object DeleteItem(object obj, object parent, object item);
        IEnumerable SortOnParameter(IEnumerable elist, bool ascending = true);
        void ReplaceIndexedItems(GameState gs, object list, List<IIdName> newList);
        object GetItemWithIndex(object list, int index);
        object GetObjectValue(object obj, MemberInfo mem);
        void SetObjectValue(object obj, MemberInfo mem, object val);
    }

}

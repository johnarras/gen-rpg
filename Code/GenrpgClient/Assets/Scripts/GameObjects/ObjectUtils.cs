using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.GameObjects
{
    public static class ObjectUtils
    {
        public static bool IsNull(object obj)
        {
            if (object.ReferenceEquals(obj,null))
            {
                return true;
            }

            if (obj is UnityEngine.Object unityObj)
            {
                if (unityObj == null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

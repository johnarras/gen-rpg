using MessagePack;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Genrpg.Shared.Utils
{
    /// <summary>
    /// This contains some utility functions normally found in LINQ that
    /// can be used in Unity3D which is built on .Net 2.0 without LINQ
    /// </summary>
    [MessagePackObject]
    public class ListUtils
    {


        /// <summary>
        /// Concatenate two lists. This never returns each initial list
        /// it will always make copies of them
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list1">First list</param>
        /// <param name="list2">Second list</param>
        /// <returns>New concatenated list. This will never be one of the initial lists. It is always a copy.</returns>
        public static List<T> Concat<T>(List<T> list1, List<T> list2)
        {
            if (list1 == null || list2 == null)
            {
                return new List<T>();
            }

            List<T> list3 = new List<T>();

            for (int i = 0; i < list1.Count; i++)
            {
                list3.Add(list1[i]);
            }
            for (int i = 0; i < list2.Count; i++)
            {
                list3.Add(list2[i]);
            }
            return list3;



        }

        /// <summary>
        /// Get max Id from an List of IId.
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="list">List of objects of type T  where T : IId</param>
        /// <returns>largest Id field from that list</returns>
        public static long GetMaxId<T>(List<T> list) where T : IId
        {
            if (list == null || list.Count < 1)
            {
                return 0;
            }
            long maxId = 0;
            for (int i = 0; i < list.Count; i++)
            {
                maxId = Math.Max(list[i].IdKey, maxId);
            }
            return maxId;
        }

        public static List<T> GetListItemsAfter<T>(List<T> list, int startId) where T : IId
        {
            List<T> retval = new List<T>();
            if (list == null)
            {
                return retval;
            }

            foreach (T item in list)
            {
                if (item.IdKey > startId)
                {
                    retval.Add(item);
                }
            }
            return retval;
        }

        /// <summary>
        /// Get next available Id from an List<IId>
        /// </summary>
        /// <typeparam name="T">Type of the list items</typeparam>
        /// <param name="list">The list of items of type T</param>
        /// <returns>The next available Id (max of list +1)</returns>
        public static long GetNextId<T>(List<T> list) where T : IId
        {
            return GetMaxId(list) + 1;
        }

        /// <summary>
        /// A bad implementation of OrderOn. Uses InsertionSort and Reflection.
        /// </summary>
        /// <typeparam name="T">The Type of the List</typeparam>
        /// <param name="list"></param>
        /// <param name="memberName">What member name we will sort on</param>
        /// <param name="numeric">Is this a numeric or string sorting</param>
        /// <param name="descending">Ascending or descending order</param>
        /// <returns>The newly ordered list.</returns>
        public static List<T> OrderOn<T>(List<T> list, string memberName, bool numeric = false, bool descending = false)
        {

            List<T> list2 = new List<T>();
            if (list == null)
            {
                return list2;
            }
            if (string.IsNullOrEmpty(memberName))
            {
                return list;
            }
            for (int ii = 0; ii < list.Count; ii++)
            {
                T item = list[ii];
                if (list2.Count < 1)
                {
                    list2.Add(item);
                    continue;
                }


                bool addedItem = false;
                object val = EntityUtils.GetObjectValue(item, memberName);

                if (val == null)
                {
                    val = "";
                }
                string valStr = val.ToString();

                // Now search backwards till we find something that's before this object.
                for (int i = list2.Count - 1; i >= 0; i--)
                {
                    T item2 = list2[i];
                    object val2 = EntityUtils.GetObjectValue(item2, memberName);
                    if (val2 == null)
                    {
                        val2 = "";
                    }
                    string valStr2 = val2.ToString();

                    if (!numeric && string.Compare(valStr, valStr2) >= 0)
                    {

                        list2.Insert(i + 1, item);
                        addedItem = true;
                        break;
                    }

                    if (numeric)
                    {
                        double double1 = 0.0;
                        double double2 = 0.0;
                        double.TryParse(valStr, out double1);
                        double.TryParse(valStr2, out double2);

                        if (double1 >= double2)
                        {
                            list2.Insert(i + 1, item);
                            addedItem = true;
                            break;
                        }
                    }
                }

                // All items too big put this at front.
                if (!addedItem)
                {
                    list2.Insert(0, item);
                }

            }

            if (descending)
            {
                list2.Reverse();
            }

            return list2;
        }


        public static void RemoveElementsOfType<T, REM>(List<T> list) where REM : class, T
        {
            if (list == null)
            {
                return;
            }

            List<T> list2 = new List<T>(list);

            for (int i = 0; i < list2.Count; i++)
            {
                T t = list2[i];
                REM r = t as REM;
                if (r != null)
                {
                    list.Remove(t);
                }
            }
        }

        public static List<B> ConvertFrom<D, B>(List<D> list) where D : B
        {
            if (list == null)
            {
                return new List<B>();
            }

            List<B> list2 = new List<B>();

            for (int i = 0; i < list.Count; i++)
            {
                list2.Add(list[i]);
            }
            return list2;
        }

        public static bool IsEmpty<T>(List<T> list)
        {
            if (list == null || list.Count < 1)
            {
                return true;
            }

            for (int l = 0; l < list.Count; l++)
            {
                if (list[l] == null)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

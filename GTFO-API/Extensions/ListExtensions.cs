using System.Collections.Generic;

namespace GTFO.API.Extensions
{
    /// <summary>
    /// Container for all extension for <see cref="List{T}"/>
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Converts the <see cref="List{T}"/> to <see cref="Il2CppSystem.Collections.Generic.List{T}"/>
        /// </summary>
        /// <returns>A copy of the list as <see cref="Il2CppSystem.Collections.Generic.List{T}"/></returns>
        public static Il2CppSystem.Collections.Generic.List<T> ToIl2Cpp<T>(this List<T> list)
        {
            Il2CppSystem.Collections.Generic.List<T> il2cppList = new(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                il2cppList.Add(list[i]);
            }
            return il2cppList;
        }

        /// <summary>
        /// Converts the <see cref="Il2CppSystem.Collections.Generic.List{T}"/> to <see cref="List{T}"/>
        /// </summary>
        /// <returns>A copy of the list as <see cref="List{T}"/></returns>
        public static List<T> ToManaged<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            List<T> managedList = new(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                managedList.Add(list[i]);
            }
            return managedList;
        }
    }
}

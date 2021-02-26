using System.Collections.Generic;

namespace GreasePencil
{
    public static class ListTools
    {
        public static T[] GetItems<T>(List<T> list)
        {
            return list.GetType().GetField("_items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(list) as T[];
        }
    }
}
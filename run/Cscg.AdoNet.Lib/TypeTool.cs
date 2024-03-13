using System;

namespace Cscg.AdoNet.Lib
{
    public static class TypeTool
    {
        public static T ConvertTo<T>(this object? obj)
        {
            if (obj == null)
            {
                return default!;
            }
            var tmp = Convert.ChangeType(obj, typeof(T));
            return (T)tmp;
        }
    }
}
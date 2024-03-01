using System.Collections.Generic;

namespace Cscg.Compactor.Lib
{
    public static class Reflections
    {
        private static readonly Dictionary<string, string> Descendants = new();
        private static readonly Dictionary<string, Creator> Creators = new();

        public static void CanBe(string baseType, string realType)
        {
            Descendants[baseType] = realType;
        }

        public static void Learn(string realType, Creator creator)
        {
            Creators[realType] = creator;
        }

        public static T Create<T>(string type)
        {
            Creators.TryGetValue(type, out var func);
            var obj = func?.Invoke();
            return (T)obj;
        }
    }
}
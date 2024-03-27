using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace Cscg.AdoNet.Lib
{
    public static class AdoJoinTool
    {
        private static IEnumerable<IDictionary<string, IActiveRead<TReader>>> ReadData<TReader>(this TReader reader,
            Dictionary<string, string> prefixes, Dictionary<string, Func<IActiveRead<TReader>>> fac)
            where TReader : DbDataReader
        {
            while (reader.Read())
            {
                var res = new Dictionary<string, IActiveRead<TReader>>();
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    var key = reader.GetName(index);
                    var tmp = key.Split(['_'], 2);
                    var prefix = tmp[0];
                    var table = prefixes.FirstOrDefault(p => p.Value.Split('|').Contains(prefix)).Key;
                    var item = res.TryGetValue(prefix, out var f) ? f : res[prefix] = fac[table]();
                    var subKey = tmp[1];
                    item.ReadSql(reader, subKey, index);
                }
                foreach (var key in res.Keys.ToArray())
                {
                    var value = res[key];
                    if (value is not IActiveNested an)
                        continue;
                    res[key] = (IActiveRead<TReader>)an.Inner;
                }
                yield return res;
            }
        }

        public static IEnumerable<TData1?> ReadData<TData1, TReader>(
            this TReader reader, Dictionary<string, string> prefixes)
            where TReader : DbDataReader
            where TData1 : IActiveRead<TReader>, new()
        {
            var fac = new Dictionary<string, Func<IActiveRead<TReader>>>();
            foreach (var prefix in prefixes)
            {
                var table = prefix.Key;
                fac[table] = fac.Count switch
                {
                    0 => () => new TData1(),
                    _ => throw new InvalidOperationException($"{fac.Count} ?!")
                };
            }
            foreach (var item in reader.ReadData(prefixes, fac))
            {
                var first = item.Values.OfType<TData1>().FirstOrDefault();
                yield return first;
            }
        }

        public static IEnumerable<(TData1?, TData2?)?> ReadData<TData1, TData2, TReader>(
            this TReader reader, Dictionary<string, string> prefixes)
            where TReader : DbDataReader
            where TData1 : IActiveRead<TReader>, new()
            where TData2 : IActiveRead<TReader>, new()
        {
            var fac = new Dictionary<string, Func<IActiveRead<TReader>>>();
            foreach (var prefix in prefixes)
            {
                var table = prefix.Key;
                fac[table] = fac.Count switch
                {
                    0 => () => new TData1(),
                    1 => () => new TData2(),
                    _ => throw new InvalidOperationException($"{fac.Count} ?!")
                };
            }
            foreach (var item in reader.ReadData(prefixes, fac))
            {
                var first = item.Values.OfType<TData1>().FirstOrDefault();
                var second = item.Values.Except(new object?[] { first }).OfType<TData2>().FirstOrDefault();
                yield return (first, second);
            }
        }

        public static IEnumerable<(TData1?, TData2?, TData3?)?> ReadData<TData1, TData2, TData3, TReader>(
            this TReader reader, Dictionary<string, string> prefixes)
            where TReader : DbDataReader
            where TData1 : IActiveRead<TReader>, new()
            where TData2 : IActiveRead<TReader>, new()
            where TData3 : IActiveRead<TReader>, new()
        {
            var fac = new Dictionary<string, Func<IActiveRead<TReader>>>();
            foreach (var prefix in prefixes)
            {
                var table = prefix.Key;
                fac[table] = fac.Count switch
                {
                    0 => () => new TData1(),
                    1 => () => new TData2(),
                    2 => () => new TData3(),
                    _ => throw new InvalidOperationException($"{fac.Count} ?!")
                };
            }
            foreach (var item in reader.ReadData(prefixes, fac))
            {
                var first = item.Values.OfType<TData1>().FirstOrDefault();
                var second = item.Values.Except(new object?[] { first }).OfType<TData2>().FirstOrDefault();
                var third = item.Values.Except(new object?[] { first, second }).OfType<TData3>().FirstOrDefault();
                yield return (first, second, third);
            }
        }

        public static IEnumerable<(TData1?, TData2?, TData3?, TData4?)?> ReadData<TData1, TData2, TData3, TData4, TReader>(
            this TReader reader, Dictionary<string, string> prefixes)
            where TReader : DbDataReader
            where TData1 : IActiveRead<TReader>, new()
            where TData2 : IActiveRead<TReader>, new()
            where TData3 : IActiveRead<TReader>, new()
            where TData4 : IActiveRead<TReader>, new()
        {
            var fac = new Dictionary<string, Func<IActiveRead<TReader>>>();
            foreach (var prefix in prefixes)
            {
                var table = prefix.Key;
                fac[table] = fac.Count switch
                {
                    0 => () => new TData1(),
                    1 => () => new TData2(),
                    2 => () => new TData3(),
                    3 => () => new TData4(),
                    _ => throw new InvalidOperationException($"{fac.Count} ?!")
                };
            }
            foreach (var item in reader.ReadData(prefixes, fac))
            {
                var first = item.Values.OfType<TData1>().FirstOrDefault();
                var second = item.Values.Except(new object?[]{ first }).OfType<TData2>().FirstOrDefault();
                var third = item.Values.Except(new object?[] { first, second }).OfType<TData3>().FirstOrDefault();
                var four = item.Values.Except(new object?[] { first, second, third }).OfType<TData4>().FirstOrDefault();
                yield return (first, second, third, four);
            }
        }
    }
}
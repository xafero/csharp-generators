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
                    var table = prefixes.FirstOrDefault(p => p.Value == prefix).Key;
                    var item = res.TryGetValue(table, out var f) ? f : res[table] = fac[table]();
                    var subKey = tmp[1];
                    item.ReadSql(reader, subKey, index);
                }
                yield return res;
            }
        }

        public static IEnumerable<(TData1, TData2)> ReadData<TData1, TData2, TReader>(
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
                var first = item.Values.OfType<TData1>().First();
                var second = item.Values.OfType<TData2>().First();
                yield return (first, second);
            }
        }

        public static IEnumerable<(TData1, TData2, TData3)> ReadData<TData1, TData2, TData3, TReader>(
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
                var first = item.Values.OfType<TData1>().First();
                var second = item.Values.OfType<TData2>().First();
                var third = item.Values.OfType<TData3>().First();
                yield return (first, second, third);
            }
        }
    }
}
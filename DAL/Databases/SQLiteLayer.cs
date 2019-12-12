using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DAL.Databases
{
    public class SQLiteLayer<T> where T : class, new()
    {
        ConcurrentDictionary<string, T> dictionary = new ConcurrentDictionary<string, T>();

        public bool Save(string key, T content)
        {
            return dictionary.TryAdd(key, content);
        }

        public bool Update(string key, T newValue)
        {
            if (dictionary.TryGetValue(key, out T comparisonValue))
                return dictionary.TryUpdate(key, newValue, comparisonValue);
            else
                return false;
        }

        public bool Delete(string key)
        {
            return dictionary.TryRemove(key, out T value);
        }

        public List<T> QueryALL<TKey>(Func<T, TKey> keySelector)
        {
            List<T> list = new List<T>();

            foreach (var item in dictionary.ToArray())
            {
                list.Add(item.Value);
            }

            return list.OrderBy(keySelector).ToList();
        }

        private int CalcHash(T content)
        {
            return 0;
        }
    }
}

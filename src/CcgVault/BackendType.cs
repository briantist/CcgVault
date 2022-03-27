using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using YamlDotNet.Serialization;

namespace CcgVault
{
    interface IBackendType
    {
        string Name { get; }

        string Domain { get; }
        string Username { get; }
        string Password { get; }

        Task Retrieve();
    }

    abstract class BackendTypeConfig : IDictionary<string, object>
    {
        protected readonly Dictionary<string, object> _data; 

        public BackendTypeConfig(IDictionary<string, object> data)
        {
            _data = new Dictionary<string, object>(data);
        }

        public BackendTypeConfig() : this(new Dictionary<string, object>()) { }

        protected T PropertyOrDefault<T>(string key, T defaultValue = default)
        {
            if (ContainsKey(key))
            {
                return (T)this[key];
            }
            else
            {
                return defaultValue;
            }
        }

        protected object PropertyOrDefault(string key, object defaultValue = default)
        {
            return PropertyOrDefault<object>(key, defaultValue);
        }

        protected T PropertyOrDefaultWithNamingConvention<T>(string key, INamingConvention convention, T defaultValue = default)
        {
            if (ContainsKey(key))
            {
                return (T)this[key];
            }
            else
            {
                string transformed = convention.Apply(key);
                if (ContainsKey(transformed))
                {
                    return (T)this[transformed];
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        protected object PropertyOrDefaultWithNamingConvention(string key, INamingConvention convention, object defaultValue = default)
        {
            return PropertyOrDefaultWithNamingConvention<object>(key, convention, defaultValue);
        }

        protected object GetWithNamingConvention(string key, INamingConvention convention)
        {
            if (ContainsKey(key))
            {
                return this[key];
            }
            else
            {
                string transformed = convention.Apply(key);
                if (ContainsKey(transformed))
                {
                    return this[transformed];
                }
                throw new KeyNotFoundException($"Could not find key '{key}' or '{transformed}'.");
            }
        }

        protected T GetWithNamingConvention<T>(string key, INamingConvention convention)
        {
            return (T)GetWithNamingConvention(key, convention);
        }

        // interface implementations

        public object this[string key] { get => ((IDictionary<string, object>)_data)[key]; set => ((IDictionary<string, object>)_data)[key] = value; }

        public ICollection<string> Keys => ((IDictionary<string, object>)_data).Keys;

        public ICollection<object> Values => ((IDictionary<string, object>)_data).Values;

        public int Count => ((ICollection<KeyValuePair<string, object>>)_data).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, object>>)_data).IsReadOnly;

        public void Add(string key, object value)
        {
            ((IDictionary<string, object>)_data).Add(key, value);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((ICollection<KeyValuePair<string, object>>)_data).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, object>>)_data).Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)_data).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, object>)_data).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)_data).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, object>>)_data).GetEnumerator();
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, object>)_data).Remove(key);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)_data).Remove(item);
        }

        public bool TryGetValue(string key, out object value)
        {
            return ((IDictionary<string, object>)_data).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_data).GetEnumerator();
        }
    }
}

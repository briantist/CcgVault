using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.ObjectFactories;
using VaultSharp;
using YamlDotNet.Core.Events;
using System.Dynamic;
using YamlDotNet.Serialization.NodeTypeResolvers;

namespace CcgVault
{
    class Config
    {
        public class _Defaults
        {
            public Uri VaultAddr { get; set; }
            public _Auth Auth { get; set; }
        }

        public class _Auth
        {
            public string Type { get; set; }
            public IDictionary<string, object> Config { get; set; }
        }

        public class _Source
        {
            public string Type { get; set; }
            public ExpandoObject Config { get; set; }
        }

        private class ExpandoNodeTypeResolver : INodeTypeResolver
        {
            public bool Resolve(NodeEvent nodeEvent, ref Type currentType)
            {
                if (currentType == typeof(object))
                {
                    if (nodeEvent is SequenceStart)
                    {
                        currentType = typeof(List<object>);
                        return true;
                    }
                    if (nodeEvent is MappingStart)
                    {
                        currentType = typeof(ExpandoObject);
                        return true;
                    }
                }

                return false;
            }
        }

        private class ConfigFactory : IObjectFactory
        {
            private PluginInput _input;
            private IObjectFactory _fallback;

            public ConfigFactory(PluginInput input) : this(input, new DefaultObjectFactory()) { }

            public ConfigFactory(PluginInput input, IObjectFactory fallback)
            {
                _input = input;
                _fallback = fallback;
            }

            public object Create(Type type)
            {
                if (type == typeof(Config))
                {
                    return new Config(_input);
                }
                else
                {
                    return _fallback.Create(type);
                }
            }
        }

        public _Defaults Defaults { get; set; }
        public IDictionary<string, _Source> Sources { get; set; }

        private PluginInput _input;

        private string _backend_choice => _input.Arguments.First();

        public IBackendType GetBackend(IVaultClient client)
        {
            var source = Sources[_backend_choice];
            switch (source.Type)
            {
                case "kv1":
                    return new KV1BackendType(client, new KV1BackendConfig(source.Config));

                case "kv2":
                    return new KV2BackendType(client, new KV2BackendConfig(source.Config));

                default:
                    throw new NotImplementedException(message: $"Backend type {source.Type} is not implemented.");
            }
        }


        public Config(PluginInput input)
        {
            _input = input;
        }

        public static Config Load(PluginInput input)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithObjectFactory(new ConfigFactory(input))
                .WithNodeTypeResolver(
                    new ExpandoNodeTypeResolver(),
                    where: lss => lss.InsteadOf<DefaultContainersNodeTypeResolver>())
                .Build();

            using (var stream = new StreamReader(input.Path))
            {
                return deserializer.Deserialize<Config>(stream);
            }
        }
    }
}

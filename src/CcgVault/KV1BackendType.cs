using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.Commons;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CcgVault
{
    class KV1BackendConfig : BackendTypeConfig
    {
        public KV1BackendConfig(IDictionary<string, object> data) : base(data) { }
        public KV1BackendConfig() : base() { }

        public string Path
        {
            get => GetWithNamingConvention<string>("Path", UnderscoredNamingConvention.Instance);
            set => this["Path"] = value;
        }
        public string MountPoint
        {
            get => PropertyOrDefaultWithNamingConvention<string>("MountPoint", UnderscoredNamingConvention.Instance);
            set => this["MountPoint"] = value;
        }
    }

    class KV1BackendType : IBackendType
    {
        public string Name => "KV1";

        private IVaultClient _client;

        private Secret<Dictionary<string, object>> _data;

        public KV1BackendConfig Config { get; private set; }

        public string Domain => (string)_data.Data["Domain"];
        public string Username => (string)_data.Data["Username"];
        public string Password => (string)_data.Data["Password"];

        public KV1BackendType(IVaultClient client, KV1BackendConfig config)
        {
            _client = client;
            Config = config;
        }

        public async Task Retrieve()
        {
            _data = await _client.V1.Secrets.KeyValue.V1.ReadSecretAsync(Config.Path, Config.MountPoint);
        }
    }
}

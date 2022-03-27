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
    class KV2BackendConfig : BackendTypeConfig
    {
        public KV2BackendConfig(IDictionary<string, object> data) : base(data) { }
        public KV2BackendConfig() : base() { }

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

    class KV2BackendType : IBackendType
    {
        public string Name => "KV2";

        private IVaultClient _client;

        private Secret<SecretData<Dictionary<string, string>>> _data;

        public KV2BackendConfig Config { get; private set; }

        public string Domain => _data.Data.Data["Domain"];
        public string Username => _data.Data.Data["Username"];
        public string Password => _data.Data.Data["Password"];

        public KV2BackendType(IVaultClient client, KV2BackendConfig config)
        {
            _client = client;
            Config = config;
        }

        public async Task Retrieve()
        {
            _data = await _client.V1.Secrets.KeyValue.V2.ReadSecretAsync<Dictionary<string, string>>(Config.Path, mountPoint: Config.MountPoint);
        }
    }
}

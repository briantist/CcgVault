using System;
using System.Threading.Tasks;
using System.IO;

namespace CcgVault
{
    class TokenFromFileAuthType : IAuthType
    {
        public string FilePath { get; private set; }
        private string _token;
        private DateTime _lastKnownWriteTime;

        public TokenFromFileAuthType(string FilePath)
        {
            this.FilePath = FilePath;
            _lastKnownWriteTime = File.GetLastWriteTimeUtc(FilePath);
            RefreshAsync();
        }

        public Task RefreshAsync()
        {
            _token = File.ReadAllText(FilePath).Trim();
            return Task.CompletedTask;
        }

        public async Task<string> AuthenticateAsync()
        {
            var lastWrite = File.GetLastWriteTimeUtc(FilePath);
            if (lastWrite != _lastKnownWriteTime)
            {
                _lastKnownWriteTime = lastWrite;
                await RefreshAsync();
            }

            return _token;
        }
    }
}

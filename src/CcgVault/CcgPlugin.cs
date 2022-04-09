using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace CcgVault
{
    // See: https://docs.microsoft.com/en-us/windows/win32/api/ccgplugins/nf-ccgplugins-iccgdomainauthcredentials-getpasswordcredentials
    [Guid("6ECDA518-2010-4437-8BC3-46E752B7B172")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICcgDomainAuthCredentials
    {
        void GetPasswordCredentials(
            [MarshalAs(UnmanagedType.LPWStr), In] string pluginInput,
            [MarshalAs(UnmanagedType.LPWStr)] out string domainName,
            [MarshalAs(UnmanagedType.LPWStr)] out string username,
            [MarshalAs(UnmanagedType.LPWStr)] out string password);
    }

    [Guid("01BF101D-BFB6-433F-B416-02885CDC5AD3")]
    [ProgId("CcgCredProvider")]
    public class CcgPlugin : ServicedComponent, ICcgDomainAuthCredentials
    {
        public void GetPasswordCredentials(
            [MarshalAs(UnmanagedType.LPWStr), In] string pluginInput,
            [MarshalAs(UnmanagedType.LPWStr)] out string domainName,
            [MarshalAs(UnmanagedType.LPWStr)] out string username,
            [MarshalAs(UnmanagedType.LPWStr)] out string password)
        {
            var input = new PluginInput(pluginInput);
            var config = Config.Load(input);

            var tokenFromFile = new TokenFromFileAuthType((string)config.Defaults.Auth.Config["file_path"]);
            var getToken = tokenFromFile.AuthenticateAsync();
            Task.WaitAll(getToken);
            var token = getToken.Result;

            var authConfig = new TokenAuthMethodInfo(token);
            var clientSettings = new VaultClientSettings(
                vaultServerUriWithPort: config.Defaults.VaultAddr.ToString(),
                authMethodInfo: authConfig);

            var client = new VaultClient(clientSettings);

            var backend = config.GetBackend(client);

            var getInfo = backend.Retrieve();
            Task.WaitAll(getInfo);

            domainName = backend.Domain;
            username = backend.Username;
            password = backend.Password;
        }

        public string Ping()
        {
            return "Pong";
        }
    }
}

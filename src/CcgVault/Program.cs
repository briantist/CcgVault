using COMAdmin;
using Fclp;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CcgVault
{
    class Program
    {
        interface ICliCommand
        {
            void Invoke();
            void Help(string text);
        }

        class CliCommandTest : ICliCommand
        {
            public string Input { get; set; }

            public void Invoke()
            {
                using (CcgPlugin ccg = new CcgPlugin())
                {
                    string domain, user, pass;
                    ccg.GetPasswordCredentials(Input, out domain, out user, out pass);

                    Console.WriteLine($"Domain: {domain}\nUser: {user}\nPass: {pass}");
                }
            }

            public void Help(string text)
            {
                Console.WriteLine("ccgvault.exe test");
                Console.WriteLine();
                Console.WriteLine("Tests the credential process with the given input string.");
                Console.WriteLine();
                Console.WriteLine("The following options are accepted:");
                Console.WriteLine(text);
                Exit(-1);
            }
        }

        class CliCommandPing : ICliCommand
        {
            public void Help(string text)
            {
                Console.WriteLine("ccgvault.exe ping");
                Console.WriteLine();
                Console.WriteLine("Runs a simple method against the COM+ service, implicitly registering it in the process.");
                Console.WriteLine();
                Console.WriteLine("The following options are accepted:");
                Console.WriteLine(text);
                Exit(-1);
            }

            public void Invoke()
            {
                using (var ccg = new CcgPlugin())
                {
                    Console.WriteLine("Expecting response: 'Pong'");
                    Console.WriteLine($"Got response: '{ccg.Ping()}'");
                }
            }
        }

        class CliCommandRegistry : ICliCommand
        {
            public bool Permission { get; set; }
            public bool ComClass { get; set; }

            public void Help(string text)
            {
                Console.WriteLine("ccgvault.exe registry");
                Console.WriteLine();
                Console.WriteLine(@"Adds the CcgVault CLSID to the allowed CCG plugins and/or sets permission and ownership of the CCG\COMClasses key.");
                Console.WriteLine();
                Console.WriteLine("The following options are accepted:");
                Console.WriteLine(text);
                Exit(-1);
            }

            public void Invoke()
            {
                var subkey = @"SYSTEM\CurrentControlSet\Control\CCG\COMClasses";

                if (!(Permission || ComClass))
                {
                    Console.WriteLine("Neither --permission nor --comclass were specified. Choose one or both.");
                    Exit(-1);
                }

                RegistryKey key;

                if (Permission)
                {
                    using (new TokenPrivilege(TokenPrivilege.SE_TAKE_OWNERSHIP_NAME, TokenPrivilege.SE_RESTORE_NAME))
                    { 
                        key = Registry.LocalMachine.OpenSubKey(subkey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership);

                        if (key == null)
                        {
                            Console.WriteLine($"{subkey} not found or error opening key.");
                            Exit(-1);
                        }

                        const string SID_ADMINISTRATORS = "S-1-5-32-544";
                        var admins = new SecurityIdentifier(SID_ADMINISTRATORS).Translate(typeof(NTAccount));
                        var ac = key.GetAccessControl();
                        ac.SetOwner(admins);
                        key.SetAccessControl(ac);
                        key.Close();

                        key = Registry.LocalMachine.OpenSubKey(subkey, RegistryKeyPermissionCheck.ReadWriteSubTree, RegistryRights.TakeOwnership);
                        ac = key.GetAccessControl();
                        ac.AddAccessRule(new RegistryAccessRule(admins,
                            RegistryRights.FullControl,
                            InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                            PropagationFlags.None,
                            AccessControlType.Allow));
                        key.SetAccessControl(ac);
                        key.Close();
                    }
                }

                if (ComClass)
                {
                    var pluginId = (GuidAttribute)Attribute.GetCustomAttribute(typeof(CcgPlugin), typeof(GuidAttribute), true);

                    key = Registry.LocalMachine.OpenSubKey(subkey, RegistryKeyPermissionCheck.ReadWriteSubTree);
                    key.CreateSubKey($"{{{pluginId.Value}}}");
                    key.Close();
                }
            }
        }

        class CliCommandService : ICliCommand
        {
            public bool Install { get; set; }
            public bool Uninstall { get; set; }

            public void Help(string text)
            {
                Console.WriteLine("ccgvault.exe service");
                Console.WriteLine();
                Console.WriteLine("Installs or uninstalls a Windows (NT) service to associate with the COM+ application.");
                Console.WriteLine();
                Console.WriteLine("The following options are accepted:");
                Console.WriteLine(text);
                Exit(-1);
            }

            public void Invoke()
            {
                if (Install && Uninstall)
                {
                    Console.WriteLine("Both --install and --uninstall were specified. Choose one or the other.");
                    Exit(-1);
                }

                if (!(Install || Uninstall))
                {
                    Console.WriteLine("Neither --install nor --uninstall were specified. Choose one or the other.");
                    Exit(-1);
                }

                var appId = Assembly.GetExecutingAssembly().GetCustomAttribute<ApplicationIDAttribute>().Value;
                var serviceName = Assembly.GetExecutingAssembly().GetCustomAttribute<ApplicationNameAttribute>().Value;

                // https://docs.microsoft.com/en-us/windows/win32/api/comadmin/nf-comadmin-icomadmincatalog2-createserviceforapplication
                var oCatalog = (ICOMAdminCatalog2)Activator.CreateInstance(Type.GetTypeFromProgID("ComAdmin.COMAdminCatalog"));

                if (Install)
                {
                    try
                    {
                        oCatalog.CreateServiceForApplication(
                            bstrApplicationIDOrName: appId.ToString("B"), // Using an AppID, the GUID needs to be formatted with curly braces {}
                            bstrServiceName: serviceName,
                            bstrStartType: "SERVICE_DEMAND_START",
                            bstrErrorControl: "SERVICE_ERROR_NORMAL",
                            bstrDependencies: null,
                            bstrRunAs: null,
                            bstrPassword: null,
                            bDesktopOk: false);
                    }
                    catch (COMException e) when (e.HResult == -2147023823) // 0x80070431 ERROR_SERVICE_EXISTS
                    {
                        // ignore
                    }
                }

                if (Uninstall)
                { 
                    try
                    {
                        oCatalog.DeleteServiceForApplication(bstrApplicationIDOrName: appId.ToString("B")); // Using an AppID, the GUID needs to be formatted with curly braces {}
                    }
                    catch (COMException e) when (e.HResult == -2146368458) // 0x80110436 COMADMIN_E_SERVICENOTINSTALLED
                    {
                        // ignore
                    }
                }
            }
        }

        public static void Main(string[] args)
        {
            string cmd = "";
            string[] arguments = args;

            if (args.Length > 0 && !args[0].StartsWith("-", StringComparison.InvariantCulture) && !args[0].StartsWith("/", StringComparison.InvariantCulture))
            {
                cmd = args[0];
                arguments = arguments.Skip(1).ToArray();
            }

            ICommandLineParserResult result;
            ICliCommand command;

            switch (cmd)
            {
                case "test":
                    var _test = new FluentCommandLineParser<CliCommandTest>();

                    _test.SetupHelp("?", "help")
                        .Callback(text => _test.Object.Help(text));

                    _test.Setup(arg => arg.Input)
                        .As('i', "input")
                        .Required();

                    result = _test.Parse(arguments);
                    command = _test.Object;

                    break;

                case "ping":
                    var _ping = new FluentCommandLineParser<CliCommandPing>();

                    _ping.SetupHelp("?", "help")
                        .Callback(text => _ping.Object.Help(text));

                    result = _ping.Parse(arguments);
                    command = _ping.Object;
                    
                    break;

                case "service":
                    var _service = new FluentCommandLineParser<CliCommandService>();

                    _service.SetupHelp("?", "help")
                        .Callback(text => _service.Object.Help(text));

                    _service.Setup<bool>(arg => arg.Install)
                        .As('i', "install");

                    _service.Setup<bool>(arg => arg.Uninstall)
                        .As('u', "uninstall");

                    result = _service.Parse(arguments);
                    command = _service.Object;

                    break;
                        
                case "terminate":
                    // We don't use a using here, because once the server is terminated,
                    // the Dispose call is going to fail too. We're going to exit right
                    // after this anyway.
                    var ccg = new CcgPlugin();
                    try
                    {
                        ccg.Terminate();
                    }
                    catch (COMException e) when (e.HResult == -2147023170) // 0x800706BE
                    {
                        // this is expected; the server process will terminate,
                        // which will prevent the remote procedure call from completing.
                    }
                    Exit(0);
                    return;

                case "registry":
                    var _registry = new FluentCommandLineParser<CliCommandRegistry>();

                    _registry.SetupHelp("?", "help")
                        .Callback(text => _registry.Object.Help(text));

                    _registry.Setup<bool>(arg => arg.Permission)
                        .As('p', "permission");

                    _registry.Setup<bool>(arg => arg.ComClass)
                        .As('c', "comclass");

                    result = _registry.Parse(arguments);
                    command = _registry.Object;

                    break;

                case "help":
                case "":
                default:
                    Console.WriteLine("Usage:\n");
                    Console.WriteLine("ccgvault.exe <command> [--help|-?|[--opt1] [--opt2] ...]\n");
                    Console.WriteLine("Valid commands:\n");
                    Console.WriteLine("test");
                    Console.WriteLine("ping");
                    Console.WriteLine("service");
                    Console.WriteLine("terminate");
                    Console.WriteLine("registry");
                    Exit(0);
                    return;
            }

            if (result.HasErrors)
            {
                Console.WriteLine(result.ErrorText);
                Exit(-1);
            }

            command.Invoke();

            Exit();
        }

        static void Exit(int code = 0)
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Debugger is attached. Press any key to exit.");
                Console.ReadKey();
            }

            Environment.Exit(code);
        }
    }
}

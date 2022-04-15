using COMAdmin;
using Fclp;
using System;
using System.Diagnostics;
using System.EnterpriseServices;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

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
                        
                case "help":
                case "":
                default:
                    Console.WriteLine("Usage:\n");
                    Console.WriteLine("ccgvault.exe <command> [--help|-?|[--opt1] [--opt2] ...]\n");
                    Console.WriteLine("Valid commands:\n");
                    Console.WriteLine("test");
                    Console.WriteLine("ping");
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

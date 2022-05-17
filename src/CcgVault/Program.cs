using Fclp;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace CcgVault
{
    partial class Program
    {
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

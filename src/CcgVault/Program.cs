using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fclp;

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
                    var p = new FluentCommandLineParser<CliCommandTest>();

                    p.SetupHelp("?", "help")
                     .Callback(text => p.Object.Help(text));

                    p.Setup(arg => arg.Input)
                     .As('i', "input")
                     .Required();

                    result = p.Parse(arguments);
                    command = p.Object;

                    break;

                case "help":
                case "":
                default:
                    Console.WriteLine("Usage:\n");
                    Console.WriteLine("ccgvault.exe <command> [--help|-?|[--opt1] [--opt2] ...]\n");
                    Console.WriteLine("Valid commands:\n");
                    Console.WriteLine("test");
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

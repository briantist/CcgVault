using System;

namespace CcgVault
{
    partial class Program
    {
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
    }
}

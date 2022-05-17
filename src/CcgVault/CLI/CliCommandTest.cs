using System;

namespace CcgVault
{
    partial class Program
    {
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
    }
}

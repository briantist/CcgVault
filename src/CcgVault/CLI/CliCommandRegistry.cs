using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace CcgVault
{
    partial class Program
    {
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
    }
}

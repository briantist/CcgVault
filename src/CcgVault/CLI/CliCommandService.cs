using COMAdmin;
using System;
using System.EnterpriseServices;
using System.Reflection;
using System.Runtime.InteropServices;

namespace CcgVault
{
    partial class Program
    {
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
    }
}

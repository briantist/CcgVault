using System.EnterpriseServices;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CcgVault")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("CcgVault")]
[assembly: AssemblyCopyright("Copyright © 2022 Brian Scholer")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ApplicationName("CcgVault")]
[assembly: ApplicationActivation(ActivationOption.Server)]
[assembly: ApplicationID("2D79156B-1DE7-4A29-9428-7B7A591B176E")]
[assembly: ApplicationAccessControl(
    AccessChecksLevel = AccessChecksLevelOption.Application,
    Authentication = AuthenticationOption.None,
    Value = false)]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
//[assembly: ComVisible(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("f621ff11-c6f8-4cde-8b6c-0d411c55659d")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: InternalsVisibleTo(
    "Test, PublicKey=" +
        "0024000004800000940000000602000000240000525341310004000001000100adfa0f7741d088" +
        "7fb2527dc1eefa61008d1587bb0a4b5be8dcf11f86464fa6c57d3ae26eadd266e73d494eb4c46e" +
        "ff1b21ce79afc473dc2dd61644ed7b11802eec4e76bb11c37864f9bf6e67cf4e3298a2e41f806f" +
        "d68114f442fb9e93f6e30c33ff96337e19bbce84d4751ed984d0a9241f374ac3258b84cb6d7e49" +
        "85b44acc")]

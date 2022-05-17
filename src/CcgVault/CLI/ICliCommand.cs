namespace CcgVault
{
    partial class Program
    {
        interface ICliCommand
        {
            void Invoke();
            void Help(string text);
        }
    }
}

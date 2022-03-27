using System.Threading.Tasks;

namespace CcgVault
{
    interface IAuthType
    {
        Task<string> AuthenticateAsync();
        Task RefreshAsync();
    }
}

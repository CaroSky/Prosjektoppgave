using Blazor.Shared;
using System.Threading.Tasks;
using SharedModels.Entities;

namespace Blazor.Services
{
    public interface IAuthService
    {
        Task<LoginResult> Login(LoginModel loginModel);
        Task Logout();
        Task<RegisterResult> Register(RegisterModel registerModel);
    }
}
using api.models.DTO;
using api.models.Responses;

namespace api.services.Repositores
{
    public interface IUserRepository
    {
        Task<LoginResponse> Login(UserLoginDTO user);
        Task<GeneralResponse> Register(UserRegisterDTO user);
    }
}

using api.models.Entities;
using api.models.Responses;

namespace api.services.Repositores
{
    public interface IStatusRepository
    {
        Task<List<Status>> GetAll();
        Task<Status?> GetById(int id);
        Task<GeneralResponse> Create(Status status);
        Task<GeneralResponse> Update(int id, Status status);
        Task<GeneralResponse> Delete(int id);
    }
}

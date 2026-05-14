using api.models.Entities;
using api.models.Responses;

namespace api.services.Repositores
{
    public interface IAreaRepository
    {
        Task<List<Area>> GetAll();
        Task<Area?> GetById(int id);
        Task<GeneralResponse> Create(Area area);
        Task<GeneralResponse> Update(int id, Area area);
        Task<GeneralResponse> Delete(int id);
    }
}

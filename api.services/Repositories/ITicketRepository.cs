using api.models.DTO;
using api.models.Entities;
using api.models.Responses;

namespace api.services.Repositores
{
    public interface ITicketRepository
    {
        Task<List<Ticket>> GetAll();
        Task<Ticket?> GetById(int id);
        Task<GeneralResponse> Create(TicketDTO ticket);
        Task<GeneralResponse> Update(int id, TicketDTO ticket);
    }
}

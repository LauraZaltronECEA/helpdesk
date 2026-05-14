using api.models.DTO;
using api.models.Entities;
using api.models.Responses;
using api.services.Repositores;
using Microsoft.AspNetCore.Mvc;

namespace api.helpdesk.Controllers.v1;

[ApiController]
[Route("api/v1/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketRepository _ticketRepository;

    public TicketsController(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Ticket>>> GetAll()
    {
        return Ok(await _ticketRepository.GetAll());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Ticket>> GetById(int id)
    {
        var ticket = await _ticketRepository.GetById(id);

        if (ticket == null)
        {
            return NotFound(new GeneralResponse { Estado = false, Codigo = 0, Mensaje = "Ticket no encontrado" });
        }

        return Ok(ticket);
    }

    [HttpPost]
    public async Task<ActionResult<GeneralResponse>> Create([FromBody] TicketDTO ticket)
    {
        return Ok(await _ticketRepository.Create(ticket));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GeneralResponse>> Update(int id, [FromBody] TicketDTO ticket)
    {
        return Ok(await _ticketRepository.Update(id, ticket));
    }

}

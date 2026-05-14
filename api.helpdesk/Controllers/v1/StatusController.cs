using api.models.Entities;
using api.models.Responses;
using api.services.Repositores;
using Microsoft.AspNetCore.Mvc;

namespace api.helpdesk.Controllers.v1;

[ApiController]
[Route("api/v1/status")]
public class StatusController : ControllerBase
{
    private readonly IStatusRepository _statusRepository;

    public StatusController(IStatusRepository statusRepository)
    {
        _statusRepository = statusRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Status>>> GetAll()
    {
        return Ok(await _statusRepository.GetAll());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Status>> GetById(int id)
    {
        var status = await _statusRepository.GetById(id);

        if (status == null)
        {
            return NotFound(new GeneralResponse { Estado = false, Codigo = 0, Mensaje = "Status no encontrado" });
        }

        return Ok(status);
    }

    [HttpPost]
    public async Task<ActionResult<GeneralResponse>> Create([FromBody] Status status)
    {
        return Ok(await _statusRepository.Create(status));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GeneralResponse>> Update(int id, [FromBody] Status status)
    {
        return Ok(await _statusRepository.Update(id, status));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<GeneralResponse>> Delete(int id)
    {
        return Ok(await _statusRepository.Delete(id));
    }
}

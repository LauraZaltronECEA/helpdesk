using api.models.Entities;
using api.models.Responses;
using api.services.Repositores;
using Microsoft.AspNetCore.Mvc;

namespace api.helpdesk.Controllers.v1;

[ApiController]
[Route("api/v1/areas")]
public class AreasController : ControllerBase
{
    private readonly IAreaRepository _areaRepository;

    public AreasController(IAreaRepository areaRepository)
    {
        _areaRepository = areaRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<Area>>> GetAll()
    {
        return Ok(await _areaRepository.GetAll());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Area>> GetById(int id)
    {
        var area = await _areaRepository.GetById(id);

        if (area == null)
        {
            return NotFound(new GeneralResponse { Estado = false, Codigo = 0, Mensaje = "Area no encontrada" });
        }

        return Ok(area);
    }

    [HttpPost]
    public async Task<ActionResult<GeneralResponse>> Create([FromBody] Area area)
    {
        return Ok(await _areaRepository.Create(area));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<GeneralResponse>> Update(int id, [FromBody] Area area)
    {
        return Ok(await _areaRepository.Update(id, area));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<GeneralResponse>> Delete(int id)
    {
        return Ok(await _areaRepository.Delete(id));
    }
}

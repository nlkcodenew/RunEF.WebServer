using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunEF.WebServer.Application.Commands.Clients;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Queries.Clients;

namespace RunEF.WebServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAllClients()
    {
        try
        {
            var query = new GetAllClientsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetClientById(Guid id)
    {
        try
        {
            var query = new GetClientByIdQuery(id);
            var result = await _mediator.Send(query);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("online")]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetOnlineClients()
    {
        try
        {
            var query = new GetOnlineClientsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientRequest request)
    {
        try
        {
            var command = new CreateClientCommand(request);
            var result = await _mediator.Send(command);
            return result.Value != null 
                ? CreatedAtAction(nameof(GetClientById), new { id = result.Value.Id }, result.Value)
                : BadRequest(new { message = "Failed to create client" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClientDto>> UpdateClient(Guid id, [FromBody] UpdateClientRequest request)
    {
        try
        {
            var command = new UpdateClientCommand(id, request);
            var result = await _mediator.Send(command);
            return Ok(result.Value);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/block")]
    public async Task<ActionResult> BlockClient(Guid id)
    {
        try
        {
            var request = new BlockClientRequest { ComputerCode = "", IsBlocked = true };
            var command = new BlockClientCommand(id, request);
            await _mediator.Send(command);
            return Ok(new { message = "Client blocked successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/heartbeat")]
    public async Task<ActionResult> UpdateHeartbeat(string id)
    {
        try
        {
            var command = new UpdateHeartbeatCommand(id);
            await _mediator.Send(command);
            return Ok(new { message = "Heartbeat updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
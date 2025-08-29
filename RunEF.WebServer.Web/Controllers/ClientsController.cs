using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunEF.WebServer.Web.Models;
using RunEF.WebServer.Web.Services;
using MediatR;
using RunEF.WebServer.Application.Queries.Clients;
using RunEF.WebServer.Application.Commands.Clients;
using RunEF.WebServer.Application.DTOs;
using AutoMapper;

namespace RunEF.WebServer.Web.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ClientsController> _logger;
        private readonly IRealTimeService _realTimeService;
        private readonly IMapper _mapper;

        public ClientsController(IMediator mediator, ILogger<ClientsController> logger, IRealTimeService realTimeService, IMapper mapper)
        {
            _mediator = mediator;
            _logger = logger;
            _realTimeService = realTimeService;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var query = new GetAllClientsQuery();
                var result = await _mediator.Send(query);
                
                if (result.IsSuccess)
                {
                    var clients = _mapper.Map<List<ClientModel>>(result.Value);
                    await _realTimeService.SendSystemLog($"Loaded {clients.Count} clients");
                    return View(clients);
                }
                else
                {
                    await _realTimeService.SendSystemLog($"Failed to load clients: {result.Error}", "Error");
                    return View(new List<ClientModel>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading clients");
                await _realTimeService.SendSystemLog($"Error loading clients: {ex.Message}", "Error");
                return View(new List<ClientModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Block(int id)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                var request = new BlockClientRequest { ComputerCode = "", IsBlocked = true };
                var command = new BlockClientCommand(clientGuid.Value, request);
                var result = await _mediator.Send(command);
                
                if (result.IsSuccess)
                {
                    await _realTimeService.SendSystemLog($"Client {id} has been blocked", "Warning");
                    await _realTimeService.SendClientStatusChange(id, false);
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = result.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blocking client {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error blocking client {id}: {ex.Message}", "Error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(int id)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                var command = new UnblockClientCommand(clientGuid.Value);
                var result = await _mediator.Send(command);
                
                if (result.IsSuccess)
                {
                    await _realTimeService.SendSystemLog($"Client {id} has been unblocked", "Success");
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = result.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unblocking client {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error unblocking client {id}: {ex.Message}", "Error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateHeartbeat(int id)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                var clientQuery = new GetClientByIdQuery(clientGuid.Value);
                var clientResult = await _mediator.Send(clientQuery);
                
                if (!clientResult.IsSuccess)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                var command = new UpdateHeartbeatCommand(clientResult.Value.ComputerCode);
                var result = await _mediator.Send(command);
                
                if (result.IsSuccess)
                {
                    await _realTimeService.SendSystemLog($"Heartbeat updated for client {id}");
                    await _realTimeService.SendClientStatusChange(id, true);
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = result.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating heartbeat for client {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error updating heartbeat for client {id}: {ex.Message}", "Error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                var command = new DeleteClientCommand(clientGuid.Value);
                var result = await _mediator.Send(command);
                
                if (result.IsSuccess)
                {
                    await _realTimeService.SendSystemLog($"Client {id} has been deleted", "Warning");
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = result.Error });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting client {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error deleting client {id}: {ex.Message}", "Error");
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return NotFound();
                }
                
                var query = new GetClientByIdQuery(clientGuid.Value);
                var result = await _mediator.Send(query);
                
                if (result.IsSuccess)
                {
                    var client = _mapper.Map<ClientModel>(result.Value);
                    await _realTimeService.SendSystemLog($"Viewing details for client {id}");
                    return View(client);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading client details {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error loading client details {id}: {ex.Message}", "Error");
                return NotFound();
            }
        }
        
        private async Task<Guid?> GetClientGuidById(int id)
        {
            try
            {
                var query = new GetAllClientsQuery();
                var result = await _mediator.Send(query);
                
                if (result.IsSuccess)
                {
                    var client = result.Value.FirstOrDefault(c => c.Id.GetHashCode() == id);
                    return client?.Id;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
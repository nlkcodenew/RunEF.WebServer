using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
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

        [OutputCache(PolicyName = "Clients")]
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

        [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "id" })]
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
        
        [HttpPost]
        public async Task<IActionResult> Restart(int id)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                // Send restart command to client
                var success = await SendCommandToClient(clientGuid.Value, "restart");
                
                if (success)
                {
                    await _realTimeService.SendSystemLog($"Restart command sent to client {id}", "Info");
                    return Json(new { success = true, message = "Restart command sent successfully" });
                }
                
                return Json(new { success = false, message = "Failed to send restart command" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restarting client {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error restarting client {id}: {ex.Message}", "Error");
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> Shutdown(int id)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                // Send shutdown command to client
                var success = await SendCommandToClient(clientGuid.Value, "shutdown");
                
                if (success)
                {
                    await _realTimeService.SendSystemLog($"Shutdown command sent to client {id}", "Warning");
                    return Json(new { success = true, message = "Shutdown command sent successfully" });
                }
                
                return Json(new { success = false, message = "Failed to send shutdown command" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error shutting down client {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error shutting down client {id}: {ex.Message}", "Error");
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SendCommand(int id, [FromBody] SendCommandRequest request)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                if (string.IsNullOrWhiteSpace(request.Command))
                {
                    return Json(new { success = false, message = "Command cannot be empty" });
                }
                
                // Send custom command to client
                var result = await SendCommandToClientWithResult(clientGuid.Value, request.Command);
                
                if (result.Success)
                {
                    await _realTimeService.SendSystemLog($"Command '{request.Command}' sent to client {id}", "Info");
                    return Json(new { success = true, message = "Command sent successfully", result = result.Output });
                }
                
                return Json(new { success = false, message = result.ErrorMessage ?? "Failed to send command" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command to client {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error sending command to client {id}: {ex.Message}", "Error");
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> ControlFactory(int id, [FromBody] FactoryControlRequest request)
        {
            try
            {
                var clientGuid = await GetClientGuidById(id);
                if (clientGuid == null)
                {
                    return Json(new { success = false, message = "Client not found" });
                }
                
                if (string.IsNullOrWhiteSpace(request.Factory) || string.IsNullOrWhiteSpace(request.Action))
                {
                    return Json(new { success = false, message = "Factory and Action are required" });
                }
                
                // Validate factory (only B and D are supported)
                if (request.Factory.ToUpper() != "B" && request.Factory.ToUpper() != "D")
                {
                    return Json(new { success = false, message = "Only Factory B and D are supported" });
                }
                
                // Validate action
                var validActions = new[] { "start", "stop", "restart" };
                if (!validActions.Contains(request.Action.ToLower()))
                {
                    return Json(new { success = false, message = "Valid actions are: start, stop, restart" });
                }
                
                // Send factory control command to client
                var command = $"factory_{request.Factory.ToLower()}_{request.Action.ToLower()}";
                var result = await SendCommandToClientWithResult(clientGuid.Value, command);
                
                if (result.Success)
                {
                    await _realTimeService.SendSystemLog($"Factory {request.Factory} {request.Action} command sent to client {id}", "Info");
                    
                    // Simulate factory status update
                    var status = request.Action.ToLower() == "start" ? "Running" : 
                                request.Action.ToLower() == "stop" ? "Stopped" : "Restarting";
                    
                    return Json(new { 
                        success = true, 
                        message = $"Factory {request.Factory} {request.Action} command sent successfully",
                        status = status
                    });
                }
                
                return Json(new { success = false, message = result.ErrorMessage ?? "Failed to control factory" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error controlling factory on client {ClientId}", id);
                await _realTimeService.SendSystemLog($"Error controlling factory on client {id}: {ex.Message}", "Error");
                return Json(new { success = false, message = ex.Message });
            }
        }
        
        private async Task<bool> SendCommandToClient(Guid clientId, string command)
        {
            try
            {
                // In a real implementation, this would send the command to the actual client
                // For now, we'll simulate the command sending
                await Task.Delay(100); // Simulate network delay
                
                // Log the command
                _logger.LogInformation("Sending command '{Command}' to client {ClientId}", command, clientId);
                
                // Simulate success (in real implementation, this would depend on actual client response)
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send command '{Command}' to client {ClientId}", command, clientId);
                return false;
            }
        }
        
        private async Task<CommandResult> SendCommandToClientWithResult(Guid clientId, string command)
        {
            try
            {
                // In a real implementation, this would send the command to the actual client and wait for response
                // For now, we'll simulate the command execution
                await Task.Delay(200); // Simulate network delay and execution time
                
                // Log the command
                _logger.LogInformation("Sending command '{Command}' to client {ClientId}", command, clientId);
                
                // Simulate different responses based on command type
                var output = command.ToLower() switch
                {
                    "restart" => "System restart initiated",
                    "shutdown" => "System shutdown initiated",
                    var cmd when cmd.StartsWith("factory_") => $"Factory command '{command}' executed successfully",
                    _ => $"Command '{command}' executed successfully"
                };
                
                return new CommandResult
                {
                    Success = true,
                    Output = output
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send command '{Command}' to client {ClientId}", command, clientId);
                return new CommandResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
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
    
    // Request models for new endpoints
    public class SendCommandRequest
    {
        public string Command { get; set; } = string.Empty;
    }
    
    public class FactoryControlRequest
    {
        public string Factory { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
    
    public class CommandResult
    {
        public bool Success { get; set; }
        public string? Output { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
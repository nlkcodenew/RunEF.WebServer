using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunEF.WebServer.Web.Models;
using MediatR;
using RunEF.WebServer.Application.Queries.Clients;
using AutoMapper;

namespace RunEF.WebServer.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;
    private readonly IMapper _mapper;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger, IMapper mapper)
    {
        _mediator = mediator;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var model = new DashboardViewModel();

            // Get all clients using MediatR
            var allClientsResult = await _mediator.Send(new GetAllClientsQuery());
            if (allClientsResult.IsSuccess)
            {
                model.AllClients = _mapper.Map<List<ClientModel>>(allClientsResult.Value);
            }

            // Get online clients using MediatR
            var onlineClientsResult = await _mediator.Send(new GetOnlineClientsQuery());
            if (onlineClientsResult.IsSuccess)
            {
                model.OnlineClients = _mapper.Map<List<ClientModel>>(onlineClientsResult.Value);
            }

            // Add current web user as a client if authenticated
            var isWebUserLoggedIn = User.Identity?.IsAuthenticated ?? false;
            var webUserClientCount = isWebUserLoggedIn ? 1 : 0;
            
            model.TotalClients = model.AllClients.Count + webUserClientCount;
            model.OnlineClientsCount = model.OnlineClients.Count + webUserClientCount;
            model.OfflineClientsCount = model.TotalClients - model.OnlineClientsCount;

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return View(new DashboardViewModel());
        }
    }
}
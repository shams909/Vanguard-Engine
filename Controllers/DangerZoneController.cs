using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vanguard_Engine.Services;

namespace Vanguard_Engine.Controllers;

[Authorize(Roles = "Admin")]
public class DangerZoneController : BaseController
{
    private readonly IClientRequestService _clientRequestService;
    private readonly IVIPRequestService _vipRequestService;
    private readonly IUserService _userService;
    private readonly IAuditLogService _auditLog;

    public DangerZoneController(
        IClientRequestService clientRequestService,
        IVIPRequestService vipRequestService,
        IUserService userService,
        IAuditLogService auditLog)
    {
        _clientRequestService = clientRequestService;
        _vipRequestService = vipRequestService;
        _userService = userService;
        _auditLog = auditLog;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var clientReqs = await _clientRequestService.GetAllRequestsAsync();
        var vipReqs = await _vipRequestService.GetAllRequestsAsync();
        var users = await _userService.GetAllAsync(1, 1000); // Fetch up to 1000 users for purge view

        ViewBag.ClientRequests = clientReqs;
        ViewBag.VIPRequests = vipReqs;
        ViewBag.Users = users;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeClientRequest(string id)
    {
        var (success, error) = await _clientRequestService.DeleteRequestAsync(id);
        if (success)
            SetSuccess("Client Request and all linked shifts/applications purged permanently.");
        else
            SetError(error);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeVipRequest(string id)
    {
        var (success, error) = await _vipRequestService.DeleteRequestAsync(id);
        if (success)
            SetSuccess("VIP Request purged permanently. Assigned guards were released.");
        else
            SetError(error);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PurgeUser(string id)
    {
        var success = await _userService.DeleteAsync(id);
        if (success)
        {
            await _auditLog.LogAsync("User", id, "DangerZone_Purge", "system", null, "DELETED", "Hard deleted user and all associated records", "Admin");
            SetSuccess("User account and all linked records purged permanently.");
        }
        else
        {
            SetError("Failed to purge user or user not found.");
        }

        return RedirectToAction(nameof(Index));
    }
}


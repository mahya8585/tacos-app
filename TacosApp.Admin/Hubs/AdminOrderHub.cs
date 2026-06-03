using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TacosApp.Admin.Hubs;

[Authorize]
public sealed class AdminOrderHub : Hub
{
}

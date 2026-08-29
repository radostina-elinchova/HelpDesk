using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HelpDeskApp.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}

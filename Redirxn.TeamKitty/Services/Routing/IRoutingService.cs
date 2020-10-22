using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Routing
{
    public interface IRoutingService
    {
        Task GoBack();
        Task GoBackModal();
        Task NavigateTo(string route);
    }
}

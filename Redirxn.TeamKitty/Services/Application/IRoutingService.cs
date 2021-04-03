using Redirxn.TeamKitty.Views;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Application
{
    public interface IRoutingService
    {
        Task GoBack();
        Task GoBackModal();
        Task NavigateTo(string route);
    }
}

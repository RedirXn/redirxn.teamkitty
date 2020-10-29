using Redirxn.TeamKitty.Models;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public interface IKittyDataStore
    {
        Task<Kitty> GetKitty(string kittyId);
        Task SaveKittyToDb(Kitty kitty);                
    }
}

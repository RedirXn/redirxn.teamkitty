using Redirxn.TeamKitty.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public interface IJoinCodeDataStore
    {
        Task<string> SetNewJoinCode(string kittyId, string code);        
        Task<string> GetKittyIdWithCode(string joinCode);
        Task<List<JoinCode>> GetCodesByKittyId(string kittyId);
        Task DeleteCode(JoinCode c);
    }
}

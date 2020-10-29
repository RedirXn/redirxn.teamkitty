using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Logic
{
    public interface IInviteService
    {
        Task<string> GetJoinCode(string id);
        Task<string> GetKittyIdWithCode(string joinCode);
    }
}

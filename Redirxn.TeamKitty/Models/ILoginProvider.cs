using System;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Models
{
    public interface ILoginProvider
    {
        Task<Tuple<string,UserInfo>> GetEmailByLoggingIn();
    }
}
using System;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Models
{
    public interface ILoginProvider
    {
        Task<Tuple<Tuple<string,string>,UserInfo>> GetEmailByLoggingIn();
        Task<String> GetIdFromRefresh(string refreshToken);
    }
}
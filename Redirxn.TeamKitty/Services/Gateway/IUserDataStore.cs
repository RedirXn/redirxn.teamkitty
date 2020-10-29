using Redirxn.TeamKitty.Models;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public interface IUserDataStore
    {
        void Init(string activeToken);
        Task<UserInfo> GetUserDetail(string email);
        Task SaveUserDetailToDb(UserInfo userDetail);        
    }
}

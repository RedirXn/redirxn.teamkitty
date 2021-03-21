using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Redirxn.TeamKitty.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(ILoginProvider))]
namespace Redirxn.TeamKitty.Droid
{    
    public class AndroidLogin : ILoginProvider
    {
        const string clientId = "3e4mi6c6fvl6kuegcv4j1sdaea";
        const string domain = "teamkitty.auth.us-east-1.amazoncognito.com";
        const string callbackUri = "com.redirxn.teamkitty:/callback";
        //const string callbackUri = "https://teamkitty.auth.us-east-1.amazoncognito.com/android/com.redirxn.teamkitty/callback";
        const string responseType = "token";
        const string scope = "aws.cognito.signin.user.admin+email+openid+profile";
        
        public AndroidLogin() { }
        public async Task<Tuple<string, UserInfo>> GetEmailByLoggingIn()
        {
            var authResult = await WebAuthenticator.AuthenticateAsync(
                new Uri($"https://{domain}/login?client_id={clientId}&response_type={responseType}&scope={scope}&redirect_uri={callbackUri}"),
                new Uri(callbackUri)
                );

            
            var claims = JWT.JsonWebToken.Base64UrlDecode(authResult.IdToken.Split('.')[1]);
            var cString = System.Text.Encoding.UTF8.GetString(claims);

            var c = JsonConvert.DeserializeObject<AuthClaims>(cString);
            var u = new UserInfo { Id = c.Email.ToLower(), Name = c.NickName };

            return new Tuple<string, UserInfo>(authResult.IdToken, u);
        }

        private class AuthClaims
        {
            public string NickName;
            public string Email;
        }       
    }

}
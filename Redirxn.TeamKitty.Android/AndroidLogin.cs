﻿using System;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
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
        const string responseType = "token";
        const string scope = "aws.cognito.signin.user.admin+email+openid+profile";
        const string userPoolId = "us-east-1_EnMDRXOrG";


        public AndroidLogin() { }
        public async Task<Tuple<Tuple<string,string>, UserInfo>> GetEmailByLoggingIn()
        {
            WebAuthenticatorResult authResult;
            try
            {
                authResult = await WebAuthenticator.AuthenticateAsync(
                    new Uri($"https://{domain}/login?client_id={clientId}&response_type={responseType}&scope={scope}&redirect_uri={callbackUri}"),
                    new Uri(callbackUri)
                    );
            }
            
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                authResult = null;
            }

            if (authResult == null)
                return null;
            var claims = JWT.JsonWebToken.Base64UrlDecode(authResult.IdToken.Split('.')[1]);
            var cString = System.Text.Encoding.UTF8.GetString(claims);
            
            var c = JsonConvert.DeserializeObject<AuthClaims>(cString);
            var u = new UserInfo { Id = c.Email.ToLower(), Name = c.NickName };
            var tokens = new Tuple<string, string>(authResult.IdToken, authResult.RefreshToken);
            return new Tuple<Tuple<string, string>, UserInfo>(tokens, u);
        }

        public async Task<string> GetIdFromRefresh(string refreshToken)
        {
            var authReq = new AdminInitiateAuthRequest
            {
                UserPoolId = userPoolId,
                ClientId = clientId,
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH
            };
            
            AdminInitiateAuthResponse authResp = await new AmazonCognitoIdentityProviderClient().AdminInitiateAuthAsync(authReq);

            return authResp.AuthenticationResult.IdToken;
        }

        private class AuthClaims
        {
            public string NickName;
            public string Email;
        }       
    }

}
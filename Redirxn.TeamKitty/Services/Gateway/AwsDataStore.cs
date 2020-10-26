using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public class AwsDataStore : IDataStore, IDisposable
    {
        const string CognitoPoolId = "us-east-1:f779f47a-cfed-4016-a282-81f3313bd471";

        CognitoAWSCredentials _credentials;
        AmazonDynamoDBClient _client;
        DynamoDBContext _context;


        public void Dispose()
        {
            if (_context != null) _context.Dispose();
            if (_client != null) _client.Dispose();
            if (_credentials != null) _credentials.Dispose();
        }

        public void Init(string activeToken)
        {
            if (_context == null)
            {
                _credentials = new CognitoAWSCredentials(
                   CognitoPoolId,
                   RegionEndpoint.USEast1
                   );
                _credentials.AddLogin("graph.facebook.com", activeToken);

                _client = new AmazonDynamoDBClient(_credentials, RegionEndpoint.USEast1);
                _context = new DynamoDBContext(_client);
            }
        }

        public async Task<UserInfo> GetUserDetail(string email)
        {
            var u = await _context.LoadAsync<User>(email);
            if (u == null || u.Info == "{}") return null;
            var myInfo = JsonConvert.DeserializeObject<UserInfo>(u.Info);
            return myInfo;
        }

        public async Task<UserInfo> CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName)
        {
            var kitty = new Kitty { Id = loginData.Email + '|' + newKittyName, Config = "{}", LedgerSummary = "{}" };
            if (userDetail == null)
            {
                userDetail = new UserInfo { Id = loginData.Email, Name = loginData.Name, KittyNames = new[] { newKittyName }, DefaultKitty = newKittyName };
            }
            else if (userDetail.KittyNames == null)
            {
                userDetail.KittyNames = new[] { newKittyName };
                userDetail.DefaultKitty = newKittyName;
            }
            else
            {
                userDetail.KittyNames.Concat(new[] { newKittyName });
                userDetail.DefaultKitty = newKittyName;
            }
            var u = new User { Id = loginData.Email, Info = JsonConvert.SerializeObject(userDetail) };

            await _context.SaveAsync(kitty);
            await _context.SaveAsync(u);
            return userDetail;
        }

        public void SaveStockItem(string mainName, string mainNamePlural, string stockName, decimal price, decimal stockPrice)
        {
            
        }

        [DynamoDBTable("Kitties")]
        class Kitty
        {
            [DynamoDBHashKey]
            public string Id { get; set; }
            public string Config { get; set; }
            public string LedgerSummary { get; set; }
        }

        [DynamoDBTable("Users")]
        class User
        {
            [DynamoDBHashKey]
            public string Id { get; set; }
            public string Info { get; set; }
        }
    }
}

using Amazon;
using Amazon.CognitoIdentity;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Redirxn.TeamKitty.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Gateway
{
    public class DynamoDataStore : IKittyDataStore, IJoinCodeDataStore, IUserDataStore, IDisposable
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
            var u = await _context.LoadAsync<DynamoUser>(email);
            if (u == null || u.Info == "{}") return new UserInfo();
            var myInfo = JsonConvert.DeserializeObject<UserInfo>(u.Info);
            return myInfo;
        }
        public async Task SaveUserDetailToDb(UserInfo userDetail)
        {
            var u = new DynamoUser { Id = userDetail.Id, Info = JsonConvert.SerializeObject(userDetail) };
            await _context.SaveAsync(u);
        }
        public async Task<Kitty> GetKitty(string kittyId)
        {
            if (string.IsNullOrWhiteSpace(kittyId))
            {
                return null;
            }
            DynamoKitty kittyFromDb = await GetDynamoKittyAsync(kittyId);
            if (kittyFromDb == null)
            {
                return null;
            }
            var kitty = GetKittyFromDbKitty(kittyFromDb);
            return kitty;
        }

        private async Task<DynamoKitty> GetDynamoKittyAsync(string kittyId)
        {
            return await _context.LoadAsync<DynamoKitty>(kittyId) ?? new DynamoKitty();
        }
        public async Task SaveKittyToDb(Kitty kitty)
        {
            var kittyDb = GetDbKittyFromKitty(kitty);
            await _context.SaveAsync(kittyDb);
        }

        private DynamoKitty GetDbKittyFromKitty(Kitty kitty)
        {
            return new DynamoKitty
            {
                Id = kitty.Id,
                Config = JsonConvert.SerializeObject(kitty.KittyConfig),
                LedgerSummary = JsonConvert.SerializeObject(kitty.Ledger),
                Administrators = kitty.Administrators.ToList()
            };
        }

        private Kitty GetKittyFromDbKitty(DynamoKitty kittyDb)
        {
            return new Kitty
            {
                Id = kittyDb.Id,
                KittyConfig = JsonConvert.DeserializeObject<KittyConfig>(kittyDb.Config),
                Ledger = JsonConvert.DeserializeObject<Ledger>(kittyDb.LedgerSummary),
                Administrators = kittyDb.Administrators
            };
        }
        public async Task<string> SetNewJoinCode(string kittyId, string code)
        {
            var codeDb = new DynamoJoinCode
            {
                Id = code,
                KittyId = kittyId,
                Expiry = DateTime.Now.AddDays(1).ToString()
            };
            
            await _context.SaveAsync(codeDb);
            return codeDb.Id;
        }
                
        public async Task<List<JoinCode>> GetCodesByKittyId(string kittyId)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                TableName = "KittyCodes",
                IndexName = "KittyId-index",
                KeyConditionExpression = "KittyId = :id",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                    {":id", new AttributeValue { S =  kittyId } }
                },
                ScanIndexForward = true
            };

            var result = await _client.QueryAsync(queryRequest); 

            List<JoinCode> codes = new List<JoinCode>();
            foreach (var i in result.Items)
            {
                codes.Add(new JoinCode
                {
                    Code = i["Id"].S,
                    Expiry = DateTime.Parse(i["Expiry"].S)
                });
            }

            return codes;
        }

        public async Task<string> GetKittyIdWithCode(string joinCode)
        {
            var code = await _context.LoadAsync<DynamoJoinCode>(joinCode);
            return (DateTime.Parse(code?.Expiry) > DateTime.Now) ? code.KittyId : null;
        }
        public async Task DeleteCode(JoinCode code)
        {
            var c = new DynamoJoinCode
            {
                Id = code.Code,
            };
            await _context.DeleteAsync(c);
        }

        [DynamoDBTable("Kitties")]
        class DynamoKitty
        {
            [DynamoDBHashKey]
            public string Id { get; set; }
            public string Config { get; set; }
            public string LedgerSummary { get; set; }
            public List<string> Administrators { get; set; }
        }

        [DynamoDBTable("Users")]
        class DynamoUser
        {
            [DynamoDBHashKey]
            public string Id { get; set; }
            public string Info { get; set; }
        }

        [DynamoDBTable("KittyCodes")]
        class DynamoJoinCode
        {
            [DynamoDBHashKey]
            public string Id { get; set; }
            [DynamoDBGlobalSecondaryIndexHashKey]
            public string KittyId { get; set; }
            public string Expiry { get; set; }
        }

    }
}

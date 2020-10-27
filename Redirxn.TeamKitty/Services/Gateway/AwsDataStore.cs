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
            var u = await _context.LoadAsync<DynamoUser>(email);
            if (u == null || u.Info == "{}") return new UserInfo();
            var myInfo = JsonConvert.DeserializeObject<UserInfo>(u.Info);
            return myInfo;
        }
                
        public async Task<Kitty> GetKitty(string kittyId)
        {
            if (string.IsNullOrWhiteSpace(kittyId))
            {
                return new Kitty();
            }
            DynamoKitty kittyFromDb = await GetDynamoKittyAsync(kittyId);
            if (kittyFromDb == null)
            {
                return new Kitty();
            }
            var kitty = GetKittyFromDbKitty(kittyFromDb);
            return kitty;
        }

        private async Task<DynamoKitty> GetDynamoKittyAsync(string kittyId)
        {
            return await _context.LoadAsync<DynamoKitty>(kittyId) ?? new DynamoKitty();
        }

        public async Task<Kitty> CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName)
        {
            var kittyId = loginData.Email + '|' + newKittyName;
            var kittyDb = new DynamoKitty { Id = kittyId, Config = "{}", LedgerSummary = "{}", Administrators = new List<string> { loginData.Email } };
            if (userDetail == null || userDetail.Id == string.Empty)
            {
                userDetail = new UserInfo { Id = loginData.Email, Name = loginData.Name, KittyNames = new List<string> { kittyId }, DefaultKitty = kittyId };
            }
            else
            {
                userDetail.KittyNames.Add(kittyId);
                userDetail.DefaultKitty = kittyId;
            }
            var u = new DynamoUser { Id = loginData.Email, Info = JsonConvert.SerializeObject(userDetail) };

            await _context.SaveAsync(kittyDb);
            await _context.SaveAsync(u);

            return GetKittyFromDbKitty(kittyDb);
        }

        public async Task<Kitty> SaveStockItem(string kittyId, StockItem stockItem)
        {
            var kittyDb = await GetDynamoKittyAsync(kittyId);            
            var kitty = GetKittyFromDbKitty(kittyDb);
            
            if (kitty.KittyConfig.StockItems.Any(si => si.MainName == stockItem.MainName))
            {
                kitty.KittyConfig.StockItems = ReplaceStockItem(kitty.KittyConfig.StockItems, stockItem);
            }
            else
            {
                kitty.KittyConfig.StockItems = kitty.KittyConfig.StockItems.Concat(new[] { stockItem });
            }

            kittyDb = GetDbKittyFromKitty(kitty);
            
            await _context.SaveAsync(kittyDb);
            return kitty;
        }
        public async Task<Kitty> DeleteStockItem(string kittyId, string mainName)
        {
            var kittyDb = await GetDynamoKittyAsync(kittyId);
            var kitty = GetKittyFromDbKitty(kittyDb);

            if (!kitty.KittyConfig.StockItems.Any(si => si.MainName == mainName))
            {
                return kitty;
            }

            kitty.KittyConfig.StockItems = RemoveStockItem(kitty.KittyConfig.StockItems, mainName);

            kittyDb = GetDbKittyFromKitty(kitty);
            await _context.SaveAsync(kittyDb);
            return kitty;
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

        private IEnumerable<StockItem> ReplaceStockItem(IEnumerable<StockItem> enumerable, StockItem value)
        {
            return enumerable.Select(x => x.MainName == value.MainName ? value : x);
        }
        private IEnumerable<StockItem> RemoveStockItem(IEnumerable<StockItem> enumerable, string stockItemName)
        {
            return enumerable.Where((x) => x.MainName != stockItemName);
        }

        public async Task<string> SetNewJoinCode(string kittyId)
        {
            var codeDb = new JoinCode
            {
                Id = CreateJoinCode(),
                KittyId = kittyId,
                Expiry = DateTime.Now.AddDays(1).ToString()
            };
            
            await _context.SaveAsync(codeDb);
            return codeDb.Id;
        }

        public async Task<string> ResetJoinCode(string kittyId)
        {
            List<JoinCode> codes = await GetCodesByKittyId(kittyId);
            string keepCode = string.Empty;
            foreach (var c in codes)
            {
                await _context.DeleteAsync<JoinCode>(c);
                keepCode = c.Id;
            }
            if (string.IsNullOrEmpty(keepCode))
            {
                return await SetNewJoinCode(kittyId);
            }
            var codeDb = new JoinCode
            {
                Id = keepCode,
                KittyId = kittyId,
                Expiry = DateTime.Now.AddDays(1).ToString()
            };

            await _context.SaveAsync(codeDb);
            return codeDb.Id;
        }

        private async Task<List<JoinCode>> GetCodesByKittyId(string kittyId)
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
                    Id = i["Id"].S,
                    KittyId = i["KittyId"].S,
                    Expiry = i["Expiry"].S
                });
            }

            return codes;
        }

        private string CreateJoinCode()
        {
            var chars = "ABCDEFGHIJKLMNPQRSTUVWXYZ23456789";
            var stringChars = new char[6];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }

        public async Task<Kitty> JoinKittyWithCode(NetworkAuthData loginData, UserInfo userDetail, string joinCode)
        {
            var code = await _context.LoadAsync<JoinCode>(joinCode);
            if (code == null || DateTime.Parse(code.Expiry) < DateTime.Now)
            {
                return null;
            }
            var kitty = await GetKitty(code.KittyId);

            if (userDetail == null || userDetail.Id == string.Empty)
            {
                userDetail = new UserInfo { Id = loginData.Email, Name = loginData.Name, KittyNames = new List<string> { kitty.Id }, DefaultKitty = kitty.Id };
            }
            else
            {
                userDetail.KittyNames.Add(kitty.Id);
                userDetail.DefaultKitty = kitty.Id;
            }
            var u = new DynamoUser { Id = loginData.Email, Info = JsonConvert.SerializeObject(userDetail) };

            await _context.SaveAsync(u);

            return kitty;
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
        class JoinCode
        {
            [DynamoDBHashKey]
            public string Id { get; set; }
            [DynamoDBGlobalSecondaryIndexHashKey]
            public string KittyId { get; set; }
            public string Expiry { get; set; }
        }

    }
}

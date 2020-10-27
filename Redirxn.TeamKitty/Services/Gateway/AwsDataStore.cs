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
            var kitty = new Kitty
            {
                Id = kittyId,
                KittyConfig = JsonConvert.DeserializeObject<KittyConfig>(kittyFromDb.Config),
                Ledger = JsonConvert.DeserializeObject<Ledger>(kittyFromDb.LedgerSummary)
            };
            return kitty;
        }

        private async Task<DynamoKitty> GetDynamoKittyAsync(string kittyId)
        {
            return await _context.LoadAsync<DynamoKitty>(kittyId) ?? new DynamoKitty();
        }

        public async Task<Kitty> CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName)
        {
            var kittyId = loginData.Email + '|' + newKittyName;
            var kittyDb = new DynamoKitty { Id = kittyId, Config = "{}", LedgerSummary = "{}" };
            if (userDetail == null || userDetail.Id == string.Empty)
            {
                userDetail = new UserInfo { Id = loginData.Email, Name = loginData.Name, KittyNames = new[] { kittyId }, DefaultKitty = kittyId };
            }
            else if (userDetail.KittyNames == null)
            {
                userDetail.KittyNames = new[] { kittyId };
                userDetail.DefaultKitty = kittyId;
            }
            else
            {
                userDetail.KittyNames.Concat(new[] { kittyId });
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
                LedgerSummary = JsonConvert.SerializeObject(kitty.Ledger)
            };
        }

        private Kitty GetKittyFromDbKitty(DynamoKitty kittyDb)
        {
            return new Kitty
            {
                Id = kittyDb.Id,
                KittyConfig = JsonConvert.DeserializeObject<KittyConfig>(kittyDb.Config),
                Ledger = JsonConvert.DeserializeObject<Ledger>(kittyDb.LedgerSummary)
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

        [DynamoDBTable("Kitties")]
        class DynamoKitty
        {
            [DynamoDBHashKey]
            public string Id { get; set; }
            public string Config { get; set; }
            public string LedgerSummary { get; set; }
        }

        [DynamoDBTable("Users")]
        class DynamoUser
        {
            [DynamoDBHashKey]
            public string Id { get; set; }
            public string Info { get; set; }
        }
    }
}

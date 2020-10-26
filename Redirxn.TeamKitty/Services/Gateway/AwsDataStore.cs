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
            if (u == null || u.Info == "{}") return null;
            var myInfo = JsonConvert.DeserializeObject<UserInfo>(u.Info);
            return myInfo;
        }
                
        public async Task<Kitty> GetKitty(string kittyId)
        {
            DynamoKitty kittyFromDb = await GetDynamoKittyAsync(kittyId);
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
            return await _context.LoadAsync<DynamoKitty>(kittyId);
        }

        public async Task<UserInfo> CreateNewKitty(NetworkAuthData loginData, UserInfo userDetail, string newKittyName)
        {
            var kittyId = loginData.Email + '|' + newKittyName;
            var kitty = new DynamoKitty { Id = kittyId, Config = "{}", LedgerSummary = "{}" };
            if (userDetail == null)
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

            await _context.SaveAsync(kitty);
            await _context.SaveAsync(u);
            return userDetail;
        }

        public async Task<KittyConfig> SaveStockItem(string kittyName, StockItem stockItem)
        {
            var kitty = await GetDynamoKittyAsync(kittyName);

            var kittyConfig = JsonConvert.DeserializeObject<KittyConfig>(kitty.Config);
            
            if (kittyConfig == null)
            {
                kittyConfig = new KittyConfig();
            }
            if (kittyConfig.StockItems == null)
            {
                kittyConfig.StockItems = new List<StockItem>();
            }
            if (kittyConfig.StockItems.Any(si => si.MainName == stockItem.MainName))
            {
                kittyConfig.StockItems = ReplaceStockItem(kittyConfig.StockItems, stockItem);
            }
            else
            {
                kittyConfig.StockItems = kittyConfig.StockItems.Concat(new[] { stockItem });
            }
            kitty.Config = JsonConvert.SerializeObject(kittyConfig);
            
            await _context.SaveAsync(kitty);
            return kittyConfig;
        }

        private IEnumerable<StockItem> ReplaceStockItem(IEnumerable<StockItem> enumerable, StockItem value)
        {
            return enumerable.Select(x => x.MainName == value.MainName ? value : x);
        }
        private IEnumerable<StockItem> RemoveStockItem(IEnumerable<StockItem> enumerable, string stockItemName)
        {
            return enumerable.Where((x) => x.MainName != stockItemName);
        }

        public async Task DeleteStockItem(string kittyName, string mainName)
        {
            var kitty = await GetDynamoKittyAsync(kittyName);

            var kittyConfig = JsonConvert.DeserializeObject<KittyConfig>(kitty.Config);

            if (kittyConfig == null || kittyConfig.StockItems == null || !kittyConfig.StockItems.Any(si => si.MainName == mainName))
            {
                return;
            }
                
            kittyConfig.StockItems = RemoveStockItem(kittyConfig.StockItems, mainName);
            
            kitty.Config = JsonConvert.SerializeObject(kittyConfig);

            await _context.SaveAsync(kitty);            
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

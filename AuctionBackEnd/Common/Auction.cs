using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using AuctionBackEnd.Data;
using AuctionBackEnd.Database;
using DotNetEnv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AuctionBackEnd.Common
{
    public static class Auction
    {
        private static readonly string JsonFile = System.IO.File.ReadAllText("ja_jp.json");
        private static readonly Dictionary<string, string> Json = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonFile);
        public static async Task<IActionResult> ActiveAuctionList()
        {
            await using var context = new SqlDbContext();
            var data = context.Auctions.Where(x => !x.IsEnd);
            var dataList = data.ToList().Select(TranslateItemName);
            return new OkObjectResult(dataList);
        }

        public static async Task<IActionResult> GetAuction(string auctionUuid)
        {
            await using var context = new SqlDbContext();
            var data = context.Auctions.FirstOrDefault(x => x.AuctionUuid == auctionUuid);
            if (data == null)
            {
                return new BadRequestObjectResult("Invalid AuctionId");
            }
            
            return new OkObjectResult(TranslateItemName(data));
        }

        private static AuctionData TranslateItemName(AuctionData data)
        {
            var itemInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(data.ItemInfo);
            var translationKey = itemInfo["translationKey"].ToString();
            data.MaterialName = Json.ContainsKey(translationKey!) ? Json[translationKey] : translationKey;
            return data;
        }
        
        public static async Task<IActionResult> Bid(Bid data)
        {
            try
            {
                var userData = await Account.LoginWithToken(new Token(data.Token));
                if (userData is not OkObjectResult okObjectResult)
                {
                    return userData;
                }

                if (okObjectResult.Value is not Dictionary<string, string> dic)
                {
                    return new BadRequestObjectResult("Invalid Token");
                }
                var uuid = dic["uuid"];
                var name = dic["name"];
                
                uuid = Guid.Parse(uuid).ToString();
                
                await using var context = new SqlDbContext();
                await using var scope = await context.Database.BeginTransactionAsync();
                var auctionData = context.Auctions.FromSqlRaw("SELECT * FROM normal_auction_data WHERE auc_uuid = {0} FOR UPDATE", data.AuctionUuid).FirstOrDefault();
                if (auctionData == null)
                {
                    return new BadRequestObjectResult("Invalid AuctionId");
                }

                if (auctionData.IsEnd)
                {
                    return new BadRequestObjectResult("This auction is already end");
                }

                if (auctionData.SellerUuid == uuid)
                {
                    return new BadRequestObjectResult("You can't bid your auction");
                }

                if (auctionData.NowPrice >= data.Price)
                {
                    return new BadRequestObjectResult("Invalid price");
                }

                if (data.Price % auctionData.SplitMoney != 0)
                {
                    return new BadRequestObjectResult("Invalid price");
                }

                using var httpClient = new HttpClient();

                var response = await httpClient.PostAsync($"{Env.GetString("MAN10_BANK_API")}/bank/take",
                    new StringContent(JsonConvert.SerializeObject(new
                    {
                        uuid,
                        amount = data.Price,
                        plugin = "Man10Auction",
                        note = $"Auction Bid To {data.AuctionUuid}",
                        displaynote = "オークションで入札した"
                    }),Encoding.UTF8, "application/json"));
                
                if (!response.IsSuccessStatusCode)
                {
                    return new BadRequestObjectResult("Failed to take money");
                }
                
                if (auctionData.LastBidUuid != null)
                {
                    await httpClient.PostAsync($"{Env.GetString("MAN10_BANK_API")}/bank/add",
                        new StringContent(JsonConvert.SerializeObject(new
                        {
                            uuid = auctionData.LastBidUuid,
                            amount = data.Price,
                            plugin = "Man10Auction",
                            note = $"Auction Bid Cancel({data.AuctionUuid})",
                            displaynote = "オークションで他のプレイヤーが入札した"
                        }),Encoding.UTF8, "application/json"));
                }

                auctionData.LastBidUuid = uuid;
                auctionData.LastBidName = name;

                auctionData.NowPrice = data.Price;
                await context.SaveChangesAsync();
                await scope.CommitAsync();
                await httpClient.PostAsync($"{Env.GetString("DB_UPDATE_NOTICE_API")}/notice",
                    new StringContent("{\"auc_uuid\":\"" + data.AuctionUuid + "\"}",Encoding.UTF8, "application/json"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new BadRequestObjectResult("Failed to bid");
            }
            return new NoContentResult();
        }
    }
}
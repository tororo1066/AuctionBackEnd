using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AuctionBackEnd.Controllers;
using AuctionBackEnd.Data;
using AuctionBackEnd.Database;
using JWT.Algorithms;
using JWT.Builder;
using JWT.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AuctionBackEnd.Common
{
    public static class Account
    {
        public static async Task<IActionResult> SignUp(SignUpUser signUpUser)
        {
            if (signUpUser == null)
            {
                return new BadRequestObjectResult("Invalid client request");
            } 
            //uuidを取得する
            var uuid = await Utils.GetUuid(signUpUser.Name);
            if (uuid == null)
            {
                return new BadRequestObjectResult("Invalid Name");
            }
            await using var context = new SqlDbContext();
            var data = context.Clients.FirstOrDefault(
                x => x.LinkId == signUpUser.LinkId 
                     && x.Uuid.Replace("-","") == uuid);
            if (data == null)
            {
                return new BadRequestObjectResult("Invalid LinkId");
            }
                
            var (pass, salt) = Utils.GetHashWithSalt(signUpUser.Pass, null);
            data.Pass = pass;
            data.Salt = salt;
            await context.SaveChangesAsync();
            
            //jwtトークンを生成して返す
            var jwtToken = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(DotNetEnv.Env.GetString("JWT_SECRET_KEY"))
                .AddClaim("exp", DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeSeconds())
                .AddClaim("uuid", uuid)
                .AddClaim("pass", pass)
                .Encode();
            return new OkObjectResult(jwtToken);
        }

        public static async Task<IActionResult> Login(LoginUser loginUser)
        {
            if (loginUser == null)
            {
                return new BadRequestObjectResult("Invalid client request");
            }
            
            var uuid = await Utils.GetUuid(loginUser.Name);

            if (uuid == null)
            {
                return new BadRequestObjectResult("Invalid Name");
            }

            await using var context = new SqlDbContext();
            var data = context.Clients.FirstOrDefault(x => x.Uuid.Replace("-","") == uuid);
            if (data == null)
            {
                return new BadRequestObjectResult("Invalid Account");
            }

            var hashedPassword = Utils.GetHashWithSalt(loginUser.Pass, data.Salt).pass;
            if (data.Pass != hashedPassword)
            {
                return new BadRequestObjectResult("Invalid Password");
            }
            
            //jwtトークンを生成して返す
            var jwtToken = new JwtBuilder()
                .WithAlgorithm(new HMACSHA256Algorithm())
                .WithSecret(DotNetEnv.Env.GetString("JWT_SECRET_KEY"))
                .AddClaim("exp", DateTimeOffset.UtcNow.AddMonths(1).ToUnixTimeSeconds())
                .AddClaim("uuid", uuid)
                .AddClaim("pass", data.Pass)
                .Encode();
            
            return new OkObjectResult(jwtToken);
        }

        public static async Task<IActionResult> LoginWithToken(Token token)
        {
            try
            {
                var decode = new JwtBuilder()
                    .WithAlgorithm(new HMACSHA256Algorithm())
                    .WithSecret(DotNetEnv.Env.GetString("JWT_SECRET_KEY"))
                    .Decode(token.TokenString);

                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(decode);
                var uuid = json["uuid"].ToString();
                var pass = json["pass"].ToString();
                await using (var context = new SqlDbContext())
                {
                    var data = context.Clients.FirstOrDefault(x => 
                        x.Uuid.Replace("-","") == uuid
                        && x.Pass == pass);
                    if (data == null)
                    {
                        return new BadRequestObjectResult("Invalid Account");
                    }
                }

                var name = await Utils.GetMinecraftName(uuid);

                if (name == null)
                {
                    return new BadRequestObjectResult("Invalid Minecraft Account");
                }

                //nameとuuidを返す
                return new OkObjectResult(new Dictionary<string, string>
                {
                    {"name", name},
                    {"uuid", uuid}
                });
            }
            catch (Exception e)
            {
                return e is TokenExpiredException ? new BadRequestObjectResult("Token Expired") 
                    : new BadRequestObjectResult("Invalid Token");
            }
        }
    }
    
}
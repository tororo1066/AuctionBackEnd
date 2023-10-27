using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AuctionBackEnd
{
    public static class Utils
    {
        public static (string pass, string salt) GetHashWithSalt(string password, string salt)
        {
            salt ??= BCrypt.Net.BCrypt.GenerateSalt(12);


            return (BCrypt.Net.BCrypt.HashPassword(password, salt), salt);
        }

        public static async Task<string> GetUuid(string name)
        {
            string uuid;
            using var client = new HttpClient();
            try
            {
                var response = await client.GetAsync($"https://api.mojang.com/users/profiles/minecraft/{name}");
                var jsonString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                uuid = data["id"].ToString();
            }
            catch (Exception)
            {
                return null;
            }

            return uuid;
        }

        public static async Task<string> GetMinecraftName(string uuid)
        {
            string name;
            using var client = new HttpClient();
            try
            {
                var response =
                    await client.GetAsync($"https://sessionserver.mojang.com/session/minecraft/profile/{uuid}");
                var jsonString = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                name = data["name"].ToString();
            }
            catch (Exception)
            {
                return null;
            }

            return name;
        }
    }
}
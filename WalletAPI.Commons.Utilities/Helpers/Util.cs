using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WalletAPI.Services.Models;

namespace WalletAPI.Commons.Utilities.Helpers
{
    public static class Util
    {
        public static async Task<Root> APICall(string url)
        {
            var client = new HttpClient();
            var res = await client.GetAsync(url);
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Root>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }        
    }
}


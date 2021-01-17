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
            // Initializing HTTPClient
            var client = new HttpClient();
            // A Get request call from the url provided
            var res = await client.GetAsync(url);
            // Reading the content of the Get request call as a string
            var json = await res.Content.ReadAsStringAsync();
            // Deserializing our json into object and returning it
            return JsonSerializer.Deserialize<Root>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }        
    }
}


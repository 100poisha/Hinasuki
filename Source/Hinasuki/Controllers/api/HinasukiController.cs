using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Hinasuki.Controllers.api
{
    [Produces("application/json")]
    [Route("api/Hinasuki")]
    public class HinasukiController : Controller
    {
        private IMemoryCache MemoryCache { get; }

        public HinasukiController(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache;
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public Task<long> Get()
        {
            return MemoryCache.GetOrCreateAsync("Count", async entry =>
            {
                using (var connection = new SqlConnection(Settings.ConnectionString))
                {
                    var count = await connection.QuerySingleAsync<long>("SELECT MAX(Count) FROM Hinasuki");

                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3);

                    return count;
                }
            });
        }

        [HttpPost]
        public async Task Post()
        {
            using (var connection = new SqlConnection(Settings.ConnectionString))
            {
                await connection.ExecuteAsync("UPDATE Hinasuki SET Count = Count + 1");
            }
        }
    }
}
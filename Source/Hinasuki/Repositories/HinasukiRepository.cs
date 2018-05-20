using Dapper;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Hinasuki.Repositories
{
    public class HinasukiRepository
    {
        public HinasukiRepository(string connectionString)
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task AddCount()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                await connection.ExecuteAsync("UPDATE Hinasuki SET Count = Count + 1");
            }
        }

        public async Task<long> GetCount()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                return await connection.QuerySingleAsync<long>("SELECT MAX(Count) FROM Hinasuki");
            }
        }

        public string ConnectionString { get; }
    }
}

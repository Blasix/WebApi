using System.Data;
using Dapper;
using LU2.Models;
using Microsoft.Data.SqlClient;

namespace LU2.Repositories;

public interface IEnvironment2DRepository
{
    Task<IEnumerable<Environment2D>> GetByUserIdAsync(string userId);
    Task AddAsync(Environment2D environment);
    Task DeleteAsync(Guid id);
}

public class Environment2DRepository(string connectionString) : IEnvironment2DRepository
{

    public async Task<IEnumerable<Environment2D>> GetByUserIdAsync(string userId)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            return await db.QueryAsync<Environment2D>("SELECT * FROM Environments2D WHERE UserId = @UserId", new { UserId = userId });
        }
    }
    
    public async Task AddAsync(Environment2D environment)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            var sql = "INSERT INTO Environments2D (Id, Name, MaxHeight, MaxLength, UserId) VALUES (@Id, @Name, @MaxHeight, @MaxLength, @UserId)";
            await db.ExecuteAsync(sql, environment);
        }
    }
    
    public async Task DeleteAsync(Guid id)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            var sql = "DELETE FROM Environments2D WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = id });
        }
    }
}
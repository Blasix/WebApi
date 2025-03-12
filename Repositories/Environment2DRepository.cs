using System.Data;
using Dapper;
using ICT1._3_API.Models;
using Microsoft.Data.SqlClient;

namespace ICT1._3_API.Repositories;

public interface IEnvironment2DRepository
{
    Task<IEnumerable<Environment2D>> GetAllAsync();
    Task<Environment2D> GetByIdAsync(Guid id);
    Task<IEnumerable<Environment2D>> GetByUserIdAsync(string userId);
    Task AddAsync(Environment2D environment);
    Task UpdateAsync(Environment2D environment);
    Task DeleteAsync(Guid id);
}

public class Environment2DRepository(string connectionString) : IEnvironment2DRepository
{
    public async Task<IEnumerable<Environment2D>> GetAllAsync()
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            return await db.QueryAsync<Environment2D>("SELECT * FROM Environments2D");
        }
    }
    
    public async Task<Environment2D> GetByIdAsync(Guid id)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            return await db.QueryFirstOrDefaultAsync<Environment2D>("SELECT * FROM Environments2D WHERE Id = @Id", new { Id = id });
        }
    }

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

    public async Task UpdateAsync(Environment2D environment)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            var sql = "UPDATE Environments2D SET Name = @Name, MaxHeight = @MaxHeight, MaxLength = @MaxLength, UserId = @UserId WHERE Id = @Id";
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
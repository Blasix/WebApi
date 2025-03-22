using System.Data;
using Dapper;
using LU2.Models;
using Microsoft.Data.SqlClient;

namespace LU2.Repositories;

public interface IObject2DRepository
{
    Task<Object2D> GetByIdAsync(Guid id);
    Task<IEnumerable<Object2D>> GetByEnvironmentIdAsync(Guid environmentId);
    Task AddAsync(Object2D object2D);
    Task DeleteAsync(Guid id);
    Task DeleteByEnvironmentIdAsync(Guid environmentId);
}

public class Object2DRepository(string connectionString) : IObject2DRepository
{
    public async Task<Object2D> GetByIdAsync(Guid id)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            return await db.QueryFirstOrDefaultAsync<Object2D>("SELECT * FROM Object2D WHERE Id = @Id", new { Id = id });
        }
    }

    public async Task<IEnumerable<Object2D>> GetByEnvironmentIdAsync(Guid environmentId)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            return await db.QueryAsync<Object2D>("SELECT * FROM Object2D WHERE EnvironmentId = @EnvironmentId", new { EnvironmentId = environmentId });
        }
    }
    
    public async Task AddAsync(Object2D object2D)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            var sql = "INSERT INTO Object2D (Id, PrefabId, PositionX, PositionY, ScaleX, ScaleY, RotationZ, SortingLayer, EnvironmentId) VALUES (@Id, @PrefabId, @PositionX, @PositionY, @ScaleX, @ScaleY, @RotationZ, @SortingLayer, @EnvironmentId)";
            await db.ExecuteAsync(sql, object2D);
        }
    }
    
    public async Task DeleteAsync(Guid id)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            var sql = "DELETE FROM Object2D WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = id });
        }
    }
    
    public async Task DeleteByEnvironmentIdAsync(Guid environmentId)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            var sql = "DELETE FROM Object2D WHERE EnvironmentId = @EnvironmentId";
            await db.ExecuteAsync(sql, new { EnvironmentId = environmentId });
        }
    }
}
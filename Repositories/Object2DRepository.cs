using System.Data;
using Dapper;
using ICT1._3_API.Models;
using Microsoft.Data.SqlClient;

namespace ICT1._3_API.Repositories;

public class Object2DRepository(string connectionString)
{
    public async Task<IEnumerable<Object2D>> GetAllAsync()
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            return await db.QueryAsync<Object2D>("SELECT * FROM Object2D");
        }
    }
    
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

    public async Task UpdateAsync(Object2D object2D)
    {
        using (IDbConnection db = new SqlConnection(connectionString))
        {
            var sql = "UPDATE Object2D SET PrefabId = @PrefabId, PositionX = @PositionX, PositionY = @PositionY, ScaleX = @ScaleX, ScaleY = @ScaleY, RotationZ = @RotationZ, SortingLayer = @SortingLayer, EnvironmentId = @EnvironmentId WHERE Id = @Id";
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
}
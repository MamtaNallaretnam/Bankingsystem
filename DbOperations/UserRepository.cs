﻿using BankingSystem.DBContext;
using BankingSystem.Models;
using MySqlConnector;

namespace BankingSystem.DbOperations;

public interface IUserRepository : IRepository<User>
{
    Task<User?> AuthenticateUser(string username, string password);
    Task SignInAsync(User user);
}

public class UserRepository : Repository, IUserRepository
{
    private readonly AppDbContext _dbContext;

    public UserRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<IEnumerable<User>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<User> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task CreateAsync(User entity)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(User entity)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        var connection = _dbContext.GetConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"DELETE FROM user WHERE user_id = @u;";
        cmd.Parameters.AddWithValue("u", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<User?> ReadUserAsync(MySqlDataReader reader)
    {
        
        // using reader make a new user
        if (await reader.ReadAsync() == false)
            return null;
        var user = new User();
        user.UserId = reader.GetInt32("user_id");
        user.UserName = reader.GetString("user_name");
        user.UserType = (UserType) reader.GetInt16("user_type");
        user.LastLoginTimestamp = reader.GetDateTime("last_login_timestamp");
        return user;
    }

    public async Task<User?> AuthenticateUser(string username, string password)
    {
        var connection = _dbContext.GetConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = @"SELECT * FROM user 
                        WHERE user_name = @u AND password_hash = SHA2(@p,256);";
        cmd.Parameters.AddWithValue("u", username);
        cmd.Parameters.AddWithValue("p", password);
        using var reader = await cmd.ExecuteReaderAsync();
        return await ReadUserAsync(reader);
    }

    public async Task SignInAsync(User user)
    {
        var connection = _dbContext.GetConnection();
        await using var cmd = connection.CreateCommand();
        // set last login time
        cmd.CommandText = @"UPDATE user SET last_login_timestamp = @l WHERE user_id = @u;";
        cmd.Parameters.AddWithValue("l", DateTime.Now);
        cmd.Parameters.AddWithValue("o", new Random().Next(100000, 999999));
        cmd.Parameters.AddWithValue("u", user.UserId);
        await cmd.ExecuteNonQueryAsync();
    }
}

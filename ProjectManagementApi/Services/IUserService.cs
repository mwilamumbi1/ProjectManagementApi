

using IdentityServer3.Core.Services;
using Microsoft.Data.SqlClient;

using Microsoft.EntityFrameworkCore;
using ProjectManagementApi.DataContext;
using System.Data;



namespace ProjectManagementApi.Services

{

    public class UserService : IUserService

    {

        private readonly PMDataContext _context;



        public UserService(PMDataContext context)

        {

            _context = context;

        }



        public async Task<List<string>> GetPermissionsByUserIdAsync(int userId)

        {

            var permissions = new List<string>();



            using var command = _context.Database.GetDbConnection().CreateCommand();

            command.CommandText = "core.GetPermissionsByUserId"; // Replace with your actual stored procedure name

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@UserID", userId));



            if (command.Connection.State != ConnectionState.Open)

            {

                await command.Connection.OpenAsync();

            }



            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())

            {

                permissions.Add(reader.GetString(reader.GetOrdinal("PermissionName")));

            }



            return permissions;

        }

    }

}
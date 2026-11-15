using Npgsql;

namespace Homework2.Infrastructure.Helpers;

public interface IPostgresConnectionFactory
{
    NpgsqlConnection GetConnection();
}

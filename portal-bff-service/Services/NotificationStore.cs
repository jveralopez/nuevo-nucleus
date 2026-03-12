using System.Data;
using Microsoft.Data.Sqlite;

namespace PortalBffService.Services;

public class NotificationStore
{
    private readonly string _connectionString;

    public NotificationStore(string connectionString)
    {
        _connectionString = connectionString;
        EnsureCreated();
    }

    public IReadOnlyList<NotificationRecord> GetByUser(string userId, bool unreadOnly, int limit, int offset)
    {
        var items = new List<NotificationRecord>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT Id, UserId, Title, Detail, CreatedAt, ReadAt
FROM notifications
WHERE UserId = $userId
  AND ($unreadOnly = 0 OR ReadAt IS NULL)
ORDER BY CreatedAt DESC
LIMIT $limit OFFSET $offset;";
        command.Parameters.AddWithValue("$userId", userId);
        command.Parameters.AddWithValue("$unreadOnly", unreadOnly ? 1 : 0);
        command.Parameters.AddWithValue("$limit", limit);
        command.Parameters.AddWithValue("$offset", offset);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new NotificationRecord(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                DateTimeOffset.Parse(reader.GetString(4)),
                reader.IsDBNull(5) ? null : DateTimeOffset.Parse(reader.GetString(5))));
        }

        return items;
    }

    public void Add(NotificationRecord record)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "INSERT OR IGNORE INTO notifications (Id, UserId, Title, Detail, CreatedAt, ReadAt) VALUES ($id, $userId, $title, $detail, $createdAt, $readAt)";
        command.Parameters.AddWithValue("$id", record.Id);
        command.Parameters.AddWithValue("$userId", record.UserId);
        command.Parameters.AddWithValue("$title", record.Title);
        command.Parameters.AddWithValue("$detail", record.Detail);
        command.Parameters.AddWithValue("$createdAt", record.CreatedAt.ToString("O"));
        var readAt = command.Parameters.Add("$readAt", SqliteType.Text);
        readAt.Value = (object?)record.ReadAt?.ToString("O") ?? DBNull.Value;
        command.ExecuteNonQuery();
    }

    public bool MarkRead(string userId, string id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE notifications SET ReadAt = $readAt WHERE Id = $id AND UserId = $userId";
        command.Parameters.AddWithValue("$readAt", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$id", id);
        command.Parameters.AddWithValue("$userId", userId);
        return command.ExecuteNonQuery() > 0;
    }

    public int MarkAllRead(string userId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "UPDATE notifications SET ReadAt = $readAt WHERE UserId = $userId AND ReadAt IS NULL";
        command.Parameters.AddWithValue("$readAt", DateTimeOffset.UtcNow.ToString("O"));
        command.Parameters.AddWithValue("$userId", userId);
        return command.ExecuteNonQuery();
    }

    public NotificationSummary GetSummary(string userId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT
  COUNT(*) AS Total,
  SUM(CASE WHEN ReadAt IS NULL THEN 1 ELSE 0 END) AS Unread
FROM notifications
WHERE UserId = $userId;";
        command.Parameters.AddWithValue("$userId", userId);
        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        if (!reader.Read())
        {
            return new NotificationSummary(0, 0);
        }

        var total = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
        var unread = reader.IsDBNull(1) ? 0 : reader.GetInt64(1);
        return new NotificationSummary((int)total, (int)unread);
    }

    public void Clear(string userId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM notifications WHERE UserId = $userId";
        command.Parameters.AddWithValue("$userId", userId);
        command.ExecuteNonQuery();
    }

    private void EnsureCreated()
    {
        var builder = new SqliteConnectionStringBuilder(_connectionString);
        if (!string.IsNullOrWhiteSpace(builder.DataSource))
        {
            var dir = Path.GetDirectoryName(builder.DataSource);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"
CREATE TABLE IF NOT EXISTS notifications (
  Id TEXT PRIMARY KEY,
  UserId TEXT NOT NULL,
  Title TEXT NOT NULL,
  Detail TEXT NOT NULL,
  CreatedAt TEXT NOT NULL,
  ReadAt TEXT NULL
);
";
        command.ExecuteNonQuery();
    }
}

public record NotificationRecord(
    string Id,
    string UserId,
    string Title,
    string Detail,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);

public record NotificationSummary(int Total, int Unread);

using Microsoft.Data.Sqlite;

namespace MasterLink.Desktop.Data;

public static class SystemRepository
{
    private static readonly HashSet<string> EditableFields = new() { "name", "description", "notes" };

    public static SystemItem Add(SqliteConnection c, int boardId, string name)
    {
        var position = MaxPosition(c, boardId) + 1;
        var finalName = name.Trim().Length == 0 ? "New System" : name.Trim();
        using var cmd = c.CreateCommand();
        cmd.CommandText = """
            INSERT INTO SystemItem (BoardId, Position, Name) VALUES ($boardId, $position, $name);
            SELECT last_insert_rowid();
            """;
        cmd.Parameters.AddWithValue("$boardId", boardId);
        cmd.Parameters.AddWithValue("$position", position);
        cmd.Parameters.AddWithValue("$name", finalName);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        return new SystemItem { Id = id, BoardId = boardId, Position = position, Name = finalName };
    }

    public static string? UpdateField(SqliteConnection c, int systemId, string field, string? value)
    {
        if (!EditableFields.Contains(field)) return "Not an editable field";

        using var cmd = c.CreateCommand();
        cmd.CommandText = $"UPDATE SystemItem SET {ColumnFor(field)} = $value WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", systemId);
        cmd.Parameters.AddWithValue("$value", (object?)value ?? DBNull.Value);
        cmd.ExecuteNonQuery();
        return null;
    }

    public static void Delete(SqliteConnection c, int systemId)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "DELETE FROM SystemItem WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", systemId);
        cmd.ExecuteNonQuery();
    }

    public static List<SystemTree> GetTreesForBoard(SqliteConnection c, int boardId)
    {
        var systems = new List<SystemItem>();
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = "SELECT Id, BoardId, Position, Name, Description, Notes FROM SystemItem WHERE BoardId = $boardId ORDER BY Position ASC;";
            cmd.Parameters.AddWithValue("$boardId", boardId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                systems.Add(new SystemItem
                {
                    Id = reader.GetInt32(0),
                    BoardId = reader.GetInt32(1),
                    Position = reader.GetInt32(2),
                    Name = reader.GetString(3),
                    Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                });
            }
        }

        return systems
            .Select(s => new SystemTree { System = s, Blocks = BlockRepository.ListForSystem(c, s.Id) })
            .ToList();
    }

    private static int MaxPosition(SqliteConnection c, int boardId)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT MAX(Position) FROM SystemItem WHERE BoardId = $boardId;";
        cmd.Parameters.AddWithValue("$boardId", boardId);
        var result = cmd.ExecuteScalar();
        return result is DBNull or null ? -1 : Convert.ToInt32(result);
    }

    private static string ColumnFor(string field) => field switch
    {
        "name" => "Name",
        "description" => "Description",
        "notes" => "Notes",
        _ => throw new ArgumentOutOfRangeException(nameof(field)),
    };
}

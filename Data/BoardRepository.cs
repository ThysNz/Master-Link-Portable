using Microsoft.Data.Sqlite;

namespace MasterLink.Desktop.Data;

public static class BoardRepository
{
    public record OpResult(Board? Board, string? Error);

    public static Board? Find(SqliteConnection c, string name)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Position FROM Board WHERE Name = $name;";
        cmd.Parameters.AddWithValue("$name", name);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public static Board? FindById(SqliteConnection c, int id)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Position FROM Board WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? Map(reader) : null;
    }

    public static OpResult Create(SqliteConnection c, string name)
    {
        name = name.Trim();
        if (name.Length == 0) return new OpResult(null, "Name is required");
        if (Find(c, name) != null) return new OpResult(null, "A board with that name already exists");

        var position = MaxPosition(c) + 1;
        using var cmd = c.CreateCommand();
        cmd.CommandText = "INSERT INTO Board (Name, Position) VALUES ($name, $position); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$name", name);
        cmd.Parameters.AddWithValue("$position", position);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        return new OpResult(new Board { Id = id, Name = name, Position = position }, null);
    }

    public static OpResult Rename(SqliteConnection c, int boardId, string newName)
    {
        newName = newName.Trim();
        var existing = FindById(c, boardId);
        if (existing == null) return new OpResult(null, "Board not found");
        if (newName.Length == 0) return new OpResult(null, "Name is required");
        if (newName != existing.Name && Find(c, newName) != null)
            return new OpResult(null, "A board with that name already exists");

        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE Board SET Name = $newName WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$newName", newName);
        cmd.Parameters.AddWithValue("$id", boardId);
        cmd.ExecuteNonQuery();

        existing.Name = newName;
        return new OpResult(existing, null);
    }

    public static void Delete(SqliteConnection c, int boardId)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "DELETE FROM Board WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", boardId);
        cmd.ExecuteNonQuery();
    }

    public static List<(Board Board, int SystemCount)> List(SqliteConnection c)
    {
        var boards = new List<Board>();
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = "SELECT Id, Name, Position FROM Board ORDER BY Position ASC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) boards.Add(Map(reader));
        }

        var result = new List<(Board, int)>();
        foreach (var b in boards)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM SystemItem WHERE BoardId = $boardId;";
            cmd.Parameters.AddWithValue("$boardId", b.Id);
            var count = System.Convert.ToInt32(cmd.ExecuteScalar());
            result.Add((b, count));
        }
        return result;
    }

    private static int MaxPosition(SqliteConnection c)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT MAX(Position) FROM Board;";
        var result = cmd.ExecuteScalar();
        return result is DBNull or null ? -1 : Convert.ToInt32(result);
    }

    private static Board Map(SqliteDataReader r) => new()
    {
        Id = r.GetInt32(0),
        Name = r.GetString(1),
        Position = r.GetInt32(2),
    };
}

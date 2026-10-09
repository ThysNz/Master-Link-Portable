using Microsoft.Data.Sqlite;

namespace MasterLink.Desktop.Data;

// Blocks plus their two child tables. Table/column names are fixed,
// code-only strings (never user input), so building SQL with them is safe -
// same approach as Master Idea's TextRowRepository.
public static class BlockRepository
{
    private static readonly HashSet<string> EnvColumns = new() { "Application", "Path", "UserName", "Password" };
    private static readonly HashSet<string> CommandColumns = new() { "Command", "Purpose" };

    public static List<BlockTree> ListForSystem(SqliteConnection c, int systemId)
    {
        var blocks = new List<Block>();
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = "SELECT Id, SystemId, Position, Title FROM Block WHERE SystemId = $id ORDER BY Position ASC, Id ASC;";
            cmd.Parameters.AddWithValue("$id", systemId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                blocks.Add(new Block { Id = r.GetInt32(0), SystemId = r.GetInt32(1), Position = r.GetInt32(2), Title = r.GetString(3) });
        }

        return blocks.Select(b => new BlockTree
        {
            Block = b,
            Env = ListEnv(c, b.Id),
        }).ToList();
    }

    public static Block AddBlock(SqliteConnection c, int systemId)
    {
        var position = MaxPosition(c, "Block", "SystemId", systemId) + 1;
        var id = Insert(c, "INSERT INTO Block (SystemId, Position) VALUES ($p, $pos);", systemId, position);
        return new Block { Id = id, SystemId = systemId, Position = position };
    }

    public static void UpdateTitle(SqliteConnection c, int blockId, string title) =>
        Exec(c, "UPDATE Block SET Title = $v WHERE Id = $id;", blockId, title);

    public static void DeleteBlock(SqliteConnection c, int blockId) =>
        Exec(c, "DELETE FROM Block WHERE Id = $id;", blockId, null);

    // ---- Environment rows ----

    public static List<EnvEntry> ListEnv(SqliteConnection c, int blockId)
    {
        var rows = new List<EnvEntry>();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id, BlockId, Position, Application, Path, UserName, Password FROM EnvEntry WHERE BlockId = $id ORDER BY Position ASC, Id ASC;";
        cmd.Parameters.AddWithValue("$id", blockId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            rows.Add(new EnvEntry
            {
                Id = r.GetInt32(0), BlockId = r.GetInt32(1), Position = r.GetInt32(2),
                Application = r.GetString(3), Path = r.GetString(4), UserName = r.GetString(5), Password = r.GetString(6),
            });
        r.Close();
        foreach (var e in rows) e.Commands = ListCommands(c, e.Id);
        return rows;
    }

    public static EnvEntry AddEnv(SqliteConnection c, int blockId)
    {
        var position = MaxPosition(c, "EnvEntry", "BlockId", blockId) + 1;
        var id = Insert(c, "INSERT INTO EnvEntry (BlockId, Position) VALUES ($p, $pos);", blockId, position);
        return new EnvEntry { Id = id, BlockId = blockId, Position = position };
    }

    public static void UpdateEnv(SqliteConnection c, int id, string column, string value)
    {
        if (!EnvColumns.Contains(column)) throw new ArgumentOutOfRangeException(nameof(column));
        Exec(c, $"UPDATE EnvEntry SET {column} = $v WHERE Id = $id;", id, value);
    }

    public static void DeleteEnv(SqliteConnection c, int id) =>
        Exec(c, "DELETE FROM EnvEntry WHERE Id = $id;", id, null);

    // ---- Command rows ----

    public static List<CommandEntry> ListCommands(SqliteConnection c, int envId)
    {
        var rows = new List<CommandEntry>();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT Id, EnvId, Position, Command, Purpose FROM CommandEntry WHERE EnvId = $id ORDER BY Position ASC, Id ASC;";
        cmd.Parameters.AddWithValue("$id", envId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            rows.Add(new CommandEntry
            {
                Id = r.GetInt32(0), EnvId = r.GetInt32(1), Position = r.GetInt32(2),
                Command = r.GetString(3), Purpose = r.GetString(4),
            });
        return rows;
    }

    public static CommandEntry AddCommand(SqliteConnection c, int envId)
    {
        var position = MaxPosition(c, "CommandEntry", "EnvId", envId) + 1;
        var id = Insert(c, "INSERT INTO CommandEntry (EnvId, Position) VALUES ($p, $pos);", envId, position);
        return new CommandEntry { Id = id, EnvId = envId, Position = position };
    }

    public static void UpdateCommand(SqliteConnection c, int id, string column, string value)
    {
        if (!CommandColumns.Contains(column)) throw new ArgumentOutOfRangeException(nameof(column));
        Exec(c, $"UPDATE CommandEntry SET {column} = $v WHERE Id = $id;", id, value);
    }

    public static void DeleteCommand(SqliteConnection c, int id) =>
        Exec(c, "DELETE FROM CommandEntry WHERE Id = $id;", id, null);

    // ---- helpers ----

    private static int Insert(SqliteConnection c, string sql, int parentId, int position)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = sql + " SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$p", parentId);
        cmd.Parameters.AddWithValue("$pos", position);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    private static void Exec(SqliteConnection c, string sql, int id, string? value)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("$id", id);
        if (sql.Contains("$v")) cmd.Parameters.AddWithValue("$v", value ?? "");
        cmd.ExecuteNonQuery();
    }

    private static int MaxPosition(SqliteConnection c, string table, string parentColumn, int parentId)
    {
        using var cmd = c.CreateCommand();
        cmd.CommandText = $"SELECT MAX(Position) FROM {table} WHERE {parentColumn} = $id;";
        cmd.Parameters.AddWithValue("$id", parentId);
        var result = cmd.ExecuteScalar();
        return result is DBNull or null ? -1 : Convert.ToInt32(result);
    }
}

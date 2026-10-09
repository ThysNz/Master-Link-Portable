using System.IO;
using Microsoft.Data.Sqlite;

namespace MasterLink.Desktop.Data;

// Portable-app convention (same as Master Idea): the db file lives next to
// the .exe so the whole thing runs from a USB stick with no installer.
public static class Database
{
    public static string DbPath { get; } = Path.Combine(AppContext.BaseDirectory, "MasterLink.db");

    public static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    public static void Initialize()
    {
        using var connection = CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS Board (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE,
                Position INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS SystemItem (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                BoardId INTEGER NOT NULL REFERENCES Board(Id) ON DELETE CASCADE,
                Position INTEGER NOT NULL DEFAULT 0,
                Name TEXT NOT NULL,
                Description TEXT,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS Block (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SystemId INTEGER NOT NULL REFERENCES SystemItem(Id) ON DELETE CASCADE,
                Position INTEGER NOT NULL DEFAULT 0,
                Title TEXT NOT NULL DEFAULT 'Environment'
            );

            CREATE TABLE IF NOT EXISTS EnvEntry (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                BlockId INTEGER NOT NULL REFERENCES Block(Id) ON DELETE CASCADE,
                Position INTEGER NOT NULL DEFAULT 0,
                Application TEXT NOT NULL DEFAULT '',
                Path TEXT NOT NULL DEFAULT '',
                UserName TEXT NOT NULL DEFAULT '',
                Password TEXT NOT NULL DEFAULT ''
            );

            CREATE TABLE IF NOT EXISTS CommandEntry (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EnvId INTEGER NOT NULL REFERENCES EnvEntry(Id) ON DELETE CASCADE,
                Position INTEGER NOT NULL DEFAULT 0,
                Command TEXT NOT NULL DEFAULT '',
                Purpose TEXT NOT NULL DEFAULT ''
            );
            """;
        cmd.ExecuteNonQuery();

        MigrateAccessEntries(connection);
        MigrateCommandsToEnvRows(connection);
    }

    // Second-version databases hung Commands off the Block. Commands now belong
    // to an Environment row, so re-home each block's commands onto that
    // block's first row (adding a blank row first if the block had none).
    private static void MigrateCommandsToEnvRows(SqliteConnection connection)
    {
        using var probe = connection.CreateCommand();
        probe.CommandText = "SELECT COUNT(*) FROM pragma_table_info('CommandEntry') WHERE name = 'BlockId';";
        if (Convert.ToInt32(probe.ExecuteScalar()) == 0) return;

        using var tx = connection.BeginTransaction();
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            ALTER TABLE CommandEntry RENAME TO CommandEntry_old;

            CREATE TABLE CommandEntry (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EnvId INTEGER NOT NULL REFERENCES EnvEntry(Id) ON DELETE CASCADE,
                Position INTEGER NOT NULL DEFAULT 0,
                Command TEXT NOT NULL DEFAULT '',
                Purpose TEXT NOT NULL DEFAULT ''
            );

            INSERT INTO EnvEntry (BlockId, Position)
                SELECT DISTINCT o.BlockId, 0 FROM CommandEntry_old o
                WHERE NOT EXISTS (SELECT 1 FROM EnvEntry e WHERE e.BlockId = o.BlockId);

            INSERT INTO CommandEntry (EnvId, Position, Command, Purpose)
                SELECT (SELECT MIN(e.Id) FROM EnvEntry e WHERE e.BlockId = o.BlockId),
                       o.Position, o.Command, o.Purpose
                FROM CommandEntry_old o ORDER BY o.Id;

            DROP TABLE CommandEntry_old;
            """;
        cmd.ExecuteNonQuery();
        tx.Commit();
    }

    // First-version databases kept a flat Access list per System. Fold each
    // such list into one "Environment" block (Link -> Path) so no data is
    // lost, then drop the old table.
    private static void MigrateAccessEntries(SqliteConnection connection)
    {
        using var exists = connection.CreateCommand();
        exists.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='AccessEntry';";
        if (Convert.ToInt32(exists.ExecuteScalar()) == 0) return;

        using var tx = connection.BeginTransaction();
        using var cmd = connection.CreateCommand();
        cmd.Transaction = tx;
        cmd.CommandText = """
            INSERT INTO Block (SystemId, Position, Title)
                SELECT DISTINCT SystemId, 0, 'Environment' FROM AccessEntry;

            INSERT INTO EnvEntry (BlockId, Position, Application, Path, UserName, Password)
                SELECT b.Id, a.Position, '', a.Link, a.UserName, a.Password
                FROM AccessEntry a JOIN Block b ON b.SystemId = a.SystemId
                ORDER BY a.Position;

            DROP TABLE AccessEntry;
            """;
        cmd.ExecuteNonQuery();
        tx.Commit();
    }
}

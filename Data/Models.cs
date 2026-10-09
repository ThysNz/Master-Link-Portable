namespace MasterLink.Desktop.Data;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Position { get; set; }
}

public class SystemItem
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public int Position { get; set; }

    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Notes { get; set; }
}

// A titled, collapsible section of a System (default title "Environment").
// Holds an environment table; each row has its own Commands list. Seq # is never stored:
// it is the row's 1-based position in its list.
public class Block
{
    public int Id { get; set; }
    public int SystemId { get; set; }
    public int Position { get; set; }
    public string Title { get; set; } = "Environment";
}

public class EnvEntry
{
    public int Id { get; set; }
    public int BlockId { get; set; }
    public int Position { get; set; }
    public string Application { get; set; } = "";
    public string Path { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";

    // Loaded with the row; not a column on EnvEntry.
    public List<CommandEntry> Commands { get; set; } = new();
}

public class CommandEntry
{
    public int Id { get; set; }
    public int EnvId { get; set; }
    public int Position { get; set; }
    public string Command { get; set; } = "";
    public string Purpose { get; set; } = "";
}

public class BlockTree
{
    public Block Block { get; set; } = new();
    public List<EnvEntry> Env { get; set; } = new();
}

public class SystemTree
{
    public SystemItem System { get; set; } = new();
    public List<BlockTree> Blocks { get; set; } = new();
}

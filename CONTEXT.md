# Master Link

A portable WPF desktop app for keeping track of systems and how to get into them. Same Board structure, theme and interaction style as Master Idea, but its own app, database (`MasterLink.db`) and domain. Standalone project at F:\Claude\Projects\Master-Link-Portable (sibling of Master Idea Portable).

## Language

**Board**:
A named collection of Systems (e.g. "Production Servers"). The top-level container; the Boards list is the home page.

**System**:
One thing you need to reach — a name, a short description, any number of Blocks and one Notes field. Replaces Master Idea's Idea. No Status.

**Block**:
A titled, collapsible section of a System (default title "Environment"). Holds an Environment table; each row has its own Commands list. A System can have many Blocks ("+ Add block").

**Board view**:
The read-only page shown when a Board is opened (default). Shows Systems, Blocks, Environment and Command rows as plain text with passwords masked and only the Copy buttons (Copy user, Copy pwd, Copy command); no add/delete/edit controls. "Edit" opens the editable Board page; "Done" there returns to the Board view.

**Environment row**:
One row in a Block's table: Seq #, Application, Path, User, Password. Path is plain text (URL, file path or UNC path); "Open" hands it to the shell, "Copy user" / "Copy pwd" use the clipboard.

**Command row**:
One row in an Environment row's collapsible Commands list (many per application): Seq #, Command, Purpose.

**Seq #**:
Not stored — the row's 1-based position in its list, renumbered when a row is added or removed.

**Notes**:
One free-text field per System.

## Security note

Passwords are stored as plain text in `MasterLink.db` (masked on screen only). Anyone who can read the file can read them. Keep the db on trusted media; encryption (e.g. a master password) is not implemented.

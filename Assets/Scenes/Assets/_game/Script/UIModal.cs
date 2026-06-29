// Tracks whether a blocking UI panel (map / quiz book / PC menu) is open, so world
// interactions (E / C / F) ignore key input while a panel is up. Counter-based so
// overlapping panels don't unblock each other early.
public static class UIModal
{
    static int _open;

    public static bool IsOpen => _open > 0;

    public static void Open() { _open++; }
    public static void Close() { if (_open > 0) _open--; }
}

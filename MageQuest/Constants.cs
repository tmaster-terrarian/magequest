namespace MageQuest;

public static class Consts
{
    public const int SHFT = 0x9;
    public const int MULT = 0b1 << 0x9;

    public const int MaxFallingSpeed = 0x600;

    public const int ScreenWidthPixels = 320;
    public const int ScreenHeightPixels = 240;
    public const int ScreenWidth = ScreenWidthPixels << SHFT;
    public const int ScreenHeight = ScreenHeightPixels << SHFT;
}

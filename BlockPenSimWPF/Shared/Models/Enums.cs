namespace BlockPenSimWPF.Shared.Models
{
    public enum Direction
    {
        Front,
        Side,
        Top,
    }

    public enum Orientation
    {
        ForwardsTall,
        ForwardsWide,
        SidewaysTall,
        SidewaysLong,
        FlatLong,
        FlatWide,
    }

    public enum BlockFillMethod
    {
        LWH,
        LHW,
        WLH,
        WHL,
        HLW,
        HWL,
        ALL,
    }

    public enum SortDirection
    {
        ASC,
        DESC,
    }

    public enum Theme
    {
        Default,
        Light,
        Dark,
    }
}

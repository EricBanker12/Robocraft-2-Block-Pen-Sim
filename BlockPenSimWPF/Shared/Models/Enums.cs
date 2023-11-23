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

    public enum SplashShape
    {
        None,
        Cone,
        Cylinder,
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

    public enum SimulationFormTab
    {
        Blocks,
        Weapons,
    }
}

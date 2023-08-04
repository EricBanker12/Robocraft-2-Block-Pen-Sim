using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Robocraft2BlockPenSimApp.Shared.Models
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
        FlatWide
    }

    public enum BlockFillMethod
    {
        LWH,
        LHW,
        WLH,
        WHL,
        HLW,
        HWL,
        ALL
    }

    public enum SortDirection
    {
        ASC,
        DESC,
    }
}

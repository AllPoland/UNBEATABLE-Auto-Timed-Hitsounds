using Rhythm;

namespace AutoTimedHitsounds;

public class HeightHelper
{
    public static Height GetOpposite(Height height)
    {
        switch(height)
        {
            default:
                return Height.None;
            case Height.Top:
                return Height.Low;
            case Height.Low:
                return Height.Top;
        }
    }
}
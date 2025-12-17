using Rhythm;

namespace AutoTimedHitsounds;

public static class HeightUtil
{
    public static Height OppositeHeight(Height height)
    {
        switch(height)
        {
            case Height.Low:
                return Height.Top;
            case Height.Top:
                return Height.Low;
            default:
                return Height.None;
        }
    }
}
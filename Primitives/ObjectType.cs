namespace LevelConstructor
{
    public enum ObjectType
    {
        LaserSource,
        LaserReceiver_0,
        LaserReceiver_1,
        LaserReceiver_2,
        LaserReceiver_3,
        LaserReceiver_4,

        Barier,
        BarierBomb,

        MovedReflectorAngle,
        MovedReflectorAll,
        MovedReflectorTriangle,
        MovedReflectorTube,
        MovedTeleport,
        MovedNone,

        StacionarReflectorAll,
        StacionarReflectorAngle,
        StacionarReflectorTriangle,
        StacionarTube,
        StacionarTeleport,
        StacionarMirror,

        StacionarControllerRed,
        StacionarControllerBlue,
        StacionarControllerGreen,
        // new
        MovedLaserSource,
        LaserXSource,
        MovedLaserXSource
    }

    internal static class ObjectTypeExtensions
    {
        internal static bool IsLaserSource(this ObjectType x)
        {
            return (x == ObjectType.LaserSource || x == ObjectType.MovedLaserSource ||
                    x == ObjectType.LaserXSource || x == ObjectType.MovedLaserXSource);
        }

        internal static string ToStringExt(this ObjectType x)
        {
            if (x == ObjectType.StacionarControllerBlue || x == ObjectType.StacionarControllerGreen || x == ObjectType.StacionarControllerRed)
                return "Button";
            return x.ToString();
        }
    }

}
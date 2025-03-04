using System;

namespace AdaptiveTrafficSystem.Utils
{
    public class IdGenerator
    {
        public static string Generate() => Guid.NewGuid().ToString("N");
    }
}
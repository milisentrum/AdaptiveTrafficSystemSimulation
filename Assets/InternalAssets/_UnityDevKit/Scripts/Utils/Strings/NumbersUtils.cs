using System;

namespace UnityDevKit.Utils.Strings
{
    public static class NumbersUtils
    {
        public static string ToStringWithAccuracy(this float value, int accuracy)
            => Math.Round(value, accuracy).ToString();
    }
}
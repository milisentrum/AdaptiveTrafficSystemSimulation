using System;
using System.Globalization;

namespace UnityDevKit.Utils.TimeHandlers
{
    public static class TimeHandlers
    {
        private static string GetCurrentTime() =>
            DateTime.Now.ToString("dd_MMMM_yyyy_HH_mm", CultureInfo.CreateSpecificCulture("ru-RU"));
    }
}
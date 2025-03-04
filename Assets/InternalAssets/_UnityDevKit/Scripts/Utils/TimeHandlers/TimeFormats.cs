using System;
using System.Globalization;

namespace UnityDevKit.Utils.TimeHandlers
{
    public static class TimeFormats
    {
        public static DateTime SecondsToDatetime(this float time) => new DateTime().AddSeconds(time);

        public static string ToReportTimeFormat(this DateTime dateTime) => dateTime.ToString("T");
        
        public static string ToMinutesString(this float time)
        {
            var dateTime = time.SecondsToDatetime();
            return $"{dateTime.Minute} минут {dateTime.Second} сек"; // TODO -- add culture
        }

        public static string ToDateTimeString(this DateTime time) =>
            time.ToString("dd MMM yyyy HH:mm:ss", CultureInfo.CreateSpecificCulture("ru-RU"));
    }
}
using System;

namespace Helper
{
    /// <summary>
    /// 时间便捷类
    /// 作者：容泳森
    /// 创建时间：2025-8-12
    /// </summary>
    public static class TimeHelper
    {
        public static readonly DateTime START_TIME = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));

        public static long UtcNow => DateTime.UtcNow.ToLong();
        public static long Now => DateTime.Now.ToLong();

        public static long CurrentTimestamp => ToUnixTimestamp(DateTime.Now);
        static readonly DateTime TimestampZero = DateTime.Parse("1970-1-1");

        public static long ToUnixTimestamp(this DateTime date)
        {
            if (date.Kind == DateTimeKind.Utc)
            {
                var ts = (long)(date.Subtract(TimestampZero)).TotalSeconds;
                if (ts < 0)
                    return 0;
                return ts;
            }
            else
            {
                var ts = (long)(date.ToUniversalTime().Subtract(TimestampZero)).TotalSeconds;
                if (ts < 0)
                    return 0;
                return ts;
            }
        }

        /**根据秒数 0-9填充"0"**/
        private static string SecondFill(string orgStr, int len = 2, string fillStr = "0")
        {
            string rs = orgStr;
            while (rs.Length < len)
            {
                rs = fillStr + rs;
            }

            return rs;
        }

        private static string SecondFill(int orgStr, int len = 2, string fillStr = "0")
        {
            return SecondFill(orgStr.ToString(), len, fillStr);
        }

        /// <summary>
        /// 时间戳 => DateTime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime ToDateTime(this long timeStamp)
        {
            // long lTime = long.Parse( timeStamp + "0000000" );
            //   var toNow = new TimeSpan( timeStamp );
            return START_TIME.AddSeconds(timeStamp);
        }

        /// <summary>
        /// DateTime => 时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static long ToLong(this DateTime time)
        {
            return (long)(time - START_TIME).TotalSeconds;
        }

        public static DateTime ToDateTime(string date)
        {
            return DateTime.Parse(date);
        }

        public static long DisNow(this long time)
        {
            return Math.Abs(Now - time);
        }

        public static long DisNow(this DateTime dt)
        {
            return DisNow(dt.ToLong());
        }

        /// <summary>
        /// 计算两个日期之间隔了多少天。
        /// </summary>
        /// <param name="date1">时间戳1</param>
        /// <param name="date2">时间戳2</param>
        /// <returns></returns>
        public static int DaysBetweenDates(string date1, string date2)
        {
            //两个DateTime相减得到TimeSpan，然后再取绝对值
            return Math.Abs((ToDateTime(date2) - ToDateTime(date1)).Days);
        }

        /// <summary>
        /// 当前时间是否在 startTime - endTime之间
        /// </summary>
        /// <param name="startTime">2020/9/14 13:00:00</param>
        /// <param name="endTime">2020/9/14 14:00:00</param>
        /// <returns></returns>
        public static bool IsMiddleByCurrTime(string startTime, string endTime)
        {
            DateTime startDT = Convert.ToDateTime(startTime);
            DateTime endDT = Convert.ToDateTime(endTime);
            long cur = DateTime.Now.ToLong();
            return cur > startDT.ToLong() && cur < endDT.ToLong();
        }

        public static string Time2String2(string timeStr)
        {
            DateTime dt = Convert.ToDateTime(timeStr);
            return $"{dt.Month}月{dt.Day}日{dt.Hour}点";
//        return Time2String(timeStr, "yyyy/MM/dd hh");
        }

        /// <summary>
        /// 时分秒显示
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public static string FormatTime(long totalSeconds)
        {
            long hours = totalSeconds / 3600;
            long minutes = (totalSeconds - hours * 3600) / 60;
            long seconds = totalSeconds - hours * 3600 - minutes * 60;
            return $"{hours}:{minutes}:{seconds}";
        }

        /// <summary>
        /// 时分秒显示
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public static string FormatTime2(long totalSeconds)
        {
            long hours = totalSeconds / 3600;
            long minutes = (totalSeconds - hours * 3600) / 60;
            long seconds = totalSeconds - hours * 3600 - minutes * 60;
            return $"{hours}时:{minutes}分:{seconds}秒";
        }

        /// <summary>
        ///根据秒数 算出时间 格式  00:00:00
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="showHour"></param>
        /// <param name="showSecs"></param>
        /// <returns></returns>
        public static string FormatTime3(long sec, bool showHour = true, bool showSecs = true)
        {
            sec = sec < 0 ? 0 : sec;
            int hour = (int)Math.Floor((double)sec / 3600);
            int min = (int)Math.Floor((double)sec / 60) % 60;
            int secs = (int)Math.Floor((double)sec % 60);
            if (showHour || hour > 0)
            {
                return SecondFill(hour) +
                       ":" + SecondFill(min) +
                       (showSecs ? (":" + SecondFill(secs)) : "");
            }
            else
            {
                return SecondFill(min) +
                       (showSecs ? (":" + SecondFill(secs)) : "");
            }
        }

        /// <summary>
        /// 分秒显示
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public static string FormatTwoTime(long totalSeconds)
        {
            long minutes = totalSeconds / 60;
            long seconds = (totalSeconds - (minutes * 60));
            return string.Format("{0}:{1}", minutes, seconds);
        }

        /// <summary>
        /// 天显示
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public static string FormatDayTime(long totalSeconds)
        {
            long days = (totalSeconds / 3600) / 24;
            long hours = (totalSeconds / 3600) - (days * 24);
            long minutes = (totalSeconds - (hours * 3600) - (days * 86400)) / 60;
            long seconds = totalSeconds - (hours * 3600) - (minutes * 60) - (days * 86400);
            return string.Format("{0}:{1}:{2}:{3}", days, hours, minutes, seconds);
        }

        /// <summary>
        /// 天显示
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public static string FormatDayTime2(long totalSeconds)
        {
            long days = (totalSeconds / 3600) / 24;
            if (days > 0)
            {
                return $"{days}天";
            }

            long hours = (totalSeconds / 3600) - (days * 24);
            if (hours > 0)
            {
                return $"{hours}时";
            }

            long minutes = (totalSeconds - (hours * 3600) - (days * 86400)) / 60;
            if (minutes > 0)
            {
                return $"{minutes}分钟";
            }

            long seconds = totalSeconds - (hours * 3600) - (minutes * 60) - (days * 86400);
            if (seconds > 0)
            {
                return $"{seconds}秒";
            }

            return string.Empty;
        }

        /// <summary>
        /// 天显示
        /// </summary>
        /// <param name="totalSeconds"></param>
        /// <returns></returns>
        public static string FormatDayTime3(long totalSeconds)
        {
            long days = (totalSeconds / 3600) / 24;
            if (days > 0)
            {
                return $"{days}天";
            }

            long hours = (totalSeconds / 3600) - (days * 24);
            if (hours > 0)
            {
                return $"{hours}小时";
            }

            return "1小时";
        }

        public static string Format4(long timestamp)
        {
            var totalSeconds = timestamp - CurrentTimestamp;
            return FormatDayTime2(totalSeconds);
        }

        public static string Format4(int timestamp)
        {
            return Format4((long)timestamp);
        }

        public static string Format2(long timestamp)
        {
            var totalSeconds = timestamp - CurrentTimestamp;
            return FormatTime2(totalSeconds);
        }
    }
}
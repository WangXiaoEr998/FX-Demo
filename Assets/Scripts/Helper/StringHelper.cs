using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Helper
{
    /// <summary>
    /// string便捷类
    /// 作者：容泳森
    /// 创建时间：2025-8-12
    /// </summary>
    public static class StringHelper
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool Contains(string str, string value)
        {
            return str.Contains(value);
        }

        /// <summary>
        /// 计算长度，如果是中文算两个字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int Length(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return 0;
            }

            int len = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if ((int)str[i] > 127)
                {
                    //在 ASCII码表中，英文的范围是0-127，而汉字则是大于127
                    len += 2;
                }
                else
                {
                    len += 1;
                }
            }

            return len;
        }

        /// <summary>
        /// 获取切割字符  （,）
        /// </summary>
        /// <param name="reward"></param>
        /// <returns></returns>
        public static string[] GetSplit(string reward)
        {
            return reward.Split(',');
        }

        //输入校正，返回限制内的字符
        public static string InputRevise(string str, int limitLen)
        {
            int len = Length(str);
            if (len > limitLen)
            {
                return InputRevise(str.Substring(0, str.Length - 1), limitLen);
            }

            return str;
        }

        //替换字符串
        public static string Replace(string str, string oldValue, string newValue)
        {
            return str.Replace(oldValue, newValue);
        }

        //判断字符串中是否含有中文
        public static bool HasChinese(string str)
        {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        /// <summary>判断一个字符串是否是正整数</summary>
        public static bool IsInteger(string s)
        {
            string pattern = @"^\d*$";
            return Regex.IsMatch(s, pattern);
        }

        /// <summary>色号</summary>
        public static string ToColor(this string content, string color)
        {
            return $"<color=#{color}>{content}</color>";
        }

        public static string ToColor(this string content, Color color)
        {
            return ToColor(content, ColorUtility.ToHtmlStringRGB(color));
        }

        /// <summary>转百分比</summary>
        public static string ToPercentage(this float f)
        {
            return f.ToString("0.##%");
        }

        /// <summary>是否需要转百分比</summary>
        public static string ToNeedPercentage(this float str)
        {
            var fStr = str.ToString(CultureInfo.InvariantCulture);
            return IsInteger(fStr) ? fStr : str.ToPercentage();
        }

        /// <summary>转小数点保留1位</summary>
        public static string ToF1(this float f)
        {
            return f.ToString("F1");
        }

        /// <summary>转小数点保留2位</summary>
        public static string ToF2(this float f)
        {
            return f.ToString("F2");
        }

        /// <summary>转小数点保留3位</summary>
        public static string ToF3(this float f)
        {
            return f.ToString("F3");
        }

        /// <summary>是否需要转小数点保留1位</summary>
        public static string ToNeedF1(this float str)
        {
            var fStr = str.ToString(CultureInfo.InvariantCulture);
            return IsInteger(fStr) ? fStr : str.ToF1();
        }

        //判断字符串是否为空值
        public static bool IsEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        //数字转换为千分位显示   如：10000000 转换为： 10,000,000
        public static string NumberToThousands(long value)
        {
            return value.ToString("N0");
        }

        //将千分位转换回数字		如：10,000,000 转换为： 10000000
        public static int ThousandthToNumber(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return int.Parse(value,
                    NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign);
            }

            return 0;
        }

        //去除所有空格
        public static string RemoveAllSpaces(this string str)
        {
            if (IsEmpty(str))
            {
                return str;
            }
            else
            {
                return str.Replace(" ", "");
            }
        }


        //格式转换
        public static string ToFormat(string str, params object[] args)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            for (int i = 0; i < args.Length; i++)
            {
                str = str.Replace("{" + i + "}", args[i].ToString());
            }

            return str;
        }


        private const string SpecificChar = "[ \\[ \\] \\^ \\-_*×――(^)$%~!/@#$…&%￥—+=<>《》|!！??？:：•`·、。，；,.;\"‘’“”-]";

        public static string RemoverSpecificChar(string word)
        {
            return Regex.Replace(word, SpecificChar, "");
        }


        static string[] intArr = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", };
        static string[] strArr = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九", };
        static string[] Chinese = { "", "十", "百", "千", "万", "十", "百", "千", "亿" };
        static StringBuilder builder = new StringBuilder();

        public static string ToChinese(this int num)
        {
            string inputNum = num.ToString();
            builder.Clear();
            for (int i = 0; i < inputNum.Length; i++)
            {
                builder.Append(strArr[inputNum[i] - 48]); //ASCII编码 0为48
                builder.Append(Chinese[inputNum.Length - 1 - i]); //根据对应的位数插入对应的单位
            }

            return builder.ToString();
        }

        public static string ReplaceLine(this string str)
        {
            return string.IsNullOrEmpty(str) ? "" : str.Replace("\\n", "\n");
        }

        public static string Sha256(string randomString)
        {
            var crypt = new SHA256Managed();
            var hash = new StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
    }
}
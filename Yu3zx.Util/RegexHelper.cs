using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Yu3zx.Util
{
    public class RegexHelper
    {
        public static bool CheckZip(string value)
        {
            string pattern = "^[1-9]\\d{5}$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckTelPhone(string value)
        {
            string pattern = "^(\\(\\d{3,4}\\)|\\d{3,4}-)?\\d{7,8}(-\\d{1,4})?$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckCellPhone(string value)
        {
            string pattern = "^1{1}\\d{10}$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckCarNumber(string value)
        {
            string pattern = "^[\\u4e00-\\u9fa5]{1}[A-Z]{1}[A-Z_0-9]{5}$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckNumbersCharactersUnderLine(string value)
        {
            string pattern = "^[a-zA-Z0-9_]+$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckNumbersAndCharacters(string value)
        {
            string pattern = "^[a-zA-Z0-9]+$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckCharacters(string value)
        {
            string pattern = "^[a-zA-Z]+$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckNumbers(string value)
        {
            string pattern = "^[0-9]+$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckIP(string value)
        {
            string pattern = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";
            return Regex.IsMatch(value, pattern);
        }

        public static bool CheckHostname(string value)
        {
            string pattern = "^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\\-]*[a-zA-Z0-9])\\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\\-]*[A-Za-z0-9])$";
            return Regex.IsMatch(value, pattern);
        }
    }
}

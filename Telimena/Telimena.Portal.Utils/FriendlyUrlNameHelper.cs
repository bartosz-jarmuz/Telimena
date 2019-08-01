using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Telimena.Portal.Utils
{

    /// <summary>
    /// Helps creating 'friendly' URLs (no escape characters etc)
    /// </summary>
    /// <returns></returns>
    public static class FriendlyUrlNameHelper
    {
        /// <summary>
        /// Verifies whether a string contains only characters valid for 'friendly' URLs (no escape characters etc)
        /// </summary>
        /// <returns></returns>
        public static bool IsUrlFriendly(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            if (!Regex.IsMatch(input, @"^[a-zA-Z0-9\-]+$"))
            {
                return false;
            }

            if (!Regex.IsMatch(input, @"^[a-zA-Z0-9]{1}"))
            {
                return false;
            }

            if (!Regex.IsMatch(input, @"[a-zA-Z0-9]$"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifies whether a string contains only characters valid for 'friendly' URLs (no escape characters etc)
        /// </summary>
        /// <returns></returns>
        public static string MakeUrlFriendly(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            var output = Regex.Replace(input, @"[^a-zA-Z0-9\-]", "");
            output = TrimInvalidStart(output);
            output = TrimInvalidEnd(output);
            return output;
        }

        private static string TrimInvalidStart(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            while (!Regex.IsMatch(input, @"^[a-zA-Z0-9]{1}"))
            {
                input = input.Substring(1);
                input = TrimInvalidStart(input);
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }
            }

            return input;
        }

        private static string TrimInvalidEnd(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }
            while (!Regex.IsMatch(input, @"[a-zA-Z0-9]$"))
            {
            
                input = input.Remove(input.Length-1);
                input = TrimInvalidEnd(input);
                if (string.IsNullOrWhiteSpace(input))
                {
                    return null;
                }
            }

            return input;
        }
    }
}

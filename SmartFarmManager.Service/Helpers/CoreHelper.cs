using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Helpers
{
    public static partial class CoreHelper
    {
        
        public static bool MatchMqttPattern(this string topic, string pattern)
        {
            var topicParts = topic.Split('/');
            var patternParts = pattern.Split('/');

            if (topicParts.Length != patternParts.Length) return false;

            for (var i = 0; i < patternParts.Length; i++)
            {
                // if the pattern part is +, then it matches anything except /
                if (patternParts[i] == "+" && !MatchAnythingExceptForwardSlash().IsMatch(topicParts[i]))
                {
                    return false;
                }

                if (patternParts[i] != "+" && patternParts[i] != topicParts[i])
                {
                    return false;
                }
            }

            return true;
        }
        [GeneratedRegex(@"([^/]+)")]
        private static partial Regex MatchAnythingExceptForwardSlash();
    }
}

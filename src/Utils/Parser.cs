using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf_test.src.Utils
{
    public static class Parser
    {
        public static string ParseToTimeString (int timeInSeconds) 
        {
            int hours = timeInSeconds / 3600;
            int minutes = timeInSeconds % 3600 / 60;
            int seconds = timeInSeconds % 60;

            if (hours > 0)
                return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            else
                return $"{minutes:D2}:{seconds:D2}";
        }

        public static int ParseTimeToInt(string time)
        {
            try
            {
                string[] strings = time.Split(':');
                if (strings.Length == 1)
                {
                    return int.Parse(strings[0]);
                }
                else if (strings.Length == 2)
                {
                    return int.Parse(strings[0]) * 60 + int.Parse(strings[1]);
                }
                else
                {
                    return int.Parse(strings[0]) * 3600 + int.Parse(strings[1]) * 60 + int.Parse(strings[2]);
                }
            } catch (FormatException e)
            {
                throw new FormatException("Separate time units with a semicolon (hh:mm:ss) or enter time in seconds (300).", e);
            }
        }


        public static int[] ParseMultipleTimeToInt(List<(string, string)> customTimes)
        {
            List<int> temp = [];
            foreach (var entry in customTimes)
            {
                int time = ParseTimeToInt(entry.Item1);
                temp.AddRange(Enumerable.Repeat(time, int.Parse(entry.Item2)));
            }
            return [.. temp];
        }
    }
}

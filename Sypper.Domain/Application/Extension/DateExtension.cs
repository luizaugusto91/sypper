using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sypper.Domain.Application.Extension
{
    public static class DateExtension
    {
        #region Validate
        public static bool IsDate(this string input)
        {
            DateTime date;
            return DateTime.TryParse(input, out date);
        }
        #endregion

        #region Date Compare
        public static bool IsNow(this DateTime input)
        {
            return DateTime.Now.Date == input.Date ? true : false;
        }

        public static bool AfterDate(this DateTime input) 
        { 
            return DateTime.Now.Date > input.Date ? true : false;
        }

        public static bool AfterDate(this DateTime input, DateTime compare)
        {
            return compare.Date > input.Date ? true : false;
        }

        public static bool AfterOrNowDate(this DateTime input)
        {
            return DateTime.Now.Date >= input.Date ? true : false;
        }

        public static bool AfterOrNowDate(this DateTime input, DateTime compare)
        {
            return compare.Date >= input.Date ? true : false;
        }

        public static bool BeforeDate(this DateTime input)
        {
            return DateTime.Now.Date < input.Date ? true : false;
        }

        public static bool BeforeDate(this DateTime input, DateTime compare)
        {
            return compare.Date < input.Date ? true : false;
        }

        public static bool BeforeOrNowDate(this DateTime input)
        {
            return DateTime.Now.Date <= input.Date ? true : false;
        }

        public static bool BeforeOrNowDate(this DateTime input, DateTime compare)
        {
            return compare.Date <= input.Date ? true : false;
        }

        public static bool InRange(this DateTime input, DateTime[] range) 
        {
            return range.ToList().Exists(r => r == input);
        }

        public static bool GetIndexRange(this DateTime input, DateTime[] range, out int index)
        {
            index = -1;
            if (range.ToList().Exists(r => r == input))
            {
                for (int i = 0; i < range.Count(); i++) 
                {
                    if (range[i] == input)
                    {
                        index = i;
                        break;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}

using Sypper.Domain.Application.Enumerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sypper.Domain.Application.Processing
{
    public class HtmlFieldModel
    {
        public int index { get; set; }
        public string name { get; set; } = string.Empty;
        public string id { get; set; } = string.Empty;
        public string label { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public Type type { get; set; }
        public dynamic value { get; set; }
        public bool visible { get; set; }
        public string reference { get; set; } = string.Empty;

        public HtmlFieldEnum GetHtmlFielMode() 
        {
            var ctype = Type.GetTypeCode(type);
            HtmlFieldEnum htype;
            switch (ctype)
            {
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Double:
                case TypeCode.String:
                    htype = HtmlFieldEnum.Input;
                    break;
                case TypeCode.DateTime:
                    htype = HtmlFieldEnum.Date;
                    break;
                case TypeCode.Boolean:
                    htype = HtmlFieldEnum.Checkbox;
                    break;
                default:
                    htype = HtmlFieldEnum.Input;
                    break;
            }
            return htype;
        }
        public string GetHtmlTypeField() 
        {
            var ctype = Type.GetTypeCode(type);
            string htype = "";
            switch (ctype)
            {
                case TypeCode.Int16:
                    htype = "number";
                    break;
                case TypeCode.Int32:
                    htype = "number";
                    break;
                case TypeCode.Int64:
                    htype = "number";
                    break;
                case TypeCode.Double:
                    htype = "number";
                    break;
                case TypeCode.String:
                    htype = "text";
                    break;
                case TypeCode.DateTime:
                    var datetime = Convert.ToDateTime(value);

                    // Verifica tipo de campo
                    if (datetime.TimeOfDay != DateTime.Parse("00:00:00").TimeOfDay)
                    {
                        // DateTime
                        if (datetime.Date != DateTime.Parse("0001-01-01").Date)
                        {
                            htype = "datetime-local";
                        }
                        else
                        {
                            // Time
                            htype = "time";
                        }
                    }
                    else
                    {
                        // Date
                        htype = "date";
                    }
                    break;
                case TypeCode.Boolean:
                    htype = "checkbox";
                    break;
                default:
                    htype = "text";
                    break;
            }
            return htype;
        }
    }
}

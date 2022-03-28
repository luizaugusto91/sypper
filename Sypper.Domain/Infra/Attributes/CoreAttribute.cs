using System.Reflection;

namespace Sypper.Domain.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class Core : Attribute
    {
        public bool PrimaryKey { get; set; }
        public bool Restrict { get; set; }
        public bool Index { get; set; }
        public bool Exclude { get; set; }
        public bool Ignore { get; set; }

        public Core() {}

        public Core(List<CustomAttributeNamedArgument> Attributes)
        {
            try
            {
                foreach (var att in Attributes)
                {
                    dynamic value = att.TypedValue.Value;
                    switch (att.MemberName)
                    {
                        case "PrimaryKey":
                            PrimaryKey = Convert.ToBoolean(value);
                            break;
                        case "Restrict":
                            Restrict = Convert.ToBoolean(value);
                            break;
                        case "Index":
                            Index = Convert.ToBoolean(value);
                            break;
                        case "Exclude":
                            Exclude = Convert.ToBoolean(value);
                            break;
                        case "Ignore":
                            Ignore = Convert.ToBoolean(value);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
            }
        }
    }
}

using System.Reflection;

namespace Sypper.Domain.Infra.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class Visual : Attribute
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public bool Visivel { get; set; } = true;
        public int Sequencia { get; set; } = 3;

        public Visual() { }

        public Visual(List<CustomAttributeNamedArgument> Attributes) 
        {
            try
            {
                foreach (var att in Attributes) 
                {
                    dynamic value = att.TypedValue.Value;
                    switch (att.MemberName)
                    {
                        case "Titulo":
                            Titulo = Convert.ToString(value);
                            break;
                        case "Descricao":
                            Descricao = Convert.ToString(value);
                            break;
                        case "Visivel":
                            Visivel = Convert.ToBoolean(value);
                            break;
                        case "Sequencia":
                            Sequencia = Convert.ToInt32(value);
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

using Sypper.Domain.Application.Processing;
using Sypper.Domain.Infra.Attributes;

namespace Sypper.Domain.Application.Extension
{
    public static class HtmlExtension
    {
        public static List<HtmlFieldModel> GetHtmlRecordFields<T>(this SQLModel record) 
        { 
            List<HtmlFieldModel> result = new List<HtmlFieldModel>();

            if (record != null)
            {
                var props = typeof(T).GetProperties();

                foreach (var prop in props)
                {
                    HtmlFieldModel field = new HtmlFieldModel();
                    field.name = prop.Name;
                    field.id = prop.Name;
                    field.value = prop.GetValue(record);
                    field.type = prop.PropertyType;
                    field.visible = false;

                    result.Add(field);
                }

                var props_visual = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(Visual))).ToList();

                result.ForEach(field => 
                {
                    if (props_visual.Exists(x => x.Name == field.name)) 
                    {
                        var Arg = props_visual.Where(x => x.Name == field.name).FirstOrDefault().CustomAttributes.Where(a => a.AttributeType.Name == "Visual").ToList();
                        if (Arg.Count > 0) 
                        {
                            var attribute = new Visual(Arg[0].NamedArguments.ToList());

                            field.index = attribute.Sequencia;
                            field.visible = attribute.Visivel;
                            field.label = attribute.Titulo;
                            field.description = attribute.Descricao;
                        }
                        
                    }
                });

                // Obtem as referencias de propriedades cruzadas
                /*var props_core = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(Core)));

                foreach (var prop in props_visual)*/
            }

            return result.OrderBy(r => r.index).ToList();
        }
    }
}

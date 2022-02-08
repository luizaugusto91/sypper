using Sypper.Domain.Infra.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sypper.Domain.Application.Extension
{
    public static class VisualExtension
    {
        public static List<string> GetTableAttributes<T>(this T record)
        {            
            List<(int index, string name)> list = new List<(int index, string name)>();

            if (record != null)
            {            
                var props = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(Visual)));

                foreach(var prop in props)                
                {
                    var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Visual").ToList();
                    if (Arg.Count > 0)
                    {
                        var attribute = new Visual(Arg[0].NamedArguments.ToList());

                        if (attribute.Visivel) list.Add((attribute.Sequencia, attribute.Titulo));
                    }
                }
            }

            return list.OrderBy(r => r.index).Select(r => r.name).ToList();
        }

        public static List<string> GetTableFieldsDescription<T>(this T record)
        {
            List<(int index, string name)> list = new List<(int index, string name)>();

            if (record != null)
            {
                var props = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(Visual)));

                foreach (var prop in props)
                {
                    var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Visual").ToList();
                    if (Arg.Count > 0)
                    {
                        var attribute = new Visual(Arg[0].NamedArguments.ToList());

                        if (attribute.Visivel) list.Add((attribute.Sequencia, attribute.Descricao));
                    }
                }
            }

            return list.OrderBy(r => r.index).Select(r => r.name).ToList();
        }

        public static List<Type> GetTableFieldsType<T>(this T record)
        {
            List<(int index, Type type)> list = new List<(int index, Type type)>();

            if (record != null)
            {
                var props = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(Visual)));

                foreach (var prop in props)
                {
                    var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Visual").ToList();
                    if (Arg.Count > 0)
                    {
                        var attribute = new Visual(Arg[0].NamedArguments.ToList());

                        if (attribute.Visivel) list.Add((attribute.Sequencia, attribute.GetType()));
                    }
                }
            }

            return list.OrderBy(r => r.index).Select(r => r.type).ToList();
        }

        public static List<List<dynamic>> GetTableValues<T>(this List<T> records)
        { 
            List<List<dynamic>> result = new List<List<dynamic>>();            

            if (records.HasValue()) 
            {                
                foreach (var record in records)
                {
                    List<(int index, dynamic value)> listValue = new List<(int, dynamic)>();

                    var props = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(Visual)));

                    foreach (var prop in props)
                    {
                        var att = prop.CustomAttributes.Where(r => r.AttributeType == typeof(Visual)).ToList();
                        if (att.HasValue())
                        {
                            var attribute = new Visual(att[0].NamedArguments.ToList());
                            if (attribute.Visivel)
                            {
                                #pragma warning disable CS8620 // O argumento não pode ser usado para o parâmetro devido a diferenças na nulidade dos tipos de referência.
                                listValue.Add((attribute.Sequencia, prop.GetValue(record)));
                                #pragma warning restore CS8620 // O argumento não pode ser usado para o parâmetro devido a diferenças na nulidade dos tipos de referência.
                            }                            
                        }
                    }                    
                    result.Add(listValue.OrderBy(r => r.index).Select(r => r.value).ToList());
                }
            }

            return result;
        }
    }
}

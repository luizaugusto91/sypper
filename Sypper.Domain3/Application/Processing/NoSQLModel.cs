using Sypper.Domain.Application.Enumerators;
using Sypper.Domain.Infra.Data;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sypper.Domain.Application.Processing
{
    public class NoSQLModel
    {
        private string table { get; set; } = string.Empty;

        public ObjectId _id { get; set; }

        #region Table
        public void SetTable(string Table)
        {
            this.table = Table;
        }

        public string GetTable()
        {
            return this.table;
        }
        #endregion   

        #region Internal
        private string FilterBy(TypeFilterEnum TypeFilter, TypeCode Type, string Value)
        {
            string result;

            switch (TypeFilter)
            {
                case TypeFilterEnum.Igual:          // ==
                    {
                        //if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"= {Value}";
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                case TypeFilterEnum.Contem:         // LIKE %V%
                    {
                        if (Type == TypeCode.String) result = "{'$regex' : '^" + Value + "$', '$options' : 'i'}";
                        else if (Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                case TypeFilterEnum.NaoContem:      // LIKE %V%
                    {
                        if (Type == TypeCode.String) result = "{'$regex' : '^((?!" + Value + ").)*$', '$options' : 'i'}";
                        else if (Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                case TypeFilterEnum.Inicia:         // LIKE V%
                    {
                        if (Type == TypeCode.String) result = "{'$regex' : '^" + Value + "', '$options' : 'i'}";
                        else if (Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                case TypeFilterEnum.Termina:        // LIKE %V
                    {
                        if (Type == TypeCode.String) result = "{'$regex' : '" + Value + "$', '$options' : 'i'}";
                        else if (Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                case TypeFilterEnum.Maior:          // >
                    {
                        if (Type == TypeCode.Int16 || Type == TypeCode.Int32 || Type == TypeCode.Int64 || Type == TypeCode.Double) result = "{ $gt: " + Value + " }";
                        else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                case TypeFilterEnum.MaiorIgual:     // >=
                    {
                        if (Type == TypeCode.Int16 || Type == TypeCode.Int32 || Type == TypeCode.Int64 || Type == TypeCode.Double) result = "{ $gte: " + Value + " }";
                        else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                case TypeFilterEnum.Menor:          // <
                    {
                        if (Type == TypeCode.Int16 || Type == TypeCode.Int32 || Type == TypeCode.Int64 || Type == TypeCode.Double) result = "{ $lt: " + Value + " }";
                        else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                case TypeFilterEnum.MenorIgual:     // <=
                    {
                        if (Type == TypeCode.Int16 || Type == TypeCode.Int32 || Type == TypeCode.Int64 || Type == TypeCode.Double) result = "{ $lte: " + Value + " }";
                        else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;

                default:
                    {
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $eq: \"" + Value + "\" }";
                        else result = "{ $eq: \"" + Value + "\" }";
                    }
                    break;
            }

            return result;
        }

        private string FilterBy(TypeFilterEnum TypeFilter, TypeCode Type, List<string> Value)
        {
            string result;

            switch (TypeFilter)
            {
                case TypeFilterEnum.Forade:         // NOT IN () { $nin: [ 5, 15 ] }
                    {
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $nin [ " + string.Join("', '", Value) + " ] }";
                        else result = "{ $in [ " + string.Join("', '", Value) + " ] }";
                    }
                    break;
                case TypeFilterEnum.Em:             // IN () { $in: [ 5, 15 ] }
                    {
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $in [ " + string.Join("', '", Value) + " ] }";
                        else result = "{ $in [ " + string.Join("', '", Value) + " ] }";
                    }
                    break;
                case TypeFilterEnum.Entre:          // BETWEEN  {"$gte": "2019-06-22 08:10:00", "$lt": "2019-06-22 08:15:00"}
                    {
                        if (Value.Count == 2)
                        {
                            if (Type == TypeCode.String || Type == TypeCode.DateTime)
                            {
                                DateTime D0 = Convert.ToDateTime(Value[0]);
                                DateTime D1 = Convert.ToDateTime(Value[1]);

                                result = D0 < D1 ? "{ \"$gte\": \"" + D0.ToString("yyyy-MM-dd HH:mm:ss") + "\", \"$lte\" : \"" + D1.ToString("yyyy-MM-dd HH:mm:ss") + "\" } " : "{ \"$gte\": \"" + D1.ToString("yyyy-MM-dd HH:mm:ss") + "\", \"$lte\" : \"" + D0.ToString("yyyy-MM-dd HH:mm:ss") + "\" } ";
                            }
                            else
                            {                                
                                result = "{ $in [ " + string.Join("', '", Value) + " ] }";
                            }
                        }
                        else
                        {
                            if (Type == TypeCode.String || Type == TypeCode.DateTime) result = "{ $in [ " + string.Join("', '", Value) + " ] }";
                            else result = "{ $in [ " + string.Join("', '", Value) + " ] }";
                        }
                    }
                    break;

                default:
                    {
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"IN ('{string.Join("', '", Value)}')";
                        else result = "{ $in [ " + string.Join("', '", Value) + " ] }";
                    }
                    break;
            }

            return result;
        }

        public List<FieldsFilterModel> FilterByKey(string Value)
        {
            List<FieldsFilterModel> result = new List<FieldsFilterModel>();

            // Obtem os campos da classe
            FieldsModel key = new FieldsModel();
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                // Captura os dados da chave primaria
                var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Core").ToList();
                if (Arg.Count > 0)
                {
                    var ArgF = Arg[0].NamedArguments.ToList();
                    var ArgK = ArgF.Where(a => a.MemberName == "PrimaryKey").ToList();
                    if (ArgK.Count == 1)
                    {
                        key.name = prop.Name;
                        Type property = prop.PropertyType;
                        key.type = System.Type.GetTypeCode(property);
                        key.value = prop.GetValue(this, null).ToString();
                    };
                }
            }

            if (key.name != "")
            {
                var val = Value;
                string value;

                // Verifica se a propriedade possui valor, ignorar nulos
                if (val != null)
                {
                    switch (key.type)
                    {
                        case TypeCode.Int16:
                            value = val.ToString();
                            break;
                        case TypeCode.Int32:
                            value = val.ToString();
                            break;
                        case TypeCode.Int64:
                            value = val.ToString();
                            break;
                        case TypeCode.Double:
                            value = val.ToString();
                            break;
                        case TypeCode.String:
                            value = $"'{val.ToString()}'";
                            break;
                        case TypeCode.DateTime:
                            var datetime = Convert.ToDateTime(val);

                            // Verifica tipo de campo
                            if (datetime.TimeOfDay != DateTime.Parse("00:00:00").TimeOfDay)
                            {
                                // DateTime
                                if (datetime.Date != DateTime.Parse("0001-01-01").Date)
                                {
                                    value = $"'{datetime.ToString("yyyy-MM-dd HH:mm:ss")}'";
                                }
                                else
                                {
                                    // Time
                                    value = $"'{datetime.ToString("HH:mm:ss")}'";
                                }
                            }
                            else
                            {
                                // Date
                                value = $"'{datetime.ToString("yyyy-MM-dd")}'";
                            }
                            break;
                        case TypeCode.Boolean:
                            value = val.ToString();
                            break;
                        default:
                            value = $"'{val.ToString()}'";
                            break;
                    }
                    // Adiciona o registro a lista
                    result.Add(new FieldsFilterModel() { name = key.name, type = key.type, method = TypeFilterEnum.Igual, value = value });
                }
            }

            return result;
        }

        public List<FieldsFilterModel> FilterByIndex(List<FieldsModel> Value)
        {
            List<FieldsFilterModel> result = new List<FieldsFilterModel>();

            // Obtem os campos da classe
            List<FieldsModel> keys = new List<FieldsModel>();
            var props = this.GetType().GetProperties();
            
            foreach (var prop in props)
            {
                // Captura os dados da chave primaria
                var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Core").ToList();
                if (Arg.Count > 0)
                {
                    var ArgF = Arg[0].NamedArguments.ToList();
                    var ArgK = ArgF.Where(a => a.MemberName == "Index").ToList();
                    if (ArgK.Count == 1)
                    {
                        if (Value.Exists(i => i.name == prop.Name))
                        {
                            FieldsModel key = new FieldsModel();
                            key.name = prop.Name;
                            Type property = prop.PropertyType;
                            key.type = Type.GetTypeCode(property);
                            key.value = Value.Where(i => i.name == prop.Name).ToList()[0].value; ;
                            keys.Add(key);
                        }
                    }
                }
            }

            keys.ForEach(key =>
            {
                var val = key.value;
                string value;

                switch (key.type)
                {
                    case TypeCode.Int16:
                        value = val.ToString();
                        break;
                    case TypeCode.Int32:
                        value = val.ToString();
                        break;
                    case TypeCode.Int64:
                        value = val.ToString();
                        break;
                    case TypeCode.Double:
                        value = val.ToString();
                        break;
                    case TypeCode.String:
                        value = $"'{val.ToString()}'";
                        break;
                    case TypeCode.DateTime:
                        var datetime = Convert.ToDateTime(val);

                        // Verifica tipo de campo
                        if (datetime.TimeOfDay != DateTime.Parse("00:00:00").TimeOfDay)
                        {
                            // DateTime
                            if (datetime.Date != DateTime.Parse("0001-01-01").Date)
                            {
                                value = $"'{datetime.ToString("yyyy-MM-dd HH:mm:ss")}'";
                            }
                            else
                            {
                                // Time
                                value = $"'{datetime.ToString("HH:mm:ss")}'";
                            }
                        }
                        else
                        {
                            // Date
                            value = $"'{datetime.ToString("yyyy-MM-dd")}'";
                        }
                        break;
                    case TypeCode.Boolean:
                        value = val.ToString();
                        break;
                    default:
                        value = $"'{val.ToString()}'";
                        break;
                }
                // Adiciona o registro a lista
                result.Add(new FieldsFilterModel() { name = key.name, type = key.type, method = TypeFilterEnum.Igual, value = value });
            });
            return result;
        }

        public string MakeFilter(List<FieldsFilterModel> filters) 
        {
            List<string> querys = new List<string>();
            string result = string.Empty;
            try
            {
                var groups = filters.GroupBy(r => r.name).ToList();

                groups.ForEach(item =>
                {
                    if (item.Count() > 1)
                    {
                        List<string> values = new List<string>();
                        string FieldName = "";
                        TypeFilterEnum TypeFilter = TypeFilterEnum.Igual;
                        TypeCode Type = TypeCode.String;

                        foreach (var field in item)
                        {
                            values.Add(field.value);
                            FieldName = field.name;
                            TypeFilter = field.method;
                            Type = field.type;
                        }
                        querys.Add("\"" + FieldName + "\": " + FilterBy(TypeFilter, Type, values));
                    }
                    else
                    {
                        foreach (var field in item)
                        {
                            querys.Add("\"" + field.name + "\": " + FilterBy(field.method, field.type, field.value));
                        }
                    }
                });

                result = "{" + string.Join(", ",querys.ToList()) + "}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return result;
        }
        #endregion
    }
}

using Sypper.Domain.Application.Enumerators;
using Sypper.Domain.Application.Processing;
using Sypper.Domain.Infra.Attributes;
using Sypper.Domain.Infra.Data;

namespace Sypper.Domain.Application.Extension
{
    public static class SQLExtension
    {
        #region FIELDS
        private static List<string> GetFiels<T>(this T obj) where T : SQLModel
        {
            var fields = new List<string>();

            // Obtem os campos da classe excluindo os registros a serem ignorados
            var ignore = typeof(T).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(Core)));
            List<string> list = new List<string>();

            foreach (var prop in ignore)
            {
                var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Core").ToList();
                if (Arg.Count > 0)
                {
                    var attribute = new Core(Arg[0].NamedArguments.ToList());

                    if (attribute.Ignore) list.Add(prop.Name);
                }
            }

            // Obtem os campos da classe
            var props = obj.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (!list.Exists(r => r == prop.Name))
                {
                    fields.Add(prop.Name);
                }
            }

            return fields;
        }
        #endregion

        #region FILTER
        private static string FilterBy<T>(this T obj, TypeFilterEnum TypeFilter, TypeCode Type, List<string> Value) where T : SQLModel
        {
            string result;

            switch (TypeFilter)
            {
                case TypeFilterEnum.Forade:         // NOT IN ()
                    {
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"NOT IN ('{string.Join("', '", Value)}')";
                        else result = $"IN ({string.Join(", ", Value)})";
                    }
                    break;
                case TypeFilterEnum.Em:             // IN ()
                    {
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"IN ('{string.Join("', '", Value)}')";
                        else result = $"IN ({string.Join(", ", Value)})";
                    }
                    break;
                case TypeFilterEnum.Entre:          // BETWEEN
                    {
                        if (Value.Count == 2)
                        {
                            if (Type == TypeCode.DateTime)
                            {
                                DateTime D0 = Convert.ToDateTime(Value[0]);
                                DateTime D1 = Convert.ToDateTime(Value[1]);

                                result = D0 < D1 ? $"BETWEEN '{Value[0]}' AND '{Value[1]}')" : $"BETWEEN '{Value[1]}' AND '{Value[0]}')";
                            }
                            else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"IN ('{string.Join("', '", Value)}')";
                            else result = $"IN ({string.Join(", ", Value)})";
                        }
                        else
                        {
                            if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"IN ('{string.Join("', '", Value)}')";
                            else result = $"IN ({string.Join(", ", Value)})";
                        }
                    }
                    break;

                default:
                    {
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"IN ('{string.Join("', '", Value)}')";
                        else result = $"IN ({string.Join(", ", Value)})";
                    }
                    break;
            }

            return result;
        }

        public static List<FieldsFilterModel> FilterByKey<T>(this T obj, string Value) where T : SQLModel
        {
            List<FieldsFilterModel> result = new List<FieldsFilterModel>();

            // Obtem os campos da classe
            FieldsModel key = new FieldsModel();
            var props = obj.GetType().GetProperties();
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
                        key.value = prop.GetValue(obj, null).ToString();
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

        public static List<FieldsFilterModel> FilterByIndex<T>(this T obj, List<FieldsModel> Value) where T : SQLModel
        {
            List<FieldsFilterModel> result = new List<FieldsFilterModel>();

            // Obtem os campos da classe
            List<FieldsModel> keys = new List<FieldsModel>();
            var props = obj.GetType().GetProperties();
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

            foreach (var key in keys)
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

            }

            return result;
        }
        #endregion

        #region SELECT
        public static string Select<T>(this T obj) where T : SQLModel
        {
            if (obj.GetTable() == "")
                return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<string> Fields = obj.GetFiels();

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {obj.GetTable()}";

            return SQL;
        }

        // Utilizado para consultas com limite ou paginação
        public static string Select<T>(this T obj, int Limit = -1, int Offset = -1) where T : SQLModel
        {
            // Verifica se o table está preenchida
            if (obj.GetTable() == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;

            List<string> Fields = obj.GetFiels();

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {obj.GetTable()}";

            // Verifica se havera limite de registros ou paginação
            if (Limit > 0)
            {
                if (Offset > 0)
                {
                    SQL += $" LIMIT {Limit} OFFSET {Offset}";
                }
                else
                {
                    SQL += $" LIMIT {Limit}";
                }
            }
            return SQL;
        }

        // Utilizado para consultas por chave primaria
        public static string Select<T>(this T obj, dynamic Key) where T : SQLModel
        {
            // Verifica se o table está preenchida
            if (obj.GetTable() == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;

            List<string> Fields = obj.GetFiels();

            // Filter
            List<FieldsFilterModel> FilterByFields = obj.FilterByKey(Key.ToString());

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {obj.GetTable()}";

            // Verifica se possui filtros
            if (FilterByFields != null)
            {
                string filter = "";
                List<string> filters = new List<string>();
                List<string> filtered = new List<string>();
                foreach (var fields in FilterByFields)
                {
                    // Verifica se o campo ja foi filtrado, caso de duplicados
                    if (!filtered.Exists(x => x == fields.name))
                    {
                        // Verifica se o filtro possui campos duplicados
                        var count = FilterByFields.Where(x => x.name == fields.name).Count();
                        if (count > 1)
                        {
                            // Adiciona o campo a lista de duplicados
                            filtered.Add(fields.name);

                            List<string> Values = new List<string>();
                            foreach (var dup in FilterByFields.Where(x => x.name == fields.name).ToList())
                            {
                                Values.Add(dup.value);
                            }
                            filters.Add($"{fields.name} {obj.FilterBy<T>(fields.method, fields.type, Values)}");
                        }
                        else
                        {
                            throw new NotImplementedException("Metodo não implementado");
                            //filters.Add($"{fields.name} {obj.FilterBy<T>(fields.method, fields.type, fields.value)}");
                        }
                    }
                }
                filter = string.Join(" AND ", filters);
                SQL = SQL + " WHERE " + filter;
            }
            return SQL;
        }

        // Utilizado para consultas com filtros declarados
        public static string Select<T>(this T obj, List<FieldsFilterModel> FilterByFields, bool ConditionalAnd = true) where T : SQLModel
        {
            // Verifica se o table está preenchida
            if (obj.GetTable() == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;

            List<string> Fields = obj.GetFiels();

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {obj.GetTable()}";

            // Verifica se possui filtros
            if (FilterByFields != null)
            {
                string filter = "";
                List<string> filters = new List<string>();
                List<string> filtered = new List<string>();
                foreach (var fields in FilterByFields)
                {
                    // Verifica se o campo ja foi filtrado, caso de duplicados
                    if (!filtered.Exists(x => x == fields.name))
                    {
                        // Verifica se o filtro possui campos duplicados
                        var count = FilterByFields.Where(x => x.name == fields.name).Count();
                        if (count > 1)
                        {
                            // Adiciona o campo a lista de duplicados
                            filtered.Add(fields.name);

                            List<string> Values = new List<string>();
                            foreach (var dup in FilterByFields.Where(x => x.name == fields.name).ToList())
                            {
                                Values.Add(dup.value);
                            }
                            filters.Add($"{fields.name} {obj.FilterBy(fields.method, fields.type, Values)}");
                        }
                        else
                        {
                            throw new NotImplementedException("Metodo não implementado");
                            //filters.Add($"{fields.name} {obj.FilterBy(fields.method, fields.type, fields.value)}");
                        }
                    }
                }
                if (ConditionalAnd) filter = string.Join(" AND ", filters);
                else filter = string.Join(" OR ", filters);
                SQL = SQL + " WHERE " + filter;
            }
            return SQL;
        }

        // Utilizado para consultas com filtros declarados e limite ou paginação
        public static string Select<T>(this T obj, List<FieldsFilterModel>? FilterByFields = null, int Limit = -1, int Offset = -1) where T : SQLModel
        {
            // Verifica se o table está preenchida
            if (obj.GetTable() == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;

            List<string> Fields = obj.GetFiels();

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {obj.GetTable()}";

            // Verifica se possui filtros
            if (FilterByFields != null)
            {
                string filter = "";
                List<string> filters = new List<string>();
                List<string> filtered = new List<string>();
                foreach (var fields in FilterByFields)
                {
                    // Verifica se o campo ja foi filtrado, caso de duplicados
                    if (!filtered.Exists(x => x == fields.name))
                    {
                        // Verifica se o filtro possui campos duplicados
                        var count = FilterByFields.Where(x => x.name == fields.name).Count();
                        if (count > 1)
                        {
                            // Adiciona o campo a lista de duplicados
                            filtered.Add(fields.name);

                            List<string> Values = new List<string>();
                            foreach (var dup in FilterByFields.Where(x => x.name == fields.name).ToList())
                            {
                                Values.Add(dup.value);
                            }
                            filters.Add($"{fields.name} {obj.FilterBy(fields.method, fields.type, Values)}");
                        }
                        else
                        {
                            throw new NotImplementedException("Metodo não implementado");
                            //filters.Add($"{fields.name} {obj.FilterBy(fields.method, fields.type, fields.value)}");
                        }
                    }
                }
                filter = string.Join(" AND ", filters);
                SQL = SQL + " WHERE " + filter;
            }

            // Verifica se havera limite de registros ou paginação
            if (Limit > 0)
            {
                if (Offset > 0)
                {
                    SQL = SQL + $" LIMIT {Limit} OFFSET {Offset}";
                }
                else
                {
                    SQL = SQL + $" LIMIT {Limit}";
                }
            }
            return SQL;
        }

        // Utilizado para consultas manuais podendo ter limite ou paginação
        public static string Select<T>(this T obj, string Filter) where T : SQLModel
        {
            // Verifica se o table está preenchida
            if (obj.GetTable() == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;

            List<string> Fields = obj.GetFiels();

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {obj.GetTable()}";

            // Verifica se possui filtros
            if (Filter != "")
            {
                SQL += " WHERE " + Filter;
            }

            return SQL;
        }

        // Utilizado para consultas manuais podendo ter limite ou paginação
        public static string Select<T>(this T obj, string Filter, int Limit = -1, int Offset = -1) where T : SQLModel
        {
            // Verifica se o table está preenchida
            if (obj.GetTable() == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;

            List<string> Fields = obj.GetFiels();

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {obj.GetTable()}";

            // Verifica se possui filtros
            if (Filter != "")
            {
                SQL += " WHERE " + Filter;
            }

            // Verifica se havera limite de registros ou paginação
            if (Limit > 0)
            {
                if (Offset > 0)
                {
                    SQL += $" LIMIT {Limit} OFFSET {Offset}";
                }
                else
                {
                    SQL += $" LIMIT {Limit}";
                }
            }
            return SQL;
        }
        #endregion
    }
}

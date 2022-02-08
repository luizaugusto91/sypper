using Sypper.Domain.Application.Enumerators;
using Sypper.Domain.Infra.Data;

namespace Sypper.Domain.Application.Processing
{
    public class SQLModel
    {
        private string table { get; set; } = string.Empty;

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
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
                    }
                    break;

                case TypeFilterEnum.Contem:         // LIKE %V%
                    {
                        if (Type == TypeCode.String) result = $"ILIKE '%{Value}%'";
                        else if (Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
                    }
                    break;

                case TypeFilterEnum.Inicia:         // LIKE V%
                    {
                        if (Type == TypeCode.String) result = $"ILIKE '{Value}%'";
                        else if (Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
                    }
                    break;

                case TypeFilterEnum.Termina:        // LIKE %V
                    {
                        if (Type == TypeCode.String) result = $"ILIKE '%{Value}'";
                        else if (Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
                    }
                    break;

                case TypeFilterEnum.Maior:          // >
                    {
                        if (Type == TypeCode.Int16 || Type == TypeCode.Int32 || Type == TypeCode.Int64 || Type == TypeCode.Double) result = $"> {Value}";
                        else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
                    }
                    break;

                case TypeFilterEnum.MaiorIgual:     // >=
                    {
                        if (Type == TypeCode.Int16 || Type == TypeCode.Int32 || Type == TypeCode.Int64 || Type == TypeCode.Double) result = $">= {Value}";
                        else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
                    }
                    break;

                case TypeFilterEnum.Menor:          // <
                    {
                        if (Type == TypeCode.Int16 || Type == TypeCode.Int32 || Type == TypeCode.Int64 || Type == TypeCode.Double) result = $"< {Value}";
                        else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
                    }
                    break;

                case TypeFilterEnum.MenorIgual:     // <=
                    {
                        if (Type == TypeCode.Int16 || Type == TypeCode.Int32 || Type == TypeCode.Int64 || Type == TypeCode.Double) result = $"<= {Value}";
                        else if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
                    }
                    break;

                default:
                    {
                        if (Type == TypeCode.String || Type == TypeCode.DateTime) result = $"= '{Value}'";
                        else result = $"= {Value}";
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

        public string MakeCondition(List<FieldsFilterModel> FilterByFields, bool ConditionalAnd = true)
        {
            // Verifica se possui filtros
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
                        filters.Add($"{fields.name} {FilterBy(fields.method, fields.type, Values)}");
                    }
                    else
                    {
                        filters.Add($"{fields.name} {FilterBy(fields.method, fields.type, fields.value)}");
                    }
                }
            }

            if (ConditionalAnd) filter = string.Join(" AND ", filters);
            else filter = string.Join(" OR ", filters);

            return filter;
        }
        #endregion

        #region Instruções

        #region SELECT

        public string Select()
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<string> Fields = new List<string>();

            // Obtem os campos da classe
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                Fields.Add(prop.Name);
            }

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {table}";

            return SQL;
        }

        public string Select(List<FieldsFilterModel> FilterByFields)
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<string> Fields = new List<string>();

            // Obtem os campos da classe
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                Fields.Add(prop.Name);
            }

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {table}";

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
                            filters.Add($"{fields.name} {FilterBy(fields.method, fields.type, Values)}");
                        }
                        else
                        {
                            filters.Add($"{fields.name} {FilterBy(fields.method, fields.type, fields.value)}");
                        }
                    }
                }
                filter = string.Join(" AND ", filters);
                SQL = SQL + " WHERE " + filter;
            }
            return SQL;
        }

        // Utilizado para consultas com filtros declarados
        public string Select(List<FieldsFilterModel> FilterByFields, bool ConditionalAnd = true)
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<string> Fields = new List<string>();

            // Obtem os campos da classe
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                Fields.Add(prop.Name);
            }

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {table}";

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
                            filters.Add($"{fields.name} {FilterBy(fields.method, fields.type, Values)}");
                        }
                        else
                        {
                            filters.Add($"{fields.name} {FilterBy(fields.method, fields.type, fields.value)}");
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
        public string Select(List<FieldsFilterModel> FilterByFields = null, int Limit = -1, int Offset = -1)
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<string> Fields = new List<string>();

            // Obtem os campos da classe
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                Fields.Add(prop.Name);
            }

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {table}";

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
                            filters.Add($"{fields.name} {FilterBy(fields.method, fields.type, Values)}");
                        }
                        else
                        {
                            filters.Add($"{fields.name} {FilterBy(fields.method, fields.type, fields.value)}");
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

        // Utilizado para consultas com limite ou paginação
        public string Select(int Limit = -1, int Offset = -1)
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<string> Fields = new List<string>();

            // Obtem os campos da classe
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                Fields.Add(prop.Name);
            }

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {table}";

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

        // Utilizado para consultas manuais podendo ter limite ou paginação
        public string Select(string Filter, int Limit = -1, int Offset = -1)
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<string> Fields = new List<string>();

            // Obtem os campos da classe
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                Fields.Add(prop.Name);
            }

            // Monta o SQL
            SQL = $"SELECT {string.Join(", ", Fields)} FROM {table}";

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

        #region INSERT
        public string Insert()
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<FieldsModel> Fields = new List<FieldsModel>();

            // Obtem os campos da classe
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                // Verifica se são campos auto-incremento
                bool Restric = false;
                var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Core").ToList();
                if (Arg.Count > 0)
                {
                    var ArgF = Arg[0].NamedArguments.ToList();
                    var ArgV = ArgF.Where(a => a.MemberName == "Restrict").ToList();
                    if (ArgV.Count > 0) Restric = true;
                }

                if (!Restric)
                {
                    Type propertyType = prop.PropertyType;
                    TypeCode typeCode = Type.GetTypeCode(propertyType);
                    string value = "";

                    var val = prop.GetValue(this, null);

                    // Verifica se a propriedade possui valor, ignorar nulos
                    if (val != null)
                    {
                        switch (typeCode)
                        {
                            case TypeCode.Int16:
                                value = Convert.ToInt16(val).ToString();
                                break;
                            case TypeCode.Int32:
                                value = Convert.ToInt32(val).ToString();
                                break;
                            case TypeCode.Int64:
                                value = Convert.ToInt64(val).ToString();
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

                        Fields.Add(new FieldsModel() { name = prop.Name, type = typeCode, value = value });
                    }
                }
            }

            // Monta o esquema dos campos
            string fields;
            string values;
            List<string> ListFields = new List<string>();
            List<string> ListValues = new List<string>();
            Fields.ForEach(f => ListFields.Add(f.name));
            Fields.ForEach(v => ListValues.Add(v.value));
            fields = string.Join(", ", ListFields);
            values = string.Join(", ", ListValues);

            // Monta o SQL
            SQL = $"INSERT INTO {table} ({fields}) VALUES ({values})";
            return SQL;
        }

        public string Insert(bool ReturningKey)
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<FieldsModel> Fields = new List<FieldsModel>();

            // Obtem os campos da classe
            FieldsModel key = new FieldsModel();
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                // Verifica se são campos auto-incremento
                bool Restric = false;
                var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Core").ToList();
                if (Arg.Count > 0)
                {
                    var ArgF = Arg[0].NamedArguments.ToList();
                    var ArgV = ArgF.Where(a => a.MemberName == "Restrict").ToList();
                    if (ArgV.Count > 0) Restric = true;

                    // Captura os dados da chave primaria
                    var ArgK = ArgF.Where(a => a.MemberName == "PrimaryKey").ToList();
                    if (ArgK.Count == 1)
                    {
                        key.name = prop.Name;
                        Type propertyType = prop.PropertyType;
                        key.type = System.Type.GetTypeCode(propertyType);
                        key.value = prop.GetValue(this, null).ToString();
                    };
                }

                if (!Restric)
                {
                    Type propertyType = prop.PropertyType;
                    TypeCode typeCode = Type.GetTypeCode(propertyType);
                    string value = "";

                    var val = prop.GetValue(this, null);

                    // Verifica se a propriedade possui valor, ignorar nulos
                    if (val != null)
                    {
                        switch (typeCode)
                        {
                            case TypeCode.Int16:
                                value = val.ToString();
                                break;
                            case TypeCode.Int32:
                                {
                                    try
                                    {
                                        if (!prop.IsDefined(typeof(FlagsAttribute), false))
                                        {
                                            var intval = (int)Enum.Parse((Type)prop.PropertyType, val.ToString());                                            
                                            value = intval.ToString();
                                        }
                                        else
                                        {
                                            value = val.ToString();
                                        }
                                    }
                                    catch
                                    {
                                        value = val.ToString();
                                    }
                                }                                
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

                        Fields.Add(new FieldsModel() { name = prop.Name, type = typeCode, value = value });
                    }
                }
            }

            // Monta o esquema dos campos
            string fields;
            string values;
            List<string> ListFields = new List<string>();
            List<string> ListValues = new List<string>();
            Fields.ForEach(f => ListFields.Add(f.name));
            Fields.ForEach(v => ListValues.Add(v.value));
            fields = string.Join(", ", ListFields);
            values = string.Join(", ", ListValues);

            // Monta o SQL
            SQL = $"INSERT INTO {table} ({fields}) VALUES ({values})";

            if (ReturningKey)
            {
                SQL += $" RETURNING {key.name}";
            }
            return SQL;
        }
        #endregion

        #region UPDATE
        public string Update(List<FieldsFilterModel> FilterByFields = null)
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;
            List<FieldsModel> Fields = new List<FieldsModel>();

            // Obtem os campos da classe
            FieldsModel key = new FieldsModel();
            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                // Verifica se são campos auto-incremento
                bool Restric = false;
                var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Core").ToList();
                if (Arg.Count > 0)
                {
                    var ArgF = Arg[0].NamedArguments.ToList();
                    var ArgR = ArgF.Where(a => a.MemberName == "Restrict").ToList();
                    if (ArgR.Count > 0) Restric = true;

                    // Captura os dados da chave primaria
                    var ArgK = ArgF.Where(a => a.MemberName == "PrimaryKey").ToList();
                    if (ArgK.Count == 1)
                    {
                        key.name = prop.Name;
                        Type propertyType = prop.PropertyType;
                        key.type = System.Type.GetTypeCode(propertyType);
                        key.value = prop.GetValue(this, null).ToString();
                    };
                }

                if (!Restric)
                {
                    Type propertyType = prop.PropertyType;
                    TypeCode typeCode = System.Type.GetTypeCode(propertyType);
                    string value = "";

                    var val = prop.GetValue(this, null);

                    // Verifica se a propriedade possui valor, ignorar nulos
                    if (val != null)
                    {
                        switch (typeCode)
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

                        Fields.Add(new FieldsModel() { name = prop.Name, type = typeCode, value = value });
                    }
                }
            }

            string fields;
            List<string> ListFields = new List<string>();

            foreach (var Field in Fields)
            {
                ListFields.Add($"{Field.name} = {Field.value}");
            }
            fields = string.Join(", ", ListFields);

            // Monta o SQL
            SQL = $"UPDATE {table} SET {fields}";

            // Verifica se possui filtros
            string filter = "";
            if (FilterByFields != null)
            {
                List<string> filters = new List<string>();
                List<string> filtered = new List<string>();
                foreach (var fieldfilter in FilterByFields)
                {
                    // Verifica se o campo ja foi filtrado, caso de duplicados
                    if (!filtered.Exists(x => x == fieldfilter.name))
                    {
                        // Verifica se o filtro possui campos duplicados
                        var count = FilterByFields.Where(x => x.name == fieldfilter.name).Count();
                        if (count > 1)
                        {
                            // Adiciona o campo a lista de duplicados
                            filtered.Add(fieldfilter.name);

                            List<string> Values = new List<string>();
                            foreach (var dup in FilterByFields.Where(x => x.name == fieldfilter.name).ToList())
                            {
                                Values.Add(dup.value);
                            }
                            filters.Add($"{fieldfilter.name} {FilterBy(fieldfilter.method, fieldfilter.type, Values)}");
                        }
                        else
                        {
                            filters.Add($"{fieldfilter.name} {FilterBy(fieldfilter.method, fieldfilter.type, fieldfilter.value)}");
                        }
                    }
                }
                filter = string.Join(" AND ", filters);
                SQL = SQL + " WHERE " + filter;
            }
            else
            {
                filter = $"{key.name} {FilterBy(TypeFilterEnum.Igual, key.type, key.value)}";
                SQL = SQL + " WHERE " + filter;
            }

            return SQL;
        }
        #endregion

        #region DELETE
        public string Delete(List<FieldsFilterModel> FilterByFields = null)
        {
            // Verifica se o table está preenchida
            if (table == "") return "Table não definida.";

            // Iniciar o processo de montagem do SQL
            string SQL = string.Empty;

            FieldsModel key = new FieldsModel();
            FieldsModel exclude = new FieldsModel();

            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                // Verifica se são campos auto-incremento
                var Arg = prop.CustomAttributes.Where(a => a.AttributeType.Name == "Core").ToList();
                if (Arg.Count > 0)
                {
                    // Captura os dados da chave primaria
                    var ArgF = Arg[0].NamedArguments.ToList();
                    var ArgK = ArgF.Where(a => a.MemberName == "PrimaryKey").ToList();
                    if (ArgK.Count == 1)
                    {
                        key.name = prop.Name;
                        Type propertyType = prop.PropertyType;
                        key.type = System.Type.GetTypeCode(propertyType);
                        key.value = prop.GetValue(this, null).ToString();
                    };

                    // Verifica se possui atributo de exclusão
                    var ArgE = ArgF.Where(a => a.MemberName == "Exclude").ToList();
                    if (ArgE.Count == 1)
                    {
                        exclude.name = prop.Name;
                        Type propertyType = prop.PropertyType;
                        exclude.type = System.Type.GetTypeCode(propertyType);
                        exclude.value = prop.GetValue(this, null).ToString();
                    };
                }
            }

            if (exclude != default)
            {
                SQL = $"UPDATE {table} SET {exclude.name} = true";
            }
            else
            {
                SQL = $"DELETE FROM {table}";
            }

            // Verifica se possui filtros
            string filter = "";
            if (FilterByFields != null)
            {
                List<string> filters = new List<string>();
                List<string> filtered = new List<string>();
                foreach (var fieldfilter in FilterByFields)
                {
                    // Verifica se o campo ja foi filtrado, caso de duplicados
                    if (!filtered.Exists(x => x == fieldfilter.name))
                    {
                        // Verifica se o filtro possui campos duplicados
                        var count = FilterByFields.Where(x => x.name == fieldfilter.name).Count();
                        if (count > 1)
                        {
                            // Adiciona o campo a lista de duplicados
                            filtered.Add(fieldfilter.name);

                            List<string> Values = new List<string>();
                            foreach (var dup in FilterByFields.Where(x => x.name == fieldfilter.name).ToList())
                            {
                                Values.Add(dup.value);
                            }
                            filters.Add($"{fieldfilter.name} {FilterBy(fieldfilter.method, fieldfilter.type, Values)}");
                        }
                        else
                        {
                            filters.Add($"{fieldfilter.name} {FilterBy(fieldfilter.method, fieldfilter.type, fieldfilter.value)}");
                        }
                    }
                }
                filter = string.Join(" AND ", filters);
                SQL = SQL + " WHERE " + filter;
            }
            else
            {
                filter = $"{key.name} {FilterBy(TypeFilterEnum.Igual, key.type, key.value)}";
                SQL = SQL + " WHERE " + filter;
            }

            return SQL;
        }
        #endregion

        #region Prepare
        public string PrepareSQL<T>(TypeSQLEnum Type, string Table = "")
        {
            if ((table == null || table == "") && Table == "") return "Table não definida.";

            string SQL = string.Empty;
            List<FieldsModel> Fields = new List<FieldsModel>();

            var props = this.GetType().GetProperties();
            foreach (var prop in props)
            {
                Type propertyType = prop.PropertyType;
                TypeCode typeCode = System.Type.GetTypeCode(propertyType);
                string value = "";

                var val = prop.GetValue(this, null);

                // Verifica se a propriedade possui valor, ignorar nulos
                if (val != null)
                {
                    switch (typeCode)
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
                    Fields.Add(new FieldsModel() { name = prop.Name, type = typeCode, value = value });
                }
            }

            // Monta o SQL
            switch (Type)
            {
                case TypeSQLEnum.Select:
                    {
                        string fields;
                        string values;
                        List<string> ListFields = new List<string>();
                        List<string> ListValues = new List<string>();
                        Fields.ForEach(f => ListFields.Add(f.name));
                        Fields.ForEach(v => ListValues.Add(v.value));
                        fields = string.Join(", ", ListFields);
                        values = string.Join(", ", ListValues);
                        SQL = $"SELECT {fields} FROM {table}";
                    }
                    break;

                case TypeSQLEnum.Insert:
                    {
                        string fields;
                        string values;
                        List<string> ListFields = new List<string>();
                        List<string> ListValues = new List<string>();
                        Fields.ForEach(f => ListFields.Add(f.name));
                        Fields.ForEach(v => ListValues.Add(v.value));
                        fields = string.Join(", ", ListFields);
                        values = string.Join(", ", ListValues);
                        SQL = $"INSERT INTO {table} ({fields}) VALUES ({values})";
                    }
                    break;

                case TypeSQLEnum.Update:
                    {
                        string fields;
                        List<string> ListFields = new List<string>();

                        foreach (var Field in Fields)
                        {
                            ListFields.Add($"{Field.name} = {Field.value}");
                        }
                        fields = string.Join(", ", ListFields);
                        SQL = $"UPDATE {table} SET {fields}";
                    }
                    break;
                case TypeSQLEnum.Delete:
                    {
                        SQL = $"DELETE FROM {table}";
                    }
                    break;
            }

            return SQL;
        }
        #endregion

        #endregion
    }
}

using Sypper.Domain.Generico.Processing;
using Sypper.Domain.Infra.Connection;
using Npgsql;
using System.Data;
using System.Reflection;
using Sypper.Domain.Application.Processing;
using Sypper.Domain.Infra.Data;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Sypper.Infra.Connection
{    
    public class PostgreSQLConnectionInfra
    {
        #region Properties
        private PGConnectionSettingsModel Settings;
        private NpgsqlConnection Conn;
        private bool UseTransction = true;
        #endregion

        #region Constructor
        public PostgreSQLConnectionInfra(string Server, int Port, string Username, string Password, string Database, int Timeout = 0)
        {
            Settings = new PGConnectionSettingsModel()
            {
                host = Server,
                port = Port,
                username = Username,
                password = Password,
                database = Database,
                timeout = Timeout
            };

            Conn = new NpgsqlConnection(Settings.GetPGConnectionString());            
        }

        public PostgreSQLConnectionInfra(PGConnectionSettingsModel SettingsData)
        {
            Settings = SettingsData;
            Conn = new NpgsqlConnection(Settings.GetPGConnectionString());            
        }
        #endregion

        #region Connection
        public bool Opened()
        {
            return Conn.State == System.Data.ConnectionState.Open;
        }

        public void Close()
        {
            try
            {
                Conn.Close();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"ERRO Close: {e.ErrorCode}>{e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERRO Close: {e.Message}.");
            }
        }
        #endregion

        #region Transaction
        private NpgsqlTransaction? Transaction()
        {
            if (UseTransction)
            {
                return Conn.BeginTransaction();
            }
            return default(NpgsqlTransaction);
        }

        private NpgsqlTransaction? Transaction(NpgsqlConnection Connection)
        {
            if (UseTransction)
            {
                return Connection.BeginTransaction();
            }
            return default(NpgsqlTransaction);
        }

        private void Roolback(NpgsqlTransaction? Transaction)
        {
            if (UseTransction && Transaction != null)
            {
                try
                {
                    Transaction.Rollback();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Transaction.Rollback >> {e.Message}");
                }
                
            }
        }

        private void Commit(NpgsqlTransaction? Transaction)
        {
            if (UseTransction && Transaction != null)
            {
                Transaction.Commit();
            }
        }
        #endregion

        #region Manipulate
        private static T ConvertDataRow<T>(DataTable dt)
        {
            DataRow row = dt.Rows[0];
            return GetItemDefault<T>(row);
        }

        private static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItemDefault<T>(row);
                data.Add(item);
            }
            return data;
        }

        private static T GetItemDefault<T>(DataRow dr)
        {
            T obj = Activator.CreateInstance<T>();
            List<PropertyInfo> props = typeof(T).GetProperties().ToList();

            foreach (DataColumn column in dr.Table.Columns)
            {
                var prop = props.Where(r => r.Name == column.ColumnName).FirstOrDefault();
                if (prop != null)
                {
                    Type field = prop.PropertyType;
                    var value = dr[column.ColumnName];
                    if (value.GetType().Name == "DBNull")
                    {
                        value = default;
                    }

                    prop.SetValue(obj, value, null);
                }
            }

            return (T)obj;
        }
        #endregion

        #region Query
        public ReturnModel<bool> Execute(string SQL)
        {
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            ReturnModel<bool> result = new ReturnModel<bool>();
            NpgsqlTransaction ExecuteTransaction;

            Connection.Conn.Open();            
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn);
                var affected = Command.ExecuteNonQuery();
                Commit(ExecuteTransaction);

                result.Success($"Instrução executada com sucesso. Foram alterada(s) {affected} linha(s).", true);
            }
            catch (NpgsqlException e)
            {                
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {                
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Conn.Close();
            }
            return result;
        }

        public async Task<ReturnModel<bool>> ExecuteAsync(string SQL)
        {
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            ReturnModel<bool> result = new ReturnModel<bool>();
            Connection.Conn.Open();
            NpgsqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn);
                var affected = await Command.ExecuteNonQueryAsync();
                Commit(ExecuteTransaction);
                result.Success($"Instrução executada com sucesso. Foram alterada(s) {affected} linha(s).", true);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Conn.Close();
            }
            return result;
        }

        // Utilizar apenas para inserts com retornos do tipo Inteiro/Guid/String
        public ReturnModel<dynamic> ExecuteReturning(string SQL)
        {
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            ReturnModel<dynamic> result = new ReturnModel<dynamic>();
            Connection.Conn.Open();
            NpgsqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                // Realiza a instrução                
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable("new");
                Table.Load(Reader);
                int affected = Reader.RecordsAffected;
                Commit(ExecuteTransaction);

                // Captura os resultados
                if (Table.Rows.Count > 0)
                {
                    DataRow drs = Table.Rows[0];
                    var value = drs.ItemArray[0];
                    if (value.GetType().Name == "DBNull") 
                    {                        
                        result.Success($"Instrução executada com sucesso. Foram alterada(s) {affected} linha(s).", value);
                    }
                    else
                    {
                        result.Fail("Nenhum registro afetado para ínstrução requisitada.");
                    }
                }
                else
                {
                    result.Fail("Nenhum registro afetado para ínstrução requisitada.");
                }
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Conn.Close();
            }
            return result;
        }

        public async Task<ReturnModel<dynamic>> ExecuteReturningAsync(string SQL)
        {
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            ReturnModel<dynamic> result = new ReturnModel<dynamic>();
            Connection.Conn.Open();
            NpgsqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                // Realiza a instrução                
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                NpgsqlDataReader Reader = await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable("new");
                Table.Load(Reader);
                int affected = Reader.RecordsAffected;
                Commit(ExecuteTransaction);

                // Captura os resultados
                if (Table.Rows.Count > 0)
                {
                    DataRow drs = Table.Rows[0];
                    var value = drs.ItemArray[0];
                    if (value.GetType().Name == "DBNull")
                    {
                        result.Success($"Instrução executada com sucesso. Foram alterada(s) {affected} linha(s).", value);
                    }
                    else
                    {
                        result.Fail("Nenhum registro afetado para ínstrução requisitada.");
                    }
                }
                else
                {
                    result.Fail("Nenhum registro afetado para ínstrução requisitada.");
                }
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Conn.Close();
            }
            return result;
        }

        public ReturnModel<long> Count(string SQL, string Tabela)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ReturnModel<long> result = new ReturnModel<long>();
            NpgsqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                long count = Table.Rows.Count;

                result.Success("Consulta realizada com sucesso.", count);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public async Task<ReturnModel<long>> CountAsync(string SQL, string Tabela)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ReturnModel<long> result = new ReturnModel<long>();
            NpgsqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                NpgsqlDataReader Reader = await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                long count = Table.Rows.Count;

                result.Success("Consulta realizada com sucesso.", count);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public NpgsqlDataReader Query(string SQL)
        {
            try
            {
                Conn.Open();
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Conn);
                return Command.ExecuteReader();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"ERRO Query: {e.ErrorCode}>{e.Message}. SQL:{SQL}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERRO Query: {e.Message}. SQL:{SQL}");
                return null;
            }
        }

        public async Task<NpgsqlDataReader> QueryAsync(string SQL)
        {
            try
            {
                Conn.Open();
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Conn);
                return await Command.ExecuteReaderAsync();
            }
            catch (NpgsqlException e)
            {
                Console.WriteLine($"ERRO Query: {e.ErrorCode}>{e.Message}. SQL:{SQL}");
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERRO Query: {e.Message}. SQL:{SQL}");
                return null;
            }
        }

        public ReturnModel<DataTable> Query(string SQL, string Tabela)
        {
            /*
            * Metodo preferencial para retornar consultar em DataTable.
            */
            ReturnModel<DataTable> result = new ReturnModel<DataTable>();
            NpgsqlTransaction? ExecuteTransaction;
            Conn.Open();
            ExecuteTransaction = Transaction();
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Conn);
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                result.Success("Realizado com sucesso.", Table);                
                Commit(ExecuteTransaction);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Close();
            }
            return result;
        }

        public async Task<ReturnModel<DataTable>> QueryAsync(string SQL, string Tabela)
        {
            /*
            * Metodo preferencial para retornar consultar em DataTable.
            */
            ReturnModel<DataTable> result = new ReturnModel<DataTable>();
            NpgsqlTransaction ExecuteTransaction;
            Conn.Open();
            ExecuteTransaction = Transaction();
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Conn);
                NpgsqlDataReader Reader = await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                result.Success("Realizado com sucesso.", Table);
                Commit(ExecuteTransaction);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Close();
            }
            return result;
        }

        public ReturnModel<T> QueryTo<T>(string SQL, string Tabela)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            T obj = Activator.CreateInstance<T>(); ;
            ReturnModel<T> result = new ReturnModel<T>();
            NpgsqlTransaction ExecuteTransaction;

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();                      
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    obj = ConvertDataRow<T>(Table);

                    result.Success("Consulta realizada com sucesso.", obj);
                }
                else
                {
                    result.Fail("Nenhum registro encontrada para consulta requisitada.");
                }
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public async Task<ReturnModel<T>> QueryToAsync<T>(string SQL, string Tabela)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            T obj = Activator.CreateInstance<T>(); ;
            ReturnModel<T> result = new ReturnModel<T>();
            NpgsqlTransaction ExecuteTransaction;

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                NpgsqlDataReader Reader = await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    obj = ConvertDataRow<T>(Table);

                    result.Success("Consulta realizada com sucesso.", obj);
                }
                else
                {
                    result.Fail("Nenhum registro encontrada para consulta requisitada.");
                }
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public ReturnModel<List<T>> QueryToList<T>(T Entity) where T : SQLModel
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            NpgsqlTransaction ExecuteTransaction;

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);
            string SQL = string.Empty;
            try
            {
                SQL = Entity.Select();
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public ReturnModel<List<T>> QueryToList<T>(T Entity, dynamic KeyFilter) where T : SQLModel
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            NpgsqlTransaction ExecuteTransaction;
            string SQL = string.Empty;

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                SQL = Entity.Select(Entity.FilterByKey(KeyFilter.ToString()));
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public ReturnModel<List<T>> QueryToList<T>(T Entity, List<FieldsFilterModel> FilterByFields, bool ConditionalAnd = true) where T : SQLModel
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            NpgsqlTransaction ExecuteTransaction;
            string SQL = string.Empty;

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                SQL = Entity.Select(FilterByFields, ConditionalAnd);
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public ReturnModel<List<T>> QueryToList<T>(string SQL, string Tabela, bool Especial = false)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */            
            List<T> Lista = new List<T>();
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            NpgsqlTransaction ExecuteTransaction;            

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();            
            ExecuteTransaction = Transaction(Connection.Conn);
            
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn) { CommandTimeout = 0};                
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);                

                if (Table.Rows.Count > 0)
                {
                    Lista = ConvertDataTable<T>(Table);                    
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {                
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public async Task<ReturnModel<List<T>>> QueryToListAsync<T>(string SQL, string Tabela, bool Especial = false)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            NpgsqlTransaction ExecuteTransaction;

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                NpgsqlDataReader Reader = await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public async Task<ReturnModel<List<T>>> QueryToListAsync<T>(T Entity, dynamic KeyFilter) where T : SQLModel
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            NpgsqlTransaction ExecuteTransaction;
            string SQL = string.Empty;

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                SQL = Entity.Select(Entity.FilterByKey(KeyFilter.ToString()));
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                NpgsqlDataReader Reader = await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }

        public async Task<ReturnModel<List<T>>> QueryToListAsync<T>(T Entity, List<FieldsFilterModel> FilterByFields, bool ConditionalAnd = true) where T : SQLModel
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            NpgsqlTransaction ExecuteTransaction;
            string SQL = string.Empty;

            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                SQL = Entity.Select(FilterByFields, ConditionalAnd);
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                NpgsqlDataReader Reader = await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (NpgsqlException e)
            {
                result.Fail("Falha ao executar a instrução.", new ErrorModel(e.ErrorCode, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                result.Error("Erro ao executar a instrução.", new ErrorModel(0, e.Message, SQL, e.StackTrace));
                Roolback(ExecuteTransaction);
            }
            finally
            {
                if (result.Validate()) Connection.Close();
            }
            return result;
        }
        #endregion
    }
}

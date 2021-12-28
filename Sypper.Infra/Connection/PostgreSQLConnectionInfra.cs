using Sypper.Domain.Generico.Processing;
using Sypper.Domain.Infra.Connection;
using Npgsql;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sypper.Infra.Connection
{    
    public class PostgreSQLConnectionInfra
    {
        private PGConnectionSettingsModel Settings;
        private NpgsqlConnection Conn;
        private bool UsarTransacoes = true;

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

        private NpgsqlTransaction Transaction()
        {
            if (UsarTransacoes)
            {
                return Conn.BeginTransaction();
            }
            return null;
        }

        private NpgsqlTransaction Transaction(NpgsqlConnection Connection)
        {
            if (UsarTransacoes)
            {
                return Connection.BeginTransaction();
            }
            return null;
        }

        private void Roolback(NpgsqlTransaction Transaction)
        {
            if (UsarTransacoes)
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

        private void Commit(NpgsqlTransaction Transaction)
        {
            if (UsarTransacoes)
            {
                Transaction.Commit();
            }
        }

        public ReturnModel<bool> Execute(string SQL)
        {
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            ReturnModel<bool> result = new ReturnModel<bool>();
            Connection.Conn.Open();
            NpgsqlTransaction ExecuteTransaction;
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
                ErrorModel erro = new ErrorModel() { codigo = e.ErrorCode, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                result.Fail("Falha ao executar a instrução.", erro);                
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                result.Error("Erro ao executar a instrução.", erro);                
                Roolback(ExecuteTransaction);
            }
            finally
            {
                Connection.Conn.Close();
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
                ErrorModel erro = new ErrorModel() { codigo = e.ErrorCode, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                result.Fail("Falha ao executar a instrução.", erro);
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                result.Error("Erro ao executar a instrução.", erro);
                Roolback(ExecuteTransaction);
            }
            finally
            {
                Connection.Conn.Close();
            }
            return result;
        }

        // Utilizar apenas para inserts com retornos do tipo Inteiro/Guid/String
        public ReturnModel<Int64> ExecuteReturning(string SQL)
        {
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            ReturnModel<Int64> result = new ReturnModel<Int64>();
            Connection.Conn.Open();
            NpgsqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                // Realiza a instrução
                Int64 Value = -1;
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
                    Value = Convert.ToInt64(drs.ItemArray[0]);
                    result.Success($"Instrução executada com sucesso. Foram alterada(s) {affected} linha(s).", Value);
                }
                else
                {
                    result.Fail("Nenhum registro afetado para ínstrução requisitada.");
                }
            }
            catch (NpgsqlException e)
            {
                ErrorModel erro = new ErrorModel() { codigo = e.ErrorCode, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                result.Fail("Falha ao executar ínstrução.",erro);
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                result.Error("Erro ao executar ínstrução.", erro);                
                Roolback(ExecuteTransaction);
            }
            finally
            {
                Connection.Conn.Close();
            }
            return result;
        }

        public ReturnModel<Int64> Count(string SQL, string Tabela)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ReturnModel<Int64> Processo = new ReturnModel<Int64>();
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

                Int64 count = Table.Rows.Count;

                Processo.Success("Consulta realizada com sucesso.", count);
            }
            catch (NpgsqlException e)
            {
                ErrorModel erro = new ErrorModel() { codigo = e.ErrorCode, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                Processo.Fail("Falha ao executar consulta.", erro);
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                Processo.Error("Erro ao executar consulta.", erro);
                Roolback(ExecuteTransaction);
            }
            finally
            {
                Connection.Close();
            }
            return Processo;
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

        public ReturnModel<DataTable> Query(string SQL, string Tabela)
        {
            /*
            * Metodo preferencial para retornar consultar em DataTable.
            */
            ReturnModel<DataTable> Processo = new ReturnModel<DataTable>();
            NpgsqlTransaction ExecuteTransaction;
            Conn.Open();
            ExecuteTransaction = Transaction();
            try
            {
                NpgsqlCommand Command = new NpgsqlCommand(SQL, Conn);
                NpgsqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Processo.Success("Realizado com sucesso.", Table);                
                Commit(ExecuteTransaction);
            }
            catch (NpgsqlException e)
            {
                ErrorModel erro = new ErrorModel() { codigo = e.ErrorCode, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                Processo.Fail("Falha ao executar consulta.", erro);
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                Processo.Error("Erro ao executar consulta", erro);
                Roolback(ExecuteTransaction);
            }
            finally
            {
                Close();
            }
            return Processo;
        }

        public T GetObject<T>(Dictionary<string, object> dict)
        {
            Type type = typeof(T);
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                type.GetProperty(kv.Key).SetValue(obj, kv.Value);
            }
            return (T)obj;
        }

        public ReturnModel<T> QueryTo<T>(string SQL, string Tabela)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            T obj = default;
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ReturnModel<T> Processo = new ReturnModel<T>();
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

                if (Table.Rows.Count > 0)
                {

                    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                    Dictionary<string, object> row;
                    foreach (DataRow dr in Table.Rows)
                    {
                        row = new Dictionary<string, object>();
                        foreach (DataColumn col in Table.Columns)
                        {
                            row.Add(col.ColumnName, dr[col]);
                        }
                        rows.Add(row);
                    }

                    // Caso a tabela possua serialização, é necessário utilizar o option
                    JsonSerializerOptions options = new JsonSerializerOptions();
                    options.Converters.Add(new JsonStringEnumConverter());

                    rows.ForEach(r => obj = GetObject<T>(r));
                    Processo.Success("Consulta realizada com sucesso.", obj);
                }
                else
                {
                    Processo.Fail("Nenhum registro encontrada para consulta requisitada.");
                }
            }
            catch (NpgsqlException e)
            {
                ErrorModel erro = new ErrorModel() { codigo = e.ErrorCode, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                Processo.Fail("Falha ao executar consulta.", erro);
                Roolback(ExecuteTransaction);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                Processo.Error("Erro ao executar consulta.", erro);
                Roolback(ExecuteTransaction);
            }
            finally
            {
                Connection.Close();
            }
            return Processo;
        }

        public ReturnModel<List<T>> QueryToList<T>(string SQL, string Tabela, bool Especial = false)
        {
            /*
             * Metodo preferencial para retornar consultar direto em objetos.
             * Para utilizar este metodo, lembre-se que as colunas do SQL devem possuir o mesmo nome dos atributos das classes.
             * Obs.: Não ha problema algum em haver campos adicionas ou faltando na consulta em comparação ao objeto.
             */
            List<T> Lista = new List<T>();
            PostgreSQLConnectionInfra Connection = new PostgreSQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ReturnModel<List<T>> Processo = new ReturnModel<List<T>>();
            NpgsqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            bool Error = false;
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

                    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
                    Dictionary<string, object> row;
                    foreach (DataRow dr in Table.Rows)
                    {
                        row = new Dictionary<string, object>();
                        foreach (DataColumn col in Table.Columns)
                        {
                            row.Add(col.ColumnName, dr[col]);
                        }
                        rows.Add(row);
                    }

                    // Caso a tabela possua serialização, é necessário utilizar o option
                    
                    JsonSerializerOptions options = new JsonSerializerOptions() 
                    {                         
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault                        
                    };                    
                    options.Converters.Add(new JsonStringEnumConverter());

                    if (Especial)
                    {
                        foreach (var rowa in rows)
                        {
                            Lista.Add(GetObject<T>(rowa));
                        }
                    }
                    else
                    {
                        string data = JsonSerializer.Serialize(rows);
                        Lista = JsonSerializer.Deserialize<List<T>>(data, options);
                    }
                }
                else
                {
                    Lista = new List<T>();
                }
                Processo.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (NpgsqlException e)
            {
                Error = true;
                ErrorModel erro = new ErrorModel() { codigo = e.ErrorCode, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                Processo.Fail("Falha ao executar consulta.", erro);
                Roolback(ExecuteTransaction);                
            }
            catch (Exception e)
            {
                Error = true;
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = SQL, stack = e.StackTrace };
                Processo.Error("Erro ao executar consulta.", erro);
                Roolback(ExecuteTransaction);                
            }
            finally
            {
                if (!Error) Connection.Close();
            }
            return Processo;
        }        
    }
}

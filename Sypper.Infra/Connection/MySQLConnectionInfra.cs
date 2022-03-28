using MySql.Data.MySqlClient;
using Sypper.Domain.Application.Processing;
using Sypper.Domain.Generico.Processing;
using Sypper.Domain.Infra.Connection;
using Sypper.Domain.Infra.Data;
using System.Data;
using System.Reflection;

namespace Sypper.Infra.Connection
{
    public class MySQLConnectionInfra
    {
        #region Properties
        private MYConnectionSettingsModel Settings;
        private MySqlConnection Conn;
        private bool UseTransction = true;
        #endregion

        #region Constructor
        public MySQLConnectionInfra(string Server, int Port, string Username, string Password, string Database)
        {
            Settings = new MYConnectionSettingsModel()
            {
                host = Server,
                port = Port,
                username = Username,
                password = Password,
                database = Database,                
            };

            Conn = new MySqlConnection(Settings.GetMYConnectionString());
        }

        public MySQLConnectionInfra(MYConnectionSettingsModel SettingsData)
        {
            Settings = SettingsData;
            Conn = new MySqlConnection(Settings.GetMYConnectionString());
        }
        #endregion

        #region Connection
        public static ReturnModel<bool> ServerResponse(MYConnectionSettingsModel SettingsData)
        {
            ReturnModel<bool> result = new ReturnModel<bool>();
            try
            {
                var Conn = new MySqlConnection(SettingsData.GetMYConnectionString());
                Conn.Open();
                Conn.Close();
                result.Success("Conexão estabelecida com sucesso.", true);
            }
            catch (MySqlException n)
            {
                result.Fail("Não foi possivel abrir a conexão com os parâmetros informados.", new ErrorModel(n.ErrorCode, n.Message, "NO INSTRUCTION", n.StackTrace));
            }
            catch (Exception e)
            {
                result.Error("Erro ao tentar abrir a conexão com os parâmetros informados.", new ErrorModel(0, e.Message, "NO INSTRUCTION", e.StackTrace));
            }
            return result;
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
            catch (MySqlException e)
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
        private MySqlTransaction? Transaction()
        {
            if (UseTransction)
            {
                return Conn.BeginTransaction();
            }
            return default(MySqlTransaction);
        }

        private MySqlTransaction? Transaction(MySqlConnection Connection)
        {
            if (UseTransction)
            {
                return Connection.BeginTransaction();
            }
            return default(MySqlTransaction);
        }

        private void Roolback(MySqlTransaction? Transaction)
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

        private void Commit(MySqlTransaction? Transaction)
        {
            if (UseTransction && Transaction != null)
            {
                Transaction.Commit();
            }
        }
        #endregion

        #region Query
        public ReturnModel<bool> Execute(string SQL)
        {
            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            ReturnModel<bool> result = new ReturnModel<bool>();
            MySqlTransaction ExecuteTransaction;

            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn);
                var affected = Command.ExecuteNonQuery();
                Commit(ExecuteTransaction);

                result.Success($"Instrução executada com sucesso. Foram alterada(s) {affected} linha(s).", true);
            }
            catch (MySqlException e)
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
            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            ReturnModel<bool> result = new ReturnModel<bool>();
            Connection.Conn.Open();
            MySqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {                
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn);
                var affected = await Command.ExecuteNonQueryAsync();
                Commit(ExecuteTransaction);
                result.Success($"Instrução executada com sucesso. Foram alterada(s) {affected} linha(s).", true);
            }
            catch (MySqlException e)
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
            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            ReturnModel<dynamic> result = new ReturnModel<dynamic>();
            Connection.Conn.Open();
            MySqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                // Realiza a instrução                
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                MySqlDataReader Reader = Command.ExecuteReader();
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
            catch (MySqlException e)
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
            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            ReturnModel<dynamic> result = new ReturnModel<dynamic>();
            Connection.Conn.Open();
            MySqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                // Realiza a instrução                
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                MySqlDataReader Reader = (MySqlDataReader)await Command.ExecuteReaderAsync();
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
            catch (MySqlException e)
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
            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ReturnModel<long> result = new ReturnModel<long>();
            MySqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                MySqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                long count = Table.Rows.Count;

                result.Success("Consulta realizada com sucesso.", count);
            }
            catch (MySqlException e)
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
            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ReturnModel<long> result = new ReturnModel<long>();
            MySqlTransaction ExecuteTransaction;
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                MySqlDataReader Reader = (MySqlDataReader)await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                long count = Table.Rows.Count;

                result.Success("Consulta realizada com sucesso.", count);
            }
            catch (MySqlException e)
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

        public MySqlDataReader Query(string SQL)
        {
            try
            {
                Conn.Open();
                MySqlCommand Command = new MySqlCommand(SQL, Conn);
                return Command.ExecuteReader();
            }
            catch (MySqlException e)
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

        public async Task<MySqlDataReader> QueryAsync(string SQL)
        {
            try
            {
                Conn.Open();
                MySqlCommand Command = new MySqlCommand(SQL, Conn);
                return (MySqlDataReader)await Command.ExecuteReaderAsync();
            }
            catch (MySqlException e)
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
            MySqlTransaction? ExecuteTransaction;
            Conn.Open();
            ExecuteTransaction = Transaction();
            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Conn);
                MySqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                result.Success("Realizado com sucesso.", Table);
                Commit(ExecuteTransaction);
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;
            Conn.Open();
            ExecuteTransaction = Transaction();
            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Conn);
                MySqlDataReader Reader = (MySqlDataReader)await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                result.Success("Realizado com sucesso.", Table);
                Commit(ExecuteTransaction);
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                MySqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    obj = EntityDataManipulate.ConvertDataRow<T>(Table);

                    result.Success("Consulta realizada com sucesso.", obj);
                }
                else
                {
                    result.Fail("Nenhum registro encontrada para consulta requisitada.");
                }
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);
            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn);
                Command.CommandTimeout = 0;
                MySqlDataReader Reader = (MySqlDataReader)await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    obj = EntityDataManipulate.ConvertDataRow<T>(Table);

                    result.Success("Consulta realizada com sucesso.", obj);
                }
                else
                {
                    result.Fail("Nenhum registro encontrada para consulta requisitada.");
                }
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);
            string SQL = string.Empty;
            try
            {
                SQL = Entity.Select();
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                MySqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = EntityDataManipulate.ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;
            string SQL = string.Empty;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                SQL = Entity.Select(Entity.FilterByKey(KeyFilter.ToString()));
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                MySqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = EntityDataManipulate.ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;
            string SQL = string.Empty;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                SQL = Entity.Select(FilterByFields, ConditionalAnd);
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                MySqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = EntityDataManipulate.ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                MySqlDataReader Reader = Command.ExecuteReader();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = EntityDataManipulate.ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                MySqlDataReader Reader = (MySqlDataReader)await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Tabela);
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = EntityDataManipulate.ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;
            string SQL = string.Empty;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                SQL = Entity.Select(Entity.FilterByKey(KeyFilter.ToString()));
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                MySqlDataReader Reader = (MySqlDataReader)await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = EntityDataManipulate.ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (MySqlException e)
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
            MySqlTransaction ExecuteTransaction;
            string SQL = string.Empty;

            MySQLConnectionInfra Connection = new MySQLConnectionInfra(Settings);
            Connection.Conn.Open();
            ExecuteTransaction = Transaction(Connection.Conn);

            try
            {
                SQL = Entity.Select(FilterByFields, ConditionalAnd);
                MySqlCommand Command = new MySqlCommand(SQL, Connection.Conn) { CommandTimeout = 0 };
                MySqlDataReader Reader = (MySqlDataReader)await Command.ExecuteReaderAsync();
                DataTable Table = new DataTable(Entity.GetTable());
                Table.Load(Reader);
                Commit(ExecuteTransaction);

                if (Table.Rows.Count > 0)
                {
                    Lista = EntityDataManipulate.ConvertDataTable<T>(Table);
                }
                else
                {
                    Lista = new List<T>();
                }
                result.Success("Consulta realizada com sucesso.", Lista);
            }
            catch (MySqlException e)
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

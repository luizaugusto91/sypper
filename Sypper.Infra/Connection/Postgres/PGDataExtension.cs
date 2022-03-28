using Npgsql;
using Sypper.Domain.Application.Interfaces;
using Sypper.Domain.Generico.Processing;

namespace Sypper.Infra.Connection.Postgres
{
    public class PGDataExtension : IDataConnectionExtension
    {
        #region Connection
        public ReturnModel<bool> ServerResponse(IDataSettings SettingsData)
        {
            ReturnModel<bool> result = new ReturnModel<bool>();
            try
            {
                var Conn = new NpgsqlConnection(SettingsData.GetConnectionString());
                Conn.Open();
                Conn.Close();
                result.Success("Conexão estabelecida com sucesso.", true);
            }
            catch (NpgsqlException n)
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
            return Conn.State == ConnectionState.Open;
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
    }
}

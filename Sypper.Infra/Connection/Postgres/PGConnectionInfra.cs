using Npgsql;
using Sypper.Domain.Application.Interfaces;
using Sypper.Domain.Generico.Processing;
using System.Data;
using System.Data.Common;

namespace Sypper.Infra.Connection.Postgres
{
    public class PGConnectionInfra : IDataConnection, IDataTransaction<NpgsqlTransaction>, IDataProcess
    {
        #region Properties
        private IDataSettings Settings { get; set; }
        private NpgsqlConnection Conn {get; set;}
        private bool UseTransction { get; set; } = true;
        #endregion

        

        #region Transaction
        public NpgsqlTransaction? Transaction()
        {
            if (UseTransction)
            {
                return Conn.BeginTransaction();
            }
            return default(NpgsqlTransaction);
        }

        public NpgsqlTransaction? Transaction(DbConnection Connection)
        {
            if (UseTransction)
            {
                return (NpgsqlTransaction?)Connection.BeginTransaction();
            }
            return default(NpgsqlTransaction);
        }

        public void Roolback(NpgsqlTransaction? Transaction)
        {
            if (UseTransction && Transaction != null)
            {
                try
                {
                    Transaction.Rollback();
                }
                catch (NpgsqlException e)
                {
                    Console.WriteLine($"Transaction.Rollback >> {e.Message}");
                }

            }
        }

        public void Commit(NpgsqlTransaction? Transaction)
        {
            if (UseTransction && Transaction != null)
            {
                Transaction.Commit();
            }
        }
        #endregion
    }
}

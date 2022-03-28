using System.Data.Common;

namespace Sypper.Domain.Application.Interfaces
{
    public interface IDataTransaction<T> where T : DbTransaction
    {
        #region Transaction
        public T? Transaction();

        public T? Transaction(DbConnection Connection);

        public void Roolback(T? Transaction);

        public void Commit(T? Transaction);
        #endregion
    }
}

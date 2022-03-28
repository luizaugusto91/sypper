using Sypper.Domain.Generico.Processing;
using System.Data.Common;

namespace Sypper.Domain.Application.Interfaces
{
    public interface IDataConnectionExtension
    {
        #region Connection
        public abstract ReturnModel<bool> ServerResponse(IDataSettings SettingsData);

        public abstract bool Opened(this DbConnection Conn);

        public abstract void Close(this DbConnection Conn);
        #endregion
    }
}

namespace Sypper.Domain.Application.Processing
{
    public class PGModel
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
    }
}

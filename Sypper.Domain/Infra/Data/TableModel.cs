using Sypper.Domain.Application.Processing;
using Sypper.Domain.Infra.Attributes;

namespace Sypper.Domain.Infra.Data
{
    public class TableModel : SQLModel
    {
        #region Propriedades
        [Core(PrimaryKey = true, Restrict = true)]
        public Int64 codigo { get; set; }

        [Core(Restrict = true)]
        public DateTime created { get; set; }

        [Core(Restrict = true)]
        public DateTime updated { get; set; }
        #endregion

        #region Construtores
        public TableModel()
        {
            codigo = -1;
            created = DateTime.Now;
            updated = DateTime.Now;
        }
        #endregion            
    }
}

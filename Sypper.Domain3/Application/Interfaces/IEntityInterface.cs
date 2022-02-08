using Sypper.Domain.Generico.Processing;
using System.Collections.Generic;

namespace Sypper.Domain.Application.Interfaces
{
    /*
    EntityInterface<T>
        Uso:    > Utilizar em classes de Infra que possuam conexão com entidades relacionais do PostgreSQL
                > Para toda EntityInterface é necessário um Model derivado da entidade que se deseja manipular <T>.
     */

    public interface IEntityInterface<T>
    {        
        public ReturnModel<List<T>> ReadList();

        public ReturnModel<List<T>> ReadList(int page);

        public ReturnModel<T> Read(long id);

        public ReturnModel<T> Create(T Data);

        public ReturnModel<bool> Update(T Data);

        public ReturnModel<bool> Delete(T Data);
    }
}

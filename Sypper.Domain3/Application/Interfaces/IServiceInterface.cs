using Sypper.Domain.Generico.Processing;
using System.Collections.Generic;

namespace Sypper.Domain.Application.Interfaces
{
    public interface IServiceInterface<T>
    {
        public ReturnModel<List<T>> ReadList();

        public ReturnModel<List<T>> ReadList(int page);

        public ReturnModel<T> Read(long id);

        public ReturnModel<T> Create(T Data);

        public ReturnModel<bool> Update(T Data);

        public ReturnModel<bool> Delete(T Data);
    }
}

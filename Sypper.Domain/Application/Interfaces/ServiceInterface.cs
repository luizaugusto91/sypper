using Sypper.Domain.Generico.Processing;

namespace Sypper.Domain.Application.Interfaces
{
    public interface ServiceInterface<T>
    {
        public ReturnModel<List<T>> ReadList();

        public ReturnModel<List<T>> ReadList(int page);

        public ReturnModel<T> Read(long id);

        public ReturnModel<T> Create(T Data);

        public ReturnModel<bool> Update(T Data);

        public ReturnModel<bool> Delete(T Data);
    }
}

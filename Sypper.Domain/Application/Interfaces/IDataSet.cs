using Sypper.Domain.Generico.Processing;
using Sypper.Domain.Infra.Data;

namespace Sypper.Domain.Application.Interfaces
{
    public interface IDataSet<T>
    {
        /// <summary>
        /// Obtem uma lista de registro T
        /// </summary>
        /// <typeparam name="T">Tipo do registro que deseja ser recuperado</typeparam>        
        /// <returns>Retorna um ReturnModel, onde o atributo RETORNO é uma Lista de registro do tipo T.</returns>
        /// <example>List<T> Lista = service.ReadList();</example>
        public abstract ReturnModel<List<T>> ReadList(T TableEntity);
        /// <summary>
        /// Obtem uma lista de registro T Assincrona
        /// </summary>
        /// <typeparam name="T">Tipo do registro que deseja ser recuperado</typeparam>        
        /// <returns>Retorna um ReturnModel, onde o atributo RETORNO é uma Lista de registro do tipo T.</returns>
        /// <example>List<T> Lista = service.ReadList();</example>
        public abstract Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity);

        /// <summary>
        /// Obtem uma lista de registro T
        /// </summary>
        /// <typeparam name="T">Tipo do registro que deseja ser recuperado</typeparam>
        /// <param name="PrimaryKey">Chave primária do registro</param>
        /// <returns>Retorna um ReturnModel, onde o atributo RETORNO é uma Lista de registro do tipo T.</returns>
        /// <example>List<T> Lista = service.ReadList(1);</example>
        public abstract ReturnModel<List<T>> ReadList(T TableEntity, dynamic PrimaryKey);
        /// <summary>
        /// Obtem uma lista de registro T Assincrona
        /// </summary>
        /// <typeparam name="T">Tipo do registro que deseja ser recuperado</typeparam>
        /// <param name="PrimaryKey">Chave primária do registro</param>
        /// <returns>Retorna um ReturnModel, onde o atributo RETORNO é uma Lista de registro do tipo T.</returns>
        /// <example>List<T> Lista = service.ReadList(1);</example>
        public abstract Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, dynamic PrimaryKey);

        public ReturnModel<List<T>> ReadList(T TableEntity, List<FieldsFilterModel> Filters);        
        public Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, List<FieldsFilterModel> Filters);

        public ReturnModel<List<T>> ReadList(T TableEntity, List<FieldsFilterModel> Filters, bool ConditionalAnd = true);
        public Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, List<FieldsFilterModel> Filters, bool ConditionalAnd = true);

        public ReturnModel<List<T>> ReadList(T TableEntity, int page);
        public Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, int page);

        public ReturnModel<List<T>> ReadList(T TableEntity, dynamic PrimaryKey, int page);
        public Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, dynamic PrimaryKey, int page);

        public ReturnModel<List<T>> ReadList(T TableEntity, List<FieldsFilterModel> Filters,int page);
        public Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, List<FieldsFilterModel> Filters,int page);

        public ReturnModel<T> Read(T TableEntity, long id);        
        public Task<ReturnModel<T>> ReadAsync(T TableEntity, long id);        

        public ReturnModel<T> Create(T Data);
        public Task<ReturnModel<T>> CreateAsync(T Data);

        public ReturnModel<bool> Update(T Data);
        public Task<ReturnModel<bool>> UpdateAsync(T Data);

        public ReturnModel<bool> Delete(T Data);
        public Task<ReturnModel<bool>> DeleteAsync(T Data);
    }
}

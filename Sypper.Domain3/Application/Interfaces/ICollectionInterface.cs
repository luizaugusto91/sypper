using Sypper.Domain.Generico.Processing;
using System.Collections.Generic;

namespace Sypper.Domain.Application.Interfaces
{
    /*
    ColletionInterface<T>
        Uso:    > Utilizar em classes de Infra que possuam conexão com collection do MongoDB
                > Para toda ColletionInterface é necessário um Model derivado da collection que se deseja manipular <T>.
    */


    /// <summary>
    /// Utilizar em classes de Infra que possuam conexão com collection do MongoDB
    /// </summary>
    /// <typeparam name="T">Deve receber uma classe herdade de NoSQLModel</typeparam>
    /// <example>string desc = myEnumVariable.GetAttributeOfType<DescriptionAttribute>().Description;</example>
    public interface ColletionInterface<T>
    {
        /// <summary>
        /// Retorna uma lista de <T>
        /// </summary>
        /// <typeparam name="T">Deve receber uma classe herdade de NoSQLModel</typeparam>        
        /// <returns>List<T></returns>
        /// <example>List<T> lista = object.ReadList();</example>
        public ReturnModel<List<T>> ReadList();

        /// <summary>
        /// Retorna uma lista de <T>
        /// </summary>
        /// <typeparam name="T">Deve receber uma classe herdade de NoSQLModel</typeparam>  
        /// <param name="page">Numero da pagina que deseja</param>
        /// <returns>List<T></returns>
        /// <example>List<T> lista = object.ReadList(1);</example>
        public ReturnModel<List<T>> ReadList(int page);

        /// <summary>
        /// Retorna um objeto do tipo T
        /// </summary>
        /// <typeparam name="T">Deve receber uma classe herdade de NoSQLModel</typeparam>  
        /// <param name="id">Numero do registro a ser resgatado</param>
        /// <returns>Retorna um objeto do tipo T</returns>
        /// <example>T rec = object.Read(1);</example>
        public ReturnModel<T> Read(long id);

        /// <summary>
        /// Grava um novo registro com base na informações presentes no objeto T
        /// </summary>
        /// <typeparam name="T">Deve receber uma classe herdade de NoSQLModel</typeparam>  
        /// <param name="Data">Objeto contendo a informações a serem gravadas</param>
        /// <returns>Retorno o objeto regristrado T</returns>
        /// <example>T rec = object.Create(Dados);</example>
        public ReturnModel<T> Create(T Data);        
    }
}

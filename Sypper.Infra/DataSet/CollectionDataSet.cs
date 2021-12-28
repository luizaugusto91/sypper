using Sypper.Domain.Application.Processing;
using Sypper.Domain.Generico.Processing;
using Sypper.Infra.Connection;
using MongoDB.Driver;

namespace Sypper.Infra.EntityDataSet
{
    public class CollectionDataSet<T> where T: NoSQLModel
    {
        public MongoDBConnectionInfra Conn;

        private IMongoCollection<T> BookEntity;
        public T Entity;

        public virtual ReturnModel<List<T>> ReadList()
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();            
            try
            {
                var Collection = Conn.GetCollection<T>(Entity.GetTable());
                if (Collection.Validate())
                {
                    BookEntity = Collection.retorno;
                    var Query = BookEntity.Find(book => true).ToList();
                    if (Query != null)
                    {
                        result.Success("Lista obtida com sucesso.", Query);
                    }
                    else
                    {
                        result.Fail("Falha ao obter a lista.");
                    }
                }
                else 
                {
                    result.Fail("Falha ao obter a collection.", Collection.erro);
                }
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao obter a lista.", erro);
            }
            return result;
        }

        public virtual ReturnModel<List<T>> ReadList(int page)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Collection = Conn.GetCollection<T>(Entity.GetTable());
                if (Collection.Validate())
                {
                    BookEntity = Collection.retorno;
                    var Query = BookEntity.Find(book => true).Skip((page - 1) * 100).Limit(100).ToList();
                    if (Query != null)
                    {
                        result.Success("Lista obtida com sucesso.", Query);
                    }
                    else
                    {
                        result.Fail("Falha ao obter a lista.");
                    }
                }
                else
                {
                    result.Fail("Falha ao obter a collection.", Collection.erro);
                }
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao obter a lista.", erro);
            }
            return result;
        }

        public virtual ReturnModel<T> Read(long id)
        {
            ReturnModel<T> result = new ReturnModel<T>();            
            try
            {                
                var Collection = Conn.GetCollection<T>(Entity.GetTable());
                if (Collection.Validate())
                {
                    BookEntity = Collection.retorno;
                    var Query = Conn.QueryTo<T>(Entity.MakeFilter(Entity.FilterByKey(id.ToString())), Entity.GetTable());
                    if (Query.Validate())
                    {
                        result.Success("Registro obtido com sucesso.", Query.retorno);
                    }
                    else
                    {
                        result.Fail("Falha ao obter o registro: " + Query.mensagem);
                    }
                }
                else 
                {
                    result.Fail("Falha ao obter a collection.", Collection.erro);
                }
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao obter o registro.", erro);
            }
            return result;
        }

        public virtual ReturnModel<T> Create(T Data)
        {
            ReturnModel<T> result = new ReturnModel<T>();
            try
            {
                var Collection = Conn.GetCollection<T>(Entity.GetTable());
                if (Collection.Validate())
                {
                    BookEntity = Collection.retorno;
                    BookEntity.InsertOne(Data);
                    result.Success("Registro cadastrado com sucesso.", Data);
                }
                else
                {
                    result.Fail("Falha ao obter a collection.", Collection.erro);
                }
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao inserir o registro.", erro);
            }
            return result;
        }

        public virtual ReturnModel<bool> Update(T Data)
        {
            ReturnModel<bool> result = new ReturnModel<bool>();
            try
            {
                /*var Collection = Conn.GetCollection<T>(Entity.GetTable());
                if (Collection.Validate())
                {
                    BookEntity = Collection.retorno;
                }

                var Query = Conn.Execute(Data.Update());
                if (Query.Validate())
                {
                    result.Success("Registro atualizado com sucesso.", Query.Validate());
                }
                else
                {
                    result.Fail("Falha ao atualizar o registro: " + Query.mensagem, false, Query.erro);
                }*/
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao atualizar o registro.", erro);
            }
            return result;
        }

        public virtual ReturnModel<bool> Delete(T Data)
        {
            ReturnModel<bool> result = new ReturnModel<bool>();
            try
            {
                /*var Query = Conn.Execute(Data.Delete());
                if (Query.Validate())
                {
                    result.Success("registro removido com sucesso.", Query.Validate());
                }
                else
                {
                    result.Fail("Falha ao remover o registro: " + Query.mensagem, false, Query.erro);
                }*/
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao remover o registro.", erro);
            }
            return result;
        }
    }
}

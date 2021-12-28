using Sypper.Domain.Application.Processing;
using Sypper.Domain.Generico.Processing;
using Sypper.Domain.Infra.Connection;
using Sypper.Infra.Connection;

namespace Sypper.Infra.EntityDataSet
{
    public class EntityDataSet<T> where T : SQLModel
    {
        public PostgreSQLConnectionInfra Conn;

        public T TableEntity;

        public EntityDataSet(PGConnectionSettingsModel Settings, T TableEntity)
        {
            Conn = new PostgreSQLConnectionInfra(Settings);
            this.TableEntity = TableEntity;
        }   
        
        public virtual ReturnModel<List<T>> ReadList()
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();            
            try
            {
                var Query = Conn.QueryToList<T>(TableEntity.Select(), TableEntity.GetTable());
                if (Query.Validate())
                {
                    result.Success("Lista obtida com sucesso.", Query.retorno);
                }
                else
                {
                    result.Fail("Falha ao obter a lista: " + Query.mensagem);
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
                var Query = Conn.QueryToList<T>(TableEntity.Select(100, page), TableEntity.GetTable());
                if (Query.Validate())
                {
                    result.Success("Lista obtida com sucesso.", Query.retorno);
                }
                else
                {
                    result.Fail("Falha ao obter a lista: " + Query.mensagem);
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
                var Query = Conn.QueryTo<T>(TableEntity.Select(TableEntity.FilterByKey(id.ToString())), TableEntity.GetTable());
                if (Query.Validate())
                {
                    result.Success("Registro obtido com sucesso.", Query.retorno);
                }
                else
                {
                    result.Fail("Falha ao obter o registro: " + Query.mensagem);
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
                var Query = Conn.ExecuteReturning(Data.Insert());
                if (Query.Validate())
                {
                    var user = Read(Query.retorno);
                    if (user.Validate())
                    {
                        result.Success("Registro cadastrado com sucesso.", user.retorno);
                    }
                    else
                    {
                        result.Fail($"Falha ao recuperar o registro inserido. Detalhes: {user.mensagem}", null, user.erro);
                    }
                }
                else
                {
                    result.Fail("Falha ao inserir o registro: " + Query.mensagem, null, Query.erro);
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
                var Query = Conn.Execute(Data.Update());
                if (Query.Validate())
                {
                    result.Success("Registro atualizado com sucesso.", Query.Validate());
                }
                else
                {
                    result.Fail("Falha ao atualizar o registro: " + Query.mensagem, false, Query.erro);
                }
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
                var Query = Conn.Execute(Data.Delete());
                if (Query.Validate())
                {
                    result.Success("registro removido com sucesso.", Query.Validate());
                }
                else
                {
                    result.Fail("Falha ao remover o registro: " + Query.mensagem, false, Query.erro);
                }
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

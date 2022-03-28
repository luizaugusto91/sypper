using Sypper.Domain.Application.Extension;
using Sypper.Domain.Application.Interfaces;
using Sypper.Domain.Application.Processing;
using Sypper.Domain.Generico.Processing;
using Sypper.Domain.Infra.Connection;
using Sypper.Domain.Infra.Data;
using Sypper.Infra.Connection;

namespace Sypper.Infra.EntityDataSet
{
    public class EntityDataSet<T>: IDataSet<T> where T : SQLModel
    {
        public PostgreSQLConnectionInfra Conn;

        public EntityDataSet(PGConnectionSettingsModel Settings)
        {
            Conn = new PostgreSQLConnectionInfra(Settings);            
        }

        public ReturnModel<T> Create(T Data)
        {
            ReturnModel<T> result = new ReturnModel<T>();
            try
            {
                var Query = Conn.ExecuteReturning(Data.Insert());
                if (Query.Validate())
                {
                    var user = Read(Data, Query.retorno);
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
                    result.Fail("Falha ao inserir o registro: " + Query.mensagem, Query.erro);
                }
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao inserir o registro.", erro);
            }
            return result;
        }

        public async Task<ReturnModel<T>> CreateAsync(T Data)
        {
            ReturnModel<T> result = new ReturnModel<T>();
            try
            {
                var Query = await Conn.ExecuteReturningAsync(Data.Insert());
                if (Query.Validate())
                {
                    var user = ReadAsync(Data, Query.retorno);
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
                    result.Fail("Falha ao inserir o registro: " + Query.mensagem, Query.erro);
                }
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao inserir o registro.", erro);
            }
            return result;
        }

        public ReturnModel<bool> Delete(T Data)
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

        public async Task<ReturnModel<bool>> DeleteAsync(T Data)
        {
            ReturnModel<bool> result = new ReturnModel<bool>();
            try
            {
                var Query = await Conn.ExecuteAsync(Data.Delete());
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

        public ReturnModel<T> Read(T TableEntity, long id)
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

        public async Task<ReturnModel<T>> ReadAsync(T TableEntity, long id)
        {
            ReturnModel<T> result = new ReturnModel<T>();
            try
            {
                var Query = await Conn.QueryToAsync<T>(TableEntity.Select(TableEntity.FilterByKey(id.ToString())), TableEntity.GetTable());
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

        public ReturnModel<List<T>> ReadList(T TableEntity)
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

        public ReturnModel<List<T>> ReadList(T TableEntity, dynamic PrimaryKey)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = Conn.QueryToList<T>(TableEntity.Select(TableEntity.FilterByKey(PrimaryKey.ToString())), TableEntity.GetTable());
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

        public ReturnModel<List<T>> ReadList(T TableEntity, List<FieldsFilterModel> Filters)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = Conn.QueryToList<T>(TableEntity.Select(Filters), TableEntity.GetTable());
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

        public ReturnModel<List<T>> ReadList(T TableEntity, List<FieldsFilterModel> Filters, bool ConditionalAnd = true)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {                
                var Query = Conn.QueryToList<T>(TableEntity, Filters, ConditionalAnd);
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

        public ReturnModel<List<T>> ReadList(T TableEntity, int page)
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

        public ReturnModel<List<T>> ReadList(T TableEntity, dynamic PrimaryKey, int page)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                //var Query = Conn.QueryToList<T>(TableEntity.Select(TableEntity.FilterByKey(PrimaryKey), 100, page), TableEntity.GetTable());
                var Query = Conn.QueryToList<T>(TableEntity.Select(PrimaryKey, 100, page), TableEntity.GetTable());
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

        public ReturnModel<List<T>> ReadList(T TableEntity, List<FieldsFilterModel> Filters, int page)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = Conn.QueryToList<T>(TableEntity.Select(Filters, 100, page), TableEntity.GetTable());
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

        public async Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = await Conn.QueryToListAsync<T>(TableEntity.Select(), TableEntity.GetTable());
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

        public async Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, dynamic PrimaryKey)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = await Conn.QueryToListAsync<T>(TableEntity.Select(TableEntity.FilterByKey(PrimaryKey)), TableEntity.GetTable());
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

        public async Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, List<FieldsFilterModel> Filters)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = await Conn.QueryToListAsync<T>(TableEntity, Filters);
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

        public async Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, int page)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = await Conn.QueryToListAsync<T>(TableEntity.Select(100, page), TableEntity.GetTable());
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

        public async Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, dynamic PrimaryKey, int page)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = await Conn.QueryToListAsync<T>(TableEntity.Select(TableEntity.FilterByKey(PrimaryKey), 100, page), TableEntity.GetTable());
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

        public async Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, List<FieldsFilterModel> Filters, int page)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = await Conn.QueryToListAsync<T>(TableEntity.Select(Filters, 100, page), TableEntity.GetTable());
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

        public async Task<ReturnModel<List<T>>> ReadListAsync(T TableEntity, List<FieldsFilterModel> Filters, bool ConditionalAnd = true)
        {
            ReturnModel<List<T>> result = new ReturnModel<List<T>>();
            try
            {
                var Query = await Conn.QueryToListAsync<T>(TableEntity, Filters, ConditionalAnd);
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

        public ReturnModel<bool> Update(T Data)
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

        public async Task<ReturnModel<bool>> UpdateAsync(T Data)
        {
            ReturnModel<bool> result = new ReturnModel<bool>();
            try
            {
                var Query = await Conn.ExecuteAsync(Data.Update());
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

    }
}

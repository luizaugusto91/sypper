using Sypper.Domain.Generico.Processing;
using Sypper.Domain.Infra.Connection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Sypper.Infra.Connection
{    
    public class MongoDBConnectionInfra
    {
        private MGConnectionSettingsModel Settings;
        private MongoClient Conn;
        private IMongoDatabase DB;        

        public MongoDBConnectionInfra(string Server, int Port, string Username, string Password, string Database, int Timeout = 0)
        {
            Settings = new MGConnectionSettingsModel()
            {
                host = Server,
                port = Port,
                username = Username,
                password = Password,
                database = Database,
                timeout = Timeout
            };

            Conn = new MongoClient(Settings.GetMGConnectionString());
            DB = Conn.GetDatabase(Settings.GetDataBase());
        }

        public MongoDBConnectionInfra(MGConnectionSettingsModel SettingsData)
        {
            Settings = SettingsData;
            Conn = new MongoClient(Settings.GetMGConnectionString());
            DB = Conn.GetDatabase(Settings.GetDataBase());
        }

        public bool Connected()
        {            
            return DB.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);            
        }

        public ReturnModel<IMongoCollection<T>> GetCollection<T>(string CollectionName) 
        {
            ReturnModel<IMongoCollection<T>> result = new ReturnModel<IMongoCollection<T>>();
            try
            {
                IMongoCollection<T> Collection = DB.GetCollection<T>(CollectionName);
                result.Success("Collection encontrada.", Collection);
            }
            catch (MongoException e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.ErrorLabels.ToJson(), stack = e.StackTrace };
                result.Error("Erro ao buscar a collection.", erro);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = e.Source, stack = e.StackTrace };
                result.Error("Erro ao buscar a collection.", erro);
            }
            return result;
        }

        public ReturnModel<T> QueryTo<T>(string Regex, string Collection)
        {
            MongoDBConnectionInfra Connection = new MongoDBConnectionInfra(Settings);
            IMongoCollection<T> BookEntity = default;
            ReturnModel<T> Processo = new ReturnModel<T>();
            try
            {
                var Book = Connection.GetCollection<T>(Collection);
                if (Book.Validate())
                {
                    BookEntity = Book.retorno;
                    var Query = BookEntity.Find(Regex).ToList().First();
                    if (Query != null)
                    {
                        Processo.Success("Consulta realizada com sucesso.", Query);
                    }
                    else
                    {
                        Processo.Fail("Falha ao realizar a consulta.", null);
                    }
                }
                else 
                {
                    Processo.Fail("Falha ao realizar a consulta da collection. ", Book.erro);
                }
            }
            catch (MongoException e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 1, mensagem = e.Message, detalhes = Regex, stack = e.StackTrace };
                Processo.Fail("Falha ao executar consulta.", erro);
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = Regex, stack = e.StackTrace };
                Processo.Error("Erro ao executar consulta.", erro);
            }
            return Processo;
        }

        public ReturnModel<List<T>> QueryToList<T>(string Regex, string Collection)
        {            
            MongoDBConnectionInfra Connection = new MongoDBConnectionInfra(Settings);
            IMongoCollection<T> BookEntity;
            ReturnModel<List<T>> Processo = new ReturnModel<List<T>>();
            try
            {
                var Book = Connection.GetCollection<T>(Collection);
                if (Book.Validate())
                {
                    BookEntity = Book.retorno;
                    var Query = BookEntity.Find(Regex).ToList();
                    if (Query != null)
                    {
                        Processo.Success("Consulta realizada com sucesso.", Query);
                    }
                    else
                    {
                        Processo.Fail("Falha ao realizar a consulta.", null);
                    }
                }
                else 
                {
                    Processo.Fail("Falha ao realizar a consulta da collection. ", Book.erro);
                }                
            }
            catch (MongoException e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 1, mensagem = e.Message, detalhes = Regex, stack = e.StackTrace };
                Processo.Fail("Falha ao executar consulta.", erro);                
            }
            catch (Exception e)
            {
                ErrorModel erro = new ErrorModel() { codigo = 0, mensagem = e.Message, detalhes = Regex, stack = e.StackTrace };
                Processo.Error("Erro ao executar consulta.", erro);                
            }            
            return Processo;
        }
    }
}

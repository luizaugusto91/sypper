namespace Sypper.Domain.Infra.Connection
{
    public class MGConnectionSettingsModel
    {
        public string host { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string database { get; set; }
        public int timeout { get; set; }

        public MGConnectionSettingsModel()
        {
            timeout = 0;
        }

        public MGConnectionSettingsModel(string Host, int Port, string Username, string Password, string Database, int Timeout = 0)
        {
            host = Host;
            port = Port;
            username = Username;
            password = Password;
            database = Database;
            timeout = Timeout;
        }

        public string GetMGConnectionString()
        {
            if (username != "") 
            {
                return $"mongodb://{username}:{password}@{host}:{port}";
            }
            else 
            {
                return $"mongodb://{host}:{port}";
            }            
        }

        public string GetDataBase()
        {
            return database;
        }
    }
}

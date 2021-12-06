namespace Sypper.Domain.Infra.Connection
{
    public class PGConnectionSettingsModel
    {
        public string host { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string database { get; set; }
        public int timeout { get; set; }

        public PGConnectionSettingsModel()
        {
            timeout = 0;
        }

        public PGConnectionSettingsModel(string Host, int Port, string Username, string Password, string Database, int Timeout = 0)
        {
            host = Host;
            port = Port;
            username = Username;
            password = Password;
            database = Database;
            timeout = Timeout;
        }

        public string GetPGConnectionString()
        {
            return $"Server={host};Port={port};User ID={username};Password={password};Database={database};CommandTimeout={timeout}";
        }

        public static PGConnectionSettingsModel MakeConfig(string Setting)
        {
            try
            {
                PGConnectionSettingsModel NewSetting = new PGConnectionSettingsModel();
                string[] parts = Setting.Split(';');
                foreach (string part in parts)
                {
                    switch (part.ToLower())
                    {
                        case "server":
                            NewSetting.host = part.Split('=').Last();
                            break;
                        case "port":
                            NewSetting.port = Convert.ToInt32(part.Split('=').Last());
                            break;
                        case "user id":
                            NewSetting.username = part.Split('=').Last();
                            break;
                        case "password":
                            NewSetting.password = part.Split('=').Last();
                            break;
                        case "database":
                            NewSetting.database = part.Split('=').Last();
                            break;
                        case "commandtimeout":
                            NewSetting.timeout = Convert.ToInt32(part.Split('=').Last());
                            break;
                    }
                }
                return NewSetting;
            }
            catch
            {
                return null;
            }
        }
    }
}

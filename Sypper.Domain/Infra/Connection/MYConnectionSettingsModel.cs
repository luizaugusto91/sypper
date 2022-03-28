namespace Sypper.Domain.Infra.Connection
{
    public class MYConnectionSettingsModel
    {
        public string host { get; set; } = string.Empty;
        public int port { get; set; } = 3306;
        public string username { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string database { get; set; } = string.Empty;

        public MYConnectionSettingsModel() {}

        public MYConnectionSettingsModel(string Host, int Port, string Username, string Password, string Database)
        {
            host = Host;
            port = Port;
            username = Username;
            password = Password;
            database = Database;            
        }

        public string GetMYConnectionString()
        {
            return $"Server={host}:{port};Uid={username};Pwd={password};Database={database}";
        }

        public static MYConnectionSettingsModel MakeConfig(string Setting)
        {
            try
            {
                MYConnectionSettingsModel NewSetting = new MYConnectionSettingsModel();
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
                        case "uid":
                            NewSetting.username = part.Split('=').Last();
                            break;
                        case "pwd":
                            NewSetting.password = part.Split('=').Last();
                            break;
                        case "database":
                            NewSetting.database = part.Split('=').Last();
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


namespace Sypper.Domain.Infra.Connection
{
    public class FTPSettingsModel
    {
        public string url { get; set; }
        public string usuario { get; set; }
        public string senha { get; set; }
        public int porta { get; set; }
        public string certificado { get; set; }
    }
}

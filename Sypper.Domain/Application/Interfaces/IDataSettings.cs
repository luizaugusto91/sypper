namespace Sypper.Domain.Application.Interfaces
{
    public interface IDataSettings
    {
        public string host { get; set; }
        public int port { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string database { get; set; }
        public int timeout { get; set; }

        public string GetConnectionString();
    }
}
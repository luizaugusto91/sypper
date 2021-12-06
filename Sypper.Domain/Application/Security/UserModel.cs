namespace Sypper.Domain.Application.Security
{
    public class UserModel
    {
        public long codigo { get; set; }
        public string user { get; set; }
        public string password { get; set; }
        public string name { get; set; }
        public string role { get; set; }
        public string group { get; set; }
        public bool persistent { get; set; }
        public string token { get; set; }
    }
}

namespace Sypper.Domain.Application.Security
{
    public class UserModel
    {
        public long codigo { get; set; } = -1;
        public string user { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string nick { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;
        public long groupid { get; set; } = -1;
        public string group { get; set; } = string.Empty;
        public bool persistent { get; set; } = false;
        public string token { get; set; } = string.Empty;
    }
}

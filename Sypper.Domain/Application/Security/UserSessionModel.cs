using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Sypper.Domain.Application.Security
{
    public class UserSessionModel : UserModel
    {
        private readonly IHttpContextAccessor _accessor;
        
        public UserSessionModel(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public IEnumerable<Claim> GetClaimsIdentity()
        {
            return _accessor.HttpContext.User.Claims;
        }
    }
}

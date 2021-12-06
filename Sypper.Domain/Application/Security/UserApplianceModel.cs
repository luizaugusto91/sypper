namespace Sypper.Domain.Application.Security
{
    public class UserApplianceModel : UserModel
    {
        public static UserApplianceModel UserToUserAppliance(long Codigo, UserModel User) 
        {
            UserApplianceModel user = new UserApplianceModel()
            {
                codigo = Codigo,
                name = User.name,
                user = User.user,
                password = User.password,
                persistent = User.persistent,
                role = User.role,
                token = User.token
            };
            return user;
        }
    }
}

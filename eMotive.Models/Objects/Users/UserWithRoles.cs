using ServiceStack.FluentValidation.Attributes;
using eMotive.Models.Validation.User;

namespace eMotive.Models.Objects.Users
{
    [Validator(typeof(UserWithRolesValidator))]
    public class UserWithRoles : User
    {
       // public string[] SelectedRoles { get; set; }
        public string SelectedRole { get; set; }
    }
}

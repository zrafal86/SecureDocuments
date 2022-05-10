#nullable enable

using SecureDocuments.Models;
using Splat;

namespace SecureDocuments.Services
{
    public interface IRoleAccessService
    {
        bool CheckAccess(Role role);
    }

    public sealed class RoleAccessService : IRoleAccessService
    {
        private readonly User? _user;

        public RoleAccessService()
        {
            _user = Locator.Current.GetService<IUserSettings>().User;
            if (_user != null)
            {
                var userRoleValue = (int)_user.Role;
                var values = Enum.GetValues(typeof(Role));
                var listOfAccess = new List<int>();
                foreach (var item in values)
                {
                    var currentRoleValue = (int)item;
                    if (currentRoleValue >= userRoleValue) listOfAccess.Add(currentRoleValue);
                }

                AccessList = listOfAccess.ToImmutableArray();
            }
        }

        private ImmutableArray<int> AccessList { get; }

        public bool CheckAccess(Role role)
        {
            if (AccessList != null)
                return AccessList.Contains((int)role);
            return false;
        }
    }
}
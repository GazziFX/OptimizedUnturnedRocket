using Rocket.API.Serialisation;
using System.Collections.Generic;

namespace Rocket.API
{
    public enum RocketPermissionsProviderResult { Success, UnspecifiedError, DuplicateEntry, GroupNotFound,PlayerNotFound };

    public static class IRocketPermissionsProviderExtensions
    {
        public static bool HasPermission(this IRocketPermissionsProvider rocketPermissionProvider,IRocketPlayer player, string permission)
        {
            return rocketPermissionProvider.HasPermission(player, new List<string>() { permission });
        }

        public static bool HasPermission(this IRocketPermissionsProvider rocketPermissionProvider, IRocketPlayer player, IRocketCommand command)
        {
            /*List<string> commandPermissions = command.Permissions;
            commandPermissions.Add(command.Name);
            commandPermissions.AddRange(command.Aliases);
            commandPermissions = commandPermissions.Select(a => a.ToLower()).ToList();
            return rocketPermissionProvider.HasPermission(player, commandPermissions);*/
            return rocketPermissionProvider.HasPermission(player, command.Permissions);
        }

        public static List<Permission> GetPermissions(this IRocketPermissionsProvider rocketPermissionProvider, IRocketPlayer player, string permission)
        {
            return rocketPermissionProvider.GetPermissions(player, new List<string>() { permission });
        }

        public static List<Permission> GetPermissions(this IRocketPermissionsProvider rocketPermissionProvider, IRocketPlayer player, IRocketCommand command)
        {
            /*List<string> commandPermissions = command.Permissions;
            commandPermissions.Add(command.Name);
            commandPermissions.AddRange(command.Aliases);
            commandPermissions = commandPermissions.Select(a => a.ToLower()).ToList();
            return rocketPermissionProvider.GetPermissions(player, commandPermissions);*/

            return rocketPermissionProvider.GetPermissions(player, command.Permissions);
        }
    }

    public interface IRocketPermissionsProvider
    {
        bool HasPermission(IRocketPlayer player, List<string> requestedPermissions);

        List<RocketPermissionsGroup> GetGroups(IRocketPlayer player, bool includeParentGroups);
        List<Permission> GetPermissions(IRocketPlayer player);
        List<Permission> GetPermissions(IRocketPlayer player, List<string> requestedPermissions);

        RocketPermissionsProviderResult AddPlayerToGroup(string groupId, IRocketPlayer player);
        RocketPermissionsProviderResult RemovePlayerFromGroup(string groupId, IRocketPlayer player);

        RocketPermissionsGroup GetGroup(string groupId);
        RocketPermissionsProviderResult AddGroup(RocketPermissionsGroup group);
        RocketPermissionsProviderResult SaveGroup(RocketPermissionsGroup group);
        RocketPermissionsProviderResult DeleteGroup(string groupId);

        void Reload();
    }
}
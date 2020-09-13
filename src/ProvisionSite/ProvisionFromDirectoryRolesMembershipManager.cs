using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


/// <summary>
/// Allows us to sort all users being provisioned into role
/// </summary>
internal partial class ProvisionFromDirectoryRolesMembershipManager
{
    private Dictionary<string, SingleRoleManager> _rolesManager = new Dictionary<string, SingleRoleManager>();
    private Dictionary<string, ProvisioningUser> _allUsersSet = new Dictionary<string, ProvisioningUser>();

    /// <summary>
    /// Constructor
    /// </summary>
    public ProvisionFromDirectoryRolesMembershipManager()
    {

    }


    /// <summary>
    /// Adds a user, and replaces any matching existing user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public void AddAndForceReplaceUser(ProvisioningUser user)
    {
        RemoveUser(user.UserName);
        AddUser(user);
    }

    private string CannonicalKey(string key)
    {
        return key.ToLower();
    }

    /// <summary>
    /// Remove an existing user
    /// </summary>
    /// <param name="userEmail"></param>
    /// <returns></returns>
    public bool RemoveUser(string userEmail)
    {
        string cannonicalKey = CannonicalKey(userEmail);

        //See if the user is already in our list
        ProvisioningUser existingUser = null;
        if (!_allUsersSet.TryGetValue(cannonicalKey, out existingUser))
        {
            return false;
        }


        //=======================================================================
        //We need to remove the old user
        //=======================================================================
        bool wasRemoved;
        //Remove
        wasRemoved = _allUsersSet.Remove(cannonicalKey);
        IwsDiagnostics.Assert(wasRemoved, "814-200: Internal error, item not found");
        //Remove
        wasRemoved = GetManagerForRole(existingUser.UserRole).RemoveUser(cannonicalKey);
        IwsDiagnostics.Assert(wasRemoved, "814-200: Internal error, item not found");

        return wasRemoved;
    }




    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="userToAdd"></param>
    public void AddUser(ProvisioningUser userToAdd)
    {
        string cannonicalKey = CannonicalKey(userToAdd.UserName);

        //====================================================================================
        //See if the user is already in our list
        //====================================================================================
        ProvisioningUser existingUser = null;
        if (!_allUsersSet.TryGetValue(cannonicalKey, out existingUser))
        {
            //The user is new to us, add them.
            GetManagerForRole(userToAdd.UserRole).AddUser(userToAdd);
            _allUsersSet.Add(cannonicalKey, userToAdd);
            return;
        }


        //========================================================================================
        //If the user has the same rank as the existing user, there is nothing we need to do
        //========================================================================================
        if (userToAdd.RoleRank == existingUser.RoleRank)
        {
            return;
        }

        //========================================================================================
        //If the user being added is a Administrator + Creator, the it outranks other roles
        //========================================================================================
        if (userToAdd.RoleRank == ProvisioningUser.RoleRank_SiteAdministratorCreator)
        {
            //The user being added is a Creator + Administrator. This trumps other role.
            goto replace_existing_user;
        }

        //========================================================================================
        //Two special cases: 
        //  1.The user was previously added as a CREATOR, and is being added as an EXPLORER-ADMIN
        //  2. The user was previously added as an EXPLORER-ADMIN, and is being added as a CREATOR
        //
        //In both of these cases, we want to combine the "CREATOR" and "ADMIN" roles, so that the user does not get a diminished rol
        //========================================================================================
        if (ProvisioningUser.IsCombinedUserRoleCreatorAdministrator(existingUser.RoleRank,  userToAdd.RoleRank))
        {         
            userToAdd = new ProvisioningUser(
                userToAdd.UserName, //Keep the name
                ProvisioningUser.RoleText_SiteAdministratorCreator, //Upgrade the role to Creator + Administrator 
                userToAdd.UserAuthentication, "*multiple groups (" + existingUser.SourceGroup + "+" + userToAdd.SourceGroup+")*",
                (existingUser.AllowPromotedRole || userToAdd.AllowPromotedRole) //If either user definition allows role promotion, then allow it
                ); //Note that this is because of multiple groups

            goto replace_existing_user;

        }

        //If the existing user has a role rank equal or greater than the new entry, there is nothing we need to do
        if (existingUser.RoleRank >= userToAdd.RoleRank)
        {
            return;
        }

replace_existing_user:

        //=======================================================================
        //The new instance of this user has a higher rank than the old instance. 
        //We need to remove the old user, and add the new user
        //=======================================================================
        bool wasRemoved;
        //Remove
        wasRemoved = _allUsersSet.Remove(cannonicalKey);
        IwsDiagnostics.Assert(wasRemoved, "813-641: Internal error, item not found");
        //Remove
        wasRemoved = GetManagerForRole(existingUser.UserRole).RemoveUser(cannonicalKey);
        IwsDiagnostics.Assert(wasRemoved, "813-642: Internal error, item not found");

        //Add
        _allUsersSet.Add(cannonicalKey, userToAdd);
        GetManagerForRole(userToAdd.UserRole).AddUser(userToAdd);
    }


    /// <summary>
    /// If a role manager does not yet exist, then it creates one
    /// </summary>
    /// <param name="role"></param>
    public void EnsureRoleManagerExistsForRole(string role)
    {
        GetManagerForRole(role);
    }

    /// <summary>
    /// Return the role manager for the role
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    private SingleRoleManager GetManagerForRole(string role)
    {
        var cannonicalRole = role.ToLower();
        SingleRoleManager thisRoleManager = null;

        //Add the user to a role manager
        _rolesManager.TryGetValue(cannonicalRole, out thisRoleManager);
        if (thisRoleManager == null)
        {
            thisRoleManager = new SingleRoleManager(role);
            _rolesManager.Add(cannonicalRole, thisRoleManager);
        }

        return thisRoleManager;
    }

    /// <summary>
    /// Users as XML
    /// </summary>
    /// <param name="xmlWriter"></param>
    internal void WriteUserRolesAsXml(XmlWriter xmlWriter)
    {
        //Get these role by role so we serialize them out that way
        foreach (var thisRoleManager in _rolesManager.Values)
        {
            //For all the users mapped to this role, write them out
            foreach (var thisUserInfo in thisRoleManager.GetUsers())
            {
                thisUserInfo.WriteAsXml(xmlWriter);
            }
        }
    }
}

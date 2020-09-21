using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provision a site
/// </summary>
internal partial class ProvisionSite
{
    /// <summary>
    /// A working list of site users we can add/remove from
    /// </summary>
    private class WorkingListSiteUsers
    {
        readonly List<SiteUser> _usersList;
        /// <summary>
        /// Constructor. Copy in the list
        /// </summary>
        /// <param name="users"></param>
        public WorkingListSiteUsers(IEnumerable<SiteUser> users)
        {
            _usersList = new List<SiteUser>(users);
        }

        /// <summary>
        /// Adds a user to the list, if an existing user does not already exist
        /// </summary>
        /// <param name="userToAdd"></param>
        public void AddUser(SiteUser userToAdd)
        {
            if(userToAdd == null)
            {
                IwsDiagnostics.Assert(false, "920-740: Null user being added to list");
                throw new ArgumentException("920-740: Null user being added to list");
            }

            var matchingExistingUser = FindUserByName(userToAdd.Name);
            if (matchingExistingUser != null)
            {
                IwsDiagnostics.Assert(false, "920-731: User already exists, " + userToAdd.Name);
                throw new Exception("920-731: User already exists, " + userToAdd.Name);
            }
            _usersList.Add(userToAdd);
        }

        /// <summary>
        /// Return the list of users
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<SiteUser> GetUsers()
        {
            return _usersList.AsReadOnly();
        }


        /// <summary>
        /// Looks for a user with the matching name
        /// </summary>
        /// <param name="findName"></param>
        /// <returns></returns>
        internal SiteUser FindUserByName(string findName)
        {
            foreach(var thisUser in _usersList)
            {
                if(string.Compare(thisUser.Name, findName, true) == 0)
                {
                    return thisUser;
                }
            }

            return null; //Not found
        }

        /// <summary>
        /// Removes an existing user from the list
        /// </summary>
        /// <param name="foundExistingUser"></param>
        /// <returns>TRUE if the object being removed existed in the list</returns>
        internal bool RemoveUser(SiteUser foundExistingUser)
        {
            return _usersList.Remove(foundExistingUser);
        }

        /// <summary>
        /// Remove a user by name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        internal bool RemoveUser(string userName)
        {
            var foundUser = FindUserByName(userName);
            if (foundUser == null) return false;

            return _usersList.Remove(foundUser);
        }

        /// <summary>
        /// Replace a user if it exists
        /// </summary>
        /// <param name="userAdd"></param>
        internal void ReplaceUser(SiteUser userAdd)
        {
            RemoveUser(userAdd.Name);
            AddUser(userAdd);
        }
    }
}

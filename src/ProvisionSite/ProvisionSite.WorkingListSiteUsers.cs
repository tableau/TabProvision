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
        internal SiteUser FindUser(string findName)
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
            var foundUser = FindUser(userName);
            if (foundUser == null) return false;

            return _usersList.Remove(foundUser);
        }

    }
}

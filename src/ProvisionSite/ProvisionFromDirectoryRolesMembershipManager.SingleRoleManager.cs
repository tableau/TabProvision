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
    /// <summary>
    /// 
    /// </summary>
    internal partial class SingleRoleManager
    {
        public readonly string Role;

        private Dictionary<string, ProvisioningUser> _usersSet = new Dictionary<string, ProvisioningUser>();


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="role"></param>
        public SingleRoleManager(string role)
        {
            this.Role = role;
        }

        /// <summary>
        /// Gets the users
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, ProvisioningUser>.ValueCollection GetUsers()
        {
            return _usersSet.Values;
        }

        private string CannonicalKey(string key)
        {
            return key.ToLower();
        }

        /// <summary>
        /// Remove the user
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool RemoveUser(string key)
        {
            return _usersSet.Remove(key);
        }

        /// <summary>
        /// Add a user to the set
        /// </summary>
        /// <param name="user"></param>
        internal void AddUser(ProvisioningUser user)
        {
            string cannonicalKey = CannonicalKey(user.UserName);

            if (_usersSet.ContainsKey(cannonicalKey))
            {
                //User is already there.  Don't need to do anything
            }
            else
            {
                _usersSet.Add(cannonicalKey, user);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Xml;


/// <summary>
/// Allows us to sort all users being provisioned into groups
/// </summary>
internal partial class ProvisionFromDirectoryGroupsMembershipManager
{

    /// <summary>
    /// Manages the list of users in a single group
    /// </summary>
    internal partial class SingleGroupManager
    {
        /// <summary>
        /// Name of the group in the Tableau site
        /// </summary>
        public readonly string GroupName;

        /// <summary>
        /// Instructions for what to do about the Grant License mode of the Tableau site Group
        /// </summary>
        public readonly ProvisioningGroup.GrantLicenseMode GrantLicenseInstructions;

        /// <summary>
        /// If we are using GrantLicense then what is the role, e.g. "Creator", "Explorer", "Viewer", types of admins, ...
        /// </summary>
        public readonly string GrantLicenseRole;


        /// <summary>
        /// Lock to prevent reentrant access
        /// </summary>
        private object _threadLock_modifyUsersList = new object();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="group"></param>
public SingleGroupManager(string group, ProvisioningGroup.GrantLicenseMode grantLicenseMode, string grantLicenseRole)
        {
            this.GroupName = group;
            this.GrantLicenseInstructions = grantLicenseMode;
            this.GrantLicenseRole = grantLicenseRole;
        }

        /// <summary>
        /// Group members
        /// </summary>
        private Dictionary<string, string> _usersSet = new Dictionary<string, string>();

        public Dictionary<string, string>.ValueCollection GetUserNames()
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
            //Prevent multiple streams of acccess
            lock(_threadLock_modifyUsersList)
            {
                return _usersSet.Remove(CannonicalKey(key));
            }
        }

        /// <summary>
        /// Add a user to the set
        /// </summary>
        /// <param name="user"></param>
        internal void AddUser(string userName)
        {
            string cannonicalKey = CannonicalKey(userName);

            lock(_threadLock_modifyUsersList)
            {
                if (_usersSet.ContainsKey(cannonicalKey))
                {
                    //User is already there.  Don't need to do anything
                }
                else
                {
                    _usersSet.Add(cannonicalKey, userName);
                }
            }
        }

        /// <summary>
        /// Write the XML for the members
        /// </summary>
        /// <param name="xmlWriter"></param>
        internal void WriteGroupAsXml(XmlWriter xmlWriter)
        {
            
            xmlWriter.WriteStartElement("GroupMembership");
            xmlWriter.WriteAttributeString("name", this.GroupName);

            //Group instructions for Grant License
            xmlWriter.WriteAttributeString(
                ProvisioningGroup.XmlAttribute_GrantLicenseMode,
                ProvisioningGroup.GenerateGrantLicenseModeAttributeText(this.GrantLicenseInstructions));

            //If there is a role specified, then write it into an attribute
            //Unless the Grant License Instructions are "none" or "ignore" in these cases, the role does note matter
            if((!string.IsNullOrWhiteSpace(this.GrantLicenseRole))
                && (this.GrantLicenseInstructions != ProvisioningGroup.GrantLicenseMode.Ignore)
                && (this.GrantLicenseInstructions != ProvisioningGroup.GrantLicenseMode.None)
                )
            {
                xmlWriter.WriteAttributeString(
                    ProvisioningGroup.XmlAttribute_GrantLicenseMinimumSiteRole,
                    this.GrantLicenseRole);
            }


            //For all the users mapped to this group, write them out
            foreach (var thisUserInfo in _usersSet.Values)
            {
                xmlWriter.WriteStartElement("GroupMember");
                xmlWriter.WriteAttributeString("name", thisUserInfo);
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
            
        }
    }

        
}

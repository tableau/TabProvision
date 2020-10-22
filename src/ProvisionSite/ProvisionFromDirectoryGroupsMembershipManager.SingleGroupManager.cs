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
        public ProvisioningGroup.GrantLicenseMode GrantLicenseInstructions
        {
            get
            {
                return _grantLicenseInstructions;
            }
        }

        private ProvisioningGroup.GrantLicenseMode _grantLicenseInstructions;

        /// <summary>
        /// If we are using GrantLicense then what is the role, e.g. "Creator", "Explorer", "Viewer", types of admins, ...
        /// </summary>
        public string GrantLicenseRole
        {
            get
            {
                return _grantLicenseRole;
            }
        }
        private string _grantLicenseRole;

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
            _grantLicenseInstructions = grantLicenseMode;
            _grantLicenseRole = grantLicenseRole;
        }

        /// <summary>
        /// Called if a Group comes up more than once in provisioning (e.g. multiple wildcard matches)
        /// If so, then we consider promoting the group to the higer of the two roles
        /// </summary>
        /// <param name="suggestedMode"></param>
        /// <param name="suggestedSiteRole"></param>
        public bool ConsiderGrantLicenseRoleUpgrade(ProvisioningGroup.GrantLicenseMode suggestedMode, string suggestedSiteRole)
        {
            string currentRole = _grantLicenseRole;
            var currentMode = _grantLicenseInstructions;

            //-------------------------------------------------------------------------
            //See if the suggested state is once that we need to act on.
            //-------------------------------------------------------------------------
            switch (suggestedMode)
            {
                case ProvisioningGroup.GrantLicenseMode.Ignore:
                    return false; //Do nothing

                case ProvisioningGroup.GrantLicenseMode.None:
                    return false; //Do nothing

                case ProvisioningGroup.GrantLicenseMode.OnLogin:
                    break; //Advance onward....

                default: //Degenerate case
                    IwsDiagnostics.Assert(false, "1021-106: Unknown grant license mode, " + suggestedMode.ToString());
                    throw new Exception("1021-106: Unknown grant license mode, " + suggestedMode.ToString());
            }

            //-------------------------------------------------------------------------
            //Based on the current state, take the approprate action
            //-------------------------------------------------------------------------
            switch(currentMode)
            {
                case ProvisioningGroup.GrantLicenseMode.Ignore:
                    //Apply the new mode
                    _grantLicenseInstructions = suggestedMode;
                    _grantLicenseRole = suggestedSiteRole;
                    return true; 

                case ProvisioningGroup.GrantLicenseMode.None:
                    //Apply the new mode
                    _grantLicenseInstructions = suggestedMode;
                    _grantLicenseRole = suggestedSiteRole;
                    return true;

                case ProvisioningGroup.GrantLicenseMode.OnLogin:
                    //Apply the new mode if it ranks higher
                    if (ProvisioningUser.CalculateRoleRank(currentRole) >=
                        ProvisioningUser.CalculateRoleRank(suggestedSiteRole))
                    {
                        //The current role ranks above/same as the suggested role.  Do nothing
                        return false;
                    }
                    else
                    {
                        //Apply the new mode
                        _grantLicenseInstructions = suggestedMode;
                        _grantLicenseRole = suggestedSiteRole;
                        return true;
                    }

                default: //Degenerate case
                        IwsDiagnostics.Assert(false, "1021-113: Unknown grant license mode, " + currentMode.ToString());
                        throw new Exception("1021-113: Unknown grant license mode, " + currentMode.ToString());
            }
            
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

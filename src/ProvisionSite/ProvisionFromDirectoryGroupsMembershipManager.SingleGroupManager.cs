using System;
using System.Collections.Generic;
using System.Xml;


/// <summary>
/// Allows us to sort all users being provisioned into groups
/// </summary>
internal partial class ProvisionFromDirectoryGroupsMembershipManager
{

    /// <summary>
    /// Constructor
    /// </summary>
    internal partial class SingleGroupManager
    {
        public readonly string GroupName;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="group"></param>
        public SingleGroupManager(string group)
        {
            this.GroupName = group;
        }


        /// <summary>
        /// Group members
        /// </summary>
        private Dictionary<string, string> _usersSet = new Dictionary<string, string>();

        public Dictionary<string, string>.ValueCollection GetUserNamess()
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
            return _usersSet.Remove(CannonicalKey(key));
        }

        /// <summary>
        /// Add a user to the set
        /// </summary>
        /// <param name="user"></param>
        internal void AddUser(string userName)
        {
            string cannonicalKey = CannonicalKey(userName);

            if (_usersSet.ContainsKey(cannonicalKey))
            {
                //User is already there.  Don't need to do anything
            }
            else
            {
                _usersSet.Add(cannonicalKey, userName);
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

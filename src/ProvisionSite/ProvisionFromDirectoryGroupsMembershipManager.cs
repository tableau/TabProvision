using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


/// <summary>
/// Allows us to sort all users being provisioned into groups
/// </summary>
internal partial class ProvisionFromDirectoryGroupsMembershipManager
{
    private Dictionary<string, SingleGroupManager> _groupsManager = new Dictionary<string, SingleGroupManager>();

    /// <summary>
    /// Constructor
    /// </summary>
    public ProvisionFromDirectoryGroupsMembershipManager()
    {

    }
    
    /// <summary>
    /// Add a user to a group
    /// </summary>
    /// <param name="user"></param>
    public void AddUserToGroup(ProvisionConfigExternalDirectorySync.SynchronizeGroupToGroup group, string userName)
    {
        //Get (create if necesary) the group's manager
        var groupManager = GetManagerForGroup(group.TargetGroupName);

        //Add the user toe hte group
       groupManager.AddUser(userName);
    }

    /// <summary>
    /// If a group manager does not yet exist, then it creates one
    /// </summary>
    /// <param name="group"></param>
    public void EnsureGroupManagerExistsForGroup(string group)
    {
        GetManagerForGroup(group);
    }

    /// <summary>
    /// Return the group manager for the group
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    private SingleGroupManager GetManagerForGroup(string group)
    {
        var cannonicalGroup = group.ToLower();
        SingleGroupManager thisGroupManager = null;

        //Add the user to a group manager
        _groupsManager.TryGetValue(cannonicalGroup, out thisGroupManager);
        if (thisGroupManager == null)
        {
            thisGroupManager = new SingleGroupManager(group);
            _groupsManager.Add(cannonicalGroup, thisGroupManager);
        }

        return thisGroupManager;
    }


    /// <summary>
    /// Users as XML
    /// </summary>
    /// <param name="xmlWriter"></param>
    internal void WriteUserGroupsAsXml(XmlWriter xmlWriter)
    {
        //Get these group by group so we serialize them out that way
        foreach (var thisGroupManager in _groupsManager.Values)
        {
            thisGroupManager.WriteGroupAsXml(xmlWriter);
            /*
            //For all the users mapped to this group, write them out
            foreach (var thisUserInfo in thisGroupManager.GetUsers())
            {
                thisUserInfo.WriteAsXml(xmlWriter);
            }*/
        }
    }

    /// <summary>
    /// Output the site membership block
    /// </summary>
    /// <param name="xmlWriter"></param>
    private void WriteProvisioningManifestXml_GroupMembership(XmlWriter xmlWriter)
    {
        //Get these group by group so we serialize them out that way
        foreach (var thisGroupManager in _groupsManager.Values)
        {
            thisGroupManager.WriteGroupAsXml(xmlWriter);
        }


    }

    /// <summary>
    /// If a class does not exist get for this group, then one is added to the set
    /// </summary>
    /// <param name="groupName"></param>
    internal void EnsureRoleManagerExistsForRole(string groupName)
    {
        GetManagerForGroup(groupName);
    }
}

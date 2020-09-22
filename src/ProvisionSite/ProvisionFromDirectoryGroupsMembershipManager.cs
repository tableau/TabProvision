using Microsoft.Graph;
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
    /// A lock we can use for thread safety
    /// </summary>
    private object _threadLock_ModifyGroupsList = new object();

    /// <summary>
    /// Constructor
    /// </summary>
    public ProvisionFromDirectoryGroupsMembershipManager()
    {

    }

    /// <summary>
    /// Return the group manager for the group
    /// </summary>
    /// <param name="group"></param>
    /// <param name="ifCreateGrantLicenseMode">If a group needs to be created, give it this Grant License Mode</param>
    /// <param name="ifCreateGrantLicenseRole">If the group needs to be created, give it this Grant License Role</param>
    /// <returns></returns>
    public SingleGroupManager GetManagerForGroup_CreateIfNeeded(
        string group, 
        ProvisioningGroup.GrantLicenseMode ifCreateGrantLicenseMode, 
        string ifCreateGrantLicenseRole)
    {
        var cannonicalGroup = group.ToLower();
        SingleGroupManager thisGroupManager;

        //Prevent this from getting entered my multiple threads
        lock (_threadLock_ModifyGroupsList)
        {
            //Add the user to a group manager
            _groupsManager.TryGetValue(cannonicalGroup, out thisGroupManager);

            if (thisGroupManager == null)
            {
                thisGroupManager = new SingleGroupManager(group, ifCreateGrantLicenseMode, ifCreateGrantLicenseRole);
                _groupsManager.Add(cannonicalGroup, thisGroupManager);
            }
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
    internal void EnsureRoleManagerExistsForRole(
        string groupName, 
        ProvisioningGroup.GrantLicenseMode ifCreateGrantLicenseMode,
        string ifCreateGrantLicenseRole)
    {
        GetManagerForGroup_CreateIfNeeded(groupName, ifCreateGrantLicenseMode, ifCreateGrantLicenseRole);
    }
}

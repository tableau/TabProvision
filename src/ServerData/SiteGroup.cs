using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Text;
using System.Xml;
/// <summary>
/// Information about a Grou[ in a Server's site
/// </summary>
class SiteGroup : IHasSiteItemId
{
    public readonly string Id;
    public readonly string Name;
    List<SiteUser> _usersInGroup;
    public readonly string SiteMinimumRoleOrNull = null;
    public readonly string GrantLicenseMode = null;

    SimpleLatch _groupUsersKnown = new SimpleLatch();

    /// <summary>
    /// TRUE if the set of users has been placed into this class
    /// </summary>
    public bool GroupUsersKnown
    {
        get
        {
            return _groupUsersKnown.Value;
        }
    }

    /// <summary>
    /// Any developer/diagnostic notes we want to indicate
    /// </summary>
    public readonly string DeveloperNotes = "";

    /// <summary>
    /// Returns the list of users associated with this group
    /// </summary>
    public ICollection<SiteUser> Users
    {
        get
        {
            return _usersInGroup.AsReadOnly();
        }
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="groupNode"></param>
    public SiteGroup(XmlNode groupNode, IEnumerable<SiteUser> usersToPlaceInGroup )
    {
        //If we were passed in a set of users, store them
        var usersList = new List<SiteUser>();
        if(usersToPlaceInGroup != null)
        {
            usersList.AddRange(usersToPlaceInGroup);
        }
        _usersInGroup = usersList;


        if(groupNode.Name.ToLower() != "group")
        {
            AppDiagnostics.Assert(false, "Not a group");
            throw new Exception("Unexpected content - not group");
        }

        this.Id = groupNode.Attributes["id"].Value;
        this.Name = groupNode.Attributes["name"].Value;

        //===================================================================================
        //See if there is a GRANT LICENSE ON SIGN IN mode here
        //===================================================================================
        //Get all the group nodes
        var nsManager = XmlHelper.CreateTableauXmlNamespaceManager("iwsOnline");
        var xmlGroupImportNode = groupNode.SelectSingleNode(".//iwsOnline:import", nsManager);
        if (xmlGroupImportNode != null)
        {
            this.SiteMinimumRoleOrNull = XmlHelper.GetAttributeIfExists(xmlGroupImportNode, "siteRole", null);
            this.GrantLicenseMode = XmlHelper.GetAttributeIfExists(xmlGroupImportNode, "grantLicenseMode", null);
        }
    }


    public override string ToString()
    {
        return "Group: " + this.Name + "/" + this.Id;
    }

    /// <summary>
    /// Adds a set of users.  This is typically called when initializing this object.
    /// </summary>
    /// <param name="usersList"></param>
    internal void AddUsers(IEnumerable<SiteUser> usersList)
    {
        //If add users got called (even with 0 users) mark the group users as known
        _groupUsersKnown.Trigger();

        //Nothing to add?
        if (usersList == null)
        {
            return;
        }

        _usersInGroup.AddRange(usersList);
    }

    string IHasSiteItemId.Id
    {
        get { return this.Id; }
    }
}

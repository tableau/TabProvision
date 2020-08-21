using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// Membership in a group
/// </summary>
internal class ProvisioningGroup
{
    public readonly string GroupName;
    public readonly ReadOnlyCollection<string> Members;


    /// <summary>
    /// Contructor
    /// </summary>
    /// <param name="xmlNode"></param>
    public ProvisioningGroup(XmlNode xmlNode)
    {
        this.GroupName = xmlNode.Attributes["name"].Value;

        var usersInGroup = new List<string>();
        var xNodesMembers = xmlNode.SelectNodes(".//GroupMember");
        foreach(XmlNode xmlGroupMember in xNodesMembers)
        {
            usersInGroup.Add(xmlGroupMember.Attributes["name"].Value);
        }
        this.Members = usersInGroup.AsReadOnly();
    }
}
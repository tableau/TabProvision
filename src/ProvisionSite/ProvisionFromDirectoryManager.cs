using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


/// <summary>
/// Allows us to sort all users being provisioned into role
/// </summary>
internal partial class ProvisionFromDirectoryManager
{
    /// <summary>
    /// User roles
    /// </summary>
    public readonly ProvisionFromDirectoryRolesMembershipManager RolesManager = new ProvisionFromDirectoryRolesMembershipManager();

    /// <summary>
    /// Groups
    /// </summary>
    public readonly ProvisionFromDirectoryGroupsMembershipManager GroupsMembershipManager = new ProvisionFromDirectoryGroupsMembershipManager();



    internal void GenerateProvisioningManifestFile(string provisioningManifest, ProvisionConfigExternalDirectorySync provisionConfig)
    {
        var xmlWriter = XmlWriter.Create(provisioningManifest);
        WriteProvisioningManifestXml(xmlWriter, provisionConfig);
        xmlWriter.Close();

    }

    internal void WriteProvisioningManifestXml(XmlWriter xmlWriter, ProvisionConfigExternalDirectorySync provisionConfig)
    {
        xmlWriter.WriteStartElement("SiteProvisioning");
            WriteProvisioningManifestXml_SiteMembership(xmlWriter, provisionConfig);
            WriteProvisioningManifestXml_GroupsMembership(xmlWriter, provisionConfig);
        xmlWriter.WriteEndElement();

    }

    /// <summary>
    /// Output the site membership block
    /// </summary>
    /// <param name="xmlWriter"></param>
    private void WriteProvisioningManifestXml_SiteMembership(XmlWriter xmlWriter, ProvisionConfigExternalDirectorySync provisionConfig)
    {
        xmlWriter.WriteStartElement("SiteMembership");

        //--------------------------------------------------------------------------------
        //Write the provisioning instructions into the XML
        //--------------------------------------------------------------------------------
        //Modify existing users?
        //1.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authDefaultExistingUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForExistingDefaultAuthUsers));
        //2.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authSamlExistingUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForExistingSamlUsers));
        //3.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authOpenIdExistingUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForExistingOpenIdUsers));


        //Modify missing users
        //1.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authDefaultMissingUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForMissingDefaultAuthUsers));
        //2.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authSamlMissingUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForMissingSamlUsers));
        //3.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authOpenIdMissingUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForMissingOpenIdUsers));

        //Unlicese unexptected users
        //1.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authDefaultUnexpectedUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForUnexpectedDefaultAuthUsers));
        //2.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authSamlUnexpectedUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForUnexpectedSamlUsers));
        //3.
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_authOpenIdUnexpectedUsers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForUnexpectedOpenIdUsers));

        //Write out all the users
        RolesManager.WriteUserRolesAsXml(xmlWriter);

        xmlWriter.WriteEndElement();
    }

    /// <summary>
    /// Output the groups membership block
    /// </summary>
    /// <param name="xmlWriter"></param>
    private void WriteProvisioningManifestXml_GroupsMembership(XmlWriter xmlWriter, ProvisionConfigExternalDirectorySync provisionConfig)
    {
        xmlWriter.WriteStartElement("GroupsMemberships");

        //--------------------------------------------------------------------------------
        //Write the provisioning instructions into the XML
        //--------------------------------------------------------------------------------
        //Missing group members?
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_MissingGroupMembers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForGroupMisingMembers));

        //Unexpected group members?
        xmlWriter.WriteAttributeString(
            ProvisionUserInstructions.XmlAttribute_UnexpectedGroupMembers,
            ProvisionUserInstructions.XmlAttributeText(provisionConfig.ActionForGroupUnexpectedMembers));

        GroupsMembershipManager.WriteUserGroupsAsXml(xmlWriter);

        //WriteProvisioningManifestXml_SiteMembership(xmlWriter);
        xmlWriter.WriteEndElement();
    }

}
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

/// <summary>
/// Instructions for changing content ownership from one user to another
/// </summary>
internal partial class ProvisioningChangeContentOwnership
{
    public readonly string OldOwnerName;
    public readonly string NewOwnerName;

    /// <summary>
    /// Explictly passed in value
    /// </summary>
    /// <param name="userName"></param>
    /// <param name="userRole"></param>
    /// <param name="userAuthentication"></param>
    public ProvisioningChangeContentOwnership(string oldOwnerName, string newOwnerName)
    {
        this.OldOwnerName = oldOwnerName;
        this.NewOwnerName = newOwnerName;

        ValidateClassData();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="xmlNode"></param>
    public ProvisioningChangeContentOwnership(XmlNode xmlNode)
    {
        this.OldOwnerName = xmlNode.Attributes[XmlAttribute_OldOwnerName].Value;
        this.NewOwnerName = xmlNode.Attributes[XmlAttribute_NewOwnerName].Value;

        ValidateClassData();
    }


    /// <summary>
    /// Sanity test the class' values
    /// </summary>
    private void ValidateClassData()
    {
        if(string.IsNullOrWhiteSpace(this.OldOwnerName))
        {
            throw new Exception("201202-351: oldOwnerName cannot be blank");
        }

        if (string.IsNullOrWhiteSpace(this.NewOwnerName))
        {
            throw new Exception("201202-352: newOwnerName cannot be blank");
        }
    }


    /// <summary>
    /// Write out XML for the object
    /// </summary>
    /// <param name="xmlWriter"></param>
    internal void WriteAsXml(XmlWriter xmlWriter)
    {
        xmlWriter.WriteStartElement("ChangeContentOwner");
        xmlWriter.WriteAttributeString(XmlAttribute_OldOwnerName, this.OldOwnerName);
        xmlWriter.WriteAttributeString(XmlAttribute_NewOwnerName, this.NewOwnerName);
    }
}
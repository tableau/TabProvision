using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


/// <summary>
/// Provisioning insturcitons on which users to add and and what to do with missing users
/// </summary>
internal partial class ProvisionUserInstructions
{
    public const string XmlAttribute_authSamlUnexpectedUsers = "authSamlUnexpectedUsers";
    public const string XmlAttribute_authDefaultUnexpectedUsers = "authDefaultUnexpectedUsers";
    public const string XmlAttribute_authOpenIdUnexpectedUsers = "authOpenIdUnexpectedUsers";

    public const string XmlAttribute_authSamlMissingUsers = "authSamlMissingUsers";
    public const string XmlAttribute_authDefaultMissingUsers = "authDefaultMissingUsers";
    public const string XmlAttribute_authOpenIdMissingUsers = "authOpenIdMissingUsers";

    public const string XmlAttribute_authSamlExistingUsers = "authSamlExistingUsers";
    public const string XmlAttribute_authDefaultExistingUsers = "authDefaultExistingUsers";
    public const string XmlAttribute_authOpenIdExistingUsers = "authOpenIdExistingUsers";

    public const string XmlAttribute_MissingGroupMembers    = "missingGroupMembers";                                                              
    public const string XmlAttribute_UnexpectedGroupMembers = "unexpectedGroupMembers";

    private const string AttributeValue_Report = "Report";
    private const string AttributeValue_Modify = "Modify";
    private const string AttributeValue_Add = "Add";
    private const string AttributeValue_Delete = "Delete";
    private const string AttributeValue_Unlicense = "Unlicense";

    /// <summary>
    /// XML Serialization value
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    internal static string XmlAttributeText(MissingGroupMemberAction action)
    {
        switch (action)
        {
            case MissingGroupMemberAction.Add:
                return AttributeValue_Add;

            case MissingGroupMemberAction.Report:
                return AttributeValue_Report;
            default:
                IwsDiagnostics.Assert(false, "814-412: Internal error, missing value");
                throw new Exception("814-412: Internal error, missing value");
        }
    }

    /// <summary>
    /// XML Serialization value
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    internal static string XmlAttributeText(UnexpectedGroupMemberAction action)
    {
        switch (action)
        {
            case UnexpectedGroupMemberAction.Delete:
                return AttributeValue_Delete;

            case UnexpectedGroupMemberAction.Report:
                return AttributeValue_Report;
            default:
                IwsDiagnostics.Assert(false, "814-413: Internal error, missing value");
                throw new Exception("814-413: Internal error, missing value");
        }
    }

    /// <summary>
    /// XML Serialization value
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    internal static string XmlAttributeText(ExistingUserAction action)
    {
        switch(action)
        {
            case ExistingUserAction.Modify:
                return AttributeValue_Modify;

            case ExistingUserAction.Report:
                return AttributeValue_Report;
            default:
                IwsDiagnostics.Assert(false, "814-248: Internal error, missing value");
                throw new Exception("814-248: Internal error, missing value");
        }
    }

    /// <summary>
    /// XML Serialization value
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    internal static string XmlAttributeText(MissingUserAction action)
    {
        switch (action)
        {
            case MissingUserAction.Add:
                return AttributeValue_Add;

            case MissingUserAction.Report:
                return AttributeValue_Report;
            default:
                IwsDiagnostics.Assert(false, "814-257: Internal error, missing value");
                throw new Exception("814-257: Internal error, missing value");
        }
    }

    /// <summary>
    /// XML Serialization value
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    internal static string XmlAttributeText(UnexpectedUserAction action)
    {
        switch (action)
        {
            case UnexpectedUserAction.Unlicense:
                return AttributeValue_Unlicense;

            case UnexpectedUserAction.Report:
                return AttributeValue_Report;

            case UnexpectedUserAction.Delete:
                return AttributeValue_Delete;

            default:
                IwsDiagnostics.Assert(false, "814-303: Internal error, missing UnexpectedUserAction value");
                throw new Exception("814-303: Internal error, missing UnexpectedUserAction value");
        }
    }


    /// <summary>
    /// Parse the attribute text
    /// </summary>
    /// <param name="parseText"></param>
    /// <returns></returns>
    public static MissingGroupMemberAction ParseMissingGroupMemberAction(string parseText)
    {
        if (parseText == AttributeValue_Report)
        {
            return MissingGroupMemberAction.Report;
        }

        if (parseText == AttributeValue_Add)
        {
            return MissingGroupMemberAction.Add;
        }

        IwsDiagnostics.Assert(false, "814-415: Unkown value: " + parseText);
        throw new Exception("814-415: Unkown value: " + parseText);
    }

    /// <summary>
    /// Parse the attribute text
    /// </summary>
    /// <param name="parseText"></param>
    /// <returns></returns>
    public static UnexpectedGroupMemberAction ParseUnexpectedGroupMemberAction(string parseText)
    {
        if (parseText == AttributeValue_Report)
        {
            return UnexpectedGroupMemberAction.Report;
        }

        if (parseText == AttributeValue_Delete)
        {
            return UnexpectedGroupMemberAction.Delete;
        }

        IwsDiagnostics.Assert(false, "814-414: Unkown value: " + parseText);
        throw new Exception("814-414: Unkown value: " + parseText);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    public static UnexpectedUserAction ParseUnexpectedUserActionFromAttribute(XmlNode xmlNode, string attributeName)
    {
        if (xmlNode == null)
        {
            throw new Exception("0902-315: Missing XML node");
        }

        
        var attribute = xmlNode.Attributes[attributeName];
        if(attribute == null)
        {
            throw new Exception("0902-316: XML attribute not found, '" + attributeName + "' in " + xmlNode.Name);
        }

        return ParseUnexpectedUserAction(attribute.Value);
    }

    /// <summary>
    /// Parse the attribute text
    /// </summary>
    /// <param name="parseText"></param>
    /// <returns></returns>
    public static UnexpectedUserAction ParseUnexpectedUserAction(string parseText)
    {
        if(parseText == AttributeValue_Report)
        {
            return UnexpectedUserAction.Report;
        }

        if (parseText == AttributeValue_Unlicense)
        {
            return UnexpectedUserAction.Unlicense;
        }

        if (parseText == AttributeValue_Delete)
        {
            return UnexpectedUserAction.Delete;
        }

        IwsDiagnostics.Assert(false, "811-1105: Unkown value for ParseUnexpectedUserAction: " + parseText);
        throw new Exception("811-1105: Unkown value for ParseUnexpectedUserAction: " + parseText);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    public static MissingUserAction ParseMissingUserActionFromAttribute(XmlNode xmlNode, string attributeName)
    {
        if (xmlNode == null)
        {
            throw new Exception("0902-317: Missing XML node");
        }


        var attribute = xmlNode.Attributes[attributeName];
        if (attribute == null)
        {
            throw new Exception("0902-318: XML attribute not found, '" + attributeName + "' in " + xmlNode.Name);
        }

        return ParseMissingUserAction(attribute.Value);
    }

    /// <summary>
    /// Parse the attribute text
    /// </summary>
    /// <param name="parseText"></param>
    /// <returns></returns>
    public static MissingUserAction ParseMissingUserAction(string parseText)
    {
        if (parseText == AttributeValue_Report)
        {
            return MissingUserAction.Report;
        }

        if (parseText == AttributeValue_Add)
        {
            return MissingUserAction.Add;
        }

        IwsDiagnostics.Assert(false, "814-1015: Unkown value for ParseMissingUserAction: " + parseText);
        throw new Exception("814-1015: Unkown value for ParseMissingUserAction: " + parseText);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlNode"></param>
    /// <param name="attributeName"></param>
    /// <returns></returns>
    public static ExistingUserAction ParseExistingUserActionFromAttribute(XmlNode xmlNode, string attributeName)
    {
        if (xmlNode == null)
        {
            throw new Exception("0902-319: Missing XML node");
        }


        var attribute = xmlNode.Attributes[attributeName];
        if (attribute == null)
        {
            throw new Exception("0902-320: XML attribute not found, '" + attributeName + "' in " + xmlNode.Name);
        }

        return ParseExistingUserAction(attribute.Value);
    }

    /// <summary>
    /// Parse the attribute text
    /// </summary>
    /// <param name="parseText"></param>
    /// <returns></returns>
    public static ExistingUserAction ParseExistingUserAction(string parseText)
    {
        if (parseText == AttributeValue_Report)
        {
            return ExistingUserAction.Report;
        }

        if (parseText == AttributeValue_Modify)
        {
            return ExistingUserAction.Modify;
        }

        IwsDiagnostics.Assert(false, "814-1222: Unkown value for ParseExistingUserAction: " + parseText);
        throw new Exception("814-1222: Unkown value for ParseExistingUserAction: " + parseText);
    }

}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
/// <summary>
/// Information about a Flow in a Server's site
/// </summary>
class SiteFlow : SiteDocumentBase
{
    public readonly string Description;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="flowNode"></param>
    public SiteFlow(XmlNode flowNode) : base(flowNode)
    {
        if(flowNode.Name.ToLower() != "flow")
        {
            AppDiagnostics.Assert(false, "Not a flow");
            throw new Exception("Unexpected content - not flow");
        }
        //Get the underlying data source type
        this.Description = flowNode.Attributes["description"].Value;

    }

    /// <summary>
    /// Text description
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Flow: " + this.Name + "/" + this.Description + "/" + this.Id;
    }

}

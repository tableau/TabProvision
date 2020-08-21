using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Helper class.  Files downloaded from Tableau Server can be either binary (octet-stream) or XML (application/xml),
/// for example a *.TWBX is binary (it is a *.zip file) and a *.TWB is XML.  
/// 
/// This is a helper class that mapps the returned payload type to the appropriate file system extension.  
/// 
/// It is used for helping with downloads of Workbooks (*.twb vs. *.twbx) and Datasources (*.tds vs. *.tdsx)
/// </summary>
public class DownloadPayloadTypeHelper
{
    private Dictionary<string, string> _mapContent;


    /// <summary>
    /// Generate a file content mapper with custom mappings
    /// </summary>
    /// <param name="mapContent"></param>
    private DownloadPayloadTypeHelper(Dictionary<string, string> mapContent)
    {
        _mapContent = mapContent;
    }
    

    /// <summary>
    /// Create a content mapper for common image tuype
    /// </summary>
    /// <returns></returns>
    public static DownloadPayloadTypeHelper CreateForImageDownload()
    {
        var mapContent =  new Dictionary<string, string>();
        mapContent.Add("image/png", ".png");
        mapContent.Add("image/gif", ".gif");
        mapContent.Add("image/jpeg", ".jpg");

        return new DownloadPayloadTypeHelper(mapContent);
    }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileExtensionIfBinary"></param>
    /// <param name="fileExtensionIfXml"></param>
    public DownloadPayloadTypeHelper(string fileExtensionIfBinary, string fileExtensionIfXml)
    {
        var mapContent = new Dictionary<string, string>();
        fileExtensionIfBinary = EnsureFileExensionFormat(fileExtensionIfBinary);
        fileExtensionIfXml = EnsureFileExensionFormat(fileExtensionIfXml);

        //Common mappings we expect
        mapContent.Add("application/octet-stream", fileExtensionIfBinary);
        mapContent.Add("application/xml", fileExtensionIfXml);

        _mapContent = mapContent;
    }

    /// <summary>
    /// Given a content type, return the corresponding file extension we want to save the content as
    /// </summary>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public string GetFileExtension(string contentType)
    {
        return _mapContent[contentType];
    }

    /// <summary>
    /// Adds a "." if we need it
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    private static string EnsureFileExensionFormat(string extension)
    {
        extension = extension.Trim();
        if (extension[0] != '.') { extension = "." + extension; }

        return extension;
    }

}

using System;
using System.Xml;
using System.Collections.Generic;
using System.Text;

partial class DownloadUsersList : DownloadUsersListBase
{
    /// <summary>
    /// Return the set of users in the site
    /// </summary>
    /// <param name="signIn"></param>
    /// <returns></returns>
    public static DownloadUsersList CreateAndExecute(TableauServerSignIn signIn)
    {
        var retObj = new DownloadUsersList(signIn);
        retObj.ExecuteRequest();
        return retObj;
    }
}

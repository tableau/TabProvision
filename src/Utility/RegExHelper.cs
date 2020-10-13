//==================================================================================
// Copyright © 2018 Ivo Salmre.  ALL RIGHTS RESERVED.  NO WARRANTY OF ANY KIND.
// No part of this code may be used, copied or modified in any way without explicit
// written permission.
//==================================================================================
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Web;

/// <summary>
/// Common regular expresisons
/// </summary>
internal static class RegExHelper
{

    //Test with: http://regexstorm.net/tester


    /// <summary>
    /// Email validator
    /// </summary>
    static Regex _isEmail;
    static Regex _isSimpleAlphabetic;
    static Regex _isValidTableauContentId;

    /// <summary>
    /// Valid
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static bool IsValidIdTableauContentId(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        //Create it if needed
        //THREADING: Safe.  Only downside is the possibility of creating a redundant RegEx that gets thrown away
        if (_isValidTableauContentId == null)
        {
            //a-Z or 0-9 or '-'
            _isValidTableauContentId = new Regex(@"^[A-Za-z0-9\-]*$");
        }

        return _isValidTableauContentId.IsMatch(text);
    }


    /// <summary>
    /// Checks to see if text is simple alphabetic
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static bool IsAlphabeticText(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        //Create it if needed
        //THREADING: Safe.  Only downside is the possibility of creating a redundant RegEx that gets thrown away
        if (_isSimpleAlphabetic == null)
        {
            //Added "\+" to allowable email name charachters, to allow for GMAIL type test accounts (e.g. "xxxxxxxx+9999999@gmail.com")
            _isSimpleAlphabetic = new Regex(@"^[a-zA-Z]+$");
        }

        return _isSimpleAlphabetic.IsMatch(text);
    }


    /// <summary>
    /// Checks to see if the email address is well formed
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public static bool IsEmail(string email)
    {
        if (email == null) return false;
        //Create it if needed
        //THREADING: Safe.  Only downside is the possibility of creating a redundant RegEx that gets thrown away
        if(_isEmail == null)
        {
            //Slightly generous on the suffix (e.g. '*.com') in that it allows any # of 2-10 char suffixes
            //Probably also missing some esoteric email characters that may be valid -- don't care

            //Added "\+" to allowable email name charachters, to allow for GMAIL type test accounts (e.g. "xxxxxxxxxx+9999999@gmail.com")
            _isEmail = new Regex(@"^([\w\.\-\+]+)@([\w\-]+)((\.(\w){2,10})+)$");
        }

        return _isEmail.IsMatch(email);
    }

}
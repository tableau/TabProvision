# TabProvision
[![Community Supported](https://img.shields.io/badge/Support%20Level-Community%20Supported-457387.svg)](https://www.tableau.com/support-levels-it-and-developer-tools)

## What is TabProvision?
Many Tableau Online customers need to user their organization's Azure AD directory to manage their Tableau Online deployment. 
This sample shows how to use Azure AD Groups (or groups defined explicitly in a local file) to provision Administrators/Creators/Explorers/Viewers in their Tableau Online site. 
The sample code can also be adapted to read Group membership information from any other source and to use that information to provision the users/roles/groups in a Tableau Online (or Tableau Server) site.

Some background information: Tableau Online already supports using SAML to authenticate users from Microsoft Azure (or other SAML supporting Identity Providers, IdPs). 
However, these  users need to be provisioned; they need to be added to the Tableau Online site, have their Role in the site set (e.g. "Creator", "Explorer", "Viewer") and have their authentication mechanism specified. This tool does that.

This is done in 2 steps:
1. Setup: You define (in a local XML file) (i) Azure AD groups to map to Tableau Online roles, (ii) Groups to sync to Tableau Online, (iii) Any service or external accounts
2. Execution: You run TabProvision.exe and it:
    i. Logs into your Azure AD (using credentials you provide) and pulls down the membership information of the groups you have specified.
    ii. Logs into your Tableau Online or Server site (using credentials you provide) and pushes up the Users/Roles/Authentication you wish to provision, as well as any Group membership you wish to provision
    iii. Unexpected users in the Online Site or in the Site's Group memberships can either be reported in an output report, or unlicensed/removed.

TabProvision.exe can be run as frequently as you need to keep Azure AD and you Tableau Online / Server site in sync (e.g. daily/weekly/monthly)

### How to “provision like a pro” using Grant License on Sign In and the TabProvison tool
Using Grant License on Sign In can dramatically simplify provisioning and remove adoption bottlenecks. To use Grant License on Sign In with TabProvision follow these two steps:

1.	Use a group (e.g. “All potential users”) to import all of your potential users as “Unlicensed” users to your site. This is a great way to pre-provision your organization’s members without needing licenses for each user upfront.

2.	Add that same group to the “SynchronizeGroups” section in your XML (example below)
-	Set grantLicenseMode=’true”
-	Set grantLicenseMinimumSiteRole="Viewer" or “Explorer”   

RESULTS: 
-	All of these users will become potential users for your site.                                                            
-	If/when they sign in they will get upgraded from "Unlicensed" to "Explorer" or "Viewer" 

The example XML later in this ReadMe shows exactly how this is done.   
More info: https://help.tableau.com/current/online/en-us/grant_role.htm             
          
## Versions of Tableau Online (and Tableau Server) 
TabProvision was written and tested with Tableau Online for the 2020.3 release
- It should work in all Tableau Server versions >= 2020.3  


## Getting started with TabProvision (for non-developers)
You do not need to download and compile the source code to use TabProvision. Those interested in simply running the application can download and unzip the setup from
https://github.com/tableau/TabProvision/releases  
Running setup.exe will install the TabProvision application on your Windows machine. 

Application: The application can be run in either interactive (UI) or command line mode. When running in interactive mode the application will also show you the command line for performing all of the actions, making it easy to automate.  The application UI offers three top-level options: 
   1. Provision directly from an XML manifest file. This file will explicitly contain the users/roles/groups that you want to provision to your Tableau Online/Server site
   2. Provision from Microsoft Azure AD directory. For this you will use a local XML file that indicates which Azure AD Groups to provision from
   3. Create a Provisioning Manifest XML file from your current Online (or Server) site. This signs into your Online site, downloads the list of Users and Groups and creates a local provisioning manifest XML file (it does not perform provisioning actions on your Online site). You can use this to make a 'backup' of the current state of users and groups provisioning on your Online site. This can also be useful for creating a baseline XML file that you can modify for custom provisioning needs.

NOTE: Both of these options require logging into your Tableau Online (or Tableau Server) site. The Azure AD option requires logging in to your Microsoft Azure instance.  Example:

Three XML files are used by the application. 

### REQUIRED FOR ALL: XML file for sign in secrets
```xml
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
    <!-- Replace the entire URL below with a URL to your Tableau server and site-->
    <SiteUrl value="https://xxxxYourPodxxxx.online.tableau.com/#/site/xxxxYOUR SITE HERExxxx"/>

    <!-- Information we need to sign into the Tableau site-->
    <TableauSiteLogin clientId="xxxxx@xxxx.com" secret="xxxxxxxx"/>

    <!--Information about the Azure AD instance we need to sign into to grab group/members information -->    
    <AzureAdLogin tenantId="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" clientId="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" secret="xxxxxxxxxxxxxx" />
</Configuration>
```

### REQUIRED FOR AZURE AD USERS/GROUPS: XML file with Azure AD groups
```xml
<?xml version="1.0" encoding="utf-8"?>
<!-- Valid userEmailLookup: 
          "UserPrincipalName"            : (default) Use the principal's name
       or "PreferAzureProxyPrimaryEmail" : If present, use the AzureProxy (smtp) value; if not found, use the UserPrincipal 
-->
<SynchronizeConfiguration userEmailLookup="UserPrincipalName">

    <!-- Users in these source groups will be mapped to specific roles inside the Tableau site -->
    <!-- Valid actions: authXXXXXUnexpectedUsers ="Unlicense" or "Report" or "Delete" -->
    <!-- Valid actions: authXXXXXMissingUsers    ="Add"       or "Report"             -->
    <!-- Valid actions: authXXXXXExistingUsers   ="Modify"    or "Report"             -->
    <!-- EXAMPLE: Apply changes: <SynchronizeRoles authSamlUnexpectedUsers="Unlicense" authSamlMissingUsers="Add"    authSamlExistingUsers="Modify"     authDefaultUnexpectedUsers="Unlicense" authDefaultMissingUsers="Add"    authDefaultExistingUsers="Modify"        authOpenIdUnexpectedUsers="Report" authOpenIdMissingUsers="Report" authOpenIdExistingUsers="Report">  -->
    <!-- EXAMPLE: Reports only: <SynchronizeRoles authSamlUnexpectedUsers="Report"    authSamlMissingUsers="Report" authSamlExistingUsers="Report"     authDefaultUnexpectedUsers="Report"    authDefaultMissingUsers="Report" authDefaultExistingUsers="Report"        authOpenIdUnexpectedUsers="Report" authOpenIdMissingUsers="Report" authOpenIdExistingUsers="Report"> --> 
    <SynchronizeRoles authSamlUnexpectedUsers="Unlicense" authSamlMissingUsers="Add" authSamlExistingUsers="Modify"   authDefaultUnexpectedUsers="Unlicense" authDefaultMissingUsers="Add" authDefaultExistingUsers="Modify"     authOpenIdUnexpectedUsers="Unlicense" authOpenIdMissingUsers="Report" authOpenIdExistingUsers="Report">  
         <!-- EXAMPLE: EXPLICIT GROUPS for Tableau roles: These Azure AD groups members will be assigned the specified roles in Tableau-->
         <SynchronizeRole sourceGroup="Tableau Online 001 Admins" targetRole="SiteAdministratorCreator" auth="serverDefault"/>
         <SynchronizeRole sourceGroup="Tableau Online 001 Creators" targetRole="Creator"  auth="serverDefault"/>

         <!-- EXAMPLE: ALLOW ROLE PROMOTION -->
         <!-- To support Tableau "Grant License on Sign In" you can specify the attribute  allowPromotedRole="true". This indicates that if the user already has a Role greater than the one specified in this Synchronize Role Group, then keep the higher role -->
         <!-- allowPromotedRole="true" is particularly useful when BULK ADDING users as "Unlicensed" and using Tableau's Grant License on Sign in functionality to assign a default role to members of a Tableau Site Group-->
         <SynchronizeRole sourceGroup="Tableau Online 001 Explorers" targetRole="Explorer"  auth="serverDefault"  allowPromotedRole="true"/>
         <SynchronizeRole sourceGroup="Tableau Online 001 Viewers"   targetRole="Viewer"    auth="serverDefault"  allowPromotedRole="true"/>

         <!-- RECOMMENDED: Have a group that contains ALL users you want to use Grant License on Sign In for. Provision them as "Unlicensed" and set allowPromotedRole="True"-->
         <SynchronizeRole sourceGroup="Tableau Online 001 Potential Users" targetRole="Unlicensed"    auth="serverDefault"  allowPromotedRole="true"/>

         <!-- EXAMPLE: WILDCARD group name matching: All users in groups starting with "TabProvision Groups" will be added with the specified targetRole-->         
         <SynchronizeRole sourceGroupMatch="startswith" sourceGroup="TabProvision Groups" targetRole="Unlicensed" auth="serverDefault" allowPromotedRole="true"/>

        <!-- EXAMPLE: Wildcard has additional 'filterSourceGroupContains' filter that searches for a match at any position in the name-->
         <SynchronizeRole sourceGroupMatch="startswith" filterSourceGroupContains="Explorer" sourceGroup="TabProvision Groups" targetRole="Explorer" auth="serverDefault" allowPromotedRole="true"/>
        
         <!-- EXAMPLE: OVERRIDES specify any explicit user/auth/role that we want to supersede anything we find in the groups that we syncrhonize from -->
         <SiteMembershipOverrides>
              <!-- Valid role values: "Unlicensed", "Viewer", "Explorer", "Creator", "SiteAdministratorExplorer", "SiteAdministratorCreator"-->
              <!-- EXAMPLE: This user will be licensed as "Unlicensed"-->
              <User name="xxxxPerson1xxxxx@xxxxDomainxxxxx.com"   role="Unlicensed" auth="serverDefault" allowPromotedRole="false" />
              <!-- EXAMPLE: This user will be licensed as "Unlicensed" if they don't exist on the site.                                     -->
              <!--          If they do exist and have a higher ranked role, that will be left as-is because allowPromotedRole="true" is set -->
              <User name="xxxxPerson2xxxxx@xxxxDomainxxxxx.com"   role="Unlicensed" auth="serverDefault" allowPromotedRole="true" />
         </SiteMembershipOverrides>
    </SynchronizeRoles>
  
    <!-- Users in these groups will me mapped into group membership inside the Tableau site -->
    <!-- Valid actions: unexpectedGroupMembers ="Delete" or "Report" -->
    <!-- Valid actions: missingGroupMembers    ="Add"    or "Report" -->
    <SynchronizeGroups  missingGroupMembers="Add" unexpectedGroupMembers="Delete">
         <!-- RECOMMENDED: Most groups should set grantLicenseMode="onLogin" and grantLicenseMinimumSiteRole="Viewer" or "Explorer"      -->
         <!--              This will facilitate auto-provisioning for users in these groups; as they sign in they will be given licenses -->

         <!-- Valid grantLicenseMode values:                                                                                                 -->
         <!--   grantLicenseMode="ignore"  : do nothing (default)                                                                            -->
         <!--   grantLicenseMode="none"    : REMOVE Grant Licenense on Login for the group                                                   -->
         <!--   grantLicenseMode="onLogin" : (RECOMMENDED!) ENABLE Grant License on Login (requires 'grantLicenseMinimumSiteRole' to be set) -->

         <!-- RECOMMENDED: Use Grant License On Sign In.  To do this: -->
         <!-- 1. Have a group for ALL users (see right below) -->
         <!-- 2. In your Tableau Online site set the MINIMUM SITE ROLE for this GROUP to be "Explorer" or "Viewer" and check GRANT ROLE ON SIGN IN  -->
         <!--        NOTE: This can be done by setting the 'grantLicenseMode' and 'grantLicenseMinimumSiteRole' attributes of the group (see below) -->
         <!-- 3. Add this group to your TabProvision SynchronizeGroups section (see below)                                                          -->
         <!-- 4. Add this group to your TabProvision SynchronizeRoles section (see above) with targetRole="Unlicensed" and allowPromotedRole="true" -->
         <!-- RESULTS: All of these users will become potential users for your site.                                                                -->
         <!--          If/when they sign in they will get upgraded from "Unlicensed" to "Explorer" or "Viewer"                                      -->
         <!-- More info: https://help.tableau.com/current/online/en-us/grant_role.htm                                                               -->
         <SynchronizeGroup sourceGroup="Tableau Online 001 Potential Users" targetGroup="Potential Users" grantLicenseMode="onLogin" grantLicenseMinimumSiteRole="Viewer"/>

         <!-- EXAMPLE: EXPLICIT GROUP MEMBERSHIP. These group memberships will be copied from Azure AD to Tableau -->  
         <SynchronizeGroup sourceGroup="Biz Group - Accounting" targetGroup="Accounting Analytics" grantLicenseMode="onLogin" grantLicenseMinimumSiteRole="Explorer"/>
         <SynchronizeGroup sourceGroup="Biz Group - Marketing" targetGroup="Marketing Analytics"   grantLicenseMode="onLogin" grantLicenseMinimumSiteRole="Explorer"/>

         <!-- EXAMPLE: WILDCARD GROUP NAMES. All groups starting with "TabProvision Groups" will be added    -->
         <!--          The group names will be duplicated between Azure AD and Tableau.                      -->         
         <SynchronizeMatchedGroup sourceGroupMatch="startswith" sourceGroup="TabProvision Groups" grantLicenseMode="ignore" />
        
         <!-- EXAMPLE: Wildcard has additional 'filterSourceGroupContains' filter that searches for a match at any position in the name-->
         <SynchronizeMatchedGroup sourceGroupMatch="startswith" filterSourceGroupContains="Tableau Creator" sourceGroup="TabProvision Groups"  grantLicenseMode="onLogin" grantLicenseMinimumSiteRole="Creator"  />

    </SynchronizeGroups>

</SynchronizeConfiguration>
```

### REQUIRED FOR FILE-BASED PROVISIONING OF USERS/GROUPS: XML file with Azure AD groups
```xml
<?xml version="1.0" encoding="utf-8"?>
<SiteProvisioning>
   <!-- All the users on the site go here -->
   <!-- Valid actions: authXXXXXUnexpectedUsers ="Unlicense" or "Report" or "Delete" -->
   <!-- Valid actions: authXXXXXMissingUsers    ="Add"       or "Report"             -->
   <!-- Valid actions: authXXXXXExistingUsers   ="Modify"    or "Report"             -->
   <!-- Valid roles: Creator, Explorer, ExplorerCanPublish, SiteAdministratorExplorer, SiteAdministratorCreator, Unlicensed, or Viewer -->
   <SiteMembership authSamlUnexpectedUsers="Unlicense" authSamlMissingUsers="Report" authSamlExistingUsers="Report"     authDefaultUnexpectedUsers="Report" authDefaultMissingUsers="Add" authDefaultExistingUsers="Report"      authOpenIdUnexpectedUsers="Unlicense" authOpenIdMissingUsers="Report" authOpenIdExistingUsers="Report">
       <User name="xxxxxPERSON+0xxxxxx@xxxxDOMAINxxxx.com"   role="SiteAdministratorExplorer" auth="serverDefault" />
       <User name="xxxxxPERSON+1xxxxxx@xxxxDOMAINxxxx.com"   role="Creator"                   auth="serverDefault" />
       <User name="xxxxxPERSON+2xxxxxx@xxxxDOMAINxxxx.com"   role="Viewer"                    auth="serverDefault" />
 
       <!-- RECOMMENDED: To support Tableau "Grant License on Sign In" you can specify the attribute allowPromotedRole="true". This indicates that if the user already has a Role greater than the one specified in this site membership list, then keep the higher role -->
       <!-- allowPromotedRole="true" is particularly useful when BULK ADDING users as "Unlicensed" and using Tableau's Grant License on Sign in functionality to assign a default role to members of a Tableau Site Group-->
       <!-- All of these users will become potential users for your site. If/when they sign in they will get upgraded from "Unlicensed" to "Explorer" or "Viewer"-->
       <!-- More info: https://help.tableau.com/current/online/en-us/grant_role.htm -->
      <User name="xxxxxPERSON+3xxxxxx@xxxxDOMAINxxxx.com"   role="Unlicensed"    allowPromotedRole="True"  auth="serverDefault" />
   </SiteMembership>

   <!-- A list of all the groups who's member members we want to audit-->
   <GroupsMemberships unexpectedGroupMembers="Delete" missingGroupMembers="Add">
      <!-- RECOMMENDED: Most groups should set grantLicenseMode="onLogin" and grantLicenseMinimumSiteRole="Viewer" or "Explorer"      -->
      <!--              This will facilitate auto-provisioning for users in these groups; as they sign in they will be given licenses -->

      <!-- Valid grantLicenseMode values:                                                                                                 -->
      <!--   grantLicenseMode="ignore"  : do nothing (default)                                                                            -->
      <!--   grantLicenseMode="none"    : REMOVE Grant Licenense on Login for the group                                                   -->
      <!--   grantLicenseMode="onLogin" : (RECOMMENDED!) ENABLE Grant License on Login (requires 'grantLicenseMinimumSiteRole' to be set) -->
      <GroupMembership name="Group1" grantLicenseMode="onLogin" grantLicenseMinimumSiteRole="Viewer">
          <GroupMember name="xxxxxPERSON+1xxxxxx@xxxxDOMAINxxxx.com" />
      </GroupMembership>

      <GroupMembership name="Group2"  grantLicenseMode="onLogin" grantLicenseMinimumSiteRole="Explorer">
          <GroupMember name="xxxxxPERSON+1xxxxxx@xxxxDOMAINxxxx.com" />
          <GroupMember name="xxxxxPERSON+2xxxxxx@xxxxDOMAINxxxx.com" />
      </GroupMembership>
   </GroupsMemberships>
</SiteProvisioning>
```

## You will need to create an Application with ID and Secret Token in Azure AD
The XML file that contains your sign in secrets above has a section:
```xml
    <AzureAdLogin tenantId="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" clientId="xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx" secret="xxxxxxxxxxxxxx" />
```
You will need to get/create this information from your Microsoft Azure accout portal (https://portal.azure.com/ -> Tenant Properties)
1. The "tenantId" you can get from your Azure AD portal webpage
2. For the "clientId", you need to create a new "Application" and specify what permissions it needs  
(DO THIS CAREFULLY! - IT NEEDS TO BE CORRECT)  
    i. In the Azure Portal: Go to "App Registrations"  
    ii. Choose "New Registration" and create a new application.  When done, you can copy/paste the "Application (client ID)") from here     
    iii. Set the required access permissions for the new application you just registered.
      - In the Application choose "API Permissions" and "Add a permission"
      - Choose "Microsoft Graph" -> "Application Permissions" -> "Directory" and check "Directory.Read.All (Read directory data)"  
      
    iv. Grant your Azure AD instance consent to use your new application.
      - In your Application config (see above), chose "API Permissions"  You will see an alert on this screen that says "(!) Not granted for Default..."
      - Click on the button at the top of the list that says, "Grant admin consent for Default Directory"  (THIS IS IMPORTANT!)  
      
    v. Generate a Azure secret that your application can use to sign into Azure with
      - In your application config (see above), choose "Certificates and secrets" and click "New client secret"
      - Choose the secret lifetime expiration (e.g. 1 year or never)
      - Copy/paste the secret from here.  
      
    vi. Take the Tenant Id, the Client Id (Application Id), and the Secret, and copy/paste them into your local Secrets.XML file.

That's it. You now have a client applicaiton ID registered in Azure AD with the necessary permissions, and the secret needed for TabProvision.exe to sign in to Azure as this application.

## Some advanced TabProvision capabilities
1. TabProvision.exe produces an output.csv file documenting all changes made to the Online site
2. Config XML file has toggles for simulating changes so the customer can see the changes before running them (set them all to "Report")
3. Flexible pipeline: After reading from Azure AD and processing, an intermediary XML file is generated to drive the Online provisioning.  This file can easily be adapted/hand-generated for use with other sources of Groups/Identity, or even just file-based provisioning definitions for customers.
4. Flexible identity/group options: Each group can be configured to represent either SAML (SSO) users, or ServerDefault (TableauID).  Source groups in AzureAD can be combined into a single target group. Groups within Groups inside AzureAD are handled

## Safety tips 
The REST APIs used by this application allow you to upload, download, and otherwise modify your site’s content, workbooks, data sources, content tags, etc. So yes, it is certainly possible for you to modify existing users/roles/authentication/groups on the Online/Server site. A few tips:
-	First run the system with all changes set to "Report" (e.g. instead of "Add", "Delete" or "Unlicense"). This can be configured in your XML attributes.  Doing this will generate a report (in a CSV file) that you can inspect before running and making changes. 

## Getting started with TabProvision (for developers)
Source code: The project is written in C# and should load into Visual Studio 2019 or newer.             

### What’s particularly useful in the source code? 
The code demonstrates complex aspects of both the REST API. Someone working with the code will have a great base for calling any Tableau REST APIs from C#.

The source code also contains example files in a “Secrets” subdirectory and the "ExampleConfigs" subdirectory
- Secrets\template.xml : Shows how to specify log-in information for Tableau Online (or Tableau Server) and Azure AD
- ExampleConfigs\AzureAD_SyncConfigExample.xml : Shows how to specify Groups that you want to synchronize from Azure AD
- FileSystem_SyncConfigExample.xml : Shows how to explicitly specify Groups and Users in a local file that are then provisioned in Tableau Online (or Tableau Server)
- The XML attribute allowPromotedRole="true" (used in both the Azure AD and File System XML examples) is very useful in conjunction with Tableau Online and Server's "Grant License on Sign In". Users (and Azure AD Groups) imported with this setting can take advantage of being members of Tableau Groups that specify a MINIMUM SITE ROLE for group members. This is a great way to bulk add a potentially large number of Unlicensed users, and have these users be granted licensing roles when they first sign in. https://help.tableau.com/current/online/en-us/grant_role.htm
- There is support for using wildcards ("starts with") pattern matching for Azure AD Group names. You can see this in the Azure AD XML, looking at the sourceGroupMatch="startswith" attribute in the "SynchronizeRole" XML node, and also the "SynchronizeMatchedGroup" XML node. Using pattern matching on group names can simplify your provisioning instructions.
- Support for Azure AD 'proxyaddresses' - In some cases when integrating with legacy on-premises directories, the Azure AD user principal name is not the user's email address. In these cases there is an XML attribute in the Azure AD config (above) that can be set to <SynchronizeConfiguration userEmailLookup="PreferAzureProxyPrimaryEmail"> to look up the email address in the Azure AD user proxy address records.
- Create Provisioning Manifest from current Online site - This option signs into your Online site, downloads the list of Users and Groups and creates a local provisioning manifest XML file. 

## Is TabProvision supported? 
Community supported. Using it you can accidentally modify or delete your content, just as you can by accidentally do so in the user interface. Despite efforts to write good and useful code there may be bugs that cause unexpected and undesirable behavior. The software is strictly “use at your own risk.”

The good news: This is intended to be a self-service tool. You are free to modify it in any way to meet your needs.

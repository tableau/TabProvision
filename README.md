# TabProvision
[![Community Supported](https://img.shields.io/badge/Support%20Level-Community%20Supported-457387.svg)](https://www.tableau.com/support-levels-it-and-developer-tools)

## What is TabProvision?
Many Tableau Online customers need to user their organization's Azure AD directory to manage their Tableau Online deployment. 
This sample shows how to use Azure AD Groups (or groups defined explicitly in a local file) to provision Administrators/Creators/Explorers/Viewers in their Tableau Online site. 
The sample code can also be adapted to read Group membership information from any other source and to use that information to provision the users/roles/groups in a Tableau Online (or Tableau Server) site.

Some background information: Tableau Online already supports using SAML to authenticate users from Microsoft Azure (or other SAML supporting Identity Providers, IdPs). 
However, these  users need to be provisioned; they need to be added to the Tableau Online site, have their Role in the site set (e.g. "Creator", "Explorer", "Viewer") and have their authentication mechanism specified. This tool does that.

This is done in 2 steps:
1. Setup: You defines (in a local XML file) (i) Azure AD groups to map to Tableau Online roles, (ii) Groups to sync to Tableau Online, (iii) Any service or external accounts
2. Execution: You run TabProvision.exe and it:
    i. Logs into your Azure AD (using credentials you provide) and pulls down the membership information of the groups you have specified.
    ii. Logs into your Tableau Online or Server site (using credentials you provide) and pushes up the Users/Roles/Authentication you wish to provision, as well as any Group membership you wish to provision
    iii. Unexpected users in the Online Site or in the Site's Group memberships can either be reported in an output report, or unlicensed/removed.

TabProvision.exe can be run as frequently as you need to keep Azure AD and you Tableau Online / Server site in sync (e.g. daily/weekly/monthly)


## Versions of Tableau Online (and Tableau Server) 
TabProvision was written and tested with Tableau Online for the 2020.3 release
- It should work in all Tableau Server versions >= 2020.3  


## Getting started with TabProvision (for non-developers)
You do not need to download and compile the source code to use TabProvision. Those interested in simply running the application can download and unzip the setup from
https://github.com/tableau/TabProvision/releases  
Running setup.exe will install the TabProvision application on your Windows machine. 

Application: The application can be run in either interactive (UI) or command line mode. When running in interactive mode the application will also show you the command line for performing all of the actions, making it easy to automate.  The application UI offers two top level options: 
   1. Provision directly from an XML manifest file. This file will explicitly contain the users/roles/groups that you want to provision to your Tableau Online/Server site
   2. Provision from Microsoft Azure AD directory. For this you will use a local XML file that indicates which Azure AD Groups to provision from

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
<SynchronizeConfiguration>

    <!-- Users in these source groups will be mapped to specific roles inside the Tableau site -->
    <!-- Valid actions: authXXXXXUnexpectedUsers ="Unlicense" or "Report" -->
    <!-- Valid actions: authXXXXXMissingUsers    ="Add"       or "Report" -->
    <!-- Valid actions: authXXXXXExistingUsers   ="Modify"    or "Report" -->
    <!-- Active Example      : <SynchronizeRoles authSamlUnexpectedUsers="Unlicense" authSamlMissingUsers="Add"    authSamlExistingUsers="Modify"     authDefaultUnexpectedUsers="Unlicense" authDefaultMissingUsers="Add"    authDefaultExistingUsers="Modify"        authOpenIdUnexpectedUsers="Report" authOpenIdMissingUsers="Report" authOpenIdExistingUsers="Report">  -->
    <!-- Reports only example: <SynchronizeRoles authSamlUnexpectedUsers="Report"    authSamlMissingUsers="Report" authSamlExistingUsers="Report"     authDefaultUnexpectedUsers="Report"    authDefaultMissingUsers="Report" authDefaultExistingUsers="Report"        authOpenIdUnexpectedUsers="Report" authOpenIdMissingUsers="Report" authOpenIdExistingUsers="Report"> --> 
    <SynchronizeRoles authSamlUnexpectedUsers="Unlicense" authSamlMissingUsers="Add" authSamlExistingUsers="Modify"   authDefaultUnexpectedUsers="Unlicense" authDefaultMissingUsers="Add" authDefaultExistingUsers="Modify"     authOpenIdUnexpectedUsers="Unlicense" authOpenIdMissingUsers="Report" authOpenIdExistingUsers="Report">  
         <SynchronizeRole sourceGroup="Tableau Online 001 Admins" targetRole="SiteAdministratorCreator" auth="serverDefault"/>
         <SynchronizeRole sourceGroup="Tableau Online 001 Creators" targetRole="Creator"  auth="serverDefault"/>

         <!-- To support Tableau "Grant License on Sign In" you can specify the attribute  allowPromotedRole="true". This indicates that if the user already has a Role greater than the one specified in this Synchronize Role Group, then keep the higher role -->
         <!-- allowPromotedRole="true" is particularly useful when BULK ADDING users as "Unlicensed" and using Tableau's Grant License on Sign in functionality to assign a default role to members of a Tableau Site Group-->
         <SynchronizeRole sourceGroup="Tableau Online 001 Explorers" targetRole="Explorer"  auth="serverDefault"  allowPromotedRole="true"/>
         <SynchronizeRole sourceGroup="Tableau Online 001 Viewers"   targetRole="Viewer"    auth="serverDefault"  allowPromotedRole="true"/>

         <!-- RECOMMENDED: Have a group that contains ALL users you want to use Grant License on Sign In for. Provision them as "Unlicensed" and set allowPromotedRole="True"-->
         <SynchronizeRole sourceGroup="Tableau Online 001 Potential Users" targetRole="Unlicensed"    auth="serverDefault"  allowPromotedRole="true"/>

         <!-- Specify any explicit user/auth/role that we want to supersede anything we find in the groups that we syncrhonize from -->
         <SiteMembershipOverrides>
              <User name="xxxxPersonxxxxx@xxxxDomainxxxxx.com"   role="SiteAdministratorExplorer" auth="serverDefault" />
         </SiteMembershipOverrides>
    </SynchronizeRoles>
  
    <!-- Users in these groups will me mapped into group membership inside the Tableau site -->
    <!-- Valid actions: unexpectedGroupMembers ="Delete" or "Report" -->
    <!-- Valid actions: missingGroupMembers    ="Add"    or "Report" -->
    <SynchronizeGroups  missingGroupMembers="Add" unexpectedGroupMembers="Delete">  
         <SynchronizeGroup sourceGroup="Biz Group - Accounting" targetGroup="Accounting Analytics" />
         <SynchronizeGroup sourceGroup="Biz Group - Marketing" targetGroup="Marketing Analytics" />

         <!-- RECOMMENDED: Use Grant License On Sign In.  To do this: -->
         <!-- 1. Have a group for ALL users (see right below) -->
         <!-- 2. In your Tableau Online site set the MINIMUM SITE ROLE for this GROUP to be "Explorer" or "Viewer" and check GRANT ROLE ON SIGN IN -->
         <!-- 3. Add this group to your TabProvision SynchronizeGroups section (see right below)-->
         <!-- 4. Add this group to your TabProvision SynchronizeRoles section (see above) with a targetRole as "Unlicensed" and allowPromotedRole="true" -->
         <!-- RESULT: All of these users will become potential users for your site. If/when they sign in they will get upgraded from "Unlicensed" to "Explorer" or "Viewer"-->
         <!-- More info: https://help.tableau.com/current/online/en-us/grant_role.htm -->
         <SynchronizeGroup sourceGroup="Tableau Online 001 Potential Users" targetGroup="Potential Users" />
    </SynchronizeGroups>

</SynchronizeConfiguration>
```

### REQUIRED FOR FILE-BASED PROVISIONING OF USERS/GROUPS: XML file with Azure AD groups
```xml
<?xml version="1.0" encoding="utf-8"?>
<SiteProvisioning>
   <!-- All the users on the site go here -->
   <!-- Valid actions: authXXXXXUnexpectedUsers ="Unlicense" or "Report" -->
   <!-- Valid actions: authXXXXXMissingUsers    ="Add"       or "Report" -->
   <!-- Valid actions: authXXXXXExistingUsers   ="Modify"    or "Report" -->
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
      <GroupMembership name="Group1">
          <GroupMember name="xxxxxPERSON+1xxxxxx@xxxxDOMAINxxxx.com" />
      </GroupMembership>

      <GroupMembership name="Group2">
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


## Is TabProvision supported? 
Community supported. Using it you can accidentally modify or delete your content, just as you can by accidentally do so in the user interface. Despite efforts to write good and useful code there may be bugs that cause unexpected and undesirable behavior. The software is strictly “use at your own risk.”

The good news: This is intended to be a self-service tool. You are free to modify it in any way to meet your needs.

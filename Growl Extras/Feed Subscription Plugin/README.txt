Growl for Windows - Subscriber Plugin SDK
--------------------------------------------------

This documentation is sparse at the moment, but the steps required to implement
a custom subscriber plugin are simple and outlined below.

SETUP AND DEVELOPMENT
--------------------------------------------------
1. Create a new Class Library project.
2. Add references to the following .dlls (all can be found in the 'libraries'
   folder in the SDK sample project):
	Growl.CoreLibrary.dll
	Growl.Connector.dll
	Growl.Destinations.dll
3. Create a new class that implements from Growl.Destinations.ISubscriptionHandler
   (see the FeedSubscriptionHandler.cs class in this SDK for a sample and
   explanations on how to implement each method). This class is what GfW uses 
   to register the plugin and to know which UI to show for each plugin type.
4. Create a new class that inherits from Growl.Destinations.Subscription.
   This is the class that does that actual work of checking for/receiving
   notifications and pushing them to GfW. The FeedSubscription.cs class in this
   SDK provides examples of how to implement the required methods.
5. Create a new class that inherits from Growl.Destinations.DestinationSettingsPanel.
   This is a WebForms control that handles the UI for configuring/editing the
   plugin. See the FeedSubscriptionSettings.cs file in this SDK for examples of
   how to implement the required methods. Take special care to make sure you 
   handle creating new instances and editing existing instances in your UI.

TESTING
--------------------------------------------------
To test your plugin, navigate to:
	%USERPROFILE%\Local Settings\Application Data\Growl\2.0.0.0\Subscribers

Create a new subfolder for your plugin and copy the compiled .dll and all 
supporting files (including the Growl.* libraries from Step 2 above).

When you run GfW the next time, GfW will scan the 'Subscribers' folder looking
for new folders. When your plugin folder is found, each assembly will be loaded
dynamically and scanned for the class that implements ISubscriptionHandler.
When that class is found, the Register() method will be called, and your plugin
will be available to the user when they click the 'Add Subscription' button on
the Network tab.

DEPLOYMENT
--------------------------------------------------
Once your plugin is complete, create a .zip file that contains the resulting
.dll and all other required files (including the Growl.* class libraries listed
in Step 2). At this point, you can either have user's download the files 
directly, manually create a folder in their User Profile location, and extract
the files, or you can create a installation manifest that GfW can use to 
automatically download and install your plugin. To create an installation 
manifest, follow the instructions below:

1. Make a copy of the 'feed.xml' file from this SDK and give it a meaningful
   name
2. Update the information in the new file, including the name of the plugin,
   author, description, etc.
3. Make sure the <packageurl> lists the location of the .zip file that contains
   all of the files required by your plugin.

To use the installation manifest, you can create a special type of url. The
format is:

	growl:subscriber*http://url.to.your/manifest.xml

If a user has GfW installed, the growl: protocol will launch GfW and then GfW
will download the XML file from the url specified. Using the information in the
XML file, GfW will ask the user if they want to install the plugin, and if so,
automatically download the .zip file and extract the contents to the correct
location in their User Profile folder. You can use the special link anywhere
you would use a normal link:

<a href="growl:subscriber*http://url.to.your/manifest.xml">Click to install plugin</a>

NOTE: Make sure your special growl: link contains the url to the manifest XML
file, not the url to the .zip file containing your plugin's files.


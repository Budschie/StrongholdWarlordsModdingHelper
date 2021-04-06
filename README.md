# StrongholdWarlordsModdingHelper
This is the repository of my stronghold modding helper.

This tool aims at supporting texture packs for the game Stronghold:Warlords by FireFly.
It utilizes ZIP-Archives as modding files. For modders, I will put the specification in the description.

![Logo](https://raw.githubusercontent.com/Budschie/StrongholdWarlordsModdingHelper/master/StrongholdWarlordsModdingHelper/Images/BudschieModdingTool.png)

## How to use as a user
Here is a rough outline on what you have to do to add a mod to Stronghold:Warlords:
1. Click on File->Open game. Then, you have to navigate to your Stronghold:Warlords directory. An example, where this directory might be, would be:
   "F:/SteamLibrary/steamapps/common/Stronghold Warlords".
   ![File->Open](https://raw.githubusercontent.com/Budschie/StrongholdWarlordsModdingHelper/master/StrongholdWarlordsModdingHelper/Images/OpenImage.png)

2. When you get the message you see below, you are good to go.
   ![The directory was verified successfully.](https://raw.githubusercontent.com/Budschie/StrongholdWarlordsModdingHelper/master/StrongholdWarlordsModdingHelper/Images/SuccessfullyOpened.png)
3. Now, let's go to the core of this project: The adding of mods. To add a mod to the mod list, you have to
   go to File->Import mod. Then, you will be prompted to open a mod file. Mod files have the file ending ".shwmod",
   which is the short form of "**S**trong**h**old:**W**arlords **Mod**".

4. Now, you have to enable the mod. Simply click on the check-box to enable the mod, as shown below.
   ![Checkbox on Column "Is Enabled"](https://raw.githubusercontent.com/Budschie/StrongholdWarlordsModdingHelper/master/StrongholdWarlordsModdingHelper/Images/Checkbox.png)
   
5. Yay, you are finally on the last step. To apply your mods, simply click on Modding->Apply mods. You will be prompted if you
   want to continue, here you can simply click "Yes".
   
5. 1/2: If you want to save your mod setup, you can go to File->Save mod configuration. You will be prompted if the save succeeded.

## I have provided a sample mod that you can find in this very repository. Here is the link:
https://github.com/Budschie/StrongholdWarlordsModdingHelper/blob/master/StrongholdWarlordsModdingHelper/TestSHWMOD.shwmod

## How to use as a modder
If you want to create a mod that is compatible with this mod loader, this guide is a good starting point.
First, I'd recommend you to use something like 7-zip, but you can in theory even use the windows explorer, but
that can be a bit nerve-taking, as you'd have to juggle around with file endings the most time. Anyways, let's get started.

First, you have to create a ZIP-Archive. For the purpose of this guide, let's call our imaginary ZIP-Archive "Test.zip".
Now, you have to put a file named "ModInfo.xml" into the root of said ZIP file. This step is mandatory, the program checks
if the zip file has this mod info, and it would refuse to load the mod file if there would be no ModInfo.xml.
The contaings of the XML file are as following:
The root tag has to be named "Mod".

Here is a list of attributes, that are all mandatory:

Attribute name | Attribute description
-------------- | ----------------------
ModDescription | This mod description will be displayed when you click on the mod. 
ModName | This is the title of the mod. It will be displayed as a header of the description.
ModId | This is used to identify the mod. It must be ***unique***, as it is used to determine if a mod is present twice. If you were to load a mod that has the same ModId as another, the program would simply refuse to do that.

A simple ModInfo.xml file would look like this:
```XML
<Mod>
     <!-- This is the metadata of the mod. It describes the mod to the user, and also contains the mod id. -->
    <ModMetadata ModDescription="This is a test mod.\n\nThis is an escape test." ModName="Test Mod" ModId="testmodbudschie"/>
   
     <!-- This tells the mod loader to add this file to the config.xml file. Remember that there should be this very file present in the root of the archive. The name should be unique, so that this file won't be overridden by another. -->
     <AdditionalDirectory path="testmodbudschie.v" />
</Mod>
```
Et Voilà, you already have the hardest part behind you. If you want to override anything from Stronghold:Warlords, your file should simply have the same path in your mod file. If there were to be a file in the assets named "IDontLikeStronghold3.dds" at "textures/stronghold3haters", you can simply create that very path with your own "IDontLikeStronghold3.dds" file at the same location in your mod file. And if you want to override a castle, you can simply do that by creating your own "castles" directory in your zip archive, then the Warlords directory, and in that, you put the file that you want to override. With that said, happy modding.

# Now, here comes a small, but still important step:
The only thing you have to do now, is to rename your mod file from "Test.zip" to (for example) "Test.shwmod". Now, you can deploy your file, and everybody
can install it on their computer :)


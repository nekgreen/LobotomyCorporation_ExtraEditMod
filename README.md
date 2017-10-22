# What is LobotomyCorporation_ExtraEditMod?
You can add Lob during the game of Lobotomy Corporation,
It is a mod that can set the order of appearance of abnormalities to arbitrary things.

Initially, I made it for easy translation confirmation.

***

### Distribution destination of ExtraEditMod
This repository exposes only the source code, and the ExtraEditMod artifacts are in the following uploader.
Also, when distributing, we also distribute Assembly-CSharp.dll, but we distribute it by permission from Project Moon of the right source.

https://ux.getuploader.com/Lobotomy_Corporation_Extra_Edit_Mod/

***
### What to prepare
* Microsoft Visual Studio
* Illasm
* ilasm
***

### Steps for creating ExtraEditMod
1. Convert Assembly-CSharp.dll to intermediate code (.il)
2. Use ILModIWriter to rewrite intermediate code so that ExtraEditMod can be used
3. Recompile the rewritten intermediate code and generate Assembly-CSharp.dll
4. Create dll of ExtraEditMod
5. Overwrite the current DLL with the created Assembly-CSharp.dll and add the dll of ExtraEditMod to the same folder
6. Start up and enjoy

***

### 1. Convert Assembly-CSharp.dll to intermediate code (.il)
Illasm is used to output intermediate codes. The output file name is Assembly-CSharp.il.
Illasm is software made by microsoft.

***

### 2. Use ILModIWriter to rewrite intermediate code so that ExtraEditMod can be used
Three places of intermediate code are being rewritten from ILModIWriter
* Added version of ExtraEditMod
* To create ExtraEditMod for hierarchy in game, add original mod init to Awake part of title scene
* Rewrite to make CreatureGenerateModel :: SetCreature operate on ExtraEditMod side

For details, please check ILModWriter.cs in source code.
***

### 3. Recompile the rewritten intermediate code and generate Assembly-CSharp.dll
Recompile using ilasm
ilasm is Microsoft's software.
I think that it was attached to .Net Framework

***
### 4. Create dll of ExtraEditMod
Before creating, add a reference to Assembly - CSharp.dll to Visual Studio.
Class files for each function of ExtraEditMod are as follows

<dl>
     <dt> ExtraEditMod.cs </ dt>
       <dd> Class that manages the overall functionality of this mod. Each function is executed from this class. </ dd>
     <dt> OrderCreature.cs </ dt>
     <dd> It is a class that manages overall functions of appearance order of abnormality </ dd>
     <dt> AddLob.cs </ dt>
     <dd> Lob This class manages the function to add points </ dd>
     <dt> EditSetting.cs </ dt>
     <dd> Current mod setting is a class to manage save load </ dd>
     <dt> EditSettingData.cs </ dt>
     <dd> Class for saving mod settings in json format </ dd>
</ dl>

***
### 5. Overwrite the current DLL with the created Assembly-CSharp.dll and add the dll of ExtraEditMod to the same folder
It's a good idea to take a backup before overwriting Assembly-CSharp.dll.

***

### 6. Start up and enjoy
that's all

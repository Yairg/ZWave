ZWave Program Controller
========================

This repository contains the code for a program that duplicates a ZWave network settings to a ZWave controller. The program also configures scenes on the controller.

### Assumptions:
*  You have a control think stick that is the network primary controller
* I tested this with Intermatic Home Settings HA07 Master controller and Intermatic Home Settings HA07 Handy remote.
* You have Control Think SDK install. (I downloaded the SDK from (http://forums.controlthink.com/t/2051.aspx)


### Configuration File Structure
The program reads network configuration from a config file. This file is JSON formatted and contains the follwoing Syntax:
#### Devices
A mapping between node id and device names. The syntax for every device is:

	{"Name":"<Name of the device>","NodeId":<The Node ID>}
* __Name of the device__ - Every string meaningfull name for the device
* __Node ID__ - The node Id for the device in the ZWave mesh. To determine the NodeId I used the Think Essentials and right click properties on the device

#### Schenes 
The schenes that are avialable. Format for a schene is:

	{"Id":"<Schene ID>","Name":"<Schene Name>",
	 "OffDevices":<Array Of devices that are off in the schene>,"OnDevices":<Array of devices that are on in the schene>}

* __Schene ID__ - A string ID to identify the schene should be unique and should not contain a colon
* __Schene Name__ - The name of the schene (Any string is OK)
* __Array Of devices that are off in the schene__ - An integer array of node ID for devices that should be turned off when the schene is activated 
* __Array of devices that are on in the schene__  - An integer array of node ID for devices that should be turned on when the schene is activated

Example:

	{"Id":"K2","Name":"Half Kitchen","OffDevices":[27,28],"OnDevices":[26]},


#### Controllers 
The controllers defined for the system (The controller being configured should be one of them. For every controller we define the list mapping between controller channels and scenes/ devices. The format for a controller configuration is:

	{"Name":"<Name Of the controller>","NumberOfChannels":<Number of channels>,"Channells":<Channel mapping array>}

* __Name of the controller__ - The name you will reference the controller when configuring it (This will be the second parameter to the program. This should be unique across controllers
* __Number of Channels__ - The number of channels that this controller supports (HA07 supports 12 HA09 supports 6)
* __Channel mapping array__ - An array with mapping between channel and scenes or devices. The first element in this array will be the setting for the first channel and so on. (meaning that the max size of this array is the number of channels the controller supports). 
To map a scene to a channel just state its ID 
To map a device us the following string ```d:Device Node ID: off or on``` (So ```d:32:on``` means that the channel will be put device 32 to state on)

Example:

	{"Name":"Entrance","NumberOfChannels":12,"Channells":["PR1","PR2","d:35:on","","E1","","7","8","9","10","11","12"]},


### Command line arguments:
	DuplicateZwaveNetwork.Exe <Config File Path> <Controller Name>

* __Config File Path__ - The path to the configuration file.
* __Controller Name__ - The name of the controller configuration to use when copying the network to the controller

Make sure to put the controller in "Receive Network Configuration" state to do so on HA07 
* Press and hold the INCLUDE Button for 5 seconds. COPY will flash.
* Release the INCLUDE button.
* Press and release the channel 1 OFF/DIM. The display will show "RA" which means "Receive All Information"

On the HA09 the steps are the same but the lights will flash instead of the display.


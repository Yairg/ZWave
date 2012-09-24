ZWave Program Controller
========================

This repository contains the code for a program that duplicates a ZWave network settings to a ZWave controller. The program also configures scenes on the controller.

### Assumptions:
*  You have a control think stick that is the network primary controller
* I tested this with Intermatic Home Settings HA07 Master controller and Intermatic Home Settings HA07 Handy remote.
* You have Control Think SDK install. (I downloaded the SDK from (http://forums.controlthink.com/t/2051.aspx)


### Configuration File Structure
The program reads network configuration from a config file. This file is JSON formatted and contains the follwoing Syntax:
* Devices - A mapping between node id and device names. The syntax for every device is:
	{\"Name\":\"<Name of the device>\",\"NodeId\":<The Node ID>}
	** <Name of the device> - Every string meaningfull name for the device
	** <Node ID> - The node Id for the device in the ZWave mesh. To determine the NodeId I used the Think Essentials and right click properties on the device

* Schenes - The schenes that are avialable. Format for a schene is:

{\"Id\":\"<Schene ID>\",\"Name\":\"<Schene Name>\",\"OffDevices\":<Array Of devices that are off in the schene>,\"OnDevices\":<Array of devices that are on in the schene>}

** <Schene ID> - A string ID to identify the schene should be unique and should not contain a colon
** <Schene Name> - The name of the schene (Any string is OK)
** <Array Of devices that are off in the schene> - An integer array of node ID for devices that should be turned off when the schene is activated
** <Array of devices that are on in the schene> -  - An integer array of node ID for devices that should be turned on when the schene is activated");
** <Array of devices that are on in the schene> -  - An integer array of node ID for devices that should be turned on when the schene is activated");

* Controllers - The controller defined 



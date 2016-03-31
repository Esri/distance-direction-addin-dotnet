# distance-direction-addin-dotnet

The add-in provides the ability to create geodesic features such as lines, circles, ellipses and range rings.  Features can be exported to a file geodatabase, shapefile, or KML.

![Image of Distance and Direction Addin](DistanceAndDirection.PNG) 

## Features

* Creates geodesy lines, circles, ellipses and range rings.
* Inputs can be entered manually or via a known coordinate.
* Features can be exported to a file geodatabase, shapefile, or KML.
* Addin for ArcMap 10.3.1

## Sections

* [Requirements](#requirements)
* [Instructions](#instructions)
* [Workflows](#workflows)
* [Resources](#resources)
* [Issues](#issues)
* [Contributing](#contributing)
* [Licensing](#licensing)

## Requirements

### Developers 

* Visual Studio 2013
* ArcGIS Desktop SDK for .NET 10.3.1
	* [ArcGIS Desktop for .NET Requirements](https://desktop.arcgis.com/en/desktop/latest/get-started/system-requirements/arcobjects-sdk-system-requirements.htm)

### ArcGIS for Desktop Users

* [ArcGIS Desktop 10.3.1](http://desktop.arcgis.com/en/arcmap/10.3/get-started/system-requirements/arcgis-desktop-system-requirements.htm)

## Instructions

###New to Github

* [New to Github? Get started here.](http://htmlpreview.github.com/?https://github.com/Esri/esri.github.com/blob/master/help/esri-getting-to-know-github.html)

### Working with the Add-In

## Development Environment 

* Building
	* To Build Using Visual Studio
		* Open and build solution file
	* To use MSBuild to build the solution
		* Open a Visual Studio Command Prompt: Start Menu | Visual Studio 2013 | Visual Studio Tools | Developer Command Prompt for VS2013
		* ``` cd distance-and-direction-addin-dotnet\source\DistanceAndDirection\ArcMapAddinDistanceAndDirection ```
		* ``` msbuild ArcMapAddinDistanceAndDirection.sln /property:Configuration=Release ```
	* Note : Assembly references are based on a default install of the SDK, you may have to update the references if you chose an alternate install option

## Desktop Users
* Running the add-in
	* To run from a stand-alone deployment
		* ArcMap
			* Install the add-in from the application folder by double clicking the **ArcMapAddinDistanceAndDirection.esriAddIn** file.
			* Add the add-in command to a toolbar via menu option 
				* **Customize** -> **Customize mode**
				* Select **Commands** Tab
				* Select **Add-In Controls**
				* Drag/Drop **Show Distance and Direction** command onto a toolbar
				* Close customize mode
				* Open tool by clicking the **Show Distance and Direction** command you just added
				* Dockable *Distance and Direction* tool appears
				* If you add this to a toolbar that you contstantly use the add-in will stay. To remove the add-in delete your [Normal.MXT](https://geonet.esri.com/thread/78692) file
				
## Workflows

### Create Lines Interactively 
1. Choose the **Lines** tab on the *Distance and Direction* Tool
2. Choose the type of line that is needed to be created
3. Start an interactive session by selecting the **Map Point Tool** (arrow icon) 
4. Enter a starting and ending point on the map by clicking on the map
5. Repeat until all desired graphics have been included
6. Optional - Select the Save As button to export the Line features to a file geodatabase, shapefile, or KML

### Create Lines from Known Coordinates
1. Choose the type of line that is needed to be created
2. Input the **Starting Point** where your line is going to start
3. Input the **Ending Point** of where your line is going to end
4. Press *Enter* key and the graphic will be drawn on the map
5. Repeat until all desired graphics have been included
6. Optional - Select the Save As button to export the Line features to a file geodatabase, shapefile, or KML

### Create a Line with a Range and Bearing
1. Choose the type of line that you would like to create
2. Choose **Distance and Bearing** from the second drop down (*From*) menu
3. Input the **Distance/Length** of the line and choose the unit type
4. Input the azimuth or **Angle** of the line
5. Press *Enter* key and the graphic will be drawn on the map
6. Optional - Select the Save As button to export the Line features to a file geodatabase, shapefile, or KML

### Create a Circle Interactively 
1. Choose the **Circles** tab on the *Distance and Direction* Tool
2. Start an interactive session by selecting the **Map Point Tool** (arrow icon) 
3. Click on map to enter a centerpoint and then move the cursor out from center, a circle displays on map as you move the cursor.  
4. Click map to complete the circle. 
5. Optional - Select the Save As button to export the Circle features to a file geodatabase, shapefile, or KML

### Create a Circle using the Distance Calculator
1. Choose the **Circles** tab on the *Distance and Direction* Tool
2. Start an interactive session by selecting the arrow icon 
3. Enter a starting point and the distance of the circles radius by clicking on the map
4. Expand the **Distance Calculator** section
5. Enter a **Time**
6. Enter a **Rate**
7. Press *Enter* key and the graphic will be drawn on the map
8. Optional - Select the Save As button to export the Circle features to a file geodatabase, shapefile, or KML

### Create Ellipses Interactively
1. Choose the **Ellipse** tab on the *Distance and Direction* Tool
2. Start an interactive session by selecting the arrow icon next to the **Center Point** text box
3. Choose the location where you want the ellipse to be started from 
4. Drag the cursor to the location where the major axis will end
5. Select the orientation angle of the major axis
6. Select the length of the minor axis
7. Repeat until all desired graphics have been included
8. Optional - Select the Save As button to export the Ellipse features to a file geodatabase, shapefile, or KML

### Create Range Rings Interactively 
1. Choose the **Rings** tab on the *Distance and Direction* Tool
2. Check **Interactive**
3. Select the **Map Point Tool** (arrow icon) next to the **Center Point** text box
4. *First* click on the map sets the center point (green dot)
5. Subsequent clicks draw rings from the center point
6. Select the **Map Point Tool** a second time to stop adding rings
	* To start a second set of range rings, repeat steps 3 through 6.
7. Optional - Select the Save As button to export the Range Ring features to a file geodatabase, shapefile, or KML
8. Uncheck **Interactive** to return to manual entry

### Create Range Rings Manually 
1. Choose the **Rings** tab on the *Distance and Direction* Tool
2. Select the **Map Point Tool** (arrow icon) next to the **Center Point** text box
	* Optionally copy and paste, or type coordinates in the **Center Point** text box
3. Choose the location of the Range Rings center by selecting the desired location on the map
4. Fill in the associated parameters for **Number of Rings**, **Ring Interval**, **Distance Units**, and **Number of Radials**
5. Press *Enter* key and the graphic will be drawn on the map
6. Repeat until all desired graphics have been included
7. Optional - Select the Save As button to export the Range Ring features to a file geodatabase, shapefile, or KML

## Resources

* [ArcGIS 10.3 Help](http://resources.arcgis.com/en/help/)
* [ArcGIS Blog](http://blogs.esri.com/esri/arcgis/)
* ![Twitter](https://g.twimg.com/twitter-bird-16x16.png)[@EsriDefense](http://twitter.com/EsriDefense)
* [ArcGIS Solutions Website](http://solutions.arcgis.com/military/)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an [issue](https://github.com/ArcGIS/distance-and-direction-addin-dotnet/issues).

## Contributing

Anyone and everyone is welcome to contribute. Please see our [guidelines for contributing](https://github.com/esri/contributing).

### Repository Points of Contact

#### Repository Owner: [Joe](https://github.com/jmccausland)

* Merge Pull Requests
* Creates Releases and Tags
* Manages Milestones
* Manages and Assigns Issues

#### Secondary: TBD

* Backup when the Owner is away

## Licensing

Copyright 2016 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt](license.txt) file.

[](Esri Tags: Military Analyst Defense ArcGIS ArcObjects .NET WPF ArcGISSolutions ArcMap ArcPro Add-In)
[](Esri Language: C#) 

# distance-and-direction-widget

This widget provides the ability to create geodetic features such as lines, circles, ellipses and range rings.

![Image of Distance and Directions Widget][ss]

## Features

* Creates geodetic lines, circles, ellipses and range rings.
* Inputs can be entered manually or via a known coordinate
* Widget for [Web AppBuilder for ArcGIS](http://doc.arcgis.com/en/web-appbuilder/)

## Sections

* [Requirements](#requirements)
* [Instructions](#instructions)
* [Workflows](#workflows)
* [Resources](#resources)
* [Issues](#issues)
* [Contributing](#contributing)
* [Licensing](#licensing)
*

## Requirements

* Web Appbuilder Version 1.3 December 2015
* [ArcGIS Web Appbuilder for ArcGIS](http://developers.arcgis.com/web-appbuilder/)

## Instructions
Deploying Widget

Setting Up Repository for Development

In order to develop and test widgets you need to deploy the DistanceAndDirection folder to the stemapp/widgets directory in your Web AppBuilder for ArcGIS installation. If you use Github for windows this can be accomplished using the following steps.

1. Sync the repository to your local machine.
2. Open the Repository in Windows Explorer
3. Close Github for Windows
4. Cut and paste the entire DistanceAndDirection folder into the stemapp/widgets folder
5. Launch Github for Windows and choose the option to locate the repository. This will change the location on disk to the new location.


## Workflows

### Create Lines Interactively
	* Choose the Lines tab on the Distance and Directions Widget
	* Choose the type of line that is needed to be created
	* Start an interactive session by selecting the arrow icon
	* Enter a starting and ending point on the map by clicking on the map
	* Repeat until all desired graphics have been included

### Create Lines Manually
	* Choose the type of line that is needed to be created
	* Input the first coordinate of where your line is going to start
	* Input the second coordinate of where your line is going to end
	* Press "Enter" key and the graphic will be drawn on the map
	* Repeat until all desired graphics have been included.

### Create a Line with a Range and Bearing
	* Choose the type of line that you would like to create
	* Choose Bearing and Distance from the second drop down menu
	* Input the length of the line and choose the unit type
	* Input the azimuth or angle of the line
	* Press "Enter" key and the graphic will be drawn on the map

### Create a Circle Interactively
	* Choose the Circle tab on the Distance and Direction Widget
	* Choose the type of circle you will create from in the ‘Create Circle From’ drop down list.
	* Start an interactive session by selecting the ‘Map Point’ icon
	* Click on the map to create a starting (center) point. Drag the widget to create a radius for the circle.  
	* A graphic will then be displayed on the map showing the circle you created
      Note: The ‘Center Point’ and ‘Radius/Diameter’ will update based on parameters from newly created circle.
	* If desired you can clear all graphics with the clear graphics button

### Create a Circle manually
	* Choose the Circle tab on the Distance and Direction Widget
	* Choose the type of circle you will create from in the ‘Create Circle From’ drop down list.
	* Enter a coordinate into the **Center Point text** box
	* Optionally change the units using the **Radius** dropdown box
	* Enter the desired Radius
	* Press the **Enter** key
	* A graphic will then be displayed on the map showing the circle you created
	* If desired you can clear all graphics with the clear graphics button

### Create a Circle using the Distance Calculator Interactively
	* Choose the Circle tab on the Distance and Direction Widget
	* Expand the Distance Calculator section
	* Enter a Time
	* Enter a Rate
	* Start an interactive session by selecting the ‘Map Point’ icon
	* Click on the map to create a starting (center) point.
	* A graphic will be displayed using the calculated distance and the clicked point

### Create a Circle using the Distance Calculator manually
	* Choose the Circle tab on the Distance and Direction Widget
	* Choose the type of circle you will create from in the ‘Create Circle From’ drop down list.
	* Enter a coordinate into the **Center Point text** box
	* Expand the Distance Calculator section
	* Optionally change the time units using the **Time** dropdown box
	* Enter a Time
	* Optionally change the Rate units using the **Rate** dropdown box
	* Enter a Rate
	* Press the **Enter** key
	* A graphic will then be displayed on the map showing the circle you created

### Create Ellipses Interactively
	* Choose the Ellipse tab on the Distance and Directions Widget
	* Start an interactive session by selecting the arrow icon next to the “Center Point” text box
	* Choose the location where you want the ellipse to be started from
	* Drag the cursor to the location where the major axis will end and click to set (whilst dragging the major axis will draw in both directions from the "center point")
	* Drag the mouse back towards the "center point" to resize the minor axis (the tool will not allow the minor axis to be greater that the major)
	* Graphic(s) will then be displayed on the map showing the Ellipse you created based on the values of the parameters that were set

### Create Range Rings Manually
	* Choose the Range Rings tab on the Distance and Directions Widget
	* Input the coordinates for the Range Rings center in the “Center Point” text box
	* Fill in the associated parameters for Number of Rings, Ring Interval, Distance Units
	* Fill in the parameter for Number of Radials and press the Enter Key
	* Graphic(s) will then be displayed on the map showing the Range Rings you created based on the values of the parameters that were set
	* If desired you can clear all graphics with the clear graphics button

### Create Range Rings Interactively
	* Choose the Range Rings tab on the Distance and Directions Widget
	* Check the "Interactive" check box
	* Click the "Map Point" tool
	* Fill in the parameter for "Number of Radials"
	* Start clicking on the map to create range rings
	* Double-click to finish creating range rings
	* Graphic(s) will then be displayed on the map showing range rings and radials
	* If desired you can clear all graphics with the clear graphics button	 

## General Help

  * [New to Github? Get started here.](http://htmlpreview.github.com/?https://github.com/Esri/esri.github.com/blob/master/help/esri-getting-to-know-github.html)

## Resources

  * [Web AppBuilder API](https://developers.arcgis.com/web-appbuilder/api-reference/css-framework.htm)
  * [ArcGIS API for JavaScript](https://developers.arcgis.com/javascript/)
  * [ArcGIS Blog](http://blogs.esri.com/esri/arcgis/)
  * [ArcGIS Solutions Website](http://solutions.arcgis.com/military/)

  ![Twitter](https://g.twimg.com/twitter-bird-16x16.png)[@EsriDefense](http://twitter.com/EsriDefense)

## Issues

  Find a bug or want to request a new feature?  Please let us know by submitting an [issue](https://github.com/Esri/solutions-webappbuilder-widgets/issues).

## Contributing

  Esri welcomes contributions from anyone and everyone. Please see our [guidelines for contributing](https://github.com/esri/contributing).

  If you are using [JS Hint](http://www.jshint.com/) there is a .jshintrc file included in the root folder which enforces this style.
  We allow for 120 characters per line instead of the highly restrictive 80.

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

  [ss]: images/screenshot.png
  [](Esri Tags: Military Analyst Defense ArcGIS Widget Web AppBuilder ArcGISSolutions)
  [](Esri Language: Javascript)

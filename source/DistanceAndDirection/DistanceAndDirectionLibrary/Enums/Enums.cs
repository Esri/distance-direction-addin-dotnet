// Copyright 2016 Esri 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using DistanceAndDirectionLibrary.Properties;

namespace DistanceAndDirectionLibrary
{
    // Here we create all of our globalized enumerations

    public enum LineTypes : int
    {
        [LocalizableDescription(@"EnumGeodesic", typeof(Resources))]
        Geodesic = 1,

        [LocalizableDescription(@"EnumGreatElliptic", typeof(Resources))]
        GreatElliptic = 2,

        [LocalizableDescription(@"EnumLoxodrome", typeof(Resources))]
        Loxodrome = 3
    }

    public enum LineFromTypes : int 
    {
        [LocalizableDescription(@"EnumPoints", typeof(Resources))]
        Points = 1,

        [LocalizableDescription(@"EnumBearingAndDistance", typeof(Resources))]
        BearingAndDistance = 2
    }

    public enum DistanceTypes : int 
    {
        [LocalizableDescription(@"EnumFeet", typeof(Resources))]
        Feet = 1,
        
        [LocalizableDescription(@"EnumKilometers", typeof(Resources))]
        Kilometers = 2,
        
        [LocalizableDescription(@"EnumMeters", typeof(Resources))]
        Meters = 3,

        [LocalizableDescription(@"EnumMiles", typeof(Resources))]
        Miles = 4,
        
        [LocalizableDescription(@"EnumNauticalMile", typeof(Resources))]
        NauticalMile = 5,

        [LocalizableDescription(@"EnumYards", typeof(Resources))]
        Yards = 6
    }

    public enum RateTimeTypes : int
    {
        [LocalizableDescription(@"EnumMilesSec", typeof(Resources))]
        MilesSec = 1,

        [LocalizableDescription(@"EnumMilesHour", typeof(Resources))]
        MilesHour = 2,

        [LocalizableDescription(@"EnumMetersSec", typeof(Resources))]
        MetersSec = 3,

        [LocalizableDescription(@"EnumMetersHour", typeof(Resources))]
        MetersHour = 4,

        [LocalizableDescription(@"EnumKilometersSec", typeof(Resources))]
        KilometersSec = 5,

        [LocalizableDescription(@"EnumKilometersHour", typeof(Resources))]
        KilometersHour = 6,

        [LocalizableDescription(@"EnumFeetSec", typeof(Resources))]
        FeetSec = 7,

        [LocalizableDescription(@"EnumFeetHour", typeof(Resources))]
        FeetHour = 8,

        [LocalizableDescription(@"EnumNauticalMilesSec", typeof(Resources))]
        NauticalMilesSec = 9,

        [LocalizableDescription(@"EnumNauticalMilesHour", typeof(Resources))]
        NauticalMilesHour = 10
    }

    public enum AzimuthTypes : int
    {
        [LocalizableDescription(@"EnumDegrees", typeof(Resources))]
        Degrees = 1,

        [LocalizableDescription(@"EnumMils", typeof(Resources))]
        Mils = 2
    }

    public enum CircleFromTypes : int
    {
        [LocalizableDescription(@"EnumRadius", typeof(Resources))]
        Radius = 1,

        [LocalizableDescription(@"EnumDiameter", typeof(Resources))]
        Diameter = 2
    }

    public enum EllipseTypes : int
    {
        [LocalizableDescription(@"EnumSemi", typeof(Resources))]
        Semi = 1,

        [LocalizableDescription(@"EnumFull", typeof(Resources))]
        Full = 2
    }

    public enum TimeUnits : int
    {
        [LocalizableDescription(@"EnumSeconds", typeof(Resources))]
        Seconds = 1,

        [LocalizableDescription(@"EnumMinutes", typeof(Resources))]
        Minutes = 2,

        [LocalizableDescription(@"EnumHours", typeof(Resources))]
        Hours = 3
    }

    public enum GraphicTypes : int
    {
        Line = 1,
        Circle  = 2,
        Ellipse = 3,
        RangeRing = 4,
        Point = 5
    }

    public enum GeomType : int
    {
        PolyLine = 1,
        Polygon = 2
    }

    public enum SaveAsType : int
    {
        FileGDB = 1,
        Shapefile = 2,
        KML = 3
    }

    public enum CoordinateTypes : int
    {
        [LocalizableDescription(@"EnumCTDD", typeof(Resources))]
        DD = 1,

        [LocalizableDescription(@"EnumCTDDM", typeof(Resources))]
        DDM = 2,

        [LocalizableDescription(@"EnumCTDMS", typeof(Resources))]
        DMS = 3,
        
        //[LocalizableDescription(@"EnumCTGARS", typeof(Resources))]
        //GARS = 4,

        [LocalizableDescription(@"EnumCTMGRS", typeof(Resources))]
        MGRS = 5,

        [LocalizableDescription(@"EnumCTUSNG", typeof(Resources))]
        USNG = 6,

        [LocalizableDescription(@"EnumCTUTM", typeof(Resources))]
        UTM = 7,

        [LocalizableDescription(@"EnumCTNone", typeof(Resources))]
        None = 8

    }
}

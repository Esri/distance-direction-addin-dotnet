/******************************************************************************* 
  * Copyright 2016 Esri 
  *  
  *  Licensed under the Apache License, Version 2.0 (the "License"); 
  *  you may not use this file except in compliance with the License. 
  *  You may obtain a copy of the License at 
  *  
  *  http://www.apache.org/licenses/LICENSE-2.0 
  *   
  *   Unless required by applicable law or agreed to in writing, software 
  *   distributed under the License is distributed on an "AS IS" BASIS, 
  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
  *   See the License for the specific language governing permissions and 
  *   limitations under the License. 
  ******************************************************************************/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DistanceAndDirectionLibrary;
using ArcMapAddinDistanceAndDirection.ViewModels;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace ArcMapAddinDistanceAndDirection.Tests
{
    [TestClass]
    public class ArcMapAddinDistanceAndDirectionTests
    {

        [ClassInitialize()]
        [TestCategory("ArcMapAddin")]
        public static void MyClassInitialize(TestContext testContext)
        {
            bool blnBoundToRuntime = ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
            Assert.IsTrue(blnBoundToRuntime, "Not bound to runtime");
            IAoInitialize aoInitialize = new AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeBasic);
        }

        #region Lines View Model

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LinesViewModel_ThrowsException()
        {
            var lineVM = new LinesViewModel();

            lineVM.DistanceString = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LinesViewModel_ThrowsException2()
        {
            var lineVM = new LinesViewModel();

            lineVM.LineFromType = LineFromTypes.BearingAndDistance;

            lineVM.AzimuthString = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LinesViewModel_ThrowsException3()
        {
            var lineVM = new LinesViewModel();

            lineVM.Point1Formatted = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LinesViewModel_ThrowsException4()
        {
            var lineVM = new LinesViewModel();

            lineVM.Point2Formatted = "esri";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LinesViewModel_ThrowsException5()
        {
            var lineVM = new LinesViewModel();

            lineVM.Distance = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LinesViewModel_ThrowsException6()
        {
            var lineVM = new LinesViewModel();

            lineVM.Azimuth = -1;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LinesViewModel_ThrowsException7()
        {
            var lineVM = new LinesViewModel();

            lineVM.Azimuth = 361;
        }


        [TestMethod]
        public void LineViewModel()
        {
            var lineVM = new LinesViewModel();

            // can we create an element
            Assert.IsFalse(lineVM.CanCreateElement);
            lineVM.LineAzimuthType = DistanceAndDirectionLibrary.AzimuthTypes.Degrees;
            lineVM.Azimuth = 90.1;
            Assert.AreEqual("90.1", lineVM.AzimuthString);
            lineVM.Azimuth = 90.0;
            Assert.AreEqual("90", lineVM.AzimuthString);
            lineVM.LineAzimuthType = DistanceAndDirectionLibrary.AzimuthTypes.Mils;
            Assert.AreEqual(1600.00000002, lineVM.Azimuth);

            // test points
            lineVM.Point1 = new Point() { X = -119.8, Y = 34.4 };
            lineVM.Point2 = new Point() { X = -74.1, Y = 41.8 };
            // can we create an element
            Assert.IsTrue(lineVM.CanCreateElement);

            // can't test manual input of of starting and ending points
            // they call methods that reference the ArcMap Application/Document objects
            // which is not available in unit testing

            // test Distance and Bearing mode

            // manual input of azimuth
            lineVM.LineAzimuthType = DistanceAndDirectionLibrary.AzimuthTypes.Degrees;
            lineVM.LineFromType = DistanceAndDirectionLibrary.LineFromTypes.BearingAndDistance;
            lineVM.AzimuthString = "90.1";
            Assert.AreEqual(90.1, lineVM.Azimuth);
            // manual input of distance
            lineVM.LineDistanceType = DistanceAndDirectionLibrary.DistanceTypes.Meters;
            lineVM.DistanceString = "50.5";
            Assert.AreEqual(50.5, lineVM.Distance);
            lineVM.LineDistanceType = DistanceAndDirectionLibrary.DistanceTypes.Miles;
            Assert.AreEqual(50.5, lineVM.Distance);

            // Check TrimPrecision is trimming correctly according to LineDistanceType
            lineVM.LineDistanceType = DistanceTypes.Kilometers;
            lineVM.Distance = 1.012345;
            Assert.AreEqual(1.0123, lineVM.Distance);

            lineVM.LineDistanceType = DistanceTypes.Meters;
            lineVM.Distance = 1.12;
            Assert.AreEqual(1.1, lineVM.Distance);
        }


        #endregion Lines View Model

        #region Circle View Model

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException()
        {
            var circleVM = new CircleViewModel();

            circleVM.DistanceString = "esri";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException2()
        {
            var circleVM = new CircleViewModel();

            circleVM.Point1Formatted = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException3()
        {
            var circleVM = new CircleViewModel();

            circleVM.TravelTime = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException4()
        {
            var circleVM = new CircleViewModel();

            circleVM.TravelRate = -1;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException5()
        {
            var circleVM = new CircleViewModel();

            circleVM.TimeUnit = TimeUnits.Hours;
            circleVM.RateTimeUnit = RateTimeTypes.MetersHour;
            circleVM.TravelTime = 1;
            circleVM.TravelRate = 20000000;
            circleVM.RateUnit = DistanceTypes.Miles;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException6()
        {
            var circleVM = new CircleViewModel();

            circleVM.TimeUnit = TimeUnits.Seconds;
            circleVM.RateTimeUnit = RateTimeTypes.MetersSec;
            circleVM.TravelTime = 1;
            circleVM.TravelRate = 20000000;
            circleVM.TimeUnit = TimeUnits.Hours;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException7()
        {
            var circleVM = new CircleViewModel();

            circleVM.TimeUnit = TimeUnits.Hours;
            circleVM.RateTimeUnit = RateTimeTypes.MetersHour;
            circleVM.TravelTime = 1;
            circleVM.TravelRate = 20000001;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException8()
        {
            var circleVM = new CircleViewModel();

            circleVM.TimeUnit = TimeUnits.Hours;
            circleVM.RateTimeUnit = RateTimeTypes.MetersHour;
            circleVM.TravelTime = 2;
            circleVM.TravelRate = 10000001;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException9()
        {
            var circleVM = new CircleViewModel();

            circleVM.Point1Formatted = "0 0";
            circleVM.DistanceString = "20000001";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException10()
        {
            var circleVM = new CircleViewModel();

            circleVM.Point1Formatted = "0 181";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CircleViewModel_ThrowsException11()
        {
            var circleVM = new CircleViewModel();

            circleVM.Point1Formatted = "91 0";
        }

        [TestMethod]
        public void CircleViewModel()
        {
            var circleVM = new CircleViewModel();

            // can we create an element
            Assert.IsFalse(circleVM.CanCreateElement);

            circleVM.Distance = 1000.0;

            // test points
            circleVM.Point1 = new Point() { X = -119.8, Y = 34.4 };

            Assert.AreEqual(circleVM.Point1Formatted, "34.4 -119.8");

            // Check that Distance is not converted when LineDistanceType is changed
            // #260
            circleVM.LineDistanceType = DistanceTypes.Meters;
            circleVM.Distance = 1000.0;
            circleVM.LineDistanceType = DistanceTypes.Kilometers;
            Assert.AreEqual(circleVM.Distance, 1000.0);

            circleVM.CircleType = CircleFromTypes.Diameter;
            Assert.AreEqual(circleVM.DistanceString, "2000");
        }

        #endregion Circle View Model

        #region Ellipse View Model

        [TestMethod]
        public void EllipseViewModel()
        {
            var ellipseVM = new EllipseViewModel();

            // can we create an element
            Assert.IsFalse(ellipseVM.CanCreateElement);

            ellipseVM.Distance = 1000.0;

            // test points
            ellipseVM.Point1 = new Point() { X = -119.8, Y = 34.4 };
            // can we create an element
            //Assert.IsTrue(circleVM.CanCreateElement);

            Assert.AreEqual(ellipseVM.Point1Formatted, "34.4 -119.8");

            // can't test manual input of of starting and ending points
            // they call methods that reference the ArcMap Application/Document objects
            // which is not available in unit testing

            // manual input of azimuth
            ellipseVM.AzimuthType = DistanceAndDirectionLibrary.AzimuthTypes.Degrees;
            ellipseVM.AzimuthString = "90.1";
            Assert.AreEqual(90.1, ellipseVM.Azimuth);
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EllipseViewModel_ThrowsException()
        {
            var ellipseVM = new EllipseViewModel();

            ellipseVM.MajorAxisDistanceString = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EllipseViewModel_ThrowsException2()
        {
            var ellipseVM = new EllipseViewModel();

            ellipseVM.MinorAxisDistanceString = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EllipseViewModel_ThrowsException3()
        {
            var ellipseVM = new EllipseViewModel();

            ellipseVM.AzimuthString = "esri";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EllipseViewModel_ThrowsException4()
        {
            var ellipseVM = new EllipseViewModel();

            ellipseVM.MajorAxisDistance = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EllipseViewModel_ThrowsException5()
        {
            var ellipseVM = new EllipseViewModel();

            ellipseVM.MinorAxisDistance = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EllipseViewModel_ThrowsException6()
        {
            var ellipseVM = new EllipseViewModel();

            ellipseVM.Azimuth = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EllipseViewModel_ThrowsException7()
        {
            var ellipseVM = new EllipseViewModel();

            ellipseVM.MajorAxisDistance = ellipseVM.MajorAxisLimit + 1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void EllipseViewModel_ThrowsException8()
        {
            var ellipseVM = new EllipseViewModel();

            ellipseVM.LineDistanceType = DistanceTypes.Meters;
            ellipseVM.MajorAxisDistance = ellipseVM.MajorAxisLimit;
            ellipseVM.LineDistanceType = DistanceTypes.Miles;
        }

        #endregion Ellipse View Model

        #region Range View Model
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RangeViewModel_ThrowsException()
        {
            var rangeVM = new RangeViewModel();

            rangeVM.NumberOfRings = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RangeViewModel_ThrowsException2()
        {
            var rangeVM = new RangeViewModel();

            rangeVM.NumberOfRadials = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RangeViewModel_ThrowsException3()
        {
            var rangeVM = new RangeViewModel();

            rangeVM.Distance = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RangeViewModel_ThrowsException4()
        {
            var rangeVM = new RangeViewModel();

            rangeVM.DistanceString = "esri";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RangeViewModel_ThrowsException5()
        {
            var rangeVM = new RangeViewModel();

            rangeVM.Distance = 20000001;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RangeViewModel_ThrowsException6()
        {
            var rangeVM = new RangeViewModel();

            rangeVM.LineDistanceType = DistanceTypes.Meters;
            rangeVM.Distance = 20000000;
            rangeVM.LineDistanceType = DistanceTypes.Miles;
        }

        [TestMethod]
        public void RangeViewModel()
        {
            var rangeVM = new RangeViewModel();

            // can we create an element
            Assert.IsFalse(rangeVM.CanCreateElement);

            rangeVM.Distance = 1000.0;

            // test points
            rangeVM.Point1 = new Point() { X = -119.8, Y = 34.4 };
            // can we create an element

            Assert.AreEqual(rangeVM.Point1Formatted, "34.4 -119.8");
        }
        #endregion Range View Model
    }
}

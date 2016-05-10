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
        [TestMethod]
        public void LineViewModel()
        {
            bool isBound = ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            if (!isBound)
                return;

            IAoInitialize aoInitialize = new AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

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

            aoInitialize.Shutdown();
        }

        [TestMethod]
        public void CircleViewModel()
        {
            bool isBound = ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            if (!isBound)
                return;

            IAoInitialize aoInitialize = new AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            var circleVM = new CircleViewModel();

            // can we create an element
            Assert.IsFalse(circleVM.CanCreateElement);

            circleVM.Distance = 1000.0;

            // test points
            circleVM.Point1 = new Point() { X = -119.8, Y = 34.4 };
            // can we create an element
            //Assert.IsTrue(circleVM.CanCreateElement);

            Assert.AreEqual(circleVM.Point1Formatted, "34.4 -119.8");

            // can't test manual input of of starting and ending points
            // they call methods that reference the ArcMap Application/Document objects
            // which is not available in unit testing

            aoInitialize.Shutdown();
        }

        [TestMethod]
        public void EllipseViewModel()
        {
            bool isBound = ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            if (!isBound)
                return;

            IAoInitialize aoInitialize = new AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

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

            aoInitialize.Shutdown();
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
        public void RangeViewModel()
        {
            bool isBound = ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.Desktop);
            if (!isBound)
                return;

            IAoInitialize aoInitialize = new AoInitializeClass();
            aoInitialize.Initialize(esriLicenseProductCode.esriLicenseProductCodeAdvanced);

            var rangeVM = new RangeViewModel();

            // can we create an element
            Assert.IsFalse(rangeVM.CanCreateElement);

            rangeVM.Distance = 1000.0;

            // test points
            rangeVM.Point1 = new Point() { X = -119.8, Y = 34.4 };
            // can we create an element
            //Assert.IsTrue(circleVM.CanCreateElement);

            Assert.AreEqual(rangeVM.Point1Formatted, "34.4 -119.8");

            // can't test manual input of of starting and ending points
            // they call methods that reference the ArcMap Application/Document objects
            // which is not available in unit testing

            aoInitialize.Shutdown();
        }
    }
}

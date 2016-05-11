using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProAppDistanceAndDirectionModule.ViewModels;
using DistanceAndDirectionLibrary;
using ArcGIS.Core;
using ArcGIS.Desktop.Framework;
using ArcGIS.Core.Geometry;
using ArcGIS.Core.Hosting;
using ArcGIS.Desktop.Mapping;

namespace ProAppDistanceAndDirectionModule.Tests
{
    [TestClass]
    public class ProAppDistanceAndDirectionModule
    {
        #region Lines View Model

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentException))]
        //public void ProLinesViewModel_ThrowsException()
        //{
        //    var lineVM = new ProLinesViewModel();

        //    lineVM.DistanceString = "esri";
        //}
        //[TestMethod]
        //[ExpectedException(typeof(ArgumentException))]
        //public void ProLinesViewModel_ThrowsException2()
        //{
        //    var lineVM = new ProLinesViewModel();

        //    lineVM.LineFromType = LineFromTypes.BearingAndDistance;

        //    lineVM.AzimuthString = "esri";
        //}
        //[TestMethod]
        //[ExpectedException(typeof(ArgumentException))]
        //public void ProLinesViewModel_ThrowsException3()
        //{
        //    var lineVM = new ProLinesViewModel();

        //    lineVM.Point1Formatted = "esri";
        //}
        //[TestMethod]
        //[ExpectedException(typeof(ArgumentException))]
        //public void ProLinesViewModel_ThrowsException4()
        //{
        //    var lineVM = new ProLinesViewModel();

        //    lineVM.Point2Formatted = "esri";
        //}

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentException))]
        //public void ProLinesViewModel_ThrowsException5()
        //{
        //    var lineVM = new ProLinesViewModel();

        //    lineVM.Distance = -1;
        //}
        //[TestMethod]
        //[ExpectedException(typeof(ArgumentException))]
        //public void ProLinesViewModel_ThrowsException6()
        //{
        //    var lineVM = new ProLinesViewModel();

        //    lineVM.Azimuth = -1;
        //}


        //[TestMethod]
        //public void ProLineViewModel()
        //{
        //    var lineVM = new ProLinesViewModel();

        //    // can we create an element
        //    Assert.IsFalse(lineVM.CanCreateElement);
        //    lineVM.LineAzimuthType = DistanceAndDirectionLibrary.AzimuthTypes.Degrees;
        //    lineVM.Azimuth = 90.1;
        //    Assert.AreEqual("90.1", lineVM.AzimuthString);
        //    lineVM.Azimuth = 90.0;
        //    Assert.AreEqual("90", lineVM.AzimuthString);
        //    lineVM.LineAzimuthType = DistanceAndDirectionLibrary.AzimuthTypes.Mils;
        //    Assert.AreEqual(1600.00000002, lineVM.Azimuth);

        //    // test points
        //    lineVM.Point1 = MapPointBuilder.CreateMapPoint(-119.8, 34.4);
        //    lineVM.Point2 = MapPointBuilder.CreateMapPoint(-74.1, 41.8);
        //    // can we create an element
        //    Assert.IsTrue(lineVM.CanCreateElement);

        //    // can't test manual input of of starting and ending points
        //    // they call methods that reference the ArcMap Application/Document objects
        //    // which is not available in unit testing

        //    // test Distance and Bearing mode

        //    // manual input of azimuth
        //    lineVM.LineAzimuthType = DistanceAndDirectionLibrary.AzimuthTypes.Degrees;
        //    lineVM.LineFromType = DistanceAndDirectionLibrary.LineFromTypes.BearingAndDistance;
        //    lineVM.AzimuthString = "90.1";
        //    Assert.AreEqual(90.1, lineVM.Azimuth);
        //    // manual input of distance
        //    lineVM.LineDistanceType = DistanceAndDirectionLibrary.DistanceTypes.Meters;
        //    lineVM.DistanceString = "50.5";
        //    Assert.AreEqual(50.5, lineVM.Distance);
        //    lineVM.LineDistanceType = DistanceAndDirectionLibrary.DistanceTypes.Miles;
        //    Assert.AreEqual(50.5, lineVM.Distance);

        //}

        #endregion Lines View Model

        #region Circle View Model

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProCircleViewModel_ThrowsException()
        {
            var circleVM = new ProCircleViewModel();

            circleVM.DistanceString = "esri";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProCircleViewModel_ThrowsException2()
        {
            var circleVM = new ProCircleViewModel();

            circleVM.Point1Formatted = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProCircleViewModel_ThrowsException3()
        {
            var circleVM = new ProCircleViewModel();

            circleVM.TravelTime = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProCircleViewModel_ThrowsException4()
        {
            var circleVM = new ProCircleViewModel();

            circleVM.TravelRate = -1;
        }

        //[TestMethod]
        //public void ProCircleViewModel()
        //{
        //    //Host.Initialize();

        //    var circleVM = new ProCircleViewModel();

        //    // can we create an element
        //    Assert.IsFalse(circleVM.CanCreateElement);

        //    circleVM.Distance = 1000.0;

        //    // test points
        //    circleVM.Point1 = MapPointBuilder.CreateMapPoint(-119.8, 34.4);
        //    // can we create an element
        //    //Assert.IsTrue(circleVM.CanCreateElement);

        //    Assert.AreEqual(circleVM.Point1Formatted, "34.4 -119.8");

        //    // can't test manual input of of starting and ending points
        //    // they call methods that reference the ArcMap Application/Document objects
        //    // which is not available in unit testing

        //}

        #endregion Circle View Model

        #region Ellipse View Model

        //[TestMethod]
        //public void ProEllipseViewModel()
        //{

        //    var ellipseVM = new ProEllipseViewModel();

        //    // can we create an element
        //    Assert.IsFalse(ellipseVM.CanCreateElement);

        //    ellipseVM.Distance = 1000.0;

        //    // test points
        //    ellipseVM.Point1 = MapPointBuilder.CreateMapPoint(-119.8, 34.4);
        //    // can we create an element
        //    //Assert.IsTrue(circleVM.CanCreateElement);

        //    Assert.AreEqual(ellipseVM.Point1Formatted, "34.4 -119.8");

        //    // can't test manual input of of starting and ending points
        //    // they call methods that reference the ArcMap Application/Document objects
        //    // which is not available in unit testing

        //    // manual input of azimuth
        //    ellipseVM.AzimuthType = DistanceAndDirectionLibrary.AzimuthTypes.Degrees;
        //    ellipseVM.AzimuthString = "90.1";
        //    Assert.AreEqual(90.1, ellipseVM.Azimuth);

        //}


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProEllipseViewModel_ThrowsException()
        {
            var ellipseVM = new ProEllipseViewModel();

            ellipseVM.MajorAxisDistanceString = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProEllipseViewModel_ThrowsException2()
        {
            var ellipseVM = new ProEllipseViewModel();

            ellipseVM.MinorAxisDistanceString = "esri";
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProEllipseViewModel_ThrowsException3()
        {
            var ellipseVM = new ProEllipseViewModel();

            ellipseVM.AzimuthString = "esri";
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProEllipseViewModel_ThrowsException4()
        {
            var ellipseVM = new ProEllipseViewModel();

            ellipseVM.MajorAxisDistance = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProEllipseViewModel_ThrowsException5()
        {
            var ellipseVM = new ProEllipseViewModel();

            ellipseVM.MinorAxisDistance = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProEllipseViewModel_ThrowsException6()
        {
            var ellipseVM = new ProEllipseViewModel();

            ellipseVM.Azimuth = -1;
        }

        #endregion Ellipse View Model

        #region Range View Model
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProRangeViewModel_ThrowsException()
        {
            var rangeVM = new ProRangeViewModel();

            rangeVM.NumberOfRings = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProRangeViewModel_ThrowsException2()
        {
            var rangeVM = new ProRangeViewModel();

            rangeVM.NumberOfRadials = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProRangeViewModel_ThrowsException3()
        {
            var rangeVM = new ProRangeViewModel();

            rangeVM.Distance = -1;
        }
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ProRangeViewModel_ThrowsException4()
        {
            var rangeVM = new ProRangeViewModel();

            rangeVM.DistanceString = "esri";
        }

        //[TestMethod]
        //public void ProRangeViewModel()
        //{

        //    var rangeVM = new ProRangeViewModel();

        //    // can we create an element
        //    Assert.IsFalse(rangeVM.CanCreateElement);

        //    rangeVM.Distance = 1000.0;

        //    // test points
        //    rangeVM.Point1 = MapPointBuilder.CreateMapPoint(-119.8, 34.4);
        //    // can we create an element
        //    //Assert.IsTrue(circleVM.CanCreateElement);

        //    Assert.AreEqual(rangeVM.Point1Formatted, "34.4 -119.8");

        //    // can't test manual input of of starting and ending points
        //    // they call methods that reference the ArcMap Application/Document objects
        //    // which is not available in unit testing

        //}
        #endregion Range View Model
    }
}

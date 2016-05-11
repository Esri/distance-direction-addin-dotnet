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

        [TestMethod]
        public void ProCircleViewModel()
        {
            //Host.Initialize();

            var circleVM = new ProCircleViewModel();

            // can we create an element
            Assert.IsFalse(circleVM.CanCreateElement);

            circleVM.Distance = 1000.0;
        }

        #endregion Circle View Model

        #region Ellipse View Model

        [TestMethod]
        public void ProEllipseViewModel()
        {

            var ellipseVM = new ProEllipseViewModel();

            // can we create an element
            Assert.IsFalse(ellipseVM.CanCreateElement);

            ellipseVM.Distance = 1000.0;

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

        [TestMethod]
        public void ProRangeViewModel()
        {

            var rangeVM = new ProRangeViewModel();

            // can we create an element
            Assert.IsFalse(rangeVM.CanCreateElement);

            rangeVM.Distance = 1000.0;
        }
        #endregion Range View Model
    }
}

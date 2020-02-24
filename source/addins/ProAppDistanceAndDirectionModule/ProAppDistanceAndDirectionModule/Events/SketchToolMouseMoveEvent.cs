using System;
using ArcGIS.Core.Events;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Framework;

namespace ProAppDistanceAndDirectionModule.Events
{
    internal sealed class SketchToolMouseMoveEventArgs : EventBase
    {
        public MapPoint MapPoint { get; }

        public SketchToolMouseMoveEventArgs(MapPoint mapPoint)
        {
            MapPoint = mapPoint;
        }
    }

    internal sealed class SketchToolMouseMoveEvent : CompositePresentationEvent<SketchToolMouseMoveEventArgs>
    {
        /// <summary>
        /// Allow subscribers to register for our custom event
        /// </summary>
        /// <param name="action">The callback which will be used to notify the subscriber</param>
        /// <param name="keepSubscriberReferenceAlive">Set to true to retain a strong reference</param>
        /// <returns><see cref="ArcGIS.Core.Events.SubscriptionToken"/></returns>
        public static SubscriptionToken Subscribe(Action<SketchToolMouseMoveEventArgs> action, bool keepSubscriberReferenceAlive = false)
        {
            return FrameworkApplication.EventAggregator.GetEvent<SketchToolMouseMoveEvent>()
                .Register(action, keepSubscriberReferenceAlive);
        }

        /// <summary>
        /// Allow subscribers to unregister from our custom event
        /// </summary>
        /// <param name="subscriber">The action that will be unsubscribed</param>
        public static void Unsubscribe(Action<SketchToolMouseMoveEventArgs> subscriber)
        {
            FrameworkApplication.EventAggregator.GetEvent<SketchToolMouseMoveEvent>().Unregister(subscriber);
        }
        /// <summary>
        /// Allow subscribers to unregister from our custom event
        /// </summary>
        /// <param name="token">The token that will be used to find the subscriber to unsubscribe</param>
        public static void Unsubscribe(SubscriptionToken token)
        {
            FrameworkApplication.EventAggregator.GetEvent<SketchToolMouseMoveEvent>().Unregister(token);
        }

        /// <summary>
        /// Event owner calls publish to raise the event and notify subscribers
        /// </summary>
        /// <param name="payload">The associated event information</param>
        internal static void Publish(SketchToolMouseMoveEventArgs payload)
        {
            FrameworkApplication.EventAggregator.GetEvent<SketchToolMouseMoveEvent>().Broadcast(payload);
        }
    }
}

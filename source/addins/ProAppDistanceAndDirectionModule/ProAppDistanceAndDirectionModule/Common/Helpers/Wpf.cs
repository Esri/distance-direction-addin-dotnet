﻿// Copyright 2016 Esri 
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;

namespace ProAppDistanceAndDirectionModule.Common
{
    public static class DialogCloser
    {
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloser),
                new PropertyMetadata(DialogResultChanged));

        private static void DialogResultChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null)
                window.DialogResult = e.NewValue as bool?;
        }
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
    /// <summary>
    /// Monitors the PropertyChanged event of an object that implements INotifyPropertyChanged,
    /// and executes callback methods (i.e. handlers) registered for properties of that object.
    /// </summary>
    /// <typeparam name="TPropertySource">The type of object to monitor for property changes.</typeparam>
    public class PropertyObserver<TPropertySource> : IWeakEventListener
        where TPropertySource : INotifyPropertyChanged
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of PropertyObserver, which
        /// observes the 'propertySource' object for property changes.
        /// </summary>
        /// <param name="propertySource">The object to monitor for property changes.</param>
        public PropertyObserver(TPropertySource propertySource)
        {
            if (propertySource == null)
                throw new ArgumentNullException("propertySource");

            _propertySourceRef = new WeakReference(propertySource);
            _propertyNameToHandlerMap = new Dictionary<string, Action<TPropertySource>>();
        }

        #endregion // Constructor

        #region Public Methods

        #region RegisterHandler

        /// <summary>
        /// Registers a callback to be invoked when the PropertyChanged event has been raised for the specified property.
        /// </summary>
        /// <param name="expression">A lambda expression like 'n => n.PropertyName'.</param>
        /// <param name="handler">The callback to invoke when the property has changed.</param>
        /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
        public PropertyObserver<TPropertySource> RegisterHandler(
            Expression<Func<TPropertySource, object>> expression,
            Action<TPropertySource> handler)
        {
            if (expression == null)
                throw new ArgumentNullException(Properties.Resources.WFNException);

            string propertyName = GetPropertyName(expression);
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException(Properties.Resources.WFNException);

            if (handler == null)
                throw new ArgumentNullException(Properties.Resources.WFHandle);

            TPropertySource propertySource = this.GetPropertySource();
            if (propertySource != null)
            {
                Debug.Assert(!_propertyNameToHandlerMap.ContainsKey(propertyName), "Why is the '" + propertyName + "' property being registered again?");

                _propertyNameToHandlerMap[propertyName] = handler;
                PropertyChangedEventManager.AddListener(propertySource, this, propertyName);
            }

            return this;
        }

        #endregion // RegisterHandler

        #region UnregisterHandler

        /// <summary>
        /// Removes the callback associated with the specified property.
        /// </summary>
        /// <param name="propertyName">A lambda expression like 'n => n.PropertyName'.</param>
        /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
        public PropertyObserver<TPropertySource> UnregisterHandler(Expression<Func<TPropertySource, object>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(Properties.Resources.WFNException);

            string propertyName = GetPropertyName(expression);
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException(Properties.Resources.WFAEException);

            TPropertySource propertySource = this.GetPropertySource();
            if (propertySource != null)
            {
                if (_propertyNameToHandlerMap.ContainsKey(propertyName))
                {
                    _propertyNameToHandlerMap.Remove(propertyName);
                    PropertyChangedEventManager.RemoveListener(propertySource, this, propertyName);
                }
            }

            return this;
        }

        #endregion // UnregisterHandler

        #endregion // Public Methods

        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            bool handled = false;

            if (managerType == typeof(PropertyChangedEventManager))
            {
                PropertyChangedEventArgs args = e as PropertyChangedEventArgs;
                if (args != null && sender is TPropertySource)
                {
                    string propertyName = args.PropertyName;
                    TPropertySource propertySource = (TPropertySource)sender;

                    if (String.IsNullOrEmpty(propertyName))
                    {
                        // When the property name is empty, all properties are considered to be invalidated.
                        // Iterate over a copy of the list of handlers, in case a handler is registered by a callback.
                        foreach (Action<TPropertySource> handler in _propertyNameToHandlerMap.Values.ToArray())
                            handler(propertySource);

                        handled = true;
                    }
                    else
                    {
                        Action<TPropertySource> handler;
                        if (_propertyNameToHandlerMap.TryGetValue(propertyName, out handler))
                        {
                            handler(propertySource);

                            handled = true;
                        }
                    }
                }
            }

            return handled;
        }

        #endregion // IWeakEventListener Members

        #region Private Helpers

        #region GetPropertyName

        static string GetPropertyName(Expression<Func<TPropertySource, object>> expression)
        {
            var lambda = expression as LambdaExpression;
            if (lambda == null)
                return null;

            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)lambda.Body;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            Debug.WriteLineIf(memberExpression != null, Properties.Resources.WFMessage);

            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;

                if (propertyInfo != null)
                    return propertyInfo.Name;
            }

            return null;
        }

        #endregion // GetPropertyName

        #region GetPropertySource

        TPropertySource GetPropertySource()
        {
            try
            {
                return (TPropertySource)_propertySourceRef.Target;
            }
            catch
            {
                return default(TPropertySource);
            }
        }

        #endregion // GetPropertySource

        #endregion // Private Helpers

        #region Fields

        readonly Dictionary<string, Action<TPropertySource>> _propertyNameToHandlerMap;
        readonly WeakReference _propertySourceRef;

        #endregion // Fields
    }
}

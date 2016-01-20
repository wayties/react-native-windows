﻿using ReactNative.Bridge;
using ReactNative.UIManager;
using System;
using Windows.UI.Xaml;
using Windows.Foundation;

namespace ReactNative
{
    /// <summary>
    /// Default root view for applicaitons. Provides the ability to listen for
    /// size changes so that the UI manager can re-layout its elements.
    /// 
    /// It is also responsible for handling touch events passed to any of it's
    /// child views and sending those events to JavaScript via the
    /// <see cref="UIManager.Events.RCTEventEmitter"/> module.
    /// </summary>
    public class ReactRootView : SizeMonitoringCanvas, IRootView
    {
        private IReactInstanceManager _reactInstanceManager;
        private string _jsModuleName;

        private bool _wasMeasured;
        private bool _attachScheduled;

        /// <summary>
        /// Gets the JavaScript module name.
        /// </summary>
        internal string JavaScriptModuleName
        {
            get
            {
                return _jsModuleName;
            }
        }

        /// <summary>
        /// Schedule rendering of the react component rendered by the 
        /// JavaScript application from the given JavaScript module 
        /// <paramref name="moduleName"/> using the provided
        /// <paramref name="reactInstanceManager"/> to attach to the JavaScript
        /// context of that manager.
        /// </summary>
        /// <param name="reactInstanceManager">
        /// The react instance manager.
        /// </param>
        /// <param name="moduleName">
        /// The module name.
        /// </param>
        public void StartReactApplication(IReactInstanceManager reactInstanceManager, string moduleName)
        {
            DispatcherHelpers.AssertOnDispatcher();

            if (_reactInstanceManager != null)
            {
                throw new InvalidOperationException("This root view has already been attached to an instance manager.");
            }

            _reactInstanceManager = reactInstanceManager;
            _jsModuleName = moduleName;

            if (!_reactInstanceManager.HasStartedCreatingInitialContext)
            {
                _reactInstanceManager.CreateReactContextInBackground();
            }

            // We need to wait for the initial `Measure` call, if this view has
            // not yet been measured, we set the `_attachScheduled` flag, which
            // will enable deferred attachment of the root node.
            if (_wasMeasured)
            {
                _reactInstanceManager.AttachMeasuredRootView(this);
            }
            else
            {
                _attachScheduled = true;
            }
        }

        /// <summary>
        /// Called when a child starts a native gesture.
        /// </summary>
        /// <param name="e">The event.</param>
        public void OnChildStartedNativeGesture(RoutedEventArgs ev)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Hooks into the measurement event to potentially attach the react 
        /// root view.
        /// </summary>
        /// <param name="availableSize">The available size.</param>
        /// <returns>The desired size.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            DispatcherHelpers.AssertOnDispatcher();

            var result = base.MeasureOverride(availableSize);

            _wasMeasured = true;

            var reactInstanceManager = _reactInstanceManager;
            if (_attachScheduled && reactInstanceManager != null)
            {
                _attachScheduled = false;
                reactInstanceManager.AttachMeasuredRootView(this);
            }

            return result;
        }
    }
}
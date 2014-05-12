﻿using AdjustSdk;
using AdjustSdk.Pcl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace AdjustSdk
{
    /// <summary>
    ///  The main interface to Adjust.
    ///  Use the methods of this class to tell Adjust about the usage of your app.
    ///  See the README for details.
    /// </summary>
    public class Adjust
    {
        private static DeviceUtil Util = new UtilWS();

        private static bool firstVisibilityChanged = true;

        /// <summary>
        ///  Tell Adjust that the application is activated (brought to foreground) or deactivated (sent to background).
        /// </summary>
        private static void VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            if (firstVisibilityChanged)
            {
                firstVisibilityChanged = false;
                return;
            }
            if (args.Visible)
            {
                AdjustApi.AppDidActivate();
            }
            else
            {
                AdjustApi.AppDidDeactivate();
            }
        }

        #region AdjustApi

        /// <summary>
        ///  Tell Adjust that the application did launch.
        ///
        ///  This is required to initialize Adjust. Call this in the OnLaunched
        ///  method of your Windows.UI.Xaml.Application class.
        /// </summary>
        /// <param name="appToken">
        ///   The App Token of your app. This unique identifier can
        ///   be found it in your dashboard at http://adjust.com and should always
        ///   be 12 characters long.
        /// </param>
        public static void AppDidLaunch(string appToken)
        {
            AdjustApi.AppDidLaunch(appToken, Util);
            try
            {
                Window.Current.CoreWindow.VisibilityChanged += VisibilityChanged;
            }
            catch (Exception)
            {
                AdjustFactory.Logger.Debug("Not possible to detect automatically if the app goes to the background");
            }
        }

        /// <summary>
        ///  Tell Adjust that the application is activated (brought to foreground).
        ///
        ///  This is used to keep track of the current session state.
        ///  This should only be used if the VisibilityChanged mechanism doesn't work
        /// </summary>
        public static void AppDidActivate()
        {
            AdjustApi.AppDidActivate();
        }

        /// <summary>
        ///  Tell Adjust that the application is deactivated (sent to background).
        ///
        ///  This is used to calculate session attributes like session length and subsession count.
        ///  This should only be used if the VisibilityChanged mechanism doesn't work
        /// </summary>
        public static void AppDidDeactivate()
        {
            AdjustApi.AppDidDeactivate();
        }

        /// <summary>
        ///  Tell Adjust that a particular event has happened.
        ///
        ///  In your dashboard at http://adjust.com you can assign a callback URL to each
        ///  event type. That URL will get called every time the event is triggered. On
        ///  top of that you can pass a set of parameters to the following method that
        ///  will be forwarded to these callbacks.
        /// </summary>
        /// <param name="eventToken">
        ///  The Event Token for this kind of event. They are created in the
        ///  dashboard at http://adjust.com and should be six characters long.
        /// </param>
        /// <param name="callbackParameters">
        ///  An optional dictionary containing the callback parameters.
        ///  Provide key-value-pairs to be forwarded to your callbacks.
        /// </param>
        public static void TrackEvent(string eventToken,
            Dictionary<string, string> callbackParameters = null)
        {
            AdjustApi.TrackEvent(eventToken, callbackParameters);
        }

        /// <summary>
        ///  Tell Adjust that a user generated some revenue.
        ///
        ///  The amount is measured in cents and rounded to on digit after the
        ///  decimal point. If you want to differentiate between several revenue
        ///  types, you can do so by using different event tokens. If your revenue
        ///  events have callbacks, you can also pass in parameters that will be
        ///  forwarded to your end point.
        /// </summary>
        /// <param name="amountInCents">
        ///  The amount in cents (example: 1.5 means one and a half cents)
        /// </param>
        /// <param name="eventToken">
        ///  The token for this revenue event (optional, see above)
        /// </param>
        /// <param name="callbackParameters">
        ///  Parameters for this revenue event (optional, see above)
        /// </param>
        public static void TrackRevenue(double amountInCents,
            string eventToken = null,
            Dictionary<string, string> callbackParameters = null)
        {
            AdjustApi.TrackRevenue(amountInCents, eventToken, callbackParameters);
        }

        /// <summary>
        ///  Change the verbosity of Adjust's logs.
        ///
        ///  You can increase or reduce the amount of logs from Adjust by passing
        ///  one of the following parameters. Use Log.ASSERT to disable all logging.
        /// </summary>
        /// <param name="logLevel">
        ///  The desired minimum log level (default: info)
        ///  Must be one of the following:
        ///   - Verbose (enable all logging)
        ///   - Debug   (enable more logging)
        ///   - Info    (the default)
        ///   - Warn    (disable info logging)
        ///   - Error   (disable warnings as well)
        ///   - Assert  (disable errors as well)
        /// </param>
        public static void SetLogLevel(LogLevel logLevel)
        {
            AdjustApi.SetLogLevel(logLevel);
        }

        /// <summary>
        ///  Set the tracking environment to sandbox or production.
        ///
        ///  Use sandbox for testing and production for the final build that you release.
        /// </summary>
        /// <param name="environment">
        ///  The new environment. Supported values:
        ///   - AdjustEnvironment.Sandbox
        ///   - AdjustEnvironment.Production
        /// </param>
        public static void SetEnvironment(AdjustEnvironment environment)
        {
            AdjustApi.SetEnvironment(environment);
        }

        /// <summary>
        ///  Enable or disable event buffering.
        ///
        ///  Enable event buffering if your app triggers a lot of events.
        ///  When enabled, events get buffered and only get tracked each
        ///  minute. Buffered events are still persisted, of course.
        /// </summary>
        public static void SetEventBufferingEnabled(bool enabledEventBuffering)
        {
            AdjustApi.SetEventBufferingEnabled(enabledEventBuffering);
        }

        /// <summary>
        /// Optional delegate method that will get called when a tracking attempt finished
        /// </summary>
        /// <param name="responseDelegate">The response data containing information about the activity and it's server response.</param>
        public static void SetResponseDelegate(Action<ResponseData> responseDelegate)
        {
            AdjustApi.SetResponseDelegate(responseDelegate);
        }

        /// <summary>
        /// Enable or disable the adjust SDK
        /// </summary>
        /// <param name="enabled">The flag to enable or disable the adjust SDK</param>
        public static void SetEnabled(bool enabled)
        {
            AdjustApi.SetEnabled(enabled);
        }

        /// <summary>
        /// Check if the SDK is enabled or disabled
        /// </summary>
        /// <returns>true if the SDK is enabled, false otherwise</returns>
        public static bool IsEnabled()
        {
            return AdjustApi.IsEnabled();
        }

        /// <summary>
        /// Read the URL that opened the application to search for
        /// an adjust deep link
        /// </summary>
        /// <param name="url">The url that open the application</param>
        public static void AppWillOpenUrl(Uri url)
        {
            AdjustApi.AppWillOpenUrl(url);
        }

        /// <summary>
        /// Special method used by SDK wrappers
        /// </summary>
        /// <param name="sdkPrefix">The SDK prefix to be added</param>
        public static void SetSdkPrefix(string sdkPrefix)
        {
            AdjustApi.SetSdkPrefix(sdkPrefix);
        }

        #endregion AdjustApi
    }
}
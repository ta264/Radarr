﻿using System;
using NLog;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Providers.ExternalNotification
{
    public class Growl : ExternalNotificationBase
    {
        private readonly GrowlProvider _growlProvider;

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Growl(ConfigProvider configProvider, GrowlProvider growlProvider)
            : base(configProvider)
        {
            _growlProvider = growlProvider;
        }

        public override string Name
        {
            get { return "Growl"; }
        }

        public override void OnGrab(string message)
        {
            try
            {
                if(_configProvider.GrowlNotifyOnGrab)
                {
                    _logger.Trace("Sending Notification to Growl");
                    const string title = "Episode Grabbed";

                    var growlHost = _configProvider.GrowlHost.Split(':');
                    var host = growlHost[0];
                    var port = Convert.ToInt32(growlHost[1]);

                    _growlProvider.SendNotification(title, message, host, "GRAB", port, _configProvider.GrowlPassword);
                }
            }

            catch (Exception ex)
            {
                Logger.WarnException(ex.Message, ex);
                throw;
            }
        }

        public override void OnDownload(string message, Series series)
        {
            try
            {
                if (_configProvider.GrowlNotifyOnDownload)
                {
                    _logger.Trace("Sending Notification to Growl");
                    const string title = "Episode Downloaded";
                    
                    var growlHost = _configProvider.GrowlHost.Split(':');
                    var host = growlHost[0];
                    var port = Convert.ToInt32(growlHost[1]);

                    _growlProvider.SendNotification(title, message, host, "DOWNLOAD", port, _configProvider.GrowlPassword);
                }
            }

            catch (Exception ex)
            {
                Logger.WarnException(ex.Message, ex);
                throw;
            }
        }

        public override void OnRename(string message, Series series)
        {
            
        }
    }
}

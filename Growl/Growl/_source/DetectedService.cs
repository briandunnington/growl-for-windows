using System;
using System.Collections.Generic;
using System.Text;
using ZeroconfService;

namespace Growl
{
    public class DetectedService
    {
        private NetService service;
        private ForwardComputerPlatformType platform;

        public DetectedService(NetService service, ForwardComputerPlatformType platform)
        {
            this.service = service;
            this.platform = platform;
        }

        public NetService Service
        {
            get
            {
                return this.service;
            }
        }

        public ForwardComputerPlatformType Platform
        {
            get
            {
                return this.platform;
            }
        }
    }
}

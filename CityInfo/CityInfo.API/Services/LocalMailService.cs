﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Services
{
    public class LocalMailService : IMailService
    {
        private IConfiguration _config;

        public LocalMailService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void Send(string subject, string message)
        {
            Debug.WriteLine($"Mail from {_config["mailSettings:mailFromAddress"]} to {_config["mailSettings:mailToAddress"]}, with LocalMailService");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine($"Message: {message}");
        }
    }
}

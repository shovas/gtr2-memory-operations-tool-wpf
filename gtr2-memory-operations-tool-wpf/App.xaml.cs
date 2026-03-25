using Gtr2MemOpsTool.Services;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;

namespace Gtr2MemOpsTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public static readonly AsyncBatchLogger.LogLevel DefaultLogLevel = AsyncBatchLogger.LogLevel.Info;
        public static Config Config { get; private set; } = new();
        public static AsyncBatchLogger Log { get; set; } = new AsyncBatchLogger();
        
    }
}

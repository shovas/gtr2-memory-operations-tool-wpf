using Gtr2MemOpsTool.Models;
using IniParser;
using IniParser.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Gtr2MemOpsTool.Services
{
    public class Config
    {
        private static readonly string SettingsFilePath =
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");
        public event EventHandler? ConfigLoaded; // Triggered after settings loads or reloads
        public IniData IniData { get; private set; } = new();

        public Config() {
            LoadSettings();
            //ApplySettings();
        }

        //public void ApplySettings()
        //{

        //    //// Example: Apply log level setting
        //    //if (IniData.Global.TryGetKey("LogLevel", out var logLevelKey))
        //    //{
        //    //    if (Enum.TryParse(logLevelKey.Value, true, out AsyncBatchLogger.LogLevel logLevel))
        //    //    {
        //    //        App.Log.SetLogLevel(logLevel);
        //    //        App.Log.AddInfo($"Log level set to {logLevel}");
        //    //    }
        //    //    else
        //    //    {
        //    //        App.Log.AddWarning($"Invalid log level in settings: {logLevelKey.Value}");
        //    //    }
        //    //}
        //}

        public void LoadSettings()
        {

            // Initialize settings if needed
            if (File.Exists(SettingsFilePath) is false)
            {
                SaveSettings();
            }

            // Load settings from INI file
            var parser = new FileIniDataParser();
            IniData = parser.ReadFile(SettingsFilePath);

            ConfigLoaded?.Invoke(null, EventArgs.Empty);

            //App.Log.AddInfo("Settings loaded");
        }

        public void SaveSettings()
        {
            try
            {
                var parser = new FileIniDataParser();
                parser.WriteFile(SettingsFilePath, IniData);
                App.Log.AddInfo("Settings saved");
            }
            catch (Exception ex)
            {
                App.Log.AddError($"Failed to save settings: {ex.Message}");
            }
        }
    }
}

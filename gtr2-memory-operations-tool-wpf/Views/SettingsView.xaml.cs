using Gtr2MemOpsTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IniParser;
using IniParser.Model;

namespace Gtr2MemOpsTool.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        // INI file path — adjust as needed
        private static readonly string SettingsFilePath =
            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");

        public ObservableCollection<SettingItem> SettingItems { get; set; } = [];

        public SettingsView()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            App.Config.LoadSettings();
            SettingItems.Clear();

            // Load in global settings
            foreach (var key in App.Config.IniData.Global)
            {
                //Console.WriteLine($"{key.KeyName} = {key.Value}");
                var newSettingItem = new SettingItem(key.KeyName, key.Value, key.Comments.LastOrDefault() ?? string.Empty);
                SettingItems.Add(newSettingItem);
            }

            // Load in section settings
            foreach (var section in App.Config.IniData.Sections)
            {
                foreach (var key in section.Keys)
                {
                    var newSettingItem = new SettingItem(
                        $"{section.SectionName}.{key.KeyName}",
                        key.Value,
                        key.Comments.LastOrDefault() ?? string.Empty
                    );
                    SettingItems.Add(newSettingItem);
                }
            }

            //SettingItems.Clear();

            //if (!File.Exists(SettingsFilePath))
            //{
            //    // Save blank settings to initialize the ini file
            //    SaveSettings();
            //    //MessageBox.Show($"Settings file not found:\n{SettingsFilePath}",
            //    //    "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
            //    //return;
            //}

            //var parser = new FileIniDataParser();
            //IniData data = parser.ReadFile(SettingsFilePath);
            
            //// Global (sectionless) keys
            //foreach (var key in data.Global)
            //    SettingItems.Add(new SettingItem(key.KeyName, key.Value, key.Comments.LastOrDefault() ?? string.Empty));

            //// Sectioned keys — prefix key with section for clarity
            //foreach (var section in data.Sections)
            //    foreach (var key in section.Keys)
            //        SettingItems.Add(new SettingItem(
            //            $"{section.SectionName}.{key.KeyName}",
            //            key.Value,
            //            key.Comments.LastOrDefault() ?? string.Empty
            //        ));

            App.Log.AddInfo("Settings loaded");
        }

        private void SaveSettings()
        {
            
            //try
            //{

            //    var parser = new FileIniDataParser();
            //    IniData data = File.Exists(SettingsFilePath)
            //        ? parser.ReadFile(SettingsFilePath)
            //        : new IniData();

            foreach (var item in SettingItems)
            {
                var parts = item.Key.Split('.', 2);

                if (parts.Length == 2)
                {
                    var section = parts[0];
                    var key = parts[1];
                    if (!App.Config.IniData.Sections.ContainsSection(section))
                        App.Config.IniData.Sections.AddSection(section);
                    App.Config.IniData[section][key] = item.Value;
                }
                else
                {
                    App.Config.IniData.Global[item.Key] = item.Value;
                }
            }

            //    parser.WriteFile(SettingsFilePath, data);

            //    App.Log.AddInfo("Settings saved");
            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show($"Failed to save settings:\n{ex.Message}",
            //    //    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    App.Log.AddError($"Failed to save settings: {ex.Message}");
            //}

            App.Config.SaveSettings();
        }

        private void ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
        }

        private void SearchFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(SettingItems);
            view.Filter = item =>
            {
                var settingItem = (SettingItem)item;
                return
                    settingItem.Key.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase) ||
                    settingItem.Value.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase) ||
                    settingItem.Description.Contains(SearchFilterBox.Text, StringComparison.OrdinalIgnoreCase)
                ;
            };
        }

        
    }
}

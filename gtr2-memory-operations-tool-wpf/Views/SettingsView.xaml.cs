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

            //SettingItems.Add(new SettingItem("abc", "def", "ghi"));

            LoadSettings();
        }

        private void LoadSettings()
        {
            SettingItems.Clear();

            if (!File.Exists(SettingsFilePath))
            {
                // Save blank settings to initialize the ini file
                SaveSettings();
                //MessageBox.Show($"Settings file not found:\n{SettingsFilePath}",
                //    "Settings", MessageBoxButton.OK, MessageBoxImage.Warning);
                //return;
            }

            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(SettingsFilePath);

            // Global (sectionless) keys
            foreach (var key in data.Global)
                SettingItems.Add(new SettingItem(key.KeyName, key.Value, key.Comments.LastOrDefault() ?? string.Empty));

            // Sectioned keys — prefix key with section for clarity
            foreach (var section in data.Sections)
                foreach (var key in section.Keys)
                    SettingItems.Add(new SettingItem(
                        $"{section.SectionName}.{key.KeyName}",
                        key.Value,
                        key.Comments.LastOrDefault() ?? string.Empty
                    ));

            // Expected format per line:
            //   key=value                      (no description)
            //   key=value ; description text   (inline comment as description)
            //   # comment / blank lines        (ignored)

            //foreach (var rawLine in File.ReadLines(SettingsFilePath))
            //{
            //    var line = rawLine.Trim();
            //    App.Log.AddDebug($"Parsing line: {line}");
            //    if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith(';'))
            //        continue;

            //    var eqIndex = line.IndexOf('=');
            //    if (eqIndex < 1) continue;

            //    var key = line[..eqIndex].Trim();
            //    var rest = line[(eqIndex + 1)..];

            //    // Split value and optional inline description (separated by " ; ")
            //    var semiIndex = rest.IndexOf(" ; ", StringComparison.Ordinal);
            //    string value, description;

            //    if (semiIndex >= 0)
            //    {
            //        value = rest[..semiIndex].Trim();
            //        description = rest[(semiIndex + 3)..].Trim();
            //    }
            //    else
            //    {
            //        value = rest.Trim();
            //        description = string.Empty;
            //    }

            //    SettingItems.Add(new SettingItem(key, value, description));
            //    App.Log.AddDebug($"Added {key}={value} ({description})");
            //}

            App.Log.AddInfo("Settings loaded");
        }

        private void SaveSettings()
        {
            try
            {

                var parser = new FileIniDataParser();
                IniData data = File.Exists(SettingsFilePath)
                    ? parser.ReadFile(SettingsFilePath)
                    : new IniData();

                foreach (var item in SettingItems)
                {
                    var parts = item.Key.Split('.', 2);

                    if (parts.Length == 2)
                    {
                        var section = parts[0];
                        var key = parts[1];
                        if (!data.Sections.ContainsSection(section))
                            data.Sections.AddSection(section);
                        data[section][key] = item.Value;
                    }
                    else
                    {
                        data.Global[item.Key] = item.Value;
                    }
                }

                parser.WriteFile(SettingsFilePath, data);

                //// Re-read original lines to preserve comments and ordering
                //var originalLines = File.Exists(SettingsFilePath)
                //    ? File.ReadAllLines(SettingsFilePath)
                //    : [];

                //// Build a lookup of updated values
                //var updatedValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                //foreach (var item in SettingItems)
                //    updatedValues[item.Key] = item.Value;

                //var output = new List<string>();

                //foreach (var rawLine in originalLines)
                //{
                //    var line = rawLine.Trim();

                //    // Preserve comments and blanks as-is
                //    if (string.IsNullOrEmpty(line) || line.StartsWith('#') || line.StartsWith(';'))
                //    {
                //        output.Add(rawLine);
                //        continue;
                //    }

                //    var eqIndex = line.IndexOf('=');
                //    if (eqIndex < 1) { output.Add(rawLine); continue; }

                //    var key = line[..eqIndex].Trim();

                //    if (updatedValues.TryGetValue(key, out var newValue))
                //    {
                //        // Preserve inline description if present
                //        var rest = line[(eqIndex + 1)..];
                //        var semiIndex = rest.IndexOf(" ; ", StringComparison.Ordinal);
                //        var suffix = semiIndex >= 0 ? " ; " + rest[(semiIndex + 3)..].Trim() : string.Empty;

                //        output.Add($"{key}={newValue}{suffix}");
                //    }
                //    else
                //    {
                //        output.Add(rawLine);
                //    }
                //}

                //File.WriteAllLines(SettingsFilePath, output);
                ////MessageBox.Show("Settings saved.", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
                App.Log.AddInfo("Settings saved");
            }
            catch (Exception ex)
            {
                //MessageBox.Show($"Failed to save settings:\n{ex.Message}",
                //    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                App.Log.AddError($"Failed to save settings: {ex.Message}");
            }
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

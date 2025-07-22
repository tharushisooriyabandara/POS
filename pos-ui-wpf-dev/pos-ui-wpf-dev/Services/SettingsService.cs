using System;
using System.IO;

namespace POS_UI.Services
{
    public class SettingsService
    {
        private const string SettingsFileName = "settings.txt";
        private string SettingsFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), SettingsFileName);

        public (string TenantCode, string OutletCode) LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var lines = File.ReadAllLines(SettingsFilePath);
                    string tenantCode="";
                    string outletCode="";

                    foreach (var line in lines)
                    {
                        if (line.StartsWith("TenantCode="))
                        {
                            tenantCode = line.Replace("TenantCode=", "").Trim();
                        }
                        else if (line.StartsWith("OutletCode="))
                        {
                            outletCode = line.Replace("OutletCode=", "").Trim();
                        }
                    }

                    return (tenantCode, outletCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
            }

            // Return default values if file doesn't exist or there's an error
            //return ("subway", "");
            return ("", "");
        }

        public bool SettingsExist()
        {
            return File.Exists(SettingsFilePath);
        }
    }
} 
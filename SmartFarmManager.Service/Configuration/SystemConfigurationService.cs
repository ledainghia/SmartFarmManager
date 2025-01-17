using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Configuration
{
    public class SystemConfigurationService
    {
        private readonly string _configFilePath;

        public SystemConfigurationService()
        {
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system_config.json");
        }

        // Lấy cấu hình hiện tại
        public async Task<SystemConfiguration> GetConfigurationAsync()
        {
            if (!File.Exists(_configFilePath))
            {
                var defaultConfig = new SystemConfiguration();
                await File.WriteAllTextAsync(_configFilePath, JsonSerializer.Serialize(defaultConfig));
                return defaultConfig;
            }

            var json = await File.ReadAllTextAsync(_configFilePath);
            return JsonSerializer.Deserialize<SystemConfiguration>(json) ?? new SystemConfiguration();
        }

        public async Task<bool> UpdateConfigurationAsync(SystemConfiguration config)
        {
            try
            {
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true // Định dạng dễ đọc
                });

                // Ghi cấu hình vào file JSON
                await File.WriteAllTextAsync(_configFilePath, json);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating configuration: {ex.Message}");
                return false;
            }
        }

    }
}

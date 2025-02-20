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
            // Kiểm tra nếu file không tồn tại
            if (!File.Exists(_configFilePath))
            {
                var defaultConfig = new SystemConfiguration();
                await File.WriteAllTextAsync(_configFilePath, JsonSerializer.Serialize(defaultConfig));
                return defaultConfig;
            }

            // Đọc dữ liệu JSON từ file
            var json = await File.ReadAllTextAsync(_configFilePath);

            // Kiểm tra xem dữ liệu JSON có trống không
            if (string.IsNullOrEmpty(json))
            {
                // Nếu trống, tạo và ghi lại cấu hình mặc định
                var defaultConfig = new SystemConfiguration();
                await File.WriteAllTextAsync(_configFilePath, JsonSerializer.Serialize(defaultConfig));
                return defaultConfig;
            }

            // Deserialize dữ liệu JSON
            try
            {
                return JsonSerializer.Deserialize<SystemConfiguration>(json) ?? new SystemConfiguration();
            }
            catch (JsonException)
            {
                // Nếu không thể deserialize, trả về cấu hình mặc định
                return new SystemConfiguration();
            }
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

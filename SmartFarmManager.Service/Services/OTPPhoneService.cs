using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartFarmManager.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.Services
{
    public class OTPPhoneService
    {
        private readonly ILogger<OTPPhoneService> _logger;
        private readonly IConfiguration _configuration;

        public OTPPhoneService(ILogger<OTPPhoneService> logger, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        public async Task SendOtpViaSmsAsync(string phoneNumber, string otp)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-STRINGEE-AUTH", _configuration["StringeeAuthToken"]);

                var body = new
                {
                    from = new
                    {
                        type = "external",
                        number = _configuration["StringeeNumber"],
                        alias = "STRINGEE_NUMBER"
                    },
                    to = new[]
                    {
                    new
                    {
                        type = "external",
                        number = phoneNumber,
                        alias = "TO_NUMBER"
                    }
                },
                    answer_url = _configuration["StringeeAnswerUrl"],
                    actions = new[]
                    {
                    new
                    {
                        action = "talk",
                        text = $"MÃ XÁC THỰC CỦA BẠN LÀ {otp}"
                    }
                }
                };

                var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.stringee.com/v1/call2/callout", content);

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Sent OTP to {phoneNumber}. Response: {responseContent}");
            }
        }
    }
}

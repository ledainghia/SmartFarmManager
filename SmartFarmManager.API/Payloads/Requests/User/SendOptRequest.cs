﻿namespace SmartFarmManager.API.Payloads.Requests.User
{
    public class SendOtpRequest
    {
        public string Email { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public bool IsResend { get; set; } // True if it's a resend request
    }
}

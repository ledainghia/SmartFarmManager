﻿namespace SmartFarmManager.API.Payloads.Responses.Auth
{
    public class RefreshTokenResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

}

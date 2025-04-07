﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFarmManager.Service.BusinessModels.Users
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string? RoleName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid RoleId { get; set; }
        public string? FarmName { get; set; }
        public List<string>? CageNames { get; set; }
    }
}

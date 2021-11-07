﻿using Microsoft.AspNetCore.Identity;

namespace Recruit.Server.Data
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string? Headline { get; set; }
    }
}

﻿using System;
using System.Collections.Generic;

namespace AccountManagerAPI.Models
{
    public partial class UserInfo
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int UserRole { get; set; }
        public string UserGroup { get; set; }
        public string Password { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}

using AccountManagerAPI.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;

namespace AccountManagerAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AccountDbContext context)
        {
            context.Database.EnsureCreated();

            // Look for any Users.
            if (context.UserInfo.Any())
            {
                return;   // DB has been seeded
            }

            var userInfo = new Models.UserInfo[]
            {
                new Models.UserInfo{FirstName= "Admin",LastName="Default",UserName = "DefaultAdmin",Email = "defaultAdmin@abc.com",UserRole = 0,UserGroup = "Admin",Password = "$adminDefault@2020"},
                new Models.UserInfo{FirstName= "Super",LastName="Default",UserName = "DefaultSuper",Email = "defaultSuper@abc.com",UserRole = 1,UserGroup = "Client",UserOwnerId = "1",Password = "$superDefault@2020"},
                new Models.UserInfo{FirstName= "User",LastName="Default",UserName = "DefaultUser",Email = "defaultUser@abc.com",UserRole = 2,UserGroup = "Client",UserOwnerId = "2",Password = "$userDefault@2020"},

            };

            context.UserInfo.AddRange(userInfo);
            context.SaveChanges();

        }
    }
}
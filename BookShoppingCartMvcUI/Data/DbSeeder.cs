﻿using Microsoft.AspNetCore.Identity;
using BookShoppingCartMvcUI.Constants;
namespace BookShoppingCartMvcUI.Data
{
    public class DbSeeder
    {
        public static async Task SeedDefaultData(IServiceProvider service)
        {
            var userMgr = service.GetService<UserManager<IdentityUser>>();
            var roleMgr = service.GetService<RoleManager<IdentityRole>>();

            //Adding Some Roles for Db
            await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
            await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));

            // create Admin User
            var admin = new IdentityUser 
            { 
                UserName ="admin@gmail.com",
                Email = "admin@gmail.com",
                EmailConfirmed = true,

            };
            var UserInDb = await userMgr.FindByEmailAsync(admin.Email);
            if (UserInDb == null)
            {
                await userMgr.CreateAsync(admin ,"Admin@123");
                await userMgr.AddToRoleAsync(admin,Roles.Admin.ToString());
            }
        }
    }
}

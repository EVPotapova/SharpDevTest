﻿using System.Security.Claims;
using System.Threading.Tasks;
using SharpDevTest.Models.DbModel;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;

namespace SharpDevTest.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        public override string UserName  { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public override string Email { get; set; }

        [Required]
        [Range(typeof(decimal), "0", "9999999999999999", ErrorMessage = "Value of {0} should be between {1:c} and {2:c}")]
        public decimal PwCoins { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    
}
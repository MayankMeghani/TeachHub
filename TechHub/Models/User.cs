using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace TeachHub.Models
{
    public class User:IdentityUser
    {
        public bool IsProfileComplete { get; set; } = false;

        public virtual Teacher Teacher { get; set; }
        public virtual Learner Learner { get; set; }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DemoMusicMVC.Models
{
    public class Song
    {
        [Key]
        public int songId { get; set; }
        [Required, MaxLength(20, ErrorMessage = "Name Cannot Exceed 20 Characters")]
        [Display(Name = "Song Name")]
        public string songName { get; set; } = string.Empty;
        [Required]
        public string photoPath { get; set; } = string.Empty;
        [Required]
        public string songPath { get; set; } = string.Empty;

        public string? UploadedByUserId { get; set; }

        [ForeignKey("UploadedByUserId")]
        public virtual IdentityUser? UploadedBy { get; set; }
    }
}

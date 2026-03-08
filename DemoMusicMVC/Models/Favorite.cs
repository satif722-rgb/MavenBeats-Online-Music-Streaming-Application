using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoMusicMVC.Models
{
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        [Required]
        public int SongId { get; set; }

        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    }
}

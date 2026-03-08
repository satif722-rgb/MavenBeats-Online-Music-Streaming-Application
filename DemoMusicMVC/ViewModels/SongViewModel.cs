using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace DemoMusicMVC.ViewModel
{
    public class SongViewModel
    {
        [Key]
        public int songId { get; set; }
        [Required, MaxLength(60, ErrorMessage = "Name Cannot Exceed 60 Characters")]
        [Display(Name = "Song Name")]
        public string songName { get; set; } = string.Empty;
        [Display(Name = "Photo For the Song")]
        public IFormFile? photo { get; set; }
        [Required]
        [Display(Name = "Song File")]
        [DataType(DataType.Upload)]
        public IFormFile? song { get; set; }
    }
}

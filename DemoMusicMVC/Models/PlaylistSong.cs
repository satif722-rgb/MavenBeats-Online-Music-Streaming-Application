using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoMusicMVC.Models
{
    public class PlaylistSong
    {
        [Key]
        public int PlaylistSongId { get; set; }

        [Required]
        public int PlaylistId { get; set; }

        [ForeignKey("PlaylistId")]
        public virtual Playlist? Playlist { get; set; }

        [Required]
        public int SongId { get; set; }

        [ForeignKey("SongId")]
        public virtual Song? Song { get; set; }
    }
}

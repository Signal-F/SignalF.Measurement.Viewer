using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalF.Measurement.Viewer.Models.SignalFDb
{
    [Table("Building", Schema = "dbo")]
    public partial class Building
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Room> Rooms { get; set; }

    }
}
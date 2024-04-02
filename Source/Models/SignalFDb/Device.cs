using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalF.Measurement.Viewer.Models.SignalFDb
{
    [Table("Device", Schema = "dbo")]
    public partial class Device
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid RoomId { get; set; }

        public Room Room { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public byte State { get; set; }

        public ICollection<Measurement> Measurements { get; set; }

    }
}
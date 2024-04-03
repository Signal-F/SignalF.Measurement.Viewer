using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalF.Measurement.Viewer.Models.SignalFDb
{
    [Table("Room", Schema = "dbo")]
    public partial class Room
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [ConcurrencyCheck]
        public Guid BuildingId { get; set; }

        public Building Building { get; set; }

        [Required]
        [ConcurrencyCheck]
        public string Number { get; set; }

        public ICollection<Device> Devices { get; set; }

    }
}
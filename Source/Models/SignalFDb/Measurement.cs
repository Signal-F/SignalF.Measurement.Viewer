using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SignalF.Measurement.Viewer.Models.SignalFDb
{
    [Table("Measurement", Schema = "dbo")]
    public partial class Measurement
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid DeviceId { get; set; }

        public Device Device { get; set; }

        [Required]
        public double Value { get; set; }

        [Timestamp]
        [Required]
        public byte[] Timestamp { get; set; }

    }
}
﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ExpofairTourPlanung.Models
{
    [Table("Tour", Schema = "expofair")]
    public partial class Tour
    {
        [Key]
        public int IdTour { get; set; }
        [StringLength(200)]
        public string TourName { get; set; }
        public int TourNr { get; set; }
        public string Comment { get; set; }
        public string StartWorkDay { get; set; }
        public string Footer { get; set; }
        [Column(TypeName = "date")]
        public DateTime TourDate { get; set; }
        [StringLength(20)]
        public string VehicleNr { get; set; }
        [StringLength(200)]
        public string Driver { get; set; }
        [StringLength(200)]
        public string Master { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal? Weight { get; set; }
        [Column(TypeName = "decimal(18, 0)")]
        public decimal? Volume { get; set; }
        [StringLength(200)]
        public string SecDriver { get; set; }
        [StringLength(1000)]
        public string Team { get; set; }
        [StringLength(300)]
        public string HeadLine { get; set; }
        [StringLength(100)]
        public string Hubwagen { get; set; }
        [StringLength(100)]
        public string Sackkarre { get; set; }
        [StringLength(200)]
        public string Sonstiges { get; set; }
        [StringLength(100)]
        public string CreatedBy { get; set; }
        [Column("IsSBTour")]
        public bool? IsSbtour { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreateTime { get; set; }
    }
}

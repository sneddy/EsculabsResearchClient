namespace Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public enum Gender
    {
        Male,
        Female
    };

    [Table("public.patients")]
    public partial class Patient
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("first_name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(255)]
        [Column("middle_name")]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(255)]
        [Column("last_name")]
        public string LastName { get; set; }

        [Column("birthdate", TypeName = "date")]
        public DateTime Birthdate { get; set; }

        [Column("tp")]
        public double? TP { get; set; }

        [Column("scd")]
        public double? SCD { get; set; }

        [Required]
        [StringLength(12)]
        [Column("iin")]
        public string IIN { get; set; }

        [Required]
        [Column("gender")]
        public Gender Gender { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.Core.Models.Agent
{
    public class branch_model
    {
        [Key]
        public int Ud { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? BuildingNumber { get; set; }
        public string? CityName { get; set; }
        public string? PostalZone { get; set; }
        public string? Country { get; set; } = "SA";
        public string? Governate { get; set; } = "السعودية";
        public string? Street { get; set; }
        public string? Building_Number { get; set; }
        public string? Plot_Identification { get; set; } = "1111";
        public string? City_Subdivision_Name { get; set; } = "1111";
        [NotMapped]
        public int SingletonId { get; set; }

    }
}

using System.Collections.Generic;

namespace Mw3dy.Models
{
    public class Branch
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;

        public string CityEn { get; set; } = string.Empty;
        public string CityAr { get; set; } = string.Empty;

        public string AddressEn { get; set; } = string.Empty;
        public string AddressAr { get; set; } = string.Empty;

        public string HoursEn { get; set; } = string.Empty;
        public string HoursAr { get; set; } = string.Empty;

        public double DistanceKm { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

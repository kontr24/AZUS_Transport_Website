using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AZUS_Transport_Website.Models
{
    public partial class FilteringApplications
    {
        public IEnumerable<Applications> Applications { get; set; }
        public IEnumerable<Users> Users { get; set; }

        public Users user { get; set; }
        public Applications application { get; set; }

        public bool archiveApplications { get; set; } //запомнить пользователя (checkBox)

        public SelectList TypeCars { get; set; }
        //public SelectList Director { get; set; }

    }

}
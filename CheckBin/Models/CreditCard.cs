using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckBin.Models
{
    public class CreditCard
    {
        public string bin { get; set; }
        public string brand { get; set; }
        public string sub_brand { get; set; }
        public string country_code { get; set; }
        public string country_name { get; set; }
        public string bank { get; set; }
        public string card_type { get; set; }
        public string card_category { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public string query_time { get; set; }

    }
}

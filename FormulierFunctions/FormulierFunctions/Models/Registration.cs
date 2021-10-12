using System;
using System.Collections.Generic;
using System.Text;

namespace FormulierFunctions.Models
{
    public class Registration
    {
        public string RegistrationId { get; set; }
        public string lastName { get; set; }
        public string firstName { get; set; }
        public string email { get; set; }
        public string zipcode { get; set; }
        public int age { get; set; }
        public bool isFirstTimer { get; set; }     
    }
}

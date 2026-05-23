using System;
using System.Collections.Generic;

namespace LodAcc
{
    public class Shipment
    {
        public string ShipmentId { get; set; }       
        public string Origin { get; set; }         
        public string Destination { get; set; }      
        public DateTime DepartureDate { get; set; }  
        public DateTime ArrivalDate { get; set; }    
        public string Status { get; set; }          
        public List<CargoItem> Cargo { get; set; }   
    }
}
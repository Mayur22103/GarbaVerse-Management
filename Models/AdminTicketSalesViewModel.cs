using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineEventsManagementSystemMVC.Models
{
    public class AdminTicketSalesViewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
        public decimal Price { get; set; }
        public int TicketsSold { get; set; }
    }
}
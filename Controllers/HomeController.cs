using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using OnlineEventsManagementSystemMVC.Models;
using System.Web.Security;

namespace OnlineEventsManagementSystemMVC.Controllers
{

    public class HomeController : Controller
    {
      EventsDataEntities1 db = new EventsDataEntities1();
        
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Events()
        {
            return View();
        }

        public ActionResult Gallery()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult Registration()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            // Sign out the user
            FormsAuthentication.SignOut();

            // Optionally clear the session
            Session.Clear();

            // Redirect to the login page or home page
            return RedirectToAction("Login", "Home");
        }

        public ActionResult EventList()
        {
            using (EventsDataEntities1 db = new EventsDataEntities1())
            {
                // Retrieve all events from the database
                var events = db.TblEvents.ToList();

                // Pass the list of events to the view
                return View(events);
            }
        }

        public ActionResult AddEvent()
        {
            return View();
        }


        public ActionResult UpdateEvent()
        {
            using (EventsDataEntities1 db = new EventsDataEntities1())
            {
                // Get the current user's name from the session
                string currentUserName = Session["UserName"]?.ToString(); // Adjust this if you're storing the user's identifier differently

                // Retrieve events created by the specific user
                var events = db.TblEvents.Where(e => e.CreatedBy == currentUserName).ToList();

                // Pass the list of events to the view
                return View(events);
            }
        }

        public ActionResult UpdateEventData(int id)
        {
            var evt = db.TblEvents.Find(id);
            if (evt == null)
            {
                return HttpNotFound();
            }
            return View(evt);
        }

        [HttpPost]
        public ActionResult UpdateData(TblEvent updatedEvent)
        {
            using (EventsDataEntities1 db = new EventsDataEntities1())
            {
                // Check if the event exists
                var existingEvent = db.TblEvents.Find(updatedEvent.Id);
                if (existingEvent == null)
                {
                    return HttpNotFound();
                }

                // Update the properties of the existing event
                existingEvent.EventName = updatedEvent.EventName;
                existingEvent.Location = updatedEvent.Location;
                existingEvent.EventDate = updatedEvent.EventDate;
                existingEvent.Description = updatedEvent.Description;
                existingEvent.Price = updatedEvent.Price; // Assuming you added a Price field

                // Mark the entity as modified
                db.Entry(existingEvent).State = EntityState.Modified;

                // Save the changes
                db.SaveChanges();

                return RedirectToAction("UpdateEvent"); // Redirect to your event list or success page
            }
        }


        public ActionResult DeleteEvent(int id)
        {
            var evt = db.TblEvents.Find(id);
            if (evt != null)
            {
                db.TblEvents.Remove(evt);
                db.SaveChanges();
            }
            return RedirectToAction("UpdateEvent");
        }


        // POST: Add Event Data
        [HttpPost]
        public ActionResult AddEventData(string EventName, string Location, DateTime EventDate, string Description, decimal Price)
        {
            using (EventsDataEntities1 db = new EventsDataEntities1())
            {
                // Validate the input
                if (string.IsNullOrEmpty(EventName) || string.IsNullOrEmpty(Location) ||
                    EventDate == default(DateTime) || string.IsNullOrEmpty(Description) || Price <= 0)
                {
                    ModelState.AddModelError("", "All fields are required and price must be greater than zero.");
                    return View("AddEvent"); // Return to the form view with error
                }

                // Get the admin name from the session or your authentication context
                string adminName = Session["UserName"]?.ToString(); // Ensure you have the admin's name in session

                // Create a new event object
                var newEvent = new TblEvent
                {
                    EventName = EventName,
                    Location = Location,
                    EventDate = EventDate,
                    Description = Description,
                    Price = Price,
                    CreatedBy = adminName // Set the CreatedBy property
                };

                // Add the new event to the database
                db.TblEvents.Add(newEvent);
                db.SaveChanges();

                ViewBag.Message = "Event added successfully!";
                return RedirectToAction("UpdateEvent"); // Redirect to the event list or a success page
            }
        }



        [HttpPost]
        public ActionResult Registration(string name, string userName, decimal phone, string password, string confirmPassword, string address)
        {

                // Simple validation
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(userName)  || string.IsNullOrEmpty(password) ||
                    string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(address))
                {
                    ModelState.AddModelError("", "All fields are required.");
                    return View(); // Return the view with errors
                }

                if (password != confirmPassword)
                {
                    ModelState.AddModelError("confirmPassword", "Passwords do not match.");
                    return View("Index"); // Return the view with errors
                }

                // Create a new TblRegister object
                var newUser = new TblRegister
                {
                    name = name,
                    userName = userName,
                    phone = phone,
                    password = password, // Consider hashing the password in a real application
                    address = address
                };

                // Add the new user to the database
                db.TblRegisters.Add(newUser);
                db.SaveChanges();

                ViewBag.Message = "Registration Successful"; // Set a success message
                return RedirectToAction("Index"); // Redirect to a success action
            
        }

        public ActionResult chkLogin(string userName, string password, string role)
        {
            using (EventsDataEntities1 db = new EventsDataEntities1())
            {
                // Validate input
                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Username, Password, and Role are required.");
                    return View("Login");
                }

                // Check if the user is trying to log in as an Admin
                if (role == "Admin")
                {
                    // Find the admin in the database
                    var admin = db.Admins.FirstOrDefault(a => a.AdminName == userName && a.Password == password);

                    if (admin != null)
                    {
                        // If admin is found, set session values and redirect to Admin Dashboard
                        Session["UserName"] = admin.AdminName;
                        Session["Role"] = "Admin";
                        FormsAuthentication.SetAuthCookie(admin.AdminName, false);

                        ViewBag.Message = "Admin login successful!";
                        return RedirectToAction("Index", "Home"); // Redirect to admin dashboard
                    }
                    else
                    {
                        // Admin not found, show error message
                        ModelState.AddModelError("", "Invalid Admin credentials.");
                        return View("Login");
                    }
                }
                else if (role == "User")
                {
                    // Find the user in the TblRegisters table
                    var user = db.TblRegisters.FirstOrDefault(u => u.userName == userName && u.password == password);

                    if (user != null)
                    {
                        // If user is found, set session values and redirect to User Dashboard
                        Session["UserName"] = user.userName;
                        Session["Role"] = "User";
                        FormsAuthentication.SetAuthCookie(user.userName, false);

                        ViewBag.Message = "User login successful!";
                        return RedirectToAction("Index", "Home"); // Redirect to user home page or dashboard
                    }
                    else
                    {
                        // User not found, show error message
                        ModelState.AddModelError("", "Invalid User credentials.");
                        return View("Login");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid role selected.");
                    return View("Login");
                }
            }
        }






        public ActionResult BuyTicket(int id)
        {
            // Fetch event details by ID
            var evt = db.TblEvents.Find(id);
            if (evt == null)
            {
                return HttpNotFound();
            }

            // Pass the event details to the view
            return View(evt);
        }

        [HttpPost]
        public ActionResult ConfirmPurchase(int id)
        {
            // Find the event being purchased
            var evt = db.TblEvents.Find(id);
            if (evt == null)
            {
                return HttpNotFound();
            }

            // Create a new ticket object
            var ticket = new Ticket
            {
                EventId = evt.Id,
                UserName = User.Identity.Name, // Assuming you have a user logged in
                PurchaseDate = DateTime.Now
            };

            // Add the ticket to the database
            db.Tickets.Add(ticket);
            db.SaveChanges();

            // Optionally, redirect to a confirmation view or the event list
            return RedirectToAction("PurchaseConfirmation", new { id = evt.Id });
        }

        public ActionResult PurchaseConfirmation(int id)
        {
            // Optionally fetch the event details to display confirmation
            var evt = db.TblEvents.Find(id);
            if (evt == null)
            {
                return HttpNotFound();
            }

            return View(evt);
        }



        public ActionResult MyTickets()
        {
            // Assuming User.Identity.Name contains the username of the logged-in user
            var userName = User.Identity.Name;

            // Fetch the tickets for the logged-in user
            var tickets = db.Tickets
                            .Where(t => t.UserName == userName)
                            .Include(t => t.TblEvent) // Ensure to include related event data
                            .ToList();

            return View(tickets);
        }




         public ActionResult SoldTickets()
         {

                    string adminUsername = Session["UserName"]?.ToString();
                    using (EventsDataEntities1 db = new EventsDataEntities1())
                            {
                                // Query to get events created by the specified admin and the count of tickets sold for each event
                                var ticketSales = from e in db.TblEvents
                                                  join t in db.Tickets on e.Id equals t.EventId into ticketsGroup
                                                  where e.CreatedBy == adminUsername // Filter by admin username
                                                  select new AdminTicketSalesViewModel
                                                  {
                                                      EventId = e.Id,
                                                      EventName = e.EventName,
                                                      Location = e.Location,
                                                      EventDate = e.EventDate,
                                                      Price = e.Price,
                                                      TicketsSold = ticketsGroup.Count() // Count of tickets sold
                                                  };

                                var salesList = ticketSales.ToList(); // Convert to list

                                return View(salesList);
                     }
          }
          






}
}
    

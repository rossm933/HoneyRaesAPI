using HoneyRaesAPI.Models;
namespace HoneyRaesAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<Customer> customers = new List<Customer> {

     new Customer()
     { Name = "Andrew",
       Id = 74,
        Address = "123 Fake Dr."
     },

     new Customer()
     {
         Name = "Taylor",
         Id = 3,
         Address = "545 Center Dr."
     },

     new Customer()
     {
         Name = "Derek",
         Id = 65,
         Address = "384 Happy Dr."
     }

     };
            List<Employee> employees = new List<Employee> {

    new Employee()
    {
        Name = "Harry",
        Id = 9,
        Specialty = "Surgeon"
    },
    new Employee()
    {
        Name = "Gary",
        Id = 12,
        Specialty = "Janitor"
    }

};
            List<ServiceTicket> serviceTickets = new List<ServiceTicket> {

    new ServiceTicket()
    {
        Id = 13,
        CustomerId = 65,
        EmployeeId = 9,
        Description = "Open heart surgery",
        Emergency = true,
        DateCompleted = new DateTime(2022, 5, 19)
    },

    new ServiceTicket()
    {
        Id = 24,
        CustomerId = 74,
        EmployeeId = 9,
        Description = "Check-up",
        Emergency = false,
        DateCompleted = new DateTime(2024, 3, 1)
    },

    new ServiceTicket()
    {
        Id = 56,
        CustomerId = 3,
        EmployeeId = 12,
        Description = "Fever",
        Emergency = true,
        DateCompleted = new DateTime(2024, 6, 13)
    },

    new ServiceTicket()
    {
        Id = 36,
        CustomerId = 74,
        Description = "Broken leg",
        Emergency = true,
 
      
    },

        new ServiceTicket()
    {
        Id = 16,
        CustomerId = 74,
        EmployeeId = 0,
        Description = "Bad knee",
        Emergency = false,
        DateCompleted = new DateTime(2024, 4, 29)
    }
};
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            var summaries = new[]
            {
                "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
            };

            app.MapGet("/api/servicetickets", () =>
            {
                return serviceTickets;
            });

            app.MapGet("/api/servicetickets/{id}", (int id) =>
            {
                ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(e => e.Id == id);
                if (serviceTicket == null)
                {
                    return Results.NotFound();
                }
                serviceTicket.Customer = customers.FirstOrDefault(e => e.Id == serviceTicket.CustomerId);
                serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
                return Results.Ok(serviceTicket);
            });

            app.MapGet("/api/customer", () =>
            {
                return customers;
            });

            app.MapGet("/api/customer/{id}", (int id) =>
            {
                Customer customer = customers.FirstOrDefault(e => e.Id == id);
                if (customer == null)
                {
                    return Results.NotFound();
                }
                customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
                return Results.Ok(customer);
            });

            app.MapGet("/api/employee", () =>
            {
                return employees;
            });

            app.MapGet("/api/employee/{id}", (int id) =>
            {
                Employee employee = employees.FirstOrDefault(e => e.Id == id);
                if (employee == null)
                {
                    return Results.NotFound();
                }
                employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
                return Results.Ok(employee);
            });

            app.MapPost("/api/servicetickets", (ServiceTicket serviceTicket) =>
            {
                // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
                serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
                serviceTickets.Add(serviceTicket);
                return serviceTicket;
            });
            app.MapDelete("/api/servicetickets/{id}", (int id) =>
            {
             ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
                serviceTickets.Remove(serviceTicket);
            });

            app.MapPut("/api/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
            {
                ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
                int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
                if (ticketToUpdate == null)
                {
                    return Results.NotFound();
                }
                //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
                if (id != serviceTicket.Id)
                {
                    return Results.BadRequest();
                }
                serviceTickets[ticketIndex] = serviceTicket;
                return Results.Ok();
            });

            app.MapPost("/api/serviceTickets/{id}/complete", (int id) =>
            {
                ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
                ticketToComplete.DateCompleted = DateTime.Today;
            });

            app.MapGet("/api/serviceTickets/emergencies", () =>

            {
                List<ServiceTicket> emergencies = serviceTickets.Where(st => st.Emergency == true && st.DateCompleted == null).ToList();
                return Results.Ok(emergencies);
            });
            app.MapGet("/apiserviceTickets/unassigned", () =>
            {
                List<ServiceTicket> unassigned = serviceTickets.Where(st => st.EmployeeId == null).ToList();
                return Results.Ok(unassigned);

            });

            app.MapGet("/api/customers/inactive", () =>
            {
                var inactiveCustomers = customers
                    .Where(c =>
                        !serviceTickets.Any(st =>
                            st.CustomerId == c.Id &&
                            st.DateCompleted.HasValue && st.DateCompleted.Value > DateTime.Now.AddYears(-1)
                        )
                    )
                    .ToList();

                return Results.Ok(inactiveCustomers);
            });

            app.MapGet("/api/employee/available", () =>
            {
                var availableEmployees = serviceTickets.Where(st => st.DateCompleted == null).Select(st => st.EmployeeId).ToList();
                var available = employees.Where(st => !availableEmployees.Contains(st.Id)).ToList();
                return Results.Ok(available);
            });

            app.MapGet("/api/employee/{id}/customers", (int id) =>
            {
                var employeeCustomers = serviceTickets
                .Where(st => st.EmployeeId == id)
                .Select(st => customers.FirstOrDefault(c => c.Id == st.CustomerId))
                .Distinct()
                .ToList();

                return Results.Ok(employeeCustomers);
            });

            app.MapGet("/api/employee/employeeofthemonth", () =>
            {
                var lastMonth = DateTime.Now.AddMonths(-1);
                var employeeOfTheMonth = employees
                    .OrderByDescending(e => serviceTickets.Count(st => st.EmployeeId == e.Id && st.DateCompleted.HasValue && st.DateCompleted.Value.Month == lastMonth.Month))
                    .FirstOrDefault();

                return Results.Ok(employeeOfTheMonth);
            });

            app.MapGet("/api/serviceTickets/review", () =>
            {
                List<ServiceTicket> completedTickets = serviceTickets
                .Where(st => st.DateCompleted.HasValue)
                .OrderBy(st => st.DateCompleted)
                .ToList();

                foreach (var ticket in completedTickets)
                {
                    ticket.Customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
                    ticket.Employee = employees.FirstOrDefault(e => e.Id == ticket.EmployeeId);
                }

                return Results.Ok(completedTickets);
            });

            app.MapGet("/api/serviceTickets/prioritized", () =>
            {
                var prioritizedTickets = serviceTickets
                .Where(st => !st.DateCompleted.HasValue)
                .OrderByDescending(st => st.Emergency)
                .ThenBy(st => st.EmployeeId.HasValue)
                .ToList();

                foreach (var ticket in prioritizedTickets)
                {
                    ticket.Customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
                    ticket.Employee = employees.FirstOrDefault(e => e.Id == ticket.EmployeeId);
                }

                return Results.Ok(prioritizedTickets);
            });
            app.Run();
        }
    }
}

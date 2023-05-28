using CustomersApi.Interfaces;
using CustomersApi.Models;
using CustomersApi.Data;
using Microsoft.EntityFrameworkCore;

namespace CustomersApi.Services
{
    public class CustomerService : ICustomer
    {
        private ApiDbContext dbContext;

        public CustomerService()
        {
            dbContext = new ApiDbContext();
        }

        public async Task AddCustomer(Customer customer)
        {
            var vehicleInDb = await dbContext.Vehicles.FirstOrDefaultAsync(v => v.Id == customer.VehicleId);
            if (vehicleInDb == null)
            {
                await dbContext.Vehicles.AddAsync(customer.Vehicle);
                await dbContext.SaveChangesAsync();
            }
            customer.Vehicle = null;
            await dbContext.Customers.AddAsync(customer);
            await dbContext.SaveChangesAsync();
        }
    }
}

using CustomersApi.Interfaces;
using CustomersApi.Models;
using CustomersApi.Data;
using Microsoft.EntityFrameworkCore;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

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

            var customerObjAsText = JsonConvert.SerializeObject(customer);

            // For Azure Service Bus
            string connectionString = "Endpoint=sb://vehicletestdrivedn.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=R56PbNbqgdY6ICVx4zaQ7+d2VI/OrFGr/+ASbDixo78=";
            string queueName = "azureorderqueue";
            await using var client = new ServiceBusClient(connectionString);

            ServiceBusSender sender = client.CreateSender(queueName);

            ServiceBusMessage message = new ServiceBusMessage(customerObjAsText);

            await sender.SendMessageAsync(message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FleetManagement
{
    public class Worker : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public Worker(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new ConnectionFactory()
            {
                HostName = _configuration["Hostname"],
                Port = int.Parse(_configuration["Port"]),
                UserName = _configuration["Username"],
                Password = _configuration["Password"],
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(_configuration["QueueName"], true, false, false, null);
        }

        public void Start()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"Received message: {message}");
                await Task.Yield(); // Simulate some work
            };

            _channel.BasicConsume(_configuration["QueueName"], true, consumer);
            Console.WriteLine("Worker started. Press any key to stop.");
            Console.ReadKey();
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace EntailmentCheck
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var host = CreateDefaultBuilder().Build();
                using IServiceScope serviceScope = host.Services.CreateScope();
                IServiceProvider provider = serviceScope.ServiceProvider;
                var workerInstance = provider.GetRequiredService<TaskHelper>();
                workerInstance.Execute();

            }
            catch (Exception ex)
            {
                Console.WriteLine("error in syntax");
            }
    static IHostBuilder CreateDefaultBuilder()
            {
                return Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration(app =>
                    {
                        app.AddJsonFile("appsettings.json");
                    }).ConfigureServices(services =>
                    {
                        services.AddSingleton<TaskHelper>();
                    });
            }
        }

    }
}

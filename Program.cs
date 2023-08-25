using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace TradingToolbox.Applications.Trading.Modeler.ServiceApp
{
    /// <summary>
    /// Service application's main entry point.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The application arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The host builder arguments.</param>
        /// <returns>The initialized HostBuilder using the supplied arguments (args).</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}

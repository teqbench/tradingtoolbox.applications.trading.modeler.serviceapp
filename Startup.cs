using System;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TradingToolbox.System.Cors;
using TradingToolbox.System.Data.NoSql.MongoDB.Repository.Config;
using nsPositionModeling = TradingToolbox.Trading.Modeler.Data.NoSql.MongoDB.Services.PositionModeling;

namespace TradingToolbox.Applications.Trading.Modeler.ServiceApp
{
    /// <summary>
    /// Application's startup routine to be used by the web host when starting this service application.
    /// </summary>
    public class Startup
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            this.CorsPolicyManager = new PolicyManager(configuration, "corsPolicies");
        }
        #endregion

        #region Private Properties
        /// <summary>
        /// Gets the CORS policy manager.
        /// </summary>
        /// <value>
        /// The CORS policy manager.
        /// </value>
        private PolicyManager CorsPolicyManager { get; }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the application's configuration.
        /// </summary>
        /// <value>
        /// The application's configuration.
        /// </value>
        public IConfiguration Configuration { get; }
        #endregion

        #region Public Methods
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The services collection to add service configurations to.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            this.CorsPolicyManager.BuildPolicies(services);

            // Force generated API URLs to lowercase
            services.AddRouting(options => { options.LowercaseUrls = true; });

            // To have JSON Patch support in your app, have to add NewtonsoftJson package and call AddNewtonsoftJson
            // Note, also have to add AspNetCore.JsonPatch package

            // Forcd ALL serialization to be camelCase once instead of applying at property/field.
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                })
                .AddNewtonsoftJson();

            nsPositionModeling.MongoDbService mongoDbService = new nsPositionModeling.MongoDbService(Configuration.GetSection("tradingToolboxTradingModelerMongoDbRepositoryConfig").Get<RepositoryConfig>());

            // Per https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-3.1 the following 
            // AddSingleton method signature will NOT use automatic disposal. But in this case, need to create an instance
            // of the LogDocumentDbService so can supply values to its constructor. So, to ensure it will be disposed of when
            // the app shutsdown, See https://andrewlock.net/four-ways-to-dispose-idisposables-in-asp-net-core/ for correct
            // way to dispose using IApplicationLifetime event
            services.AddSingleton<nsPositionModeling.IMongoDbService>(mongoDbService);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Trading Toolbox - Trading Modeler - REST API", Version = "v1" });
            });

        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application request pipeline to configure.</param>
        /// <param name="env">Web hosting environment information.</param>
        /// <param name="lifetime">The application's lifetime event notifier.</param>
        /// <param name="mongoDbService">The mongo database service.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime, nsPositionModeling.IMongoDbService mongoDbService)
        {
            lifetime.ApplicationStopping.Register(state =>
            {
                ((IDisposable)state).Dispose();
            }, mongoDbService);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TradingToolbox.Applications.Trading.Modeler.ServiceApp v1");
                    c.RoutePrefix = string.Empty;
                });
            }

            this.CorsPolicyManager.UsePolicies(app);

            // Add middleware to redirect HTTP to HTTPS
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        #endregion
    }
}

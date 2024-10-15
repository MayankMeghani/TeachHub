using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Services;

namespace TeachHub
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddDbContext<TeachHubContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton<FirebaseService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<FirebaseService>>();
                var config = Configuration.GetSection("Firebase:StorageBucket").Value;
                return new FirebaseService(logger, config);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                logger.LogInformation($"Request Path: {context.Request.Path}");
                logger.LogInformation($"Request Method: {context.Request.Method}");

                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    logger.LogInformation($"Endpoint: {endpoint.DisplayName}");
                }
                else
                {
                    logger.LogWarning($"No endpoint found for route: {context.Request.Path}");
                }

                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                logger.LogInformation("Routes configured.");
            });

            logger.LogInformation("Application configured.");
        }
    }
}

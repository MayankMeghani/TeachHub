using Microsoft.EntityFrameworkCore;
using TeachHub.Data;
using TeachHub.Services;
using Microsoft.AspNetCore.Identity;
using TeachHub.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TeachHub
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            services.AddDbContext<TeachHubContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<TeachHubContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";

                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToLogin = context =>
                    {
                        context.HttpContext.Response.Redirect("/Account/Login?message=Please login to access this page");
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSingleton<FirebaseService>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<FirebaseService>>();
                var firebaseBucket = Configuration.GetSection("Firebase:StorageBucket").Value;
                return new FirebaseService(logger, firebaseBucket);
            });

            services.AddSingleton<StripeService>();

            services.AddControllersWithViews();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

 }

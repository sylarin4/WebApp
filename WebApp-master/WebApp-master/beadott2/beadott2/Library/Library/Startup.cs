using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Library.Models;
using Library.Contexts;
using Library.Utility;

namespace Library
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
            services.AddMvc().AddJsonOptions(
                options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            services.AddDbContext<LibraryContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("LibraryContext")));

            services.AddDbContext<LibrarianContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("LibrarianContext")));

            // Set dependency injection for authentication.
            services.AddIdentity<Guest, IdentityRole<int>>()
                .AddEntityFrameworkStores<LibraryContext>() 
                .AddDefaultTokenProviders();

            services.AddSecondIdentity<Librarian, IdentityRole<int>>()
                .AddEntityFrameworkStores<LibrarianContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                // Set password complexity.
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = false;
                options.Password.RequiredUniqueChars = 3;

                // Set lockout settings in case of login failed.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // Configuration for user management.
                options.User.RequireUniqueEmail = true;
            });

            // Set dependency injection for Google configurationcollection.
            services.Configure<GoogleConfig>(Configuration.GetSection("Google"));

            // Set dependency injections for library service.
            services.AddTransient<ILibraryService, LibraryService>();

            // Dependency injection beállítása az alkalmazás állapotra
            services.AddSingleton<ApplicationState>();

            // Use session.
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Session's max length is 15 minutes.
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                // Scripts from client aren't able to run.
                options.Cookie.HttpOnly = true;
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            
            app.UseSession();
            app.UseAuthentication();
            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Books}/{action=Index}/{id?}");
            });
            SeedData.Initialize(serviceProvider);

        }
    }
}

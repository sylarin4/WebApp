﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;


using AdventureGameEditor.Data;
using AdventureGameEditor.Models;

namespace AdventureGameEditor
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

            services.AddDbContext<AdventureGameEditorContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("AdventureGameEditorContext")));

            // Set dependency injection for authentication.
            services.AddIdentity<User, IdentityRole<int>>()
                .AddEntityFrameworkStores<AdventureGameEditorContext>()
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

                options.SignIn.RequireConfirmedEmail = false;
            });

            // Use session.
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Session's max length is 15 minutes.
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                // Scripts from client aren't able to run.
                options.Cookie.HttpOnly = true;
            });
            // Dependency injection beállítása az alkalmazás állapotra
            services.AddSingleton<ApplicationState>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<GoogleConfig>(Configuration.GetSection("Google"));

            // Set dependency injections for GameEditorService service.
            services.AddTransient<Models.Services.IGameEditorService, Models.Services.GameEditorService>();
            services.AddTransient<Models.Services.IGameplayService, Models.Services.GameplayService>();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseSession();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            SeedTestData.Initialize(serviceProvider);
        }
    }
}

﻿#pragma warning disable CS1591

using AspNetCoreRateLimit;
using Chandler.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.OpenApi.Models;
using System;
using System.Reflection;

namespace Chandler
{
    public class Startup
    {
        private readonly ServerConfig _config;
        private readonly Database _db;
        private readonly ServerMeta _meta;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _config = JsonConvert.DeserializeObject<ServerConfig>(File.ReadAllText($"{Directory.GetCurrentDirectory()}/Data/Configs/ServerConfig.json"));
            _db = new Database(_config.Provider, _config.ConnectionString);
            _meta = new ServerMeta();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region AspNetCoreRateLimit Stuff
            //Taken from https://github.com/stefanprodan/AspNetCoreRateLimit/wiki/IpRateLimitMiddleware#setup
            services.AddOptions();
            services.AddMemoryCache();
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            #endregion

            #region General Chandler Stuff
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Title = "CHANdler API Documentation",
                    Version = "v1",
                    License = new OpenApiLicense()
                    {
                        Name = "GNU General Public License v3.0",
                        Url = new Uri("https://github.com/Naamloos/CHANdler/blob/master/LICENSE")
                    }
                });

                x.IncludeXmlComments($"{AppContext.BaseDirectory}/{Assembly.GetExecutingAssembly().GetName().Name}.xml");
            });

            services.AddMvc(x => x.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddSingleton(_db);
            services.AddSingleton(_meta);
            services.AddSingleton(_config);
            services.AddCors(o => o.AddPolicy("publicpolicy", builder =>
            {
                builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
            }));

            using var ctx = _db.GetContext();
            ctx.Database.EnsureCreated();

            if (ctx.Boards.Count() == 0)
            {
                // insert debug thread data to database
                ctx.Boards.Add(new Data.Entities.Board()
                {
                    Name = "CHANdler",
                    Tag = "c",
                    Description = "CHANdler test board",
                    ImageUrl = "https://i.kym-cdn.com/photos/images/newsfeed/000/779/388/d33.jpg"
                });

                ctx.Boards.Add(new Data.Entities.Board()
                {
                    Name = "Random",
                    Tag = "r",
                    Description = "Random shit",
                });

                ctx.Boards.Add(new Data.Entities.Board()
                {
                    Name = "Memes",
                    Tag = "m",
                    ImageUrl = "https://img.thedailybeast.com/image/upload/c_crop,d_placeholder_euli9k,h_1440,w_2560,x_0,y_0/dpr_1.5/c_limit,w_1044/fl_lossy,q_auto/v1531451526/180712-Weill--The-Creator-of-Pepe-hero_uionjj",
                    Description = "haha cool and good dank memes",
                });

                var salt = Passworder.GenerateSalt();
                var (hash, cycles) = Passworder.GenerateHash(this._config.DefaultPassword, salt);

                ctx.Passwords.Add(new Data.Entities.Password()
                {
                    Id = -1,
                    Salt = salt,
                    Cycles = cycles,
                    Hash = hash
                });

                ctx.SaveChanges();
            }
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.EnvironmentName == "Development") app.UseDeveloperExceptionPage();

            app.UseSwagger();

            app.UseSwaggerUI(x =>
            {
                x.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                x.RoutePrefix = "docs";
            });

            app.UseCors("publicpolicy");
            app.UseIpRateLimiting();
            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Default",
                    template: "{controller=Page}/{Action=Index}");
            });
        }
    }
}
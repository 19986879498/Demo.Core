using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Builder;
using Demo.Core.IFactory;
using Demo.Core.Factory;
using Microsoft.EntityFrameworkCore;
using ServiceReference1;

namespace Demo.Core
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

            services.AddControllersWithViews().AddNewtonsoftJson();
            services.AddTransient<IService,Service>();
            services.AddTransient<IJhEmrService, JhEmrService>();
        //    services.AddTransient<SqlServiceSoap, SqlServiceSoapClient>();
            //   services.AddDbContextPool<DBContextOracle>(db => db.UseOracle(this.Configuration.GetConnectionString("Default").ToString(), item => item.UseOracleSQLCompatibility("11")));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo.Core", Version = "v1" });
            });
            //ÉèÖÃ¿çÓò
            services.AddCors(item => {
                item.AddPolicy("mycors", policy =>
                {
                    policy.AllowCredentials().AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:8080", "http://127.0.0.1:8080", "http://localhost:8081", "http://127.0.0.1:8081","http://192.168.2.43:80","http://111.47.69.7:8000","http://192.168.2.43","http://jyjg.zjsrmyy.com:8000");
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
              
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Demo.Core v1"));
            // app.UseHttpsRedirection();
            
            app.UseRouting();
            app.UseCors("mycors");
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

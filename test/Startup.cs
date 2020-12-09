using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using test.Models;
using Microsoft.EntityFrameworkCore;

namespace test
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            string catalogDb = "Server=(localdb)\\mssqllocaldb;Database=catalog;Trusted_Connection=True;";
            services.AddDbContext<CatalogContext>(options => options.UseSqlServer(catalogDb));

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapControllers(); // подключаем маршрутизацию на контроллеры
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PiLcdServer.Modeli;
using PiLcdServer.Servisi;

namespace PiLcdServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.Configure<AppSettings>(builder.Configuration);

            

            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);


            var configuration = builder.Configuration;
            var useUrls = configuration.GetSection("UseUrls").Get<List<string>>();

            // Set the application URLs early
            if (useUrls != null && useUrls.Count > 0)
            {
                builder.WebHost.UseUrls(useUrls.ToArray());
            }
            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton<LcdServis>();
            builder.Services.AddHostedService<RadnikServis>();

            var app = builder.Build();

            //var appSettings = app.Services.GetRequiredService<IOptions<AppSettings>>().Value;

            //builder.WebHost.UseUrls(appSettings.UseUrls.ToArray());

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //app.UseHttpsRedirection();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

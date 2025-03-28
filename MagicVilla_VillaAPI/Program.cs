using MagicVilla_VillaAPI.Services;

namespace MagicVilla_VillaAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            #region Support XML Formatting

            //builder.Services.AddControllers(options =>//If the application type is not JSON,
            //                                          //then we want to display an error message.
            //{
            //    options.ReturnHttpNotAcceptable = true;
            //}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();//to support XML formatting 
            #endregion

            #region Using Serilog
            //Configure Serilog
            //Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
            //    .WriteTo.File("log/villalog.txt", rollingInterval: RollingInterval.Day).CreateLogger();

            //To Use Serilog
            //builder.Host.UseSerilog();
            #endregion

            builder.Services.AddControllers().AddNewtonsoftJson();

            builder.Services.AddSingleton<ILogging, LoggingV2>();

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

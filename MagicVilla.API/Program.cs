using MagicVilla.API.Middlewares;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Filters;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Repository;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

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

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddResponseCaching();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IVillaRepository, VillaRepository>();
            builder.Services.AddScoped<IVillaNumberRepository, VillaNumberRepository>();
            builder.Services.AddAutoMapper(typeof(MappingConfig));

            builder.Services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
            });
            builder.Services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(x =>
             {
                 x.RequireHttpsMetadata = false;
                 x.SaveToken = true;
                 x.TokenValidationParameters = new TokenValidationParameters
                 {
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                     ValidateIssuer = true,
                     ValidIssuer = "https://localhost:7001",
                     ValidateAudience = true,
                     ValidAudience = "dotnetmastery.com",
                     ClockSkew = TimeSpan.Zero,
                 };
             });



            builder.Services.AddControllers(option =>
            {
                option.Filters.Add<CustomExceptionFilter>();
            }).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.ClientErrorMapping[StatusCodes.Status500InternalServerError] = new ClientErrorData
                {
                    Link = "https://dotnetmaster/500"
                };
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Magic_VillaV2");
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Magic_VillaV1");

                });
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v2/swagger.json", "Magic_VillaV2");
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Magic_VillaV1");
                    options.RoutePrefix = "";
                });
            }
            //app.UseExceptionHandler("/ErrorHandling/ProcessError");

            //app.HandleError(app.Environment.IsDevelopment());

            app.UseMiddleware<CustomExceptionMiddleware>();

            app.UseStaticFiles();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            ApplyMigrations();
            app.Run();


            void ApplyMigrations()
            {
                using (var scope = app.Services.CreateScope())
                {
                    var _db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    if (_db.Database.GetPendingMigrations().Any())
                    {
                        _db.Database.Migrate();
                    }
                }
            }
        }
    }
}

using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

namespace test_b2c_authorize
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
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
            {
                opt.Audience = "{clientId}";
                opt.Authority = "https://login.microsoftonline.com/{tenantId}/";
                opt.MetadataAddress = "https://{domain}.b2clogin.com/{yourb2cdomain}.onmicrosoft.com/B2C_1_{singupSignInPolicyName}/v2.0/.well-known/openid-configuration";
            });
            services.AddControllers();

            ConfigureSwagger(services);
        }

        private static void ConfigureSwagger(IServiceCollection services)
        {
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v2", new OpenApiInfo { Title = "CCPC API", Version = "v2" });

                var securitySchema = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "JWT Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lower case
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                config.AddSecurityDefinition(securitySchema.Reference.Id, securitySchema);

                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        securitySchema,
                        Array.Empty<string>()
                    }
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

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v2/swagger.json", "API V2");
                config.DocumentTitle = "API";
                config.RoutePrefix = string.Empty;

                config.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
                config.DisplayRequestDuration();
                config.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                config.EnableDeepLinking();
                config.EnableFilter();
                config.ShowExtensions();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

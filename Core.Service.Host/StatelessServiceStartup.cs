using Core.Service.Host.ApplicationBuilderExtensions;
using Core.Service.Host.Convention.Configuration;
using Core.Service.Host.Convention.Convention;
using Core.Tool;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;

namespace Core.Service.Host
{
    public abstract class StatelessServiceStartup
    {
       protected virtual Type[] ServiceContractTypes => new Type[] { };

        protected StatelessServiceStartup()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            Configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile($"appsettings.json", reloadOnChange: true, optional: false)
                .AddJsonFile($"appsettings.{environmentName}.json", reloadOnChange: true, optional: true)
                .AddEnvironmentVariables()
                .Build();
        }

        protected IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<IServiceEndpointConvention, ServiceEndpointConvention>();

            services.Configure<ServiceSettings>(Configuration.GetSection(ServiceSettings.SectionName));
            services.Configure<ApplicationConfig>(Configuration.GetSection(ApplicationConfig.SectionName));

            if (_addSwaggerGen != null)
            {
                services.AddSwaggerGen(x => _addSwaggerGen(x));
            }

            services.AddControllers();
            RegisterStatelessService(services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (_useSwagger != null)
            {
                app.UseSwagger(x => _useSwagger(app.ApplicationServices, x));
            }

            if (_useSwaggerUi != null)
            {
                app.UseSwaggerUI(x => _useSwaggerUi(app.ApplicationServices, x));
            }


            app.UseRouting();

            const string healthPath = "/tool/health";
            const string printEnvPath = "/tool/printEnv";
            const string printOptPath = "/tool/printOpt";

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet(healthPath, async context =>
                {
                    await context.Response.WriteAsync($"===> OK <=== \n {Tools.Json.Serializer.Serialize(context.Request.Headers)}");
                });

                endpoints.MapGet(printEnvPath, async context =>
                {
                    await context.Response.WriteAsync($"{Tools.Json.Serializer.Serialize(Environment.GetEnvironmentVariables(), Formatting.Indented)}");
                });

                endpoints.MapGet(printOptPath, async context =>
                {
                    await context.Response.WriteAsync($"{Tools.Json.Serializer.Serialize(ResolveServiceConfigObject(app), Formatting.Indented)}");
                });
            });

            var serviceEndpointConvention = app.ApplicationServices.GetRequiredService<IServiceEndpointConvention>();

            app.UseServiceEndpoints(ServiceContractTypes, serviceEndpointConvention);
        }


        protected abstract void RegisterStatelessService(IServiceCollection c);
        protected virtual ServiceSettings ResolveServiceConfigObject(IApplicationBuilder app) => app.ApplicationServices.GetRequiredService<IOptions<ServiceSettings>>().Value;


        //IServiceConfigurator for allowed by design services only
        private Action<SwaggerGenOptions> _addSwaggerGen;
        private Action<IServiceProvider, SwaggerOptions> _useSwagger;
        private Action<IServiceProvider, SwaggerUIOptions> _useSwaggerUi;
        protected void ConfigureSwagger(Action<SwaggerGenOptions> addSwaggerGen, Action<IServiceProvider, SwaggerOptions> useSwagger, Action<IServiceProvider, SwaggerUIOptions> useSwaggerUi)
        {
            _addSwaggerGen = addSwaggerGen;
            _useSwagger = useSwagger;
            _useSwaggerUi = useSwaggerUi;
        }
    }
}

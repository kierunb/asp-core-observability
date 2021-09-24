using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;

namespace asp_core_observability
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

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "asp_core_observability", Version = "v1" });
            });

            services.AddHttpClient();

            // external logger configuration
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSeq();
            });

            // tracing with opentelemetry
            services.AddOpenTelemetryTracing(telemetry =>
            {
                telemetry.SetResourceBuilder(ResourceBuilder
                    .CreateDefault()
                    .AddService(Configuration.GetValue<string>("Zipkin:ServiceName")));
                telemetry.AddZipkinExporter();
                telemetry.AddConsoleExporter();
                telemetry.AddSource("Sample.DistributedTracing"); // custom source registration
                telemetry.AddGrpcClientInstrumentation();
                telemetry.AddHttpClientInstrumentation();
                telemetry.AddAspNetCoreInstrumentation();
            });

            services.AddHealthChecks()
                .AddCheck<ExampleHealthCheck>(
                    "example_health_check",
                    failureStatus: HealthStatus.Degraded,
                    tags: new[] { "example" });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "asp_core_observability v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            // Use the Prometheus middleware for metrics
            app.UseMetricServer();
            app.UseHttpMetrics();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}

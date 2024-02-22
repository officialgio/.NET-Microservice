using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Play.Common.Settings;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Settings;

namespace Play.Identity.Service;

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
        // Manual Configurations - Keep original types when inserting MongoDB Docouments
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

        // Bindings
        var serviceSettings = Configuration.GetSection(nameof(ServiceSettings)).Get<ServiceSettings>();
        var mongoDbSettings = Configuration.GetSection(nameof(MongoDBSettings)).Get<MongoDBSettings>();
        var identityServiceSettings = Configuration.GetSection(nameof(IdentityServerSettings)).Get<IdentityServerSettings>();

        // Default Configurations for Identity Mongo DB
        services.AddDefaultIdentity<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
            (
                mongoDbSettings.ConnectionString,
                serviceSettings.ServiceName
            );

        // Default Configurations for Identity Service
        services
            .AddIdentityServer(options =>
            {
                // For logging purposes
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
            })
            .AddAspNetIdentity<ApplicationUser>()
            .AddInMemoryApiScopes(identityServiceSettings.ApiScopes)
            .AddInMemoryApiResources(identityServiceSettings.ApiResources)
            .AddInMemoryClients(identityServiceSettings.Clients)
            .AddInMemoryIdentityResources(identityServiceSettings.identityResources);

        services.AddControllers();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Identity.Service", Version = "v1" });
        });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Identity.Service v1"));
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        // Add this after UseRouting()
        app.UseIdentityServer();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();
        });
    }
}

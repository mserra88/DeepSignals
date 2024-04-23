using DeepSignals.Hubs;
using DeepSignals.Services;
using DeepSignals.Workers;
using Microsoft.AspNetCore.ResponseCompression;

namespace DeepSignals
{
    public class StartUp
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public virtual void ConfigureServices(IServiceCollection services)
        {
            /*
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("https://example.com")
                            .AllowAnyHeader()
                            .WithMethods("GET", "POST")
                            .AllowCredentials();
                    });
            });

            // UseCors must be called before MapHub.
            app.UseCors();
            */
            /*
            services.AddHttpClient<WeatherClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7035/");
            });
            */

            services.AddHttpClient();
            services.AddSession();
            services.AddHttpContextAccessor();
            services.AddDistributedMemoryCache();

            services.AddRazorPages();

            services.AddSignalR(options =>
            {

                //options.DisableImplicitFromServicesParameters = true;
            }).AddMessagePackProtocol();
            //builder.Services.AddSingleton<IUserIdProvider, NameUserIdProvider>();

            /*
            AddMessagePackProtocol(options =>
                 {
                     StaticCompositeResolver.Instance.Register(
                         MessagePack.Resolvers.GeneratedResolver.Instance,
                         MessagePack.Resolvers.StandardResolver.Instance
                     );
                     options.SerializerOptions = MessagePackSerializerOptions.Standard
                         .WithResolver(StaticCompositeResolver.Instance)
                         .WithSecurity(MessagePackSecurity.UntrustedData);
                 });

             .AddMessagePackProtocol(options =>
              {
                  options.SerializerOptions = MessagePackSerializerOptions.Standard
                      .WithResolver(new CustomResolver())
                      .WithSecurity(MessagePackSecurity.UntrustedData);
              });
            */

            services.AddServerSideBlazor(options =>
            {
                options.DetailedErrors = false;
                options.DisconnectedCircuitMaxRetained = 100;
                options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(1);
                options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
                options.MaxBufferedUnacknowledgedRenderBatches = 10;
                //options.DisconnectedCircuitMaxRetained = 0;
            }).AddHubOptions(options =>
            {
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
                options.EnableDetailedErrors = false;
                options.HandshakeTimeout = TimeSpan.FromSeconds(15);
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.MaximumParallelInvocationsPerClient = 1;
                options.MaximumReceiveMessageSize = 32 * 1024;
                options.StreamBufferCapacity = 10;
            });
            
            services.AddSingleton<SQLService>();
            services.AddSingleton<ClientManagerService>();
            services.AddSingleton<TickerDataService>();

            services.AddScoped<ProtectedStorageService>();
            services.AddScoped<SignalRService>();
            services.AddHostedService<HubMainWorker>();
            services.AddHostedService<HubDisconnectionWorker>();

            services.AddMemoryCache(options =>
            {
                options.SizeLimit = 1048576; // Max cache size in bytes (1 MB)
                options.ExpirationScanFrequency = TimeSpan.FromMinutes(1); // Frequency to scan for expired items
                options.CompactionPercentage = 1;
            });

            services.AddResponseCaching(options =>
            {
                options.MaximumBodySize = 1024;
                options.UseCaseSensitivePaths = true;
            });

            services.AddResponseCompression(options =>
            {
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
            });

            /*
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = "JwtIssuer",
                            ValidAudience = "JwtAudience",
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JwtSecurityKey"))
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                var accessToken = context.Request.Query["access_token"];
                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    context.Token = accessToken;
                                }
                                return Task.CompletedTask;
                            }
                        };
                    });
            */
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();
            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
            {
                //app.UseResponseCompression();
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseResponseCompression();
            app.UseResponseCaching();
            app.UseFileServer();

            app.UseSession();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {

                endpoints.MapBlazorHub(options =>
                {
                    //options.LongPolling.PollTimeout = new TimeSpan(1, 0, 0);
                });

                endpoints.MapFallbackToPage("{*route}", "/_Host");//PARA PERMITIR URL CON PUNTOS URL/U.R.L

                endpoints.MapHub<ChatHub>(Strings.HubEndPoint, options =>
                {
                    //options.Transports = HttpTransportType.WebSockets;
                });
            });
        }
    }
}
namespace DeepSignals
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<StartUp>()
                              .ConfigureKestrel(serverOptions =>
                              {
                                  // Configuraci�n espec�fica de Kestrel, si es necesario.
                              })
                              .UseSetting(WebHostDefaults.DetailedErrorsKey, "true");
                });
    }
}
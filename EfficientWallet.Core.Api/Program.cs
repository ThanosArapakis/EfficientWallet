namespace EfficientWallet.Core.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while starting the application: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            throw;
        }
        finally
        {
            Console.WriteLine("Application has stopped.");
        }
    }

    static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>()
                        .UseSetting(WebHostDefaults.DetailedErrorsKey, "true")
                        .CaptureStartupErrors(true);
                    })
                    .ConfigureLogging(builder =>
                    {
                        builder.ClearProviders();
                    });
    }
}
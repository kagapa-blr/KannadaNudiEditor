using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Web; // Added for HeadOutlet
using Syncfusion.Blazor;
using KannadaNudiWeb.Services;
using KannadaNudiEditor.Helpers.Conversion;

namespace KannadaNudiWeb
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // Register Services
            builder.Services.AddScoped<FileConversionService>();
            builder.Services.AddScoped<TransliterationService>();
            builder.Services.AddScoped<SpeechService>();

            // Register Syncfusion
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("YOUR_COMMUNITY_LICENSE_KEY");
            builder.Services.AddSyncfusionBlazor();

            await builder.Build().RunAsync();
        }
    }
}

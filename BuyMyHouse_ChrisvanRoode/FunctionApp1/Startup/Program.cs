using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Functions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Services;
using DAL;

namespace Kennemerland.Startup {
	public class Program {
		public static void Main() {
			IHost host = new HostBuilder()
				.ConfigureFunctionsWorkerDefaults((IFunctionsWorkerApplicationBuilder Builder) => {
					Builder.UseNewtonsoftJson();
				})
				.ConfigureOpenApi()
				.ConfigureServices(Configure)
				.Build();

			host.Run();
		}

		static void Configure(HostBuilderContext Builder, IServiceCollection Services) {
			Services.AddSingleton<IHouseRepository, HouseRepository>();
			Services.AddSingleton<IUserRepository, UserRepository>();
			
			Services.AddSingleton<IHousesService, HouseService>();
			Services.AddSingleton<IUsersService, UserService>();

			Services.AddSingleton<IBlobService, BlobSerivce>();
		}
	}
}



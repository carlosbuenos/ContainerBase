using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;

namespace ApiEstudo
{
	/// <summary>
	/// 
	/// </summary>
	public class Startup
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="configuration"></param>
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}
		/// <summary>
		/// 
		/// </summary>
		public IConfiguration Configuration { get; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="services"></param>
		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = "Api Estudo GCP",
					Version = "v1",
					Description = "Exemplo de API REST criada com o ASP.NET Core 3.0 para teste de ferramentas Google Cloud como pór exemplo Pub/Sub e BigQuery",
					Contact = new OpenApiContact
					{
						Url = new System.Uri("https://github.com/carlosbuenos"),
						Email = "ccarlosbueno@outlook.com",
						Name = "Carlos Bueno"

					},
				});
				string caminhoAplicacao =
				 PlatformServices.Default.Application.ApplicationBasePath;
				string nomeAplicacao =
					PlatformServices.Default.Application.ApplicationName;
				string caminhoXmlDoc =
					Path.Combine(caminhoAplicacao, $"{nomeAplicacao}.xml");

				c.IncludeXmlComments(caminhoXmlDoc);
			});
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="app"></param>
		/// <param name="env"></param>
		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json",
					"ApiEstudo");


			});
			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});

		}
	}
}

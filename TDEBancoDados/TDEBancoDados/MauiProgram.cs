using System.Globalization;
using Microsoft.Extensions.Logging;
using TDEBancoDados.Data;
using TDEBancoDados.Pages;
using TDEBancoDados.Services;

namespace TDEBancoDados;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Datas e valores sempre no formato brasileiro (dd/MM/yyyy e R$ 0,00)
		var ptBr = new CultureInfo("pt-BR");
		CultureInfo.DefaultThreadCurrentCulture = ptBr;
		CultureInfo.DefaultThreadCurrentUICulture = ptBr;
		CultureInfo.CurrentCulture = ptBr;
		CultureInfo.CurrentUICulture = ptBr;

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton(new LojaDatabase(Path.Combine(FileSystem.AppDataDirectory, LojaDatabase.NomeArquivo)));
		builder.Services.AddSingleton<ClienteService>();
		builder.Services.AddSingleton<ProdutoService>();
		builder.Services.AddSingleton<CompraService>();
		builder.Services.AddSingleton<ItemCompraService>();

		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<ClientesPage>();
		builder.Services.AddTransient<ProdutosPage>();
		builder.Services.AddTransient<ComprasPage>();
		builder.Services.AddTransient<ItensCompraPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

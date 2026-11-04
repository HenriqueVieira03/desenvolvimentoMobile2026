using TDEBancoDados.Pages;
using TDEBancoDados.Services;

namespace TDEBancoDados;

public partial class MainPage : ContentPage
{
    private readonly ClienteService _clienteService;
    private readonly ProdutoService _produtoService;
    private readonly CompraService _compraService;

    public MainPage(ClienteService clienteService, ProdutoService produtoService, CompraService compraService)
    {
        InitializeComponent();
        _clienteService = clienteService;
        _produtoService = produtoService;
        _compraService = compraService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await this.TentarAsync(async () =>
        {
            var clientes = (await _clienteService.ListarAsync()).Count;
            var produtos = (await _produtoService.ListarAsync()).Count;
            var compras = (await _compraService.ListarAsync()).Count;
            LblResumo.Text = $"{clientes} cliente(s) • {produtos} produto(s) • {compras} compra(s)";
        });
    }

    private async void OnClientesClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(ClientesPage));

    private async void OnProdutosClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(ProdutosPage));

    private async void OnComprasClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(ComprasPage));
}

using TDEBancoDados.Models;
using TDEBancoDados.Services;

namespace TDEBancoDados.Pages;

public partial class ComprasPage : ContentPage
{
    private readonly CompraService _compraService;
    private readonly ClienteService _clienteService;
    private List<Cliente> _clientes = new();
    private Compra? _selecionada;

    public ComprasPage(CompraService compraService, ClienteService clienteService)
    {
        InitializeComponent();
        _compraService = compraService;
        _clienteService = clienteService;
        DtCompra.MaximumDate = DateTime.Today;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LimparFormulario();
        await CarregarAsync();
    }

    private Task CarregarAsync() => this.TentarAsync(async () =>
    {
        _clientes = await _clienteService.ListarAsync();
        PckCliente.ItemsSource = _clientes;
        ListaCompras.ItemsSource = await _compraService.ListarAsync();
    });

    private void OnCompraSelecionada(object? sender, SelectionChangedEventArgs e)
    {
        _selecionada = e.CurrentSelection.FirstOrDefault() as Compra;
        if (_selecionada is null)
            return;

        PckCliente.SelectedItem = _clientes.FirstOrDefault(c => c.CodigoCliente == _selecionada.CodigoCliente);
        DtCompra.Date = _selecionada.DataCompra;
        LblTotal.Text = $"Valor total: {_selecionada.ValorTotal:C} ({_selecionada.Itens.Count} item(ns))";
        LblModo.Text = $"Editando compra nº {_selecionada.CodigoCompra}";
        BtnExcluir.IsEnabled = true;
        BtnItens.IsEnabled = true;
    }

    private async void OnSalvarClicked(object? sender, EventArgs e)
    {
        var compra = new Compra
        {
            CodigoCompra = _selecionada?.CodigoCompra ?? 0,
            CodigoCliente = (PckCliente.SelectedItem as Cliente)?.CodigoCliente ?? 0,
            DataCompra = DtCompra.Date ?? DateTime.Today
        };

        var nova = compra.CodigoCompra == 0;
        if (!await this.TentarAsync(() => _compraService.SalvarAsync(compra)))
            return;

        if (nova)
        {
            // Uma compra nova só faz sentido com itens: segue direto para eles
            await DisplayAlertAsync("Sucesso", $"Compra nº {compra.CodigoCompra} registrada. Agora adicione os itens.", "OK");
            await AbrirItensAsync(compra.CodigoCompra);
            return;
        }

        await DisplayAlertAsync("Sucesso", "Compra alterada.", "OK");
        LimparFormulario();
        await CarregarAsync();
    }

    private void OnNovaClicked(object? sender, EventArgs e) => LimparFormulario();

    private async void OnItensClicked(object? sender, EventArgs e)
    {
        if (_selecionada is not null)
            await AbrirItensAsync(_selecionada.CodigoCompra);
    }

    private async void OnExcluirClicked(object? sender, EventArgs e)
    {
        if (_selecionada is null)
            return;

        if (!await this.ConfirmarExclusaoAsync($"a compra nº {_selecionada.CodigoCompra} e todos os seus itens (o estoque será devolvido)"))
            return;

        if (await this.TentarAsync(() => _compraService.ExcluirAsync(_selecionada)))
        {
            LimparFormulario();
            await CarregarAsync();
        }
    }

    private static Task AbrirItensAsync(int codigoCompra) =>
        Shell.Current.GoToAsync(nameof(ItensCompraPage), new Dictionary<string, object>
        {
            ["codigoCompra"] = codigoCompra
        });

    private void LimparFormulario()
    {
        _selecionada = null;
        ListaCompras.SelectedItem = null;
        PckCliente.SelectedItem = null;
        DtCompra.Date = DateTime.Today;
        LblTotal.Text = "Valor total: R$ 0,00 (calculado pelos itens)";
        LblModo.Text = "Nova compra";
        BtnExcluir.IsEnabled = false;
        BtnItens.IsEnabled = false;
    }
}

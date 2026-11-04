using TDEBancoDados.Models;
using TDEBancoDados.Services;

namespace TDEBancoDados.Pages;

public partial class ClientesPage : ContentPage
{
    private readonly ClienteService _clienteService;
    private Cliente? _selecionado;

    public ClientesPage(ClienteService clienteService)
    {
        InitializeComponent();
        _clienteService = clienteService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private Task CarregarAsync() => this.TentarAsync(async () =>
    {
        ListaClientes.ItemsSource = await _clienteService.ListarAsync();
    });

    private void OnClienteSelecionado(object? sender, SelectionChangedEventArgs e)
    {
        _selecionado = e.CurrentSelection.FirstOrDefault() as Cliente;
        if (_selecionado is null)
            return;

        TxtNome.Text = _selecionado.Nome;
        TxtRG.Text = _selecionado.RG.ToString();
        TxtTelefone.Text = _selecionado.Telefone;
        LblModo.Text = $"Editando cliente nº {_selecionado.CodigoCliente}";
        BtnExcluir.IsEnabled = true;
    }

    private async void OnSalvarClicked(object? sender, EventArgs e)
    {
        var cliente = new Cliente
        {
            CodigoCliente = _selecionado?.CodigoCliente ?? 0,
            Nome = TxtNome.Text ?? string.Empty,
            RG = Validacao.ParaLong(TxtRG.Text),
            Telefone = TxtTelefone.Text ?? string.Empty
        };

        var novo = cliente.CodigoCliente == 0;
        if (await this.TentarAsync(() => _clienteService.SalvarAsync(cliente)))
        {
            await DisplayAlertAsync("Sucesso", novo ? "Cliente cadastrado." : "Cliente alterado.", "OK");
            LimparFormulario();
            await CarregarAsync();
        }
    }

    private void OnNovoClicked(object? sender, EventArgs e) => LimparFormulario();

    private async void OnExcluirClicked(object? sender, EventArgs e)
    {
        if (_selecionado is null)
            return;

        if (!await this.ConfirmarExclusaoAsync($"o cliente \"{_selecionado.Nome}\""))
            return;

        if (await this.TentarAsync(() => _clienteService.ExcluirAsync(_selecionado)))
        {
            LimparFormulario();
            await CarregarAsync();
        }
    }

    private void LimparFormulario()
    {
        _selecionado = null;
        ListaClientes.SelectedItem = null;
        TxtNome.Text = string.Empty;
        TxtRG.Text = string.Empty;
        TxtTelefone.Text = string.Empty;
        LblModo.Text = "Novo cliente";
        BtnExcluir.IsEnabled = false;
    }
}

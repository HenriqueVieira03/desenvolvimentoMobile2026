using TDEBancoDados.Models;
using TDEBancoDados.Services;

namespace TDEBancoDados.Pages;

public partial class ProdutosPage : ContentPage
{
    private readonly ProdutoService _produtoService;
    private Produto? _selecionado;

    public ProdutosPage(ProdutoService produtoService)
    {
        InitializeComponent();
        _produtoService = produtoService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private Task CarregarAsync() => this.TentarAsync(async () =>
    {
        ListaProdutos.ItemsSource = await _produtoService.ListarAsync();
    });

    private void OnProdutoSelecionado(object? sender, SelectionChangedEventArgs e)
    {
        _selecionado = e.CurrentSelection.FirstOrDefault() as Produto;
        if (_selecionado is null)
            return;

        TxtNome.Text = _selecionado.NomeProduto;
        TxtTipo.Text = _selecionado.Tipo;
        TxtPreco.Text = _selecionado.Preco.ToString("0.00");
        TxtEstoque.Text = _selecionado.QtdEstoque.ToString();
        LblModo.Text = $"Editando produto nº {_selecionado.CodigoProduto}";
        BtnExcluir.IsEnabled = true;
    }

    private async void OnSalvarClicked(object? sender, EventArgs e)
    {
        var produto = new Produto
        {
            CodigoProduto = _selecionado?.CodigoProduto ?? 0,
            NomeProduto = TxtNome.Text ?? string.Empty,
            Tipo = TxtTipo.Text ?? string.Empty,
            Preco = Validacao.ParaDecimal(TxtPreco.Text),
            QtdEstoque = Validacao.ParaInteiro(TxtEstoque.Text)
        };

        var novo = produto.CodigoProduto == 0;
        if (await this.TentarAsync(() => _produtoService.SalvarAsync(produto)))
        {
            await DisplayAlertAsync("Sucesso", novo ? "Produto cadastrado." : "Produto alterado.", "OK");
            LimparFormulario();
            await CarregarAsync();
        }
    }

    private void OnNovoClicked(object? sender, EventArgs e) => LimparFormulario();

    private async void OnExcluirClicked(object? sender, EventArgs e)
    {
        if (_selecionado is null)
            return;

        if (!await this.ConfirmarExclusaoAsync($"o produto \"{_selecionado.NomeProduto}\""))
            return;

        if (await this.TentarAsync(() => _produtoService.ExcluirAsync(_selecionado)))
        {
            LimparFormulario();
            await CarregarAsync();
        }
    }

    private void LimparFormulario()
    {
        _selecionado = null;
        ListaProdutos.SelectedItem = null;
        TxtNome.Text = string.Empty;
        TxtTipo.Text = string.Empty;
        TxtPreco.Text = string.Empty;
        TxtEstoque.Text = string.Empty;
        LblModo.Text = "Novo produto";
        BtnExcluir.IsEnabled = false;
    }
}

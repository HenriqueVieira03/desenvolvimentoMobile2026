using TDEBancoDados.Models;
using TDEBancoDados.Services;

namespace TDEBancoDados.Pages;

public partial class ItensCompraPage : ContentPage, IQueryAttributable
{
    private readonly ItemCompraService _itemService;
    private readonly CompraService _compraService;
    private readonly ProdutoService _produtoService;
    private List<Produto> _produtos = new();
    private ItemCompra? _selecionado;
    private int _codigoCompra;

    public ItensCompraPage(ItemCompraService itemService, CompraService compraService, ProdutoService produtoService)
    {
        InitializeComponent();
        _itemService = itemService;
        _compraService = compraService;
        _produtoService = produtoService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("codigoCompra", out var valor))
            _codigoCompra = Convert.ToInt32(valor);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarAsync();
    }

    private Task CarregarAsync() => this.TentarAsync(async () =>
    {
        var compra = await _compraService.ObterAsync(_codigoCompra);
        if (compra is null)
        {
            await DisplayAlertAsync("Compra não encontrada", "Esta compra não existe mais.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        LblCompra.Text = $"Compra nº {compra.CodigoCompra} — {compra.Cliente?.Nome}";
        LblResumo.Text = $"{compra.DataCompra:dd/MM/yyyy} • Valor total: {compra.ValorTotal:C}";

        // Recarrega os produtos para o estoque exibido no Picker ficar atualizado
        _produtos = await _produtoService.ListarAsync();
        PckProduto.ItemsSource = _produtos;
        ListaItens.ItemsSource = await _itemService.ListarPorCompraAsync(_codigoCompra);
    });

    private void OnItemSelecionado(object? sender, SelectionChangedEventArgs e)
    {
        _selecionado = e.CurrentSelection.FirstOrDefault() as ItemCompra;
        if (_selecionado is null)
            return;

        PckProduto.SelectedItem = _produtos.FirstOrDefault(p => p.CodigoProduto == _selecionado.CodigoProduto);
        TxtQuantidade.Text = _selecionado.Quantidade.ToString();
        LblModo.Text = $"Editando item nº {_selecionado.CodigoItem}";
        BtnExcluir.IsEnabled = true;
    }

    private void OnDadosItemAlterados(object? sender, EventArgs e)
    {
        var produto = PckProduto.SelectedItem as Produto;
        var quantidade = Validacao.ParaInteiro(TxtQuantidade.Text);
        var parcial = produto is not null && quantidade > 0 ? produto.Preco * quantidade : 0;
        LblParcial.Text = $"Valor parcial: {parcial:C}";
    }

    private async void OnSalvarClicked(object? sender, EventArgs e)
    {
        var item = new ItemCompra
        {
            CodigoItem = _selecionado?.CodigoItem ?? 0,
            CodigoCompra = _codigoCompra,
            CodigoProduto = (PckProduto.SelectedItem as Produto)?.CodigoProduto ?? 0,
            Quantidade = Validacao.ParaInteiro(TxtQuantidade.Text)
        };

        if (await this.TentarAsync(() => _itemService.SalvarAsync(item)))
        {
            LimparFormulario();
            await CarregarAsync();
        }
    }

    private void OnNovoClicked(object? sender, EventArgs e) => LimparFormulario();

    private async void OnExcluirClicked(object? sender, EventArgs e)
    {
        if (_selecionado is null)
            return;

        if (!await this.ConfirmarExclusaoAsync($"o item \"{_selecionado.Titulo}\" (o estoque será devolvido)"))
            return;

        if (await this.TentarAsync(() => _itemService.ExcluirAsync(_selecionado)))
        {
            LimparFormulario();
            await CarregarAsync();
        }
    }

    private void LimparFormulario()
    {
        _selecionado = null;
        ListaItens.SelectedItem = null;
        PckProduto.SelectedItem = null;
        TxtQuantidade.Text = string.Empty;
        LblParcial.Text = "Valor parcial: R$ 0,00";
        LblModo.Text = "Novo item";
        BtnExcluir.IsEnabled = false;
    }
}

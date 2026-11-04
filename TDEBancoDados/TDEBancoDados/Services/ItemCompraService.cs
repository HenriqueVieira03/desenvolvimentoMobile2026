using SQLiteNetExtensionsAsync.Extensions;
using TDEBancoDados.Data;
using TDEBancoDados.Models;

namespace TDEBancoDados.Services;

/// <summary>
/// CRUD dos itens de uma compra. Cada operação ajusta o estoque do produto
/// e recalcula o valor total da compra dentro de uma transação.
/// </summary>
public class ItemCompraService
{
    private readonly LojaDatabase _banco;

    public ItemCompraService(LojaDatabase banco)
    {
        _banco = banco;
    }

    public async Task<List<ItemCompra>> ListarPorCompraAsync(int codigoCompra)
    {
        var db = await _banco.ObterConexaoAsync();
        var itens = await db.GetAllWithChildrenAsync<ItemCompra>(i => i.CodigoCompra == codigoCompra);
        return itens.OrderBy(i => i.CodigoItem).ToList();
    }

    public async Task SalvarAsync(ItemCompra item)
    {
        if (item.CodigoProduto <= 0)
            throw new ValidacaoException("Selecione o produto.");

        if (item.Quantidade <= 0)
            throw new ValidacaoException("Informe uma quantidade inteira maior que zero.");

        var db = await _banco.ObterConexaoAsync();

        await db.RunInTransactionAsync(conn =>
        {
            if (conn.Find<Compra>(item.CodigoCompra) is null)
                throw new ValidacaoException("A compra deste item não existe mais.");

            var produto = conn.Find<Produto>(item.CodigoProduto)
                ?? throw new ValidacaoException("O produto selecionado não existe mais.");

            var codigoItem = item.CodigoItem;
            var codigoCompra = item.CodigoCompra;
            var codigoProduto = item.CodigoProduto;
            var repetido = conn.Table<ItemCompra>()
                .Where(i => i.CodigoCompra == codigoCompra && i.CodigoProduto == codigoProduto && i.CodigoItem != codigoItem)
                .Count();
            if (repetido > 0)
                throw new ValidacaoException("Este produto já está na compra. Altere a quantidade do item existente.");

            if (codigoItem != 0)
            {
                // Na alteração, devolve primeiro ao estoque o que o item tinha reservado
                var anterior = conn.Find<ItemCompra>(codigoItem)
                    ?? throw new ValidacaoException("Item não encontrado. Ele pode ter sido excluído.");

                var produtoAnterior = conn.Find<Produto>(anterior.CodigoProduto);
                if (produtoAnterior is not null)
                {
                    produtoAnterior.QtdEstoque += anterior.Quantidade;
                    conn.Update(produtoAnterior);

                    if (produtoAnterior.CodigoProduto == produto.CodigoProduto)
                        produto = produtoAnterior;
                }
            }

            if (item.Quantidade > produto.QtdEstoque)
                throw new ValidacaoException($"Estoque insuficiente de \"{produto.NomeProduto}\". Disponível: {produto.QtdEstoque}.");

            produto.QtdEstoque -= item.Quantidade;
            conn.Update(produto);

            item.ValorParcial = produto.Preco * item.Quantidade;

            if (codigoItem == 0)
                conn.Insert(item);
            else
                conn.Update(item);

            CompraService.AtualizarTotal(conn, codigoCompra);
        });
    }

    public async Task ExcluirAsync(ItemCompra item)
    {
        var db = await _banco.ObterConexaoAsync();

        await db.RunInTransactionAsync(conn =>
        {
            var atual = conn.Find<ItemCompra>(item.CodigoItem)
                ?? throw new ValidacaoException("Item não encontrado. Ele pode já ter sido excluído.");

            var produto = conn.Find<Produto>(atual.CodigoProduto);
            if (produto is not null)
            {
                produto.QtdEstoque += atual.Quantidade;
                conn.Update(produto);
            }

            conn.Delete<ItemCompra>(atual.CodigoItem);
            CompraService.AtualizarTotal(conn, atual.CodigoCompra);
        });
    }
}

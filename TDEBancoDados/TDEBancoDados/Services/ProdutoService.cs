using TDEBancoDados.Data;
using TDEBancoDados.Models;

namespace TDEBancoDados.Services;

public class ProdutoService
{
    private readonly LojaDatabase _banco;

    public ProdutoService(LojaDatabase banco)
    {
        _banco = banco;
    }

    public async Task<List<Produto>> ListarAsync()
    {
        var db = await _banco.ObterConexaoAsync();
        return await db.Table<Produto>().OrderBy(p => p.NomeProduto).ToListAsync();
    }

    public async Task SalvarAsync(Produto produto)
    {
        produto.NomeProduto = Validacao.TextoObrigatorio(produto.NomeProduto, "Nome do produto", 50);
        produto.Tipo = Validacao.TextoObrigatorio(produto.Tipo, "Tipo", 50);

        if (produto.Preco <= 0)
            throw new ValidacaoException("Informe um preço maior que zero. Ex.: 19,90.");

        if (produto.Preco > 1_000_000)
            throw new ValidacaoException("O preço informado é alto demais (máximo R$ 1.000.000,00).");

        produto.Preco = Math.Round(produto.Preco, 2);

        if (produto.QtdEstoque < 0)
            throw new ValidacaoException("A quantidade em estoque deve ser um número inteiro igual ou maior que zero.");

        var db = await _banco.ObterConexaoAsync();

        var codigo = produto.CodigoProduto;
        var nome = produto.NomeProduto.ToLower();
        var nomeRepetido = await db.Table<Produto>()
            .Where(p => p.NomeProduto.ToLower() == nome && p.CodigoProduto != codigo)
            .CountAsync();
        if (nomeRepetido > 0)
            throw new ValidacaoException("Já existe um produto cadastrado com este nome.");

        if (codigo == 0)
        {
            await db.InsertAsync(produto);
            return;
        }

        if (await db.FindAsync<Produto>(codigo) is null)
            throw new ValidacaoException("Produto não encontrado. Ele pode ter sido excluído.");

        await db.UpdateAsync(produto);
    }

    public async Task ExcluirAsync(Produto produto)
    {
        var db = await _banco.ObterConexaoAsync();

        var codigo = produto.CodigoProduto;
        if (await db.FindAsync<Produto>(codigo) is null)
            throw new ValidacaoException("Produto não encontrado. Ele pode já ter sido excluído.");

        var itens = await db.Table<ItemCompra>().Where(i => i.CodigoProduto == codigo).CountAsync();
        if (itens > 0)
            throw new ValidacaoException($"Este produto aparece em {itens} item(ns) de compra. Remova esses itens antes de excluir o produto.");

        await db.DeleteAsync<Produto>(codigo);
    }
}

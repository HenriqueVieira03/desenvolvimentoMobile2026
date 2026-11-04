using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using TDEBancoDados.Data;
using TDEBancoDados.Models;

namespace TDEBancoDados.Services;

public class CompraService
{
    private readonly LojaDatabase _banco;

    public CompraService(LojaDatabase banco)
    {
        _banco = banco;
    }

    /// <summary>Lista as compras já com o cliente e os itens carregados (SQLiteNetExtensions).</summary>
    public async Task<List<Compra>> ListarAsync()
    {
        var db = await _banco.ObterConexaoAsync();
        var compras = await db.GetAllWithChildrenAsync<Compra>();
        return compras.OrderByDescending(c => c.DataCompra).ThenByDescending(c => c.CodigoCompra).ToList();
    }

    public async Task<Compra?> ObterAsync(int codigoCompra)
    {
        var db = await _banco.ObterConexaoAsync();
        if (await db.FindAsync<Compra>(codigoCompra) is null)
            return null;

        return await db.GetWithChildrenAsync<Compra>(codigoCompra);
    }

    public async Task SalvarAsync(Compra compra)
    {
        if (compra.CodigoCliente <= 0)
            throw new ValidacaoException("Selecione o cliente da compra.");

        if (compra.DataCompra.Date > DateTime.Today)
            throw new ValidacaoException("A data da compra não pode estar no futuro.");

        if (compra.DataCompra.Year < 2000)
            throw new ValidacaoException("Informe uma data de compra válida.");

        compra.DataCompra = compra.DataCompra.Date;

        var db = await _banco.ObterConexaoAsync();

        await db.RunInTransactionAsync(conn =>
        {
            if (conn.Find<Cliente>(compra.CodigoCliente) is null)
                throw new ValidacaoException("O cliente selecionado não existe mais.");

            if (compra.CodigoCompra == 0)
            {
                // O valor total nunca é digitado: ele é a soma dos itens
                compra.ValorTotal = 0;
                conn.Insert(compra);
                return;
            }

            if (conn.Find<Compra>(compra.CodigoCompra) is null)
                throw new ValidacaoException("Compra não encontrada. Ela pode ter sido excluída.");

            compra.ValorTotal = CalcularTotal(conn, compra.CodigoCompra);
            conn.Update(compra);
        });
    }

    /// <summary>Exclui a compra e os seus itens, devolvendo as quantidades ao estoque.</summary>
    public async Task ExcluirAsync(Compra compra)
    {
        var db = await _banco.ObterConexaoAsync();

        await db.RunInTransactionAsync(conn =>
        {
            var codigo = compra.CodigoCompra;
            if (conn.Find<Compra>(codigo) is null)
                throw new ValidacaoException("Compra não encontrada. Ela pode já ter sido excluída.");

            var itens = conn.Table<ItemCompra>().Where(i => i.CodigoCompra == codigo).ToList();
            foreach (var item in itens)
            {
                var produto = conn.Find<Produto>(item.CodigoProduto);
                if (produto is not null)
                {
                    produto.QtdEstoque += item.Quantidade;
                    conn.Update(produto);
                }

                conn.Delete<ItemCompra>(item.CodigoItem);
            }

            conn.Delete<Compra>(codigo);
        });
    }

    internal static decimal CalcularTotal(SQLiteConnection conn, int codigoCompra) =>
        conn.Table<ItemCompra>()
            .Where(i => i.CodigoCompra == codigoCompra)
            .ToList()
            .Sum(i => i.ValorParcial);

    internal static void AtualizarTotal(SQLiteConnection conn, int codigoCompra)
    {
        var compra = conn.Find<Compra>(codigoCompra);
        if (compra is null)
            return;

        compra.ValorTotal = CalcularTotal(conn, codigoCompra);
        conn.Update(compra);
    }
}

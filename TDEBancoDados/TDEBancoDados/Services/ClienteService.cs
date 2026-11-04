using System.Text.RegularExpressions;
using TDEBancoDados.Data;
using TDEBancoDados.Models;

namespace TDEBancoDados.Services;

public class ClienteService
{
    private readonly LojaDatabase _banco;

    public ClienteService(LojaDatabase banco)
    {
        _banco = banco;
    }

    public async Task<List<Cliente>> ListarAsync()
    {
        var db = await _banco.ObterConexaoAsync();
        return await db.Table<Cliente>().OrderBy(c => c.Nome).ToListAsync();
    }

    public async Task SalvarAsync(Cliente cliente)
    {
        cliente.Nome = Validacao.TextoObrigatorio(cliente.Nome, "Nome", 50);
        cliente.Telefone = Validacao.TextoObrigatorio(cliente.Telefone, "Telefone", 14);

        if (cliente.RG <= 0 || cliente.RG > 9_999_999_999)
            throw new ValidacaoException("Informe um RG válido, somente números, com até 10 dígitos.");

        if (!Regex.IsMatch(cliente.Telefone, @"^[0-9()\-\s+]+$"))
            throw new ValidacaoException("O telefone deve conter apenas números, parênteses, espaço ou hífen.");

        var digitos = cliente.Telefone.Count(char.IsDigit);
        if (digitos < 10 || digitos > 13)
            throw new ValidacaoException("Informe o telefone com DDD. Ex.: (41)99999-9999.");

        var db = await _banco.ObterConexaoAsync();

        var codigo = cliente.CodigoCliente;
        var rg = cliente.RG;
        var rgRepetido = await db.Table<Cliente>()
            .Where(c => c.RG == rg && c.CodigoCliente != codigo)
            .CountAsync();
        if (rgRepetido > 0)
            throw new ValidacaoException("Já existe um cliente cadastrado com este RG.");

        if (codigo == 0)
        {
            await db.InsertAsync(cliente);
            return;
        }

        if (await db.FindAsync<Cliente>(codigo) is null)
            throw new ValidacaoException("Cliente não encontrado. Ele pode ter sido excluído.");

        await db.UpdateAsync(cliente);
    }

    public async Task ExcluirAsync(Cliente cliente)
    {
        var db = await _banco.ObterConexaoAsync();

        var codigo = cliente.CodigoCliente;
        if (await db.FindAsync<Cliente>(codigo) is null)
            throw new ValidacaoException("Cliente não encontrado. Ele pode já ter sido excluído.");

        var compras = await db.Table<Compra>().Where(c => c.CodigoCliente == codigo).CountAsync();
        if (compras > 0)
            throw new ValidacaoException($"Este cliente possui {compras} compra(s) registrada(s). Exclua as compras antes de excluir o cliente.");

        await db.DeleteAsync<Cliente>(codigo);
    }
}

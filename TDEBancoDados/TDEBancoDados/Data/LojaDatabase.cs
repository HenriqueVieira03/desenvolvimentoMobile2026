using SQLite;
using TDEBancoDados.Models;

namespace TDEBancoDados.Data;

/// <summary>
/// Abre a conexão com o arquivo SQLite e cria as 4 tabelas do modelo lógico na primeira vez.
/// </summary>
public class LojaDatabase
{
    public const string NomeArquivo = "loja.db3";

    private readonly string _caminho;
    private readonly SemaphoreSlim _trava = new(1, 1);
    private SQLiteAsyncConnection? _conexao;

    public LojaDatabase(string caminho)
    {
        _caminho = caminho;
    }

    public async Task<SQLiteAsyncConnection> ObterConexaoAsync()
    {
        if (_conexao is not null)
            return _conexao;

        await _trava.WaitAsync();
        try
        {
            if (_conexao is null)
            {
                var conexao = new SQLiteAsyncConnection(_caminho,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

                await conexao.CreateTablesAsync<Cliente, Produto, Compra, ItemCompra>();
                _conexao = conexao;
            }

            return _conexao;
        }
        finally
        {
            _trava.Release();
        }
    }
}

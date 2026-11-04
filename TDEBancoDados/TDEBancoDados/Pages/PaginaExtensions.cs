using TDEBancoDados.Services;

namespace TDEBancoDados.Pages;

public static class PaginaExtensions
{
    /// <summary>
    /// Executa uma operação do back-end mostrando ao usuário o motivo quando a validação recusa os dados.
    /// </summary>
    public static async Task<bool> TentarAsync(this Page pagina, Func<Task> operacao)
    {
        try
        {
            await operacao();
            return true;
        }
        catch (ValidacaoException ex)
        {
            await pagina.DisplayAlertAsync("Verifique os dados", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await pagina.DisplayAlertAsync("Erro no banco de dados", ex.Message, "OK");
        }

        return false;
    }

    public static Task<bool> ConfirmarExclusaoAsync(this Page pagina, string descricao) =>
        pagina.DisplayAlertAsync("Confirmar exclusão", $"Deseja excluir {descricao}?", "Excluir", "Cancelar");
}

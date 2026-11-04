using System.Globalization;

namespace TDEBancoDados.Services;

/// <summary>
/// Erro de regra de negócio: os dados foram recusados antes de chegar ao banco.
/// </summary>
public class ValidacaoException : Exception
{
    public ValidacaoException(string mensagem) : base(mensagem) { }
}

public static class Validacao
{
    private static readonly CultureInfo PtBr = new("pt-BR");

    /// <summary>Remove espaços extras e exige um texto com até <paramref name="tamanhoMaximo"/> caracteres.</summary>
    public static string TextoObrigatorio(string? valor, string campo, int tamanhoMaximo)
    {
        var texto = (valor ?? string.Empty).Trim();

        if (texto.Length == 0)
            throw new ValidacaoException($"O campo {campo} é obrigatório.");

        if (texto.Length > tamanhoMaximo)
            throw new ValidacaoException($"O campo {campo} aceita no máximo {tamanhoMaximo} caracteres.");

        return texto;
    }

    /// <summary>Converte o texto digitado; devolve -1 quando não é um número (a regra de negócio recusa depois).</summary>
    public static decimal ParaDecimal(string? texto)
    {
        var normalizado = (texto ?? string.Empty).Trim().Replace("R$", "").Replace('.', ',');
        return decimal.TryParse(normalizado, NumberStyles.Number, PtBr, out var valor) ? valor : -1;
    }

    public static int ParaInteiro(string? texto) =>
        int.TryParse((texto ?? string.Empty).Trim(), out var valor) ? valor : -1;

    public static long ParaLong(string? texto) =>
        long.TryParse((texto ?? string.Empty).Trim(), out var valor) ? valor : -1;
}

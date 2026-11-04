using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDEBancoDados.Models;

[Table("Produto")]
public class Produto
{
    [PrimaryKey, AutoIncrement]
    public int CodigoProduto { get; set; }

    [MaxLength(50), NotNull]
    public string NomeProduto { get; set; } = string.Empty;

    [MaxLength(50), NotNull]
    public string Tipo { get; set; } = string.Empty;

    [NotNull]
    public decimal Preco { get; set; }

    [NotNull]
    public int QtdEstoque { get; set; }

    [OneToMany]
    public List<ItemCompra> Itens { get; set; } = new();

    [Ignore]
    public string Detalhes => $"Cód. {CodigoProduto} • {Tipo} • {Preco:C} • Estoque: {QtdEstoque}";

    public override string ToString() => $"{NomeProduto} — {Preco:C} (estoque: {QtdEstoque})";
}

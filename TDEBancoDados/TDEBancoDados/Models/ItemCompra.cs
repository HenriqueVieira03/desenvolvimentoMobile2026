using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDEBancoDados.Models;

[Table("ItemCompra")]
public class ItemCompra
{
    [PrimaryKey, AutoIncrement]
    public int CodigoItem { get; set; }

    [ForeignKey(typeof(Produto)), NotNull]
    public int CodigoProduto { get; set; }

    [ForeignKey(typeof(Compra)), NotNull]
    public int CodigoCompra { get; set; }

    [NotNull]
    public int Quantidade { get; set; }

    [NotNull]
    public decimal ValorParcial { get; set; }

    [ManyToOne]
    public Produto? Produto { get; set; }

    [ManyToOne]
    public Compra? Compra { get; set; }

    [Ignore]
    public string Titulo => Produto?.NomeProduto ?? $"Produto {CodigoProduto}";

    [Ignore]
    public string Detalhes => $"Qtd. {Quantidade} × {Produto?.Preco ?? 0:C} = {ValorParcial:C}";
}

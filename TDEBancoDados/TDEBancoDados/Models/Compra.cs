using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDEBancoDados.Models;

[Table("Compra")]
public class Compra
{
    [PrimaryKey, AutoIncrement]
    public int CodigoCompra { get; set; }

    [NotNull]
    public DateTime DataCompra { get; set; } = DateTime.Today;

    [NotNull]
    public decimal ValorTotal { get; set; }

    [ForeignKey(typeof(Cliente)), NotNull]
    public int CodigoCliente { get; set; }

    [ManyToOne]
    public Cliente? Cliente { get; set; }

    [OneToMany]
    public List<ItemCompra> Itens { get; set; } = new();

    [Ignore]
    public string Titulo => $"Compra nº {CodigoCompra} — {ValorTotal:C}";

    [Ignore]
    public string Detalhes => $"{Cliente?.Nome ?? "Cliente não encontrado"} • {DataCompra:dd/MM/yyyy} • {Itens.Count} item(ns)";
}

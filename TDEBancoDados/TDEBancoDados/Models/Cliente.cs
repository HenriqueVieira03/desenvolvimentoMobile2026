using SQLite;
using SQLiteNetExtensions.Attributes;

namespace TDEBancoDados.Models;

[Table("Cliente")]
public class Cliente
{
    [PrimaryKey, AutoIncrement]
    public int CodigoCliente { get; set; }

    [MaxLength(50), NotNull]
    public string Nome { get; set; } = string.Empty;

    // INT(10) no modelo lógico: até 10 dígitos, por isso long
    [NotNull]
    public long RG { get; set; }

    [MaxLength(14), NotNull]
    public string Telefone { get; set; } = string.Empty;

    [OneToMany]
    public List<Compra> Compras { get; set; } = new();

    [Ignore]
    public string Detalhes => $"Cód. {CodigoCliente} • RG {RG} • {Telefone}";

    public override string ToString() => Nome;
}

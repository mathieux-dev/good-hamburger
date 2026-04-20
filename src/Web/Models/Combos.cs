namespace GoodHamburger.Web.Models;

public record ComboDefinition(
    string Id,
    string Title,
    string Tag,
    string[] Items,
    string[] ItemIds,
    int Discount
);

public static class Combos
{
    public static readonly ComboDefinition Full = new(
        "full", "O Combão", "Combo Completo",
        new[] { "X Bacon", "Batata Frita", "Refrigerante" },
        new[] { "11111111-0000-0000-0000-000000000003", "11111111-0000-0000-0000-000000000004", "11111111-0000-0000-0000-000000000005" },
        20);

    public static readonly ComboDefinition SandRef = new(
        "sand-ref", "Duo Gelado", "Sanduíche + Bebida",
        new[] { "X Burger", "Refrigerante" },
        new[] { "11111111-0000-0000-0000-000000000001", "11111111-0000-0000-0000-000000000005" },
        15);

    public static readonly ComboDefinition SandBat = new(
        "sand-bat", "Dose Dupla", "Sanduíche + Batata",
        new[] { "X Egg", "Batata Frita" },
        new[] { "11111111-0000-0000-0000-000000000002", "11111111-0000-0000-0000-000000000004" },
        10);
}

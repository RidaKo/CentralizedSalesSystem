using CentralizedSalesSystem.API.Models.Orders.enums;

namespace CentralizedSalesSystem.API.Models.Orders

{
    public class ItemVariationOption
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal PriceAdjustment { get; set; }

        public long ItemVariationId { get; set; }

        public ItemVariation ItemVariation { get; set; } = null!;
    }
}
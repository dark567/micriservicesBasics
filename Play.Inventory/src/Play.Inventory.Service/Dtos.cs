using System;

namespace Play.Inventory.Service.Dtos
{
    public record GrantItemsDto(Guid UserID, Guid CatalogItemId, int Quantity);
    public record InventoryItemDto(Guid CatalogItemID, string Name, string Description, int Quantity, DateTimeOffset AcquiredDate);
    public record CatalogItemDto(Guid Id, string Name, string Description);
}
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Repositories;
using Play.Catalog.Service.Entities;
using Play.Common;
using MassTransit;
using Play.Catalog.Contracts;

namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("items")]
    public class ItemsController : ControllerBase
    {
        // private static readonly List<ItemDto> items = new()
        // {
        //     new ItemDto(Id: Guid.NewGuid(), Name: "Potion", Description: "Restores a small amount of HP", Price: 5, CreatedDate: DateTimeOffset.UtcNow),
        //     new ItemDto(Id: Guid.NewGuid(), Name: "Antidot", Description: "Cures poison", Price: 7, CreatedDate: DateTimeOffset.UtcNow),
        //     new ItemDto(Id: Guid.NewGuid(), Name: "Bron sword", Description: "Deals a small amount of damage", Price: 20, CreatedDate: DateTimeOffset.UtcNow),
        //     new ItemDto(Id: Guid.NewGuid(), Name: "Potion", Description: "Restores a small amount of HP", Price: 9, CreatedDate: DateTimeOffset.UtcNow),
        // };

        //private readonly ItemsRepository itemsRepository = new();
        private readonly IRepository<Item> itemsRepository;
        private readonly IPublishEndpoint publishEndPoint;

        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndPoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndPoint = publishEndPoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
        {


            var items = (await itemsRepository.GetAllAsync())
                    .Select(item => item.AsDto());


            return Ok(items);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return item.AsDto();
        }

        [HttpPost]
        public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow,
            };

            await itemsRepository.CreateAsync(item);

            await publishEndPoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemsRepository.GetAsync(id);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(existingItem);

            await publishEndPoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));


            return NoContent();
        }

        //Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var item = await itemsRepository.GetAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            await itemsRepository.RemoveAsync(item.Id);

            await publishEndPoint.Publish(new CatalogItemDeleted(id));


            return NoContent();
        }
    }
}
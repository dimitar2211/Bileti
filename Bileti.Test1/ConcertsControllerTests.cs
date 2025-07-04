using Bileti.Controllers;
using Bileti.Data;
using Bileti.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Bileti.Tests1
{
    public class ConcertsControllerTests
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // уникална база за всеки тест
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Test]
        public async Task Index_ReturnsConcerts_AndLastPurchaseMessage()
        {
            // Arrange
            var context = GetDbContext();
            context.Concerts.Add(new Concert { Name = "Concert A", LastPurchaseAt = DateTime.UtcNow.AddMinutes(-5) });
            context.Concerts.Add(new Concert { Name = "Concert B", LastPurchaseAt = DateTime.UtcNow.AddMinutes(-10) });
            await context.SaveChangesAsync();

            var controller = new ConcertsController(context);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<System.Collections.Generic.List<Concert>>(result.Model);
            Assert.Equal(2, model.Count);
            Assert.NotNull(controller.ViewData["LastPurchaseMessage"]);
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            var controller = new ConcertsController(GetDbContext());

            var result = await controller.Details(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Test]
        public async Task Details_ReturnsNotFound_WhenConcertNotFound()
        {
            var controller = new ConcertsController(GetDbContext());

            var result = await controller.Details(123); // несъществуващ

            Assert.IsType<NotFoundResult>(result);
        }

        [Test]
        public async Task Details_ReturnsView_WhenConcertExists()
        {
            var context = GetDbContext();
            context.Concerts.Add(new Concert { Id = 1, Name = "Test" });
            await context.SaveChangesAsync();

            var controller = new ConcertsController(context);
            var result = await controller.Details(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Concert>(viewResult.Model);
            Assert.Equal("Test", model.Name);
        }

        [Test]
        public async Task Buy_DecrementsTicket_WhenAvailable()
        {
            var context = GetDbContext();
            context.Concerts.Add(new Concert { Id = 1, Name = "Buyable", AvailableTickets = 3 });
            await context.SaveChangesAsync();

            var controller = new ConcertsController(context);
            var result = await controller.Buy(1);

            var concert = await context.Concerts.FindAsync(1);
            Assert.Equal(2, concert.AvailableTickets);
            Assert.NotNull(concert.LastPurchaseAt);
            Assert.Equal("You bought a ticket! :)", controller.TempData["Success"] as string);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Test]
        public async Task Buy_RemovesConcert_WhenTicketsZero()
        {
            var context = GetDbContext();
            context.Concerts.Add(new Concert { Id = 1, Name = "Last Ticket", AvailableTickets = 1 });
            await context.SaveChangesAsync();

            var controller = new ConcertsController(context);
            var result = await controller.Buy(1);

            var concert = await context.Concerts.FindAsync(1);
            Assert.Null(concert); // изтрит
            Assert.Equal("You bought a ticket! :)", controller.TempData["Success"] as string);
        }

        [Test]
        public async Task Buy_ReturnsError_WhenNoTickets()
        {
            var context = GetDbContext();
            context.Concerts.Add(new Concert { Id = 1, Name = "Sold Out", AvailableTickets = 0 });
            await context.SaveChangesAsync();

            var controller = new ConcertsController(context);
            var result = await controller.Buy(1);

            Assert.Equal("No available ticket :(", controller.TempData["Error"] as string);
        }

        [Test]
        public async Task Edit_ReturnsNotFound_IfIdsMismatch()
        {
            var controller = new ConcertsController(GetDbContext());
            var concert = new Concert { Id = 2, Name = "Edit test" };

            var result = await controller.Edit(1, concert);

            Assert.IsType<NotFoundResult>(result);
        }

        [Test]
        public async Task DeleteConfirmed_DeletesConcert()
        {
            var context = GetDbContext();
            context.Concerts.Add(new Concert { Id = 5, Name = "Delete Me" });
            await context.SaveChangesAsync();

            var controller = new ConcertsController(context);
            var result = await controller.DeleteConfirmed(5);

            Assert.Null(await context.Concerts.FindAsync(5));
            Assert.IsType<RedirectToActionResult>(result);
        }
    }
}

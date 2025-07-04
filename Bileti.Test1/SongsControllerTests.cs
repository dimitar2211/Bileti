using Bileti.Controllers;
using Bileti.Data;
using Bileti.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bileti.Tests
{
    public class SongsControllerTests
    {
        private SongsController GetControllerWithContext(ApplicationDbContext context, string userId = "test-user-id")
        {
            var controller = new SongsController(context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Test]
        public void Index_ReturnsUserSongs()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Songs.AddRange(
                new Song { SpotifyTrackId = "abc123", UserId = "test-user-id" },
                new Song { SpotifyTrackId = "xyz789", UserId = "test-user-id" },
                new Song { SpotifyTrackId = "otherUserSong", UserId = "another-user" }
            );
            context.SaveChanges();

            var controller = GetControllerWithContext(context);

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            var model = result.Model as List<string>;
            Assert.Equal(2, model.Count);
            Assert.Contains("abc123", model);
            Assert.Contains("xyz789", model);
        }

        [Test]
        public async Task AddSong_ValidLink_AddsSong()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetControllerWithContext(context);

            var link = "https://open.spotify.com/track/abc123";

            // Act
            var result = await controller.AddSong(link);

            // Assert
            var song = context.Songs.FirstOrDefault();
            Assert.NotNull(song);
            Assert.Equal("abc123", song.SpotifyTrackId);
        }

        [Test]
        public async Task AddSong_EmptyOrInvalidLink_DoesNotAdd()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = GetControllerWithContext(context);

            // Act
            await controller.AddSong(""); // Empty
            await controller.AddSong("invalid-link"); // Invalid format

            // Assert
            Assert.Empty(context.Songs);
        }

        [Test]
        public async Task AddSong_DuplicateSong_DoesNotAddAgain()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Songs.Add(new Song { SpotifyTrackId = "abc123", UserId = "test-user-id" });
            context.SaveChanges();

            var controller = GetControllerWithContext(context);
            var link = "https://open.spotify.com/track/abc123";

            // Act
            await controller.AddSong(link);

            // Assert
            Assert.Single(context.Songs);
        }

        [Test]
        public async Task DeleteSong_ValidId_DeletesSong()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Songs.Add(new Song { SpotifyTrackId = "abc123", UserId = "test-user-id" });
            context.SaveChanges();

            var controller = GetControllerWithContext(context);

            // Act
            await controller.DeleteSong("abc123");

            // Assert
            Assert.Empty(context.Songs);
        }

        [Test]
        public async Task DeleteSong_InvalidOrNotOwnedId_DoesNothing()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Songs.Add(new Song { SpotifyTrackId = "abc123", UserId = "another-user" });
            context.SaveChanges();

            var controller = GetControllerWithContext(context);

            // Act
            await controller.DeleteSong("abc123");

            // Assert
            Assert.Single(context.Songs); // Still there
        }

        [Test]
        public void ExtractSpotifyTrackId_ValidLink_ReturnsId()
        {
            var context = GetInMemoryDbContext();
            var controller = GetControllerWithContext(context);

            var result = controller
                .GetType()
                .GetMethod("ExtractSpotifyTrackId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(controller, new object[] { "https://open.spotify.com/track/abc123" }) as string;

            Assert.Equal("abc123", result);
        }

        [Test]
        public void ExtractSpotifyTrackId_InvalidLink_ReturnsNull()
        {
            var context = GetInMemoryDbContext();
            var controller = GetControllerWithContext(context);

            var result = controller
                .GetType()
                .GetMethod("ExtractSpotifyTrackId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(controller, new object[] { "invalid" }) as string;

            Assert.Null(result);
        }
    }
}

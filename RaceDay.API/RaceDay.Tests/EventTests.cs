using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Dtos;

namespace RaceDay.Tests
{
    public class EventTests
    {
        private static EventRequest ValidEvent(string type = "run") => new()
        {
            Name = "Beachfront 10K",
            EventDate = DateTime.Today.AddDays(30),
            Location = "Cape Town",
            DistanceKm = 10,
            EventType = type
        };

        [Fact]
        public async Task CreateEvent_ByOrganiser_Returns201AndSavesEvent()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org", "org@test.com", TestHelpers.OrganiserRoleId);
            var controller = new EventsController(context).WithSession(organiser.UserId, "Organiser");

            var result = await controller.CreateEvent(ValidEvent());

            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(organiser.UserId, context.Events.Single().OrganiserId);
        }

        [Fact]
        public async Task CreateEvent_InvalidEventType_Returns400()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org", "org@test.com", TestHelpers.OrganiserRoleId);
            var controller = new EventsController(context).WithSession(organiser.UserId, "Organiser");

            var result = await controller.CreateEvent(ValidEvent("swim"));

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Events);
        }

        [Fact]
        public async Task UpdateEvent_ByDifferentOrganiser_Returns403()
        {
            using var context = TestHelpers.CreateContext();
            var owner = TestHelpers.AddUser(context, "Owner", "owner@test.com", TestHelpers.OrganiserRoleId);
            var other = TestHelpers.AddUser(context, "Other", "other@test.com", TestHelpers.OrganiserRoleId);
            var ev = TestHelpers.AddEvent(context, owner.UserId);
            var controller = new EventsController(context).WithSession(other.UserId, "Organiser");

            var result = await controller.UpdateEvent(ev.EventId, ValidEvent());

            Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
        }

        [Fact]
        public async Task DeleteEvent_ByOwner_RemovesEvent()
        {
            using var context = TestHelpers.CreateContext();
            var owner = TestHelpers.AddUser(context, "Owner", "owner@test.com", TestHelpers.OrganiserRoleId);
            var ev = TestHelpers.AddEvent(context, owner.UserId);
            var controller = new EventsController(context).WithSession(owner.UserId, "Organiser");

            var result = await controller.DeleteEvent(ev.EventId);

            Assert.IsType<OkObjectResult>(result);
            Assert.Empty(context.Events);
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;

namespace RaceDay.Tests
{
    // A fake session so controllers can be tested without a web server
    public class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public string Id => "test-session";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _store.Keys;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? value)
            => _store.TryGetValue(key, out value);
    }

    public static class TestHelpers
    {
        public const int OrganiserRoleId = 1;
        public const int ParticipantRoleId = 2;

        // A fresh in-memory database for every test, with the two roles seeded
        public static RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleId = OrganiserRoleId, RoleName = "Organiser" },
                    new Role { RoleId = ParticipantRoleId, RoleName = "Participant" });
                context.SaveChanges();
            }
            return context;
        }

        // Gives a controller a session, optionally "logged in" as a user
        public static T WithSession<T>(this T controller, int? userId = null, string? role = null) where T : ControllerBase
        {
            var session = new TestSession();
            if (userId != null) session.SetInt32("UserId", userId.Value);
            if (role != null) session.SetString("Role", role);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { Session = session }
            };
            return controller;
        }

        public static User AddUser(RaceDayDbContext c, string name, string email, int roleId)
        {
            var user = new User { FullName = name, Email = email, RoleId = roleId, PasswordHash = "hash", CreatedAt = DateTime.Now };
            c.Users.Add(user);
            c.SaveChanges();
            return user;
        }

        public static Event AddEvent(RaceDayDbContext c, int organiserId)
        {
            var ev = new Event
            {
                OrganiserId = organiserId,
                Name = "Test Run",
                Location = "Cape Town",
                EventDate = DateTime.Today.AddDays(10),
                DistanceKm = 10,
                EventType = "run"
            };
            c.Events.Add(ev);
            c.SaveChanges();
            return ev;
        }

        public static Category AddCategory(RaceDayDbContext c, int eventId)
        {
            var cat = new Category { EventId = eventId, CategoryName = "Senior" };
            c.Categories.Add(cat);
            c.SaveChanges();
            return cat;
        }

        public static Enrolment AddEnrolment(RaceDayDbContext c, int participantId, int eventId, int categoryId)
        {
            var en = new Enrolment
            {
                ParticipantId = participantId,
                EventId = eventId,
                CategoryId = categoryId,
                EnrolmentDate = DateTime.Now,
                Status = "Confirmed"
            };
            c.Enrolments.Add(en);
            c.SaveChanges();
            return en;
        }
    }
}
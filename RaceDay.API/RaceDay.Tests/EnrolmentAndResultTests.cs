using Microsoft.AspNetCore.Mvc;
using RaceDay.API.Controllers;
using RaceDay.API.Dtos;

namespace RaceDay.Tests
{
    public class EnrolmentAndResultTests
    {
        [Fact]
        public async Task Enrol_ValidRequest_Returns201AndRecordsEnrolment()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org", "org@test.com", TestHelpers.OrganiserRoleId);
            var runner = TestHelpers.AddUser(context, "Runner", "runner@test.com", TestHelpers.ParticipantRoleId);
            var ev = TestHelpers.AddEvent(context, organiser.UserId);
            var cat = TestHelpers.AddCategory(context, ev.EventId);
            var controller = new EnrolmentsController(context).WithSession(runner.UserId, "Participant");

            var result = await controller.Enrol(new EnrolRequest { EventId = ev.EventId, CategoryId = cat.CategoryId });

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            var saved = context.Enrolments.Single();
            Assert.Equal(runner.UserId, saved.ParticipantId);
            Assert.Equal(ev.EventId, saved.EventId);
            Assert.Equal(cat.CategoryId, saved.CategoryId);
        }

        [Fact]
        public async Task Enrol_AlreadyEnrolled_Returns409()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org", "org@test.com", TestHelpers.OrganiserRoleId);
            var runner = TestHelpers.AddUser(context, "Runner", "runner@test.com", TestHelpers.ParticipantRoleId);
            var ev = TestHelpers.AddEvent(context, organiser.UserId);
            var cat = TestHelpers.AddCategory(context, ev.EventId);
            TestHelpers.AddEnrolment(context, runner.UserId, ev.EventId, cat.CategoryId);
            var controller = new EnrolmentsController(context).WithSession(runner.UserId, "Participant");

            var result = await controller.Enrol(new EnrolRequest { EventId = ev.EventId, CategoryId = cat.CategoryId });

            Assert.IsType<ConflictObjectResult>(result);
            Assert.Single(context.Enrolments);
        }

        [Fact]
        public async Task Enrol_CategoryFromAnotherEvent_Returns400()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org", "org@test.com", TestHelpers.OrganiserRoleId);
            var runner = TestHelpers.AddUser(context, "Runner", "runner@test.com", TestHelpers.ParticipantRoleId);
            var event1 = TestHelpers.AddEvent(context, organiser.UserId);
            var event2 = TestHelpers.AddEvent(context, organiser.UserId);
            var categoryOfEvent2 = TestHelpers.AddCategory(context, event2.EventId);
            var controller = new EnrolmentsController(context).WithSession(runner.UserId, "Participant");

            var result = await controller.Enrol(new EnrolRequest { EventId = event1.EventId, CategoryId = categoryOfEvent2.CategoryId });

            Assert.IsType<BadRequestObjectResult>(result);
            Assert.Empty(context.Enrolments);
        }

        [Fact]
        public async Task AddResult_ByOwningOrganiser_Returns201()
        {
            using var context = TestHelpers.CreateContext();
            var organiser = TestHelpers.AddUser(context, "Org", "org@test.com", TestHelpers.OrganiserRoleId);
            var runner = TestHelpers.AddUser(context, "Runner", "runner@test.com", TestHelpers.ParticipantRoleId);
            var ev = TestHelpers.AddEvent(context, organiser.UserId);
            var cat = TestHelpers.AddCategory(context, ev.EventId);
            var enrolment = TestHelpers.AddEnrolment(context, runner.UserId, ev.EventId, cat.CategoryId);
            var controller = new ResultsController(context).WithSession(organiser.UserId, "Organiser");

            var result = await controller.AddResult(new ResultRequest
            {
                EnrolmentId = enrolment.EnrolmentId,
                FinishTime = TimeSpan.FromMinutes(52),
                FinishPosition = 47
            });

            Assert.Equal(201, Assert.IsType<ObjectResult>(result).StatusCode);
            Assert.Equal(47, context.Results.Single().FinishPosition);
        }

        [Fact]
        public async Task AddResult_ByDifferentOrganiser_Returns403()
        {
            using var context = TestHelpers.CreateContext();
            var owner = TestHelpers.AddUser(context, "Owner", "owner@test.com", TestHelpers.OrganiserRoleId);
            var other = TestHelpers.AddUser(context, "Other", "other@test.com", TestHelpers.OrganiserRoleId);
            var runner = TestHelpers.AddUser(context, "Runner", "runner@test.com", TestHelpers.ParticipantRoleId);
            var ev = TestHelpers.AddEvent(context, owner.UserId);
            var cat = TestHelpers.AddCategory(context, ev.EventId);
            var enrolment = TestHelpers.AddEnrolment(context, runner.UserId, ev.EventId, cat.CategoryId);
            var controller = new ResultsController(context).WithSession(other.UserId, "Organiser");

            var result = await controller.AddResult(new ResultRequest
            {
                EnrolmentId = enrolment.EnrolmentId,
                FinishTime = TimeSpan.FromMinutes(52),
                FinishPosition = 47
            });

            Assert.Equal(403, Assert.IsType<ObjectResult>(result).StatusCode);
            Assert.Empty(context.Results);
        }
    }
}
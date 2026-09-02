using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Core.Services;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Ticket;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Services.Tests.Services
{
    [TestFixture]
    public class TicketFollowerServiceTests
    {
        private Mock<ITicketFollowerRepository> _ticketFollowerRepositoryMock = null!;
        private Mock<ITicketService> _ticketServiceMock = null!;
        private TicketFollowerService _ticketFollowerService = null!;

        [SetUp]
        public void SetUp()
        {
            _ticketFollowerRepositoryMock = new Mock<ITicketFollowerRepository>();
            _ticketServiceMock = new Mock<ITicketService>();

            _ticketFollowerService = new TicketFollowerService(_ticketFollowerRepositoryMock.Object, _ticketServiceMock.Object);
        }

        [Test]
        public async Task FollowAsyncWhenUserHasAccessAddsFollower()
        {
            const int ticketId = 1;
            const string userId = "user-1";

            _ticketServiceMock
                .Setup(service => service.CanUserAccessTicketAsync(ticketId, userId))
                .ReturnsAsync(true);

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.ExistsAsync(ticketId, userId))
                .ReturnsAsync(false);

            bool result = await _ticketFollowerService.FollowAsync(ticketId, userId, false);

            Assert.That(result, Is.True);

            _ticketFollowerRepositoryMock.Verify(repository => repository.Add(It.Is<TicketFollower>(follower => follower.TicketId == ticketId && follower.UserId == userId)), Times.Once);

            _ticketFollowerRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task FollowAsyncWhenUserHasNoAccessReturnsFalse()
        {
            const int ticketId = 1;
            const string userId = "user-1";

            _ticketServiceMock
                .Setup(service => service.CanUserAccessTicketAsync(ticketId, userId))
                .ReturnsAsync(false);

            bool result = await _ticketFollowerService.FollowAsync(ticketId, userId, false);

            Assert.That(result, Is.False);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.ExistsAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.Add(It.IsAny<TicketFollower>()),
                Times.Never);

            _ticketFollowerRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task FollowAsyncWhenUserAlreadyFollowsTicketReturnsTrueWithoutAdding()
        {
            const int ticketId = 1;
            const string userId = "user-1";

            _ticketServiceMock
                .Setup(service => service.CanUserAccessTicketAsync(ticketId, userId))
                .ReturnsAsync(true);

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.ExistsAsync(ticketId, userId))
                .ReturnsAsync(true);

            bool result = await _ticketFollowerService.FollowAsync(ticketId, userId, false);

            Assert.That(result, Is.True);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.Add(It.IsAny<TicketFollower>()),
                Times.Never);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Never);
        }

        [Test]
        public async Task FollowAsyncWhenUserIsAdministratorDoesNotCheckProjectAccess()
        {
            const int ticketId = 1;
            const string userId = "admin-1";

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.ExistsAsync(ticketId, userId))
                .ReturnsAsync(false);

            bool result = await _ticketFollowerService.FollowAsync(ticketId, userId, true);

            Assert.That(result, Is.True);

            _ticketServiceMock.Verify(
                service => service.CanUserAccessTicketAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>()),
                Times.Never);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.Add(
                    It.Is<TicketFollower>(follower =>
                        follower.TicketId == ticketId &&
                        follower.UserId == userId)),
                Times.Once);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Once);
        }

        [Test]
        public async Task FollowAsyncWhenAdministratorAlreadyFollowsTicketDoesNotAddDuplicate()
        {
            const int ticketId = 1;
            const string userId = "admin-1";

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.ExistsAsync(ticketId, userId))
                .ReturnsAsync(true);

            bool result = await _ticketFollowerService.FollowAsync(ticketId, userId, true);

            Assert.That(result, Is.True);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.Add(It.IsAny<TicketFollower>()),
                Times.Never);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Never);
        }

        [Test]
        public async Task UnfollowAsyncWhenFollowerExistsRemovesFollower()
        {
            const int ticketId = 1;
            const string userId = "user-1";

            var follower = new TicketFollower
            {
                TicketId = ticketId,
                UserId = userId
            };

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetAsync(ticketId, userId))
                .ReturnsAsync(follower);

            bool result = await _ticketFollowerService.UnfollowAsync(ticketId, userId);

            Assert.That(result, Is.True);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.Remove(follower),
                Times.Once);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Once);
        }

        [Test]
        public async Task UnfollowAsyncWhenFollowerDoesNotExistReturnsFalse()
        {
            const int ticketId = 1;
            const string userId = "user-1";

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetAsync(ticketId, userId))
                .ReturnsAsync((TicketFollower?)null);

            bool result = await _ticketFollowerService.UnfollowAsync(ticketId, userId);

            Assert.That(result, Is.False);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.Remove(It.IsAny<TicketFollower>()),
                Times.Never);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Never);
        }

        [Test]
        public async Task GetFollowedTicketsAsyncReturnsMappedTickets()
        {
            const string userId = "user-1";

            var tickets = new List<Ticket>
            {
                CreateTicket(1, "First ticket", "First project", userId, "Ivanov"),
                CreateTicket(2, "Second ticket", "Second project", "user-2", "Petrov")
            };

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetFollowedTicketsAsync(userId))
                .ReturnsAsync(tickets);

            List<TicketListVM> result = (await _ticketFollowerService.GetFollowedTicketsAsync(userId)).ToList();

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.Multiple(() =>
            {
                Assert.That(result[0].Id, Is.EqualTo(1));
                Assert.That(result[0].Title, Is.EqualTo("First ticket"));
                Assert.That(result[0].ProjectName, Is.EqualTo("First project"));
                Assert.That(result[0].CreatorName, Is.EqualTo("Ivanov"));
                Assert.That(result[0].IsCteator, Is.True);
                Assert.That(result[0].IsFollowing, Is.True);

                Assert.That(result[1].Id, Is.EqualTo(2));
                Assert.That(result[1].Title, Is.EqualTo("Second ticket"));
                Assert.That(result[1].ProjectName, Is.EqualTo("Second project"));
                Assert.That(result[1].CreatorName, Is.EqualTo("Petrov"));
                Assert.That(result[1].IsCteator, Is.False);
                Assert.That(result[1].IsFollowing, Is.True);
            });
        }

        [Test]
        public async Task GetFollowedTicketsAsyncWhenNoTicketsReturnsEmptyCollection()
        {
            const string userId = "user-1";

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetFollowedTicketsAsync(userId))
                .ReturnsAsync(new List<Ticket>());

            List<TicketListVM> result =
                (await _ticketFollowerService.GetFollowedTicketsAsync(userId)).ToList();

            Assert.That(result, Is.Empty);
        }

        private static Ticket CreateTicket(
            int id,
            string title,
            string projectName,
            string creatorId,
            string creatorLastName)
        {
            return new Ticket
            {
                Id = id,
                Title = title,
                Description = "Ticket description",
                CreatorId = creatorId,
                ProjectId = id,
                Project = new Project
                {
                    Id = id,
                    ProjectName = projectName
                },
                Creator = new ApplicationUser
                {
                    Id = creatorId,
                    FirstName = "Test",
                    LastName = creatorLastName,
                    Address = "Test address"
                }
            };
        }
    }
}

using HelpDeskApp.Core.Contracts;
using HelpDeskApp.Core.Services;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Project;
using HelpDeskApp.ViewModels.Models.Ticket;
using Moq;
using NUnit.Framework;

namespace HelpDeskApp.Services.Tests.Services
{
    [TestFixture]
    public class TicketServiceTests
    {
        private Mock<ITicketRepository> _ticketRepositoryMock = null!;
        private Mock<ITicketFollowerRepository> _ticketFollowerRepositoryMock = null!;
        private Mock<INotificationService> _notificationServiceMock = null!;
        private TicketService _ticketService = null!;

        [SetUp]
        public void SetUp()
        {
            _ticketRepositoryMock = new Mock<ITicketRepository>();
            _ticketFollowerRepositoryMock = new Mock<ITicketFollowerRepository>();
            _notificationServiceMock = new Mock<INotificationService>();

            _ticketService = new TicketService(
                _ticketRepositoryMock.Object,
                _ticketFollowerRepositoryMock.Object,
                _notificationServiceMock.Object);
        }

        [Test]
        public async Task GetAllTicketsAsyncWhenUserIsAdminReturnsAllTickets()
        {
            const string adminId = "admin-1";

            Ticket firstTicket = CreateTicket(1, "First ticket", 1, "First project", 1, "Open");
            Ticket secondTicket = CreateTicket(2, "Second ticket", 2, "Second project", 2, "Closed");

            var tickets = new List<Ticket>
            {
                firstTicket,
                secondTicket
            };

            _ticketRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(tickets);

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetFollowedTicketIdsAsync(adminId))
                .ReturnsAsync(new List<int> { secondTicket.Id });

            List<TicketListVM> result = (await _ticketService.GetAllTicketsAsync(adminId, true)).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.Single(t => t.Id == firstTicket.Id).IsFollowing, Is.False);
            Assert.That(result.Single(t => t.Id == secondTicket.Id).IsFollowing, Is.True);
        }

        [Test]
        public async Task GetAllTicketsAsyncWhenUserIsNotAdminReturnsOnlyAccessibleTickets()
        {
            const string userId = "user-1";

            Ticket allowedTicket = CreateTicket(1, "Allowed", 1, "Allowed project", 1, "Open");

            allowedTicket.Project.UsersProjects.Add(new UserProject
            {
                UserId = userId,
                ProjectId = allowedTicket.ProjectId
            });

            Ticket forbiddenTicket = CreateTicket(2, "Forbidden", 2, "Forbidden project", 1, "Open");

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllAsync())
                .ReturnsAsync(new List<Ticket> { allowedTicket, forbiddenTicket });

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetFollowedTicketIdsAsync(userId))
                .ReturnsAsync(new List<int> { allowedTicket.Id });

            List<TicketListVM> result =
                (await _ticketService.GetAllTicketsAsync(userId, false)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(allowedTicket.Id));
            Assert.That(result[0].IsFollowing, Is.True);
        }

        [Test]
        public async Task GetTicketByIdAsyncWhenTicketDoesNotExistReturnsNull()
        {
            const int ticketId = 1;

            _ticketRepositoryMock
                .Setup(repository => repository.GetWithRelatedDataAsync(ticketId))
                .ReturnsAsync((Ticket?)null);

            TicketDetailsVM? result = await _ticketService.GetTicketByIdAsync(ticketId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetTicketByIdAsyncWhenTicketExistsReturnsMappedModel()
        {
            Ticket ticket = CreateTicket(1, "Login problem", 1, "HelpDesk", 1, "Open");

            _ticketRepositoryMock
                .Setup(repository => repository.GetWithRelatedDataAsync(ticket.Id))
                .ReturnsAsync(ticket);

            TicketDetailsVM? result = await _ticketService.GetTicketByIdAsync(ticket.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(ticket.Id));
            Assert.That(result.Title, Is.EqualTo(ticket.Title));
            Assert.That(result.Status, Is.EqualTo("Open"));
            Assert.That(result.Category, Is.EqualTo("Hardware"));
            Assert.That(result.CreatorId, Is.EqualTo(ticket.CreatorId));
        }

        [Test]
        public async Task GetTicketEditAsyncWhenTicketDoesNotExistReturnsNull()
        {
            const int ticketId = 1;

            _ticketRepositoryMock
                .Setup(repository => repository.GetWithRelatedDataAsync(ticketId))
                .ReturnsAsync((Ticket?)null);

            TicketEditVM? result = await _ticketService.GetTicketEditAsync(ticketId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetTicketEditAsyncWhenTicketExistsReturnsMappedModel()
        {
            Ticket ticket = CreateTicket(1, "Edit ticket", 1, "HelpDesk", 1, "Open");
            ticket.AssigneeId = "user-1";

            var user = new ApplicationUser
            {
                Id = "user-1",
                UserName = "test-user"
            };

            _ticketRepositoryMock
                .Setup(repository => repository.GetWithRelatedDataAsync(ticket.Id))
                .ReturnsAsync(ticket);

            SetupTicketCollections(ticket.ProjectId, user);

            TicketEditVM? result = await _ticketService.GetTicketEditAsync(ticket.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(ticket.Id));
            Assert.That(result.Title, Is.EqualTo(ticket.Title));
            Assert.That(result.ProjectId, Is.EqualTo(ticket.ProjectId));
            Assert.That(result.StatusId, Is.EqualTo(1));
            Assert.That(result.AssigneeId, Is.EqualTo(ticket.AssigneeId));
            Assert.That(result.Categories, Is.Not.Empty);
            Assert.That(result.Projects, Is.Not.Empty);
            Assert.That(result.SubCategories, Is.Not.Empty);
            Assert.That(result.AvailableUsers, Is.Not.Empty);
        }

        [Test]
        public async Task GetTicketDeleteByIdAsyncWhenTicketDoesNotExistReturnsNull()
        {
            const int ticketId = 1;

            _ticketRepositoryMock
                .Setup(repository => repository.GetWithRelatedDataAsync(ticketId))
                .ReturnsAsync((Ticket?)null);

            TicketDeleteVM? result = await _ticketService.GetTicketDeleteByIdAsync(ticketId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetTicketDeleteByIdAsyncWhenTicketExistsReturnsMappedModel()
        {
            Ticket ticket = CreateTicket(1, "Delete ticket", 1, "HelpDesk", 2, "Closed");

            _ticketRepositoryMock
                .Setup(repository => repository.GetWithRelatedDataAsync(ticket.Id))
                .ReturnsAsync(ticket);

            TicketDeleteVM? result = await _ticketService.GetTicketDeleteByIdAsync(ticket.Id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(ticket.Id));
            Assert.That(result.Title, Is.EqualTo(ticket.Title));
            Assert.That(result.ProjectName, Is.EqualTo("HelpDesk"));
            Assert.That(result.Status, Is.EqualTo("Closed"));
        }

        [Test]
        public void GetTicketOpenStatusAsyncWhenOpenStatusDoesNotExistThrowsInvalidOperationException()
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetAllStatusesAsync())
                .ReturnsAsync(new List<TicketStatus>());

            InvalidOperationException? exception =
                Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await _ticketService.GetTicketOpenStatusAsync());

            Assert.That(
                exception!.Message,
                Is.EqualTo("Open ticket status is not configured."));
        }

        [Test]
        public async Task GetTicketOpenStatusAsyncWhenOpenStatusExistsReturnsMappedStatus()
        {
            var statuses = new List<TicketStatus>
            {
                new TicketStatus
                {
                    Id = 1,
                    TicketStatusName = "Open"
                },
                new TicketStatus
                {
                    Id = 2,
                    TicketStatusName = "Closed"
                }
            };

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllStatusesAsync())
                .ReturnsAsync(statuses);

            TicketStatusVM result = await _ticketService.GetTicketOpenStatusAsync();

            Assert.That(result.Id, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("Open"));
        }

        [Test]
        public void CreateTicketAsyncWhenProjectDoesNotExistThrowsKeyNotFoundException()
        {
            TicketFormVM model = CreateTicketForm();

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>());

            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _ticketService.CreateTicketAsync(model, "user-1", false));

            _ticketRepositoryMock.Verify(
                repository => repository.Add(It.IsAny<Ticket>()),
                Times.Never);
        }

        [Test]
        public void CreateTicketAsyncWhenUserCannotAccessProjectThrowsUnauthorizedAccessException()
        {
            TicketFormVM model = CreateTicketForm();

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>
                {
                    new Project
                    {
                        Id = model.ProjectId,
                        ProjectName = "HelpDesk"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUserProjectsAsync())
                .ReturnsAsync(new List<UserProject>());

            Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await _ticketService.CreateTicketAsync(model, "user-1", false));

            _ticketRepositoryMock.Verify(
                repository => repository.Add(It.IsAny<Ticket>()),
                Times.Never);
        }

        [Test]
        public void CreateTicketAsyncWhenSubCategoryDoesNotExistThrowsKeyNotFoundException()
        {
            const string userId = "user-1";
            TicketFormVM model = CreateTicketForm();

            SetupProjectAccess(model.ProjectId, userId);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllSubCategoriesAsync())
                .ReturnsAsync(new List<SubCategory>());

            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _ticketService.CreateTicketAsync(model, userId, false));
        }

        [Test]
        public void CreateTicketAsyncWhenSubCategoryBelongsToAnotherCategoryThrowsInvalidOperationException()
        {
            const string userId = "user-1";
            TicketFormVM model = CreateTicketForm();

            SetupProjectAccess(model.ProjectId, userId);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllSubCategoriesAsync())
                .ReturnsAsync(new List<SubCategory>
                {
                    new SubCategory
                    {
                        Id = model.SubCategoryId,
                        CategoryId = 999,
                        SubCategoryName = "Wrong category"
                    }
                });

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _ticketService.CreateTicketAsync(model, userId, false));
        }

        [Test]
        public void CreateTicketAsyncWhenAdminAssigneeDoesNotExistThrowsKeyNotFoundException()
        {
            TicketFormVM model = CreateTicketForm();
            model.AssigneeId = "missing-user";

            SetupValidCreateData(model, new List<UserProject>());

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUsersAsync())
                .ReturnsAsync(new List<ApplicationUser>());

            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _ticketService.CreateTicketAsync(model, "admin-1", true));
        }

        [Test]
        public void CreateTicketAsyncWhenAssigneeIsNotInProjectThrowsInvalidOperationException()
        {
            TicketFormVM model = CreateTicketForm();
            model.AssigneeId = "user-2";

            SetupValidCreateData(model, new List<UserProject>());

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUsersAsync())
                .ReturnsAsync(new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        Id = model.AssigneeId
                    }
                });

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _ticketService.CreateTicketAsync(model, "admin-1", true));
        }

        [Test]
        public async Task CreateTicketAsyncWhenUserDataIsValidCreatesTicketWithoutAssignee()
        {
            const string userId = "user-1";
            TicketFormVM model = CreateTicketForm();
            model.Title = "  New ticket  ";
            model.Description = "  New description  ";
            model.AssigneeId = "manipulated-assignee";

            var userProjects = new List<UserProject>
            {
                new UserProject
                {
                    UserId = userId,
                    ProjectId = model.ProjectId
                }
            };

            SetupValidCreateData(model, userProjects);

            Ticket? addedTicket = null;

            _ticketRepositoryMock
                .Setup(repository => repository.Add(It.IsAny<Ticket>()))
                .Callback<Ticket>(ticket => addedTicket = ticket);

            await _ticketService.CreateTicketAsync(model, userId, false);

            Assert.That(addedTicket, Is.Not.Null);
            Assert.That(addedTicket!.Title, Is.EqualTo("New ticket"));
            Assert.That(addedTicket.Description, Is.EqualTo("New description"));
            Assert.That(addedTicket.CreatorId, Is.EqualTo(userId));
            Assert.That(addedTicket.AssigneeId, Is.Null);
            Assert.That(addedTicket.StatusId, Is.EqualTo(1));

            _ticketRepositoryMock.Verify(repository => repository.Add(addedTicket), Times.Once);
            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CreateTicketAsyncWhenAdminDataIsValidCreatesTicketWithAssignee()
        {
            TicketFormVM model = CreateTicketForm();
            model.AssigneeId = "user-2";

            var userProjects = new List<UserProject>
            {
                new UserProject
                {
                    UserId = model.AssigneeId,
                    ProjectId = model.ProjectId
                }
            };

            SetupValidCreateData(model, userProjects);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUsersAsync())
                .ReturnsAsync(new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        Id = model.AssigneeId
                    }
                });

            Ticket? addedTicket = null;

            _ticketRepositoryMock
                .Setup(repository => repository.Add(It.IsAny<Ticket>()))
                .Callback<Ticket>(ticket => addedTicket = ticket);

            await _ticketService.CreateTicketAsync(model, "admin-1", true);

            Assert.That(addedTicket, Is.Not.Null);
            Assert.That(addedTicket!.AssigneeId, Is.EqualTo(model.AssigneeId));

            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task GetTicketCategoriesAsyncReturnsMappedCategories()
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>
                {
                    new Category
                    {
                        Id = 1,
                        CategoryName = "Hardware"
                    }
                });

            List<CategoryVM> result =
                (await _ticketService.GetTicketCategoriesAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Hardware"));
        }

        [Test]
        public async Task GetTicketProjectsAsyncReturnsMappedProjects()
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>
                {
                    new Project
                    {
                        Id = 1,
                        ProjectName = "HelpDesk"
                    }
                });

            List<ProjectIndexVM> result =
                (await _ticketService.GetTicketProjectsAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].ProjectName, Is.EqualTo("HelpDesk"));
        }

        [Test]
        public async Task GetTicketSubCategoriesAsyncReturnsOnlyRequestedCategory()
        {
            var subCategories = new List<SubCategory>
            {
                new SubCategory
                {
                    Id = 1,
                    CategoryId = 1,
                    SubCategoryName = "Laptop"
                },
                new SubCategory
                {
                    Id = 2,
                    CategoryId = 2,
                    SubCategoryName = "Account"
                }
            };

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllSubCategoriesAsync())
                .ReturnsAsync(subCategories);

            List<SubCategoryVM> result =
                (await _ticketService.GetTicketSubCategoriesAsync(1)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Laptop"));
        }

        [Test]
        public void EditTicketAsyncWhenTicketDoesNotExistThrowsKeyNotFoundException()
        {
            TicketEditVM model = CreateTicketEdit();

            _ticketRepositoryMock
                .Setup(repository => repository.GetByIdAsync(model.Id))
                .ReturnsAsync((Ticket?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _ticketService.EditTicketAsync(model, true));

            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public void EditTicketAsyncWhenProjectDoesNotExistThrowsKeyNotFoundException()
        {
            TicketEditVM model = CreateTicketEdit();
            Ticket ticket = CreateTicket(model.Id, "Old title", 1, "HelpDesk", 1, "Open");

            _ticketRepositoryMock.Setup(repository => repository.GetByIdAsync(model.Id)).ReturnsAsync(ticket);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>());

            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _ticketService.EditTicketAsync(model, true));
        }

        [Test]
        public async Task EditTicketAsyncWhenUserIsNotAdminPreservesProtectedFields()
        {
            TicketEditVM model = CreateTicketEdit();
            model.ProjectId = 999;
            model.StatusId = 999;
            model.AssigneeId = "manipulated-user";

            Ticket ticket = CreateTicket(model.Id, "Old title", 1, "HelpDesk", 1, "Open");
            ticket.AssigneeId = "original-assignee";

            SetupValidEditData(ticket, model);

            await _ticketService.EditTicketAsync(model, false);

            Assert.That(ticket.ProjectId, Is.EqualTo(1));
            Assert.That(ticket.StatusId, Is.EqualTo(1));
            Assert.That(ticket.AssigneeId, Is.EqualTo("original-assignee"));
            Assert.That(ticket.Title, Is.EqualTo("Edited title"));

            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);

            _notificationServiceMock.Verify(
                service => service.NotifyTicketFollowersAsync(
                    ticket.Id,
                    $"Ticket {ticket.Title} was updated."),
                Times.Once);
        }

        [Test]
        public async Task EditTicketAsyncWhenAdminDataIsValidUpdatesTicketAndSendsNotification()
        {
            TicketEditVM model = CreateTicketEdit();
            model.AssigneeId = "user-2";

            Ticket ticket = CreateTicket(model.Id, "Old title", model.ProjectId, "HelpDesk", model.StatusId, "Open");

            SetupValidEditData(ticket, model);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUsersAsync())
                .ReturnsAsync(new List<ApplicationUser>
                {
                    new ApplicationUser
                    {
                        Id = model.AssigneeId
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUserProjectsAsync())
                .ReturnsAsync(new List<UserProject>
                {
                    new UserProject
                    {
                        UserId = model.AssigneeId,
                        ProjectId = model.ProjectId
                    }
                });

            await _ticketService.EditTicketAsync(model, true);

            Assert.That(ticket.Title, Is.EqualTo(model.Title.Trim()));
            Assert.That(ticket.Description, Is.EqualTo(model.Description.Trim()));
            Assert.That(ticket.ProjectId, Is.EqualTo(model.ProjectId));
            Assert.That(ticket.StatusId, Is.EqualTo(model.StatusId));
            Assert.That(ticket.AssigneeId, Is.EqualTo(model.AssigneeId));

            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);

            _notificationServiceMock.Verify(
                service => service.NotifyTicketFollowersAsync(
                    ticket.Id,
                    $"Ticket {ticket.Title} was updated."),
                Times.Once);
        }

        [Test]
        public async Task DeleteTicketAsyncWhenTicketDoesNotExistDoesNothing()
        {
            const int ticketId = 1;

            _ticketRepositoryMock
                .Setup(repository => repository.GetByIdAsync(ticketId))
                .ReturnsAsync((Ticket?)null);

            await _ticketService.DeleteTicketAsync(ticketId);

            _ticketRepositoryMock.Verify(
                repository => repository.Remove(It.IsAny<Ticket>()),
                Times.Never);

            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task DeleteTicketAsyncWhenTicketExistsDeletesTicket()
        {
            Ticket ticket = CreateTicket(1, "Delete", 1, "HelpDesk", 1, "Open");

            _ticketRepositoryMock
                .Setup(repository => repository.GetByIdAsync(ticket.Id))
                .ReturnsAsync(ticket);

            await _ticketService.DeleteTicketAsync(ticket.Id);

            _ticketRepositoryMock.Verify(repository => repository.Remove(ticket), Times.Once);
            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task GetProjectUsersAsyncReturnsOnlyUsersFromRequestedProject()
        {
            var firstUser = new ApplicationUser
            {
                Id = "user-1",
                UserName = "first-user"
            };

            var secondUser = new ApplicationUser
            {
                Id = "user-2",
                UserName = "second-user"
            };

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUserProjectsAsync())
                .ReturnsAsync(new List<UserProject>
                {
                    new UserProject
                    {
                        ProjectId = 1,
                        UserId = firstUser.Id,
                        User = firstUser
                    },
                    new UserProject
                    {
                        ProjectId = 2,
                        UserId = secondUser.Id,
                        User = secondUser
                    }
                });

            List<ProjectUserSelectVM> result =
                (await _ticketService.GetProjectUsersAsync(1)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(firstUser.Id));
            Assert.That(result[0].FullName, Is.EqualTo(firstUser.UserName));
        }

        [Test]
        public async Task CanUserAccessTicketAsyncWhenTicketDoesNotExistReturnsFalse()
        {
            const int ticketId = 1;

            _ticketRepositoryMock
                .Setup(repository => repository.GetTicketProjectIdAsync(ticketId))
                .ReturnsAsync((int?)null);

            bool result = await _ticketService.CanUserAccessTicketAsync(ticketId, "user-1");

            Assert.That(result, Is.False);

            _ticketRepositoryMock.Verify(
                repository => repository.UserProjectExistsAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task CanUserAccessTicketAsyncReturnsRepositoryResult(bool expectedResult)
        {
            const int ticketId = 1;
            const int projectId = 2;
            const string userId = "user-1";

            _ticketRepositoryMock
                .Setup(repository => repository.GetTicketProjectIdAsync(ticketId))
                .ReturnsAsync(projectId);

            _ticketRepositoryMock
                .Setup(repository => repository.UserProjectExistsAsync(projectId, userId))
                .ReturnsAsync(expectedResult);

            bool result = await _ticketService.CanUserAccessTicketAsync(ticketId, userId);

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task IsTicketCreatorAsyncReturnsRepositoryResult(bool expectedResult)
        {
            const int ticketId = 1;
            const string userId = "user-1";

            _ticketRepositoryMock
                .Setup(repository => repository.TicketCreatorExistsAsync(ticketId, userId))
                .ReturnsAsync(expectedResult);

            bool result = await _ticketService.IsTicketCreatorAsync(ticketId, userId);

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task GetStatusesAsyncReturnsMappedStatuses()
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetAllStatusesAsync())
                .ReturnsAsync(new List<TicketStatus>
                {
                    new TicketStatus
                    {
                        Id = 1,
                        TicketStatusName = "Open"
                    },
                    new TicketStatus
                    {
                        Id = 2,
                        TicketStatusName = "Closed"
                    }
                });

            List<TicketStatusVM> result = (await _ticketService.GetStatusesAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Open"));
            Assert.That(result[1].Name, Is.EqualTo("Closed"));
        }

        [Test]
        public void ChangeStatusAsyncWhenTicketDoesNotExistThrowsKeyNotFoundException()
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetByIdAsync(1))
                .ReturnsAsync((Ticket?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _ticketService.ChangeStatusAsync(1, 2));
        }       

        [Test]
        public async Task ChangeStatusAsyncWhenStatusIsUnchangedDoesNotSaveOrNotify()
        {
            Ticket ticket = CreateTicket(1, "Ticket", 1, "HelpDesk", 1, "Open");

            _ticketRepositoryMock.Setup(repository => repository.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllStatusesAsync())
                .ReturnsAsync(new List<TicketStatus> { ticket.Status });

            await _ticketService.ChangeStatusAsync(ticket.Id, ticket.StatusId);

            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);

            _notificationServiceMock.Verify(
                service => service.NotifyTicketFollowersAsync(It.IsAny<int>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public async Task ChangeStatusAsyncWhenStatusIsValidUpdatesAndNotifiesFollowers()
        {
            Ticket ticket = CreateTicket(1, "Ticket", 1, "HelpDesk", 1, "Open");

            var closedStatus = new TicketStatus
            {
                Id = 2,
                TicketStatusName = "Closed"
            };

            _ticketRepositoryMock.Setup(repository => repository.GetByIdAsync(ticket.Id)).ReturnsAsync(ticket);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllStatusesAsync())
                .ReturnsAsync(new List<TicketStatus> { ticket.Status, closedStatus });

            await _ticketService.ChangeStatusAsync(ticket.Id, closedStatus.Id);

            Assert.That(ticket.StatusId, Is.EqualTo(closedStatus.Id));

            _ticketRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);

            _notificationServiceMock.Verify(
                service => service.NotifyTicketFollowersAsync(
                    ticket.Id,
                    $"Ticket status was changed to {closedStatus.TicketStatusName}."),
                Times.Once);
        }

        [Test]
        public async Task GetAllTicketsAsyncWithQueryNormalizesAndPaginatesResult()
        {
            const string userId = "user-1";

            var queryModel = new TicketQueryVM
            {
                SearchTerm = "  login  ",
                ProjectId = 1,
                StatusId = 1,
                CurrentPage = 100,
                PageSize = 999
            };

            Ticket ticket = CreateTicket(1, "Login problem", 1, "HelpDesk", 1, "Open");
            ticket.CreatorId = userId;
            ticket.Creator = new ApplicationUser
            {
                Id = userId,
                FirstName = "Test",
                LastName = "User"
            };

            _ticketRepositoryMock
                .Setup(repository =>
                    repository.GetFilteredCountAsync(userId, false, "login", 1, 1))
                .ReturnsAsync(14);

            _ticketRepositoryMock
                .Setup(repository =>
                    repository.GetFilteredAsync(userId, false, "login", 1, 1, 3, 6))
                .ReturnsAsync(new List<Ticket> { ticket });

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetFollowedTicketIdsAsync(userId))
                .ReturnsAsync(new List<int> { ticket.Id });

            _ticketRepositoryMock
                .Setup(repository => repository.GetFilterProjectsAsync(userId, false))
                .ReturnsAsync(new List<Project> { ticket.Project });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllStatusesAsync())
                .ReturnsAsync(new List<TicketStatus> { ticket.Status });

            TicketQueryVM result =
                await _ticketService.GetAllTicketsAsync(queryModel, userId, false);

            Assert.That(result.SearchTerm, Is.EqualTo("login"));
            Assert.That(result.CurrentPage, Is.EqualTo(3));
            Assert.That(result.PageSize, Is.EqualTo(6));
            Assert.That(result.Result.TotalItems, Is.EqualTo(14));
            Assert.That(result.Result.TotalPages, Is.EqualTo(3));

            TicketListVM resultTicket = result.Result.Items.Single();

            Assert.That(resultTicket.Id, Is.EqualTo(ticket.Id));
            Assert.That(resultTicket.CreatorName, Is.EqualTo("Test User"));
            Assert.That(resultTicket.IsCteator, Is.True);
            Assert.That(resultTicket.IsFollowing, Is.True);
        }

        [Test]
        public async Task GetAvailableTicketProjectsAsyncReturnsMappedProjects()
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetFilterProjectsAsync("user-1", false))
                .ReturnsAsync(new List<Project>
                {
                    new Project
                    {
                        Id = 1,
                        ProjectName = "HelpDesk",
                        Description = null
                    }
                });

            List<ProjectIndexVM> result =
                (await _ticketService.GetAvailableTicketProjectsAsync("user-1", false)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].ProjectName, Is.EqualTo("HelpDesk"));
            Assert.That(result[0].Description, Is.EqualTo(string.Empty));
        }

        private static Ticket CreateTicket(
            int ticketId,
            string title,
            int projectId,
            string projectName,
            int statusId,
            string statusName)
        {
            var category = new Category
            {
                Id = 1,
                CategoryName = "Hardware"
            };

            var subCategory = new SubCategory
            {
                Id = 1,
                SubCategoryName = "Laptop",
                CategoryId = category.Id,
                Category = category
            };

            return new Ticket
            {
                Id = ticketId,
                Title = title,
                Description = "Test description",
                CreatorId = "creator-1",
                ProjectId = projectId,
                Project = new Project
                {
                    Id = projectId,
                    ProjectName = projectName
                },
                StatusId = statusId,
                Status = new TicketStatus
                {
                    Id = statusId,
                    TicketStatusName = statusName
                },
                SubCategoryId = subCategory.Id,
                SubCategory = subCategory
            };
        }

        private static TicketFormVM CreateTicketForm()
        {
            return new TicketFormVM
            {
                Title = "New ticket",
                Description = "New description",
                ProjectId = 1,
                CategoryId = 1,
                SubCategoryId = 1,
                Status = "Open"
            };
        }

        private static TicketEditVM CreateTicketEdit()
        {
            return new TicketEditVM
            {
                Id = 1,
                Title = "  Edited title  ",
                Description = "  Edited description  ",
                ProjectId = 1,
                CategoryId = 1,
                SubCategoryId = 1,
                StatusId = 1,
                Status = "Open"
            };
        }

        private void SetupProjectAccess(int projectId, string userId)
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>
                {
                    new Project
                    {
                        Id = projectId,
                        ProjectName = "HelpDesk"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUserProjectsAsync())
                .ReturnsAsync(new List<UserProject>
                {
                    new UserProject
                    {
                        UserId = userId,
                        ProjectId = projectId
                    }
                });
        }

        private void SetupValidCreateData(
            TicketFormVM model,
            IEnumerable<UserProject> userProjects)
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>
                {
                    new Project
                    {
                        Id = model.ProjectId,
                        ProjectName = "HelpDesk"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUserProjectsAsync())
                .ReturnsAsync(userProjects);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllSubCategoriesAsync())
                .ReturnsAsync(new List<SubCategory>
                {
                    new SubCategory
                    {
                        Id = model.SubCategoryId,
                        CategoryId = model.CategoryId,
                        SubCategoryName = "Laptop"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllStatusesAsync())
                .ReturnsAsync(new List<TicketStatus>
                {
                    new TicketStatus
                    {
                        Id = 1,
                        TicketStatusName = "Open"
                    }
                });
        }

        private void SetupValidEditData(Ticket ticket, TicketEditVM model)
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetByIdAsync(model.Id))
                .ReturnsAsync(ticket);

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>
                {
                    new Project
                    {
                        Id = ticket.ProjectId,
                        ProjectName = "HelpDesk"
                    },
                    new Project
                    {
                        Id = model.ProjectId,
                        ProjectName = "Selected project"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllSubCategoriesAsync())
                .ReturnsAsync(new List<SubCategory>
                {
                    new SubCategory
                    {
                        Id = model.SubCategoryId,
                        CategoryId = model.CategoryId,
                        SubCategoryName = "Laptop"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllStatusesAsync())
                .ReturnsAsync(new List<TicketStatus>
                {
                    new TicketStatus
                    {
                        Id = ticket.StatusId,
                        TicketStatusName = "Open"
                    },
                    new TicketStatus
                    {
                        Id = model.StatusId,
                        TicketStatusName = "Selected status"
                    }
                });
        }

        private void SetupTicketCollections(int projectId, ApplicationUser user)
        {
            _ticketRepositoryMock
                .Setup(repository => repository.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>
                {
                    new Category
                    {
                        Id = 1,
                        CategoryName = "Hardware"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllProjectsAsync())
                .ReturnsAsync(new List<Project>
                {
                    new Project
                    {
                        Id = projectId,
                        ProjectName = "HelpDesk"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllSubCategoriesAsync())
                .ReturnsAsync(new List<SubCategory>
                {
                    new SubCategory
                    {
                        Id = 1,
                        CategoryId = 1,
                        SubCategoryName = "Laptop"
                    }
                });

            _ticketRepositoryMock
                .Setup(repository => repository.GetAllUserProjectsAsync())
                .ReturnsAsync(new List<UserProject>
                {
                    new UserProject
                    {
                        ProjectId = projectId,
                        UserId = user.Id,
                        User = user
                    }
                });
        }
    }
}
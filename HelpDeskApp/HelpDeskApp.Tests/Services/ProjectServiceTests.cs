using HelpDeskApp.Core.Services;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Project;
using Moq;
using NUnit.Framework;

namespace HelpDeskApp.Services.Tests.Services
{
    [TestFixture]
    public class ProjectServiceTests
    {
        private Mock<IProjectRepository> _projectRepositoryMock = null!;
        private Mock<ITicketFollowerRepository> _ticketFollowerRepositoryMock = null!;
        private Mock<ITicketRepository> _ticketRepositoryMock = null!;
        private ProjectService _projectService = null!;

        [SetUp]
        public void SetUp()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _ticketFollowerRepositoryMock =  new Mock<ITicketFollowerRepository>();
            _ticketRepositoryMock =  new Mock<ITicketRepository>();

            _projectService = new ProjectService(_projectRepositoryMock.Object, _ticketFollowerRepositoryMock.Object, _ticketRepositoryMock.Object);
        }
        [Test]
        public async Task GetAllProjectsAsyncWhenUserIsAdminReturnsAllProjects()
        {           
            var projects = new List<Project>
            {
                new Project
                {
                    Id = 1,
                    ProjectName = "First project",
                    Description = "First description"
                },
                new Project
                {
                    Id = 2,
                    ProjectName = "Second project",
                    Description = "Second description"
                }
            };

            _projectRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(projects);
           
            List<ProjectIndexVM> result = (await _projectService.GetAllProjectsAsync(null,true)).ToList();          
            Assert.That(result.Count,Is.EqualTo(2));
        }
        [Test]
        public async Task GetAllProjectsAsyncWhenUserIsNotAdminReturnsOnlyAssignedProjects()
        {
            const string userId = "user-1";

            var assignedProject = new Project
            {
                Id = 1,
                ProjectName = "Assigned project",
                Description = "Assigned description"
            };

            assignedProject.UsersProjects.Add(new UserProject
            {
                UserId = userId,
                ProjectId = assignedProject.Id,
                IsFavorite = true
            });

            var unavailableProject = new Project
            {
                Id = 2,
                ProjectName = "Unavailable project",
                Description = "Unavailable description"
            };

            var projects = new List<Project>
            {
                assignedProject,
                unavailableProject
            };

            _projectRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(projects);

            List<ProjectIndexVM> result = (await _projectService.GetAllProjectsAsync(userId, false)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(assignedProject.Id));
            Assert.That(result[0].IsFavorite, Is.True);
        }

        [Test]
        public async Task GetProjectDetailsAsyncWhenProjectDoesNotExistReturnsNull()
        {
            const int projectId = 1;

            _projectRepositoryMock.Setup(repository => repository.GetWithRelatedDataAsync(projectId))
                .ReturnsAsync((Project?)null);

            ProjectDetailsVM? result = await _projectService.GetProjectDetailsAsync(projectId);

            Assert.That(result, Is.Null);
            _projectRepositoryMock.Verify(repository => repository.GetAllUsersAsync(), Times.Never);
        }

        [Test]
        public async Task GetProjectDetailsAsyncWhenProjectExistsReturnsMappedModel()
        {
            const int projectId = 1;

            var assignedUser = new ApplicationUser
            {
                Id = "assigned-user",
                UserName = "assigned@example.com"
            };

            var availableUser = new ApplicationUser
            {
                Id = "available-user",
                UserName = "available@example.com"
            };

            var project = new Project
            {
                Id = projectId,
                ProjectName = "HelpDesk",
                Description = "HelpDesk project"
            };

            project.UsersProjects.Add(new UserProject
            {
                UserId = assignedUser.Id,
                ProjectId = projectId,
                User = assignedUser,
                Project = project
            });

            project.Tickets.Add(new Ticket
            {
                Id = 10,
                Title = "Login problem",
                Description = "Cannot log in",
                ProjectId = projectId,
                StatusId = 1,
                Status = new TicketStatus
                {
                    Id = 1,
                    TicketStatusName = "Open"
                }
            });

            var users = new List<ApplicationUser>
            {
                assignedUser,
                availableUser
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetWithRelatedDataAsync(projectId))
                .ReturnsAsync(project);

            _projectRepositoryMock.Setup(repository => repository.GetAllUsersAsync()).ReturnsAsync(users);

            ProjectDetailsVM? result = await _projectService.GetProjectDetailsAsync(projectId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(projectId));
            Assert.That(result.ProjectName, Is.EqualTo("HelpDesk"));
            Assert.That(result.AssignedUsers.Count, Is.EqualTo(1));
            Assert.That(result.AssignedUsers.First().Id, Is.EqualTo(assignedUser.Id));
            Assert.That(result.AvailableUsers.Count, Is.EqualTo(1));
            Assert.That(result.AvailableUsers.First().Id, Is.EqualTo(availableUser.Id));
            Assert.That(result.Tickets.Count, Is.EqualTo(1));
            Assert.That(result.Tickets.First().Status, Is.EqualTo("Open"));
        }

        [Test]
        public async Task CreateProjectAsyncWithValidModelAddsAndReturnsProject()
        {
            var model = new ProjectCreateVM
            {
                ProjectName = "New project",
                Description = "New description",
                SelectedUserIds = new List<string>
                {
                    "user-1",
                    "user-2"
                }
            };

            Project? addedProject = null;

            _projectRepositoryMock
                .Setup(repository => repository.Add(It.IsAny<Project>()))
                .Callback<Project>(project => addedProject = project);

            Project result = await _projectService.CreateProjectAsync(model);

            Assert.That(addedProject, Is.Not.Null);
            Assert.That(result, Is.SameAs(addedProject));
            Assert.That(result.ProjectName, Is.EqualTo(model.ProjectName));
            Assert.That(result.Description, Is.EqualTo(model.Description));
            Assert.That(result.UsersProjects.Count, Is.EqualTo(2));
            Assert.That(result.UsersProjects.Any(up => up.UserId == "user-1"), Is.True);
            Assert.That(result.UsersProjects.Any(up => up.UserId == "user-2"), Is.True);

            _projectRepositoryMock.Verify(repository => repository.Add(It.IsAny<Project>()), Times.Once);
            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void EditProjectAsyncWhenProjectDoesNotExistThrowsKeyNotFoundException()
        {
            var model = new ProjectEditVM
            {
                Id = 1,
                ProjectName = "Edited project",
                Description = "Edited description"
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetByIdAsync(model.Id))
                .ReturnsAsync((Project?)null);

            Assert.ThrowsAsync<KeyNotFoundException>(
                async () => await _projectService.EditProjectAsync(model));

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task EditProjectAsyncWhenProjectExistsUpdatesProject()
        {
            var project = new Project
            {
                Id = 1,
                ProjectName = "Old name",
                Description = "Old description"
            };

            var model = new ProjectEditVM
            {
                Id = project.Id,
                ProjectName = "New name",
                Description = "New description"
            };

            _projectRepositoryMock.Setup(repository => repository.GetByIdAsync(model.Id)).ReturnsAsync(project);

            await _projectService.EditProjectAsync(model);

            Assert.That(project.ProjectName, Is.EqualTo(model.ProjectName));
            Assert.That(project.Description, Is.EqualTo(model.Description));

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task DeleteProjectAsyncWhenProjectDoesNotExistReturnsFalse()
        {
            const int projectId = 1;

            _projectRepositoryMock
                .Setup(repository => repository.GetByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            bool result = await _projectService.DeleteProjectAsync(projectId);

            Assert.That(result, Is.False);

            _projectRepositoryMock.Verify(repository => repository.HasTicketsAsync(It.IsAny<int>()), Times.Never);
            _projectRepositoryMock.Verify(repository => repository.Remove(It.IsAny<Project>()), Times.Never);
            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public void DeleteProjectAsyncWhenProjectHasTicketsThrowsInvalidOperationException()
        {
            const int projectId = 1;

            var project = new Project
            {
                Id = projectId,
                ProjectName = "Project with tickets"
            };

            _projectRepositoryMock.Setup(repository => repository.GetByIdAsync(projectId)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(repository => repository.HasTicketsAsync(projectId)).ReturnsAsync(true);

            InvalidOperationException? exception =
                Assert.ThrowsAsync<InvalidOperationException>( async () => await _projectService.DeleteProjectAsync(projectId));

            Assert.That(
                exception!.Message,
                Is.EqualTo("The project cannot be deleted because it contains tickets."));

            _projectRepositoryMock.Verify(
                repository => repository.Remove(It.IsAny<Project>()),
                Times.Never);

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task DeleteProjectAsyncWhenProjectHasNoTicketsDeletesProject()
        {
            const int projectId = 1;

            var project = new Project
            {
                Id = projectId,
                ProjectName = "Empty project"
            };

            _projectRepositoryMock.Setup(repository => repository.GetByIdAsync(projectId)).ReturnsAsync(project);
            _projectRepositoryMock.Setup(repository => repository.HasTicketsAsync(projectId)).ReturnsAsync(false);

            bool result = await _projectService.DeleteProjectAsync(projectId);

            Assert.That(result, Is.True);

            _projectRepositoryMock.Verify(repository => repository.Remove(project), Times.Once);
            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task GetProjectByIdAsyncWhenProjectDoesNotExistReturnsNull()
        {
            const int projectId = 1;

            _projectRepositoryMock
                .Setup(repository => repository.GetByIdAsync(projectId))
                .ReturnsAsync((Project?)null);

            Project? result = await _projectService.GetProjectByIdAsync(projectId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetProjectByIdAsyncWhenProjectExistsReturnsMappedProject()
        {
            const int projectId = 1;

            var storedProject = new Project
            {
                Id = projectId,
                ProjectName = "Stored project",
                Description = null
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetByIdAsync(projectId))
                .ReturnsAsync(storedProject);

            Project? result = await _projectService.GetProjectByIdAsync(projectId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.SameAs(storedProject));
            Assert.That(result!.Id, Is.EqualTo(storedProject.Id));
            Assert.That(result.ProjectName, Is.EqualTo(storedProject.ProjectName));
            Assert.That(result.Description, Is.EqualTo(string.Empty));
        }

        [Test]
        public void AssignUserToProjectAsyncWhenAssignmentExistsThrowsInvalidOperationException()
        {
            const int projectId = 1;
            const string userId = "user-1";

            _projectRepositoryMock
                .Setup(repository => repository.UserProjectExistsAsync(projectId, userId))
                .ReturnsAsync(true);

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _projectService.AssignUserToProjectAsync(projectId, userId));

            _projectRepositoryMock.Verify(
                repository => repository.AddUserProject(It.IsAny<UserProject>()),
                Times.Never);

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task AssignUserToProjectAsyncWhenAssignmentDoesNotExistAddsUserProject()
        {
            const int projectId = 1;
            const string userId = "user-1";

            _projectRepositoryMock
                .Setup(repository => repository.UserProjectExistsAsync(projectId, userId))
                .ReturnsAsync(false);

            await _projectService.AssignUserToProjectAsync(projectId, userId);

            _projectRepositoryMock.Verify(
                repository => repository.AddUserProject(
                    It.Is<UserProject>(up =>
                        up.ProjectId == projectId &&
                        up.UserId == userId)),
                Times.Once);

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task RemoveUserFromProjectAsyncWhenAssignmentDoesNotExistDoesNothing()
        {
            const int projectId = 1;
            const string userId = "user-1";

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync((UserProject?)null);

            await _projectService.RemoveUserFromProjectAsync(projectId, userId);

            _ticketRepositoryMock.Verify(repository => repository.GetAllAsync(), Times.Never);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.GetFollowedTicketsAsync(It.IsAny<string>()),
                Times.Never);

            _projectRepositoryMock.Verify(
                repository => repository.RemoveUserProject(It.IsAny<UserProject>()),
                Times.Never);

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task RemoveUserFromProjectAsyncWhenUserIsAssignedRemovesRelatedAccess()
        {
            const int projectId = 1;
            const int otherProjectId = 2;
            const string userId = "user-1";

            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId,
                IsFavorite = true
            };

            var assignedTicket = new Ticket
            {
                Id = 10,
                Title = "Assigned ticket",
                Description = "Description",
                ProjectId = projectId,
                AssigneeId = userId
            };

            var otherTicket = new Ticket
            {
                Id = 20,
                Title = "Other ticket",
                Description = "Description",
                ProjectId = otherProjectId,
                AssigneeId = userId
            };

            var follower = new TicketFollower
            {
                TicketId = assignedTicket.Id,
                UserId = userId
            };

            var tickets = new List<Ticket>
            {
                assignedTicket,
                otherTicket
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync(userProject);

            _ticketRepositoryMock.Setup(repository => repository.GetAllAsync()).ReturnsAsync(tickets);

            _ticketRepositoryMock
                .Setup(repository => repository.GetByIdAsync(assignedTicket.Id))
                .ReturnsAsync(assignedTicket);

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetFollowedTicketsAsync(userId))
                .ReturnsAsync(tickets);

            _ticketFollowerRepositoryMock
                .Setup(repository => repository.GetAsync(assignedTicket.Id, userId))
                .ReturnsAsync(follower);

            await _projectService.RemoveUserFromProjectAsync(projectId, userId);

            Assert.That(assignedTicket.AssigneeId, Is.Null);
            Assert.That(otherTicket.AssigneeId, Is.EqualTo(userId));

            _ticketFollowerRepositoryMock.Verify(repository => repository.Remove(follower), Times.Once);

            _ticketFollowerRepositoryMock.Verify(
                repository => repository.GetAsync(otherTicket.Id, userId),
                Times.Never);

            _projectRepositoryMock.Verify(
                repository => repository.RemoveUserProject(userProject),
                Times.Once);

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task GetAvailableUsersAsyncReturnsMappedUsers()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser
                {
                    Id = "user-1",
                    UserName = "first-user"
                },
                new ApplicationUser
                {
                    Id = "user-2",
                    UserName = null,
                    Email = "second@example.com"
                }
            };

            _projectRepositoryMock.Setup(repository => repository.GetAllUsersAsync()).ReturnsAsync(users);

            List<ProjectUserSelectVM> result =
                (await _projectService.GetAvailableUsersAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(
                result.Any(user =>
                    user.Id == "user-1" &&
                    user.FullName == "first-user"),
                Is.True);

            Assert.That(
                result.Any(user =>
                    user.Id == "user-2" &&
                    user.FullName == "second@example.com"),
                Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task IsUserInProjectAsyncReturnsRepositoryResult(bool expectedResult)
        {
            const int projectId = 1;
            const string userId = "user-1";

            _projectRepositoryMock.Setup(repository => repository.UserProjectExistsAsync(projectId, userId))
                .ReturnsAsync(expectedResult);

            bool result = await _projectService.IsUserInProjectAsync(projectId, userId);

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public async Task GetAllProjectsAsyncWithQueryNormalizesAndPaginatesResult()
        {
            const string userId = "user-1";

            var queryModel = new ProjectQueryVM
            {
                SearchTerm = "  help  ",
                FavoritesOnly = true,
                CurrentPage = 100,
                PageSize = 999
            };

            var project = new Project
            {
                Id = 1,
                ProjectName = "HelpDesk",
                Description = null
            };

            project.UsersProjects.Add(new UserProject
            {
                ProjectId = project.Id,
                UserId = userId,
                IsFavorite = true
            });

            _projectRepositoryMock
                .Setup(repository => repository.GetFilteredCountAsync(userId, false, "help", true))
                .ReturnsAsync(14);

            _projectRepositoryMock
                .Setup(repository => repository.GetFilteredAsync(userId, false, "help", true, 3, 6))
                .ReturnsAsync(new List<Project> { project });

            ProjectQueryVM result =
                await _projectService.GetAllProjectsAsync(queryModel, userId, false);

            Assert.That(result.SearchTerm, Is.EqualTo("help"));
            Assert.That(result.CurrentPage, Is.EqualTo(3));
            Assert.That(result.PageSize, Is.EqualTo(6));
            Assert.That(result.Result.TotalItems, Is.EqualTo(14));
            Assert.That(result.Result.TotalPages, Is.EqualTo(3));

            List<ProjectIndexVM> projects = result.Result.Items.ToList();

            Assert.That(projects.Count, Is.EqualTo(1));
            Assert.That(projects[0].Id, Is.EqualTo(project.Id));
            Assert.That(projects[0].Description, Is.EqualTo(string.Empty));
            Assert.That(projects[0].IsFavorite, Is.True);
        }

        [Test]
        public async Task GetAllProjectsAsyncWithInvalidLowPageUsesFirstPage()
        {
            var queryModel = new ProjectQueryVM
            {
                SearchTerm = "   ",
                FavoritesOnly = false,
                CurrentPage = -5,
                PageSize = 6
            };

            _projectRepositoryMock.Setup(repository => repository.GetFilteredCountAsync(null, true, null, false))
                .ReturnsAsync(0);

            _projectRepositoryMock.Setup(repository => repository.GetFilteredAsync(null, true, null, false, 1, 6))
                .ReturnsAsync(new List<Project>());

            ProjectQueryVM result =
                await _projectService.GetAllProjectsAsync(queryModel, null, true);

            Assert.That(result.SearchTerm, Is.Null);
            Assert.That(result.CurrentPage, Is.EqualTo(1));
            Assert.That(result.PageSize, Is.EqualTo(6));
            Assert.That(result.Result.Items, Is.Empty);
            Assert.That(result.Result.TotalItems, Is.EqualTo(0));
        }

        [Test]
        public async Task GetProjectForDeleteAsyncWhenProjectDoesNotExistReturnsNull()
        {
            const int projectId = 1;

            _projectRepositoryMock.Setup(repository => repository.GetByIdAsync(projectId)).ReturnsAsync((Project?)null);

            ProjectDeleteVM? result =
                await _projectService.GetProjectForDeleteAsync(projectId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetProjectForDeleteAsyncWhenProjectExistsReturnsMappedModel()
        {
            const int projectId = 1;

            var project = new Project
            {
                Id = projectId,
                ProjectName = "Delete project",
                Description = null
            };

            _projectRepositoryMock.Setup(repository => repository.GetByIdAsync(projectId)).ReturnsAsync(project);

            ProjectDeleteVM? result = await _projectService.GetProjectForDeleteAsync(projectId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(project.Id));
            Assert.That(result.ProjectName, Is.EqualTo(project.ProjectName));
            Assert.That(result.Description, Is.EqualTo(string.Empty));
        }



    }
}
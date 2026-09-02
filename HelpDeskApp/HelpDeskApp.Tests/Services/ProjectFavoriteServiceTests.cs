using HelpDeskApp.Core.Services;
using HelpDeskApp.Infrastructure.Data.Entities;
using HelpDeskApp.Infrastructure.Repositories.Contracts;
using HelpDeskApp.ViewModels.Models.Project;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpDeskApp.Services.Tests.Services
{

    [TestFixture]
    public class ProjectFavoriteServiceTests
    {
        private Mock<IProjectRepository> _projectRepositoryMock = null!;
        private ProjectFavoriteService _projectFavoriteService = null!;

        [SetUp]
        public void SetUp()
        {
            _projectRepositoryMock = new Mock<IProjectRepository>();
            _projectFavoriteService = new ProjectFavoriteService(_projectRepositoryMock.Object);
        }

        [Test]
        public async Task AddToFavoritesAsyncWhenUserHasAccessMarksProjectAsFavorite()
        {
            const int projectId = 1;
            const string userId = "user-1";

            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId,
                IsFavorite = false
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync(userProject);

            bool result = await _projectFavoriteService.AddToFavoritesAsync(projectId, userId);

            Assert.That(result, Is.True);
            Assert.That(userProject.IsFavorite, Is.True);

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task AddToFavoritesAsyncWhenUserHasNoAccessReturnsFalse()
        {
            const int projectId = 1;
            const string userId = "user-1";

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync((UserProject?)null);

            bool result = await _projectFavoriteService.AddToFavoritesAsync(projectId, userId);

            Assert.That(result, Is.False);

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(), Times.Never);
        }

        [Test]
        public async Task RemoveFromFavoritesAsyncWhenUserHasAccessRemovesProjectFromFavorites()
        {
            const int projectId = 1;
            const string userId = "user-1";

            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId,
                IsFavorite = true
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync(userProject);

            await _projectFavoriteService.RemoveFromFavoritesAsync(projectId, userId);

            Assert.That(userProject.IsFavorite, Is.False);

            _projectRepositoryMock.Verify(
                repository => repository.SaveChangesAsync(),
                Times.Once);
        }

        [Test]
        public void RemoveFromFavoritesAsyncWhenUserHasNoAccessThrowsUnauthorizedAccessException()
        {
            const int projectId = 1;
            const string userId = "user-1";

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync((UserProject?)null);

            Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _projectFavoriteService.RemoveFromFavoritesAsync(projectId, userId));

            _projectRepositoryMock.Verify(repository => repository.SaveChangesAsync(),Times.Never);
        }

        [Test]
        public async Task GetFavoriteProjectsAsyncReturnsMappedFavoriteProjects()
        {
            const string userId = "user-1";

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
                    Description = null
                }
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetFavoriteProjectsAsync(userId))
                .ReturnsAsync(projects);

            List<ProjectIndexVM> result = (await _projectFavoriteService.GetFavoriteProjectsAsync(userId)).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].ProjectName, Is.EqualTo("First project"));
            Assert.That(result[0].Description, Is.EqualTo("First description"));
            Assert.That(result[1].Id, Is.EqualTo(2));
            Assert.That(result[1].ProjectName, Is.EqualTo("Second project"));
            Assert.That(result[1].Description, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task IsFavoriteAsyncWhenProjectIsFavoriteReturnsTrue()
        {
            const int projectId = 1;
            const string userId = "user-1";

            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId,
                IsFavorite = true
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync(userProject);

            bool result = await _projectFavoriteService.IsFavoriteAsync(projectId, userId);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsFavoriteAsyncWhenProjectIsNotFavoriteReturnsFalse()
        {
            const int projectId = 1;
            const string userId = "user-1";

            var userProject = new UserProject
            {
                ProjectId = projectId,
                UserId = userId,
                IsFavorite = false
            };

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync(userProject);

            bool result = await _projectFavoriteService.IsFavoriteAsync(projectId, userId);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsFavoriteAsyncWhenUserHasNoAccessReturnsFalse()
        {
            const int projectId = 1;
            const string userId = "user-1";

            _projectRepositoryMock
                .Setup(repository => repository.GetUserProjectAsync(projectId, userId))
                .ReturnsAsync((UserProject?)null);

            bool result = await _projectFavoriteService.IsFavoriteAsync(projectId, userId);

            Assert.That(result, Is.False);
        }
    }
}

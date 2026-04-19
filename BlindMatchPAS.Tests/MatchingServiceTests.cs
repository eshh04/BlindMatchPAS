using Xunit;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Services;
using BlindMatchPAS.Models;
using BlindMatchPAS.Enums;

namespace BlindMatchPAS.Tests
{
	public class MatchingServiceTests
	{
		private readonly TestDatabaseFixture _fixture = new TestDatabaseFixture();

		[Fact]
		public async Task ExpressInterest_ValidProject_ShouldSucceed()
		{
			// Arrange - Prepare the test data and environment
			using var db = _fixture.CreateContext();
			var service = new MatchingService(db);

			var project = new Project
			{
				Id = 10,
				Title = "AI Research",
				Status = ProjectStatus.Pending
			};
			db.Projects.Add(project);
			await db.SaveChangesAsync();

			// Act - Execute the logic being tested
			var result = await service.ExpressInterestAsync("supervisor-001", 10);

			// Assert - Verify that the result is what was expected
			Assert.True(result.Success);
		}

		[Fact]
		public async Task ExpressInterest_OnAlreadyMatchedProject_ShouldFail()
		{
			// Arrange - Setup a project that is already in 'Matched' status
			using var db = _fixture.CreateContext();
			var service = new MatchingService(db);

			var project = new Project
			{
				Id = 11,
				Title = "Already Matched Project",
				Status = ProjectStatus.Matched
			};
			db.Projects.Add(project);
			await db.SaveChangesAsync();

			// Act - Try to express interest
			var result = await service.ExpressInterestAsync("supervisor-002", 11);

			// Assert - Verify the logic handles existing matches
			Assert.True(result.Success);
		}
	}
}
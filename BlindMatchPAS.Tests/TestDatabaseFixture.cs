using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Data;

namespace BlindMatchPAS.Tests
{
	public class TestDatabaseFixture
	{
		public ApplicationDbContext CreateContext()
		{
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;

			return new ApplicationDbContext(options);
		}
	}
}
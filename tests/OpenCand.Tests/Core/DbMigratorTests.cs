using System.Text.RegularExpressions;
using OpenCand.Core.Database;

namespace OpenCand.Tests.Core;

public class DbMigratorTests
{
    [Fact]
    public void LoadMigrations_FindsEmbeddedScriptsInVersionOrder()
    {
        var migrations = DbMigrator.LoadMigrations();

        Assert.NotEmpty(migrations);
        Assert.Equal(migrations.Select(m => m.Version).Order(StringComparer.Ordinal), migrations.Select(m => m.Version));
        Assert.All(migrations, m => Assert.Matches(@"^\d{3}_[a-z0-9_]+$", m.Version));
        Assert.Equal(migrations.Count, migrations.Select(m => m.Version[..3]).Distinct().Count());
    }

    [Fact]
    public void Migrations_DoNotManageTheirOwnTransaction()
    {
        // DbMigrator wraps each script in a transaction together with its schema_migrations row.
        var statement = new Regex(@"^\s*(BEGIN|COMMIT|ROLLBACK|START\s+TRANSACTION)\s*;", RegexOptions.IgnoreCase | RegexOptions.Multiline);

        Assert.All(DbMigrator.LoadMigrations(), m => Assert.DoesNotMatch(statement, m.Sql));
    }

    [Fact]
    public void LoadResource_ReturnsMaterializedViews()
    {
        Assert.Contains("CREATE MATERIALIZED VIEW", DbMigrator.LoadResource("mv.sql"));
    }
}

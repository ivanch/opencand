using OpenCand.API.Model;

namespace OpenCand.Tests.Api;

public class PagingTests
{
    [Fact]
    public void Resolve_WithoutParameters_IsDisabled()
    {
        Assert.False(Paging.Resolve(null, null).Enabled);
    }

    [Fact]
    public void Resolve_DefaultsMissingValues()
    {
        var onlyPage = Paging.Resolve(3, null);
        var onlySize = Paging.Resolve(null, 20);

        Assert.True(onlyPage.Enabled);
        Assert.Equal((3, Paging.DefaultPageSize), (onlyPage.Page, onlyPage.PageSize));
        Assert.Equal((1, 20), (onlySize.Page, onlySize.PageSize));
    }

    [Fact]
    public void Offset_IsZeroBasedAndDoesNotOverflow()
    {
        Assert.Equal(0, Paging.Resolve(1, 50).Offset);
        Assert.Equal(100, Paging.Resolve(3, 50).Offset);
        Assert.Equal((long)int.MaxValue * 200 - 200, Paging.Resolve(int.MaxValue, 200).Offset);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(-1, 10)]
    [InlineData(1, 0)]
    [InlineData(1, -5)]
    [InlineData(1, Paging.MaxPageSize + 1)]
    public void Resolve_RejectsOutOfRangeValues(int page, int pageSize)
    {
        Assert.Throws<ArgumentException>(() => Paging.Resolve(page, pageSize));
    }

    [Fact]
    public void Resolve_AcceptsMaxPageSize()
    {
        Assert.Equal(Paging.MaxPageSize, Paging.Resolve(1, Paging.MaxPageSize).PageSize);
    }
}

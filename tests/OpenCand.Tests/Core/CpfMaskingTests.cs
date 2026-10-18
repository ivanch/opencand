using OpenCand.Core.Utils;

namespace OpenCand.Tests.Core;

public class CpfMaskingTests
{
    [Fact]
    public void MaskCpf_ShowsOnlyTheSixMiddleDigits()
    {
        Assert.Equal("***.456.789-**", CpfMasking.MaskCpf("12345678901"));
    }

    [Fact]
    public void MaskCpf_IsIdempotent()
    {
        var once = CpfMasking.MaskCpf("12345678901");

        Assert.Equal(once, CpfMasking.MaskCpf(once));
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("123456789012")]
    public void MaskCpf_LeavesNonCpfValuesUntouched(string value)
    {
        Assert.Equal(value, CpfMasking.MaskCpf(value));
    }
}

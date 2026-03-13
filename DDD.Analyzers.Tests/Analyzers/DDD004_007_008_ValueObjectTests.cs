using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
	DDD.Analyzers.ValueObjectImmutabilityAnalyzer,
	Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DDD.Analyzers.Tests.Analyzers
{
	public class DDD004_ValueObjectImmutabilityTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		[Fact]
		public async Task ValueObject_SoloGetters_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }
    public Money(decimal a, string c) { Amount = a; Currency = c; }
    public override bool Equals(object obj) => obj is Money o && Amount == o.Amount;
    public override int GetHashCode() => System.HashCode.Combine(Amount, Currency);
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ValueObject_GetterPrivateSet_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class Email
{
    public string Value { get; private set; }
    public Email(string v) { Value = v; }
    public override bool Equals(object obj) => obj is Email o && Value == o.Value;
    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ClaseSinAtributoValueObject_NoDiagnostico()
		{
			var source = @"
public class PlainClass { public string Name { get; set; } }";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ValueObject_ConSetterPublico_ReportaDDD004()
		{
			// El analizador también reporta DDD007/DDD008 porque la clase no tiene Equals/GetHashCode
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class {|DDD007:{|DDD008:Money|}|}
{
    {|DDD004:public decimal Amount { get; set; }|}
    {|DDD004:public string Currency { get; set; }|}
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ValueObject_UnaPropiedad_SetterPublico_ReportaDDD004()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class Temperature
{
    {|DDD004:public double Value { get; set; }|}
    public string Unit { get; }
    public Temperature(double v, string u) { Value = v; Unit = u; }
    public override bool Equals(object obj) => obj is Temperature o && Value == o.Value;
    public override int GetHashCode() => System.HashCode.Combine(Value, Unit);
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}
	}

	public class DDD007_ValueObjectEqualsMustBeOverriddenTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		[Fact]
		public async Task ValueObject_ConEquals_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class Money
{
    public decimal Amount { get; }
    public Money(decimal a) { Amount = a; }
    public override bool Equals(object obj) => obj is Money o && Amount == o.Amount;
    public override int GetHashCode() => Amount.GetHashCode();
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ValueObject_SinEquals_ReportaDDD007()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class {|DDD007:Money|}
{
    public decimal Amount { get; }
    public Money(decimal a) { Amount = a; }
    public override int GetHashCode() => Amount.GetHashCode();
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}
	}

	public class DDD008_ValueObjectGetHashCodeMustBeOverriddenTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		[Fact]
		public async Task ValueObject_ConGetHashCode_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class Money
{
    public decimal Amount { get; }
    public Money(decimal a) { Amount = a; }
    public override bool Equals(object obj) => obj is Money o && Amount == o.Amount;
    public override int GetHashCode() => Amount.GetHashCode();
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ValueObject_SinGetHashCode_ReportaDDD008()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class {|DDD008:Money|}
{
    public decimal Amount { get; }
    public Money(decimal a) { Amount = a; }
    public override bool Equals(object obj) => obj is Money o && Amount == o.Amount;
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}
	}
}

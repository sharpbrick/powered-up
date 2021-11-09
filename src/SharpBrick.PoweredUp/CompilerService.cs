namespace System.Runtime.CompilerServices;
#if NETSTANDARD2_1
/// <summary>
/// Dummy compilerServices which is only included in .NET5 (upwards). To be compatible with .NetStandard 2.1 a dummy is required.
/// <see href="https://developercommunity.visualstudio.com/t/error-cs0518-predefined-type-systemruntimecompiler/1244809" />
/// </summary>
internal static class IsExternalInit { }
#endif
using System.Reflection;

[assembly: AssemblyProduct("KoiVM")]
[assembly: AssemblyCompany("Ki")]
[assembly: AssemblyCopyright("Copyright (C) Ki 2014")]

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#elif __TRACE

[assembly: AssemblyConfiguration("Trace")]
#else

[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyVersion("{{VER}}")]
[assembly: AssemblyFileVersion("{{VER}}")]
[assembly: AssemblyInformationalVersion("{{TAG}}")]
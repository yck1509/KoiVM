using System;
using System.Reflection;

[assembly: Obfuscation(Exclude = false, Feature = "koi(dbgInfo=true,rtName=kiRT)", ApplyToMembers = false)]

[assembly: Obfuscation(Exclude = false, Feature =
	"preset(aggressive);+constants(mode=dynamic,decoderCount=10,cfg=true);" +
	"+ctrl flow(predicate=expression,intensity=100);+rename(renPublic=true,mode=sequential);" +
	"+ref proxy(mode=strong,encoding=expression,typeErasure=true);" +
	"+resources(mode=dynamic);"
#if DEBUG
 + "-anti debug;-rename;"
#endif
)]
namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("HtmlConsole")>]
[<assembly: AssemblyProductAttribute("HtmlConsole")>]
[<assembly: AssemblyDescriptionAttribute("Simple cross-platform console supporting HTML markup")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
    let [<Literal>] InformationalVersion = "1.0"

#I __SOURCE_DIRECTORY__
#I "./bin/Debug"
#r "Suave.dll"
#r "HtmlConsole.dll"
#r "Angara.Base.dll"
#r "Angara.Html.dll"
#r "Angara.Chart.dll"
#r "Angara.Table.dll"
#r "System.Collections.Immutable.dll"

open HtmlConsole
open Angara.Charting
open Angara.Data

Angara.Base.Init()

let x = [| 0..1000 |] |> Array.map (fun i -> float(i) / 200.0)
let y = x |> Array.map sin

let chart = Chart.ofList [ Plot.line(x, y, displayName = "sine") ]

"<H1>This is a chart:</H1>" |> WriteHtml

WriteHtml (Angara.Html.MakeEmbeddable "400px" chart)

let table = Table.OfColumns
                [ Column.Create ("x", x);
                  Column.Create ("sinx", y) ]

"<H1>This is a table:</H1>" |> WriteHtml

Angara.Html.MakeEmbeddable "600px" table |> WriteHtml

async {
    let mutable p = 0.0
    while true do
        do! Async.Sleep 1000
        let x = [| 0..1000 |] |> Array.map (fun i -> float(i) / 200.0)
        let y = x |> Array.map (fun x -> sin (x+p))
        let chart = Chart.ofList [ Plot.line(x, y, displayName = "sine") ]
        p <- p + 0.1
        WriteHtmlId "chart" (Angara.Html.MakeEmbeddable "400px" chart)

} |> Async.RunSynchronously
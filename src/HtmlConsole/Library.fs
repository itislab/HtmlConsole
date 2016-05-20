module HtmlConsole

open System
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Successful

module GS = 
    let private Delta = 128
    let empty<'t> = (0, Array.zeroCreate<'t> Delta)
    
    let add item gs : int * 't [] = 
        let count = fst gs
        let items = snd gs
        
        let items = 
            if Array.length items < count then items
            else 
                Array.concat [ items
                               Array.zeroCreate Delta ]
        items.[count] <- item
        (count + 1, items)
    
    let toSeq start (gs : int * 't []) = 
        let items = snd gs
        seq { 
            for i in start..(fst gs) - 1 do
                yield items.[i]
        }

let mutable internal contents = GS.empty<string * string>

let encodeJS (s : string) = 
    let sb = Text.StringBuilder()
    s |> Seq.iter (fun c -> (match c with
                             | '\"' -> sb.Append("\\\"")
                             | '\'' -> sb.Append("\\\'")
                             | '\\' -> sb.Append("\\\\")
                             | '\f' -> sb.Append("\\f")
                             | '\n' -> sb.Append("\\n")
                             | '\r' -> sb.Append("\\r")
                             | '\t' -> sb.Append("\\t");
                             | c -> let code = int c 
                                    if code < 32 || code > 127 then sb.AppendFormat("\\u{0:X04}", code) else sb.Append(c)) |> ignore)
    sb.ToString()

let private nl = Environment.NewLine


let internal documentTemplate =
    use sr = new IO.StreamReader(Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("console.html"))
    sr.ReadToEnd()

let rec appendOrReplace (id,elt) list =
    match list with
    | [] -> [ id, elt ]
    | (id',_) :: tail when id' = id -> (id,elt) :: tail
    | head :: tail -> head :: appendOrReplace (id,elt) tail
 
let internal getUpdates t = 
    let items = contents 
                |> GS.toSeq t 
                |> Seq.fold(fun updates (id,t) -> updates |> appendOrReplace (id, sprintf "{ \"id\": \"%s\", \"text\": \"%s\" }" (id |> encodeJS) (t |> encodeJS))) []
                |> Seq.map snd
                |> String.concat "," 
    sprintf "[%s]" items
    
let internal getDocument () = 
    let updates = contents |> GS.toSeq 0 |> Array.ofSeq
    let divs = updates
               |> Seq.fold (fun updates (id,t) -> updates |> appendOrReplace (id, sprintf "<div id=\"%s\">%s</div>" id t)) []
               |> Seq.map snd
               |> String.concat nl
    documentTemplate
        .Replace("{{content}}", divs)
        .Replace("{{timestamp}}", updates.Length.ToString())

/// Sample server that we want to host
let app = 
    choose 
      [ GET >=> choose 
                [ path "/" >=> request(fun _ -> OK (getDocument()));
                  path "/console.js" >=> Writers.setMimeType "application/javascript"
                                     >=> Embedded.sendResource (Reflection.Assembly.GetExecutingAssembly()) "console.js" false
                  path "/updates" >=> Writers.setMimeType "application/json"
                                  >=> request(fun r -> let t = match r.queryParam "t" with
                                                               | Choice1Of2 t -> match Int32.TryParse t with
                                                                                 | true, t -> t
                                                                                 | false, _ -> 0
                                                               | _ -> 0
                                                       OK (getUpdates t))] ]
        
/// Start server on the first available port in the range 8000..10000
/// and return the port number once the server is started (asynchronously)
let private startSuaveServer() = 
    Async.FromContinuations(fun (cont, _, _) -> 
        let startedEvent = Event<_>()
        startedEvent.Publish.Add(cont)
        async { 
            // Try random ports until we find one that works
            let rnd = System.Random()
            while true do
                let port = 8000 + rnd.Next(2000)
                let local = Suave.Http.HttpBinding.mkSimple HTTP "127.0.0.1" port
                let logger = Suave.Logging.Loggers.saneDefaultsFor Logging.LogLevel.Error
                
                let config = 
                    { defaultConfig with bindings = [ local ]
                                         logger = logger }
                
                let started, start = startWebServerAsync config app
                // If it starts OK, we get TCP binding & report success via event
                async { let! running = started
                        startedEvent.Trigger(running) } |> Async.Start
                // Try starting the server and handle SocketException
                try 
                    do! start
                with :? System.Net.Sockets.SocketException -> ()
        }
        |> Async.Start)

let mutable isOpened = false

let Open() = 
    if not isOpened then 
        isOpened <- true
        async {
            let! s = startSuaveServer()
            let binding = s.[0].Value.binding.ToString()
            System.Diagnostics.Process.Start ("http://" + binding) |> ignore
        } |> Async.RunSynchronously

let WriteHtml html = 
    Open()
    contents <- GS.add (Guid.NewGuid().ToString(), html) contents

let WriteHtmlId id html = 
    Open()
    contents <- GS.add (id, html) contents

let Save (path : string) =
    use sw = new IO.StreamWriter(path)
    sw.Write(getDocument())
module HtmlConsole

    module GS =
        let private Delta = 128
        
        let empty<'t> = (0,Array.zeroCreate<'t> Delta)

        let add item gs : (int * 't []) =
            let count = fst gs
            let items = snd gs
            let items = if Array.length items < count then items else Array.concat [ items; Array.zeroCreate Delta ]
            items.[count] <- item
            (count + 1, items)

        let toSeq start (gs : (int * 't [])) = let items = snd gs in seq { for i in start..fst gs do yield items.[i] }

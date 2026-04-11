open System.Reflection
open System.IO
open WebUI

let readResource (assembly: Assembly) (resourceName: string) =
    use stream = assembly.GetManifestResourceStream(resourceName)
    if isNull stream then
        failwithf "Resource '%s' not found in assembly." resourceName
    use reader = new StreamReader(stream)
    reader.ReadToEnd()

let tryFindResourceBySuffix (assembly: Assembly) (suffix: string) : string option =
    assembly.GetManifestResourceNames()
    |> Array.tryFind (fun n -> n.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase))

[<EntryPoint>]
let main argv =
    let assembly = Assembly.GetExecutingAssembly()

    let idxName =
        match tryFindResourceBySuffix assembly "index.html" with
        | Some n -> n
        | _ -> failwith "Could not find embedded index.html resource"

    let cssName =
        match tryFindResourceBySuffix assembly "App.css" with
        | Some n -> n
        | _ -> failwith "Could not find embedded App.css resource"

    let html = readResource assembly idxName
    let css = readResource assembly cssName
    let htmlWithCss = html.Replace("<!--CSS-->", sprintf "<style>%s</style>" css)

    let myWindow = WebUI.newWindow()
    WebUI.bind myWindow "myBtn" (WebUIEventHandler(fun _ -> printfn "Clicked!"))
    WebUI.showBrowser myWindow htmlWithCss WebUI.Browser.Chromium |> ignore
    WebUI.wait()
    0
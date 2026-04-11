open System.Reflection
open System.IO
open WebUI

let getResource name =
    let assembly = Assembly.GetExecutingAssembly()
    use stream = assembly.GetManifestResourceStream(name)
    use reader = new StreamReader(stream)
    reader.ReadToEnd()

[<EntryPoint>]
let main argv =
    let html = getResource "webui_explo.index.html"
    let css = getResource "webui_explo.static.App.css"
    let htmlWithCss = html.Replace("<!--CSS-->", sprintf "<style>%s</style>" css)
    
    let myWindow = WebUI.newWindow()
    WebUI.bind myWindow "myBtn" (WebUIEventHandler(fun _ -> printfn "Clicked!"))
    WebUI.showBrowser myWindow htmlWithCss WebUI.Browser.Chromium |> ignore
    WebUI.wait()
    0
module Tests

open System
open WebUI

let testNewWindow () =
    let window = WebUI.newWindow()
    printfn "testNewWindow: Created window with ID %u" window
    window

let testShowHtml () =
    let window = WebUI.newWindow()
    let html = "<html><head><script src=\"webui.js\"></script></head><body>Hello World!</body></html>"
    let result = WebUI.show window html
    printfn "testShowHtml: Show result = %b" result
    result

let testBindEvent () =
    let window = WebUI.newWindow()
    let handler = WebUIEventHandler(fun _ -> printfn "Button clicked!")
    WebUI.bind window "myButton" handler
    printfn "testBindEvent: Bound event handler"

let testRunJavaScript () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body><div id=\"test\">initial</div></body></html>" |> ignore
    WebUI.run window "document.getElementById('test').innerHTML = 'modified';"
    printfn "testRunJavaScript: JavaScript executed"

let testScriptWithResult () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body></body></html>" |> ignore
    let result = WebUI.script window "return 1 + 1;" 1000u 4096u
    printfn "testScriptWithResult: Script returned '%s'" result

let testWindowNavigation () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Page 1</body></html>" |> ignore
    WebUI.navigate window "data:text/html,<html><body>Page 2</body></html>"
    printfn "testWindowNavigation: Navigated to new URL"

let testWindowClose () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Test</body></html>" |> ignore
    WebUI.close window
    printfn "testWindowClose: Window closed"

let testDestroyWindow () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Test</body></html>" |> ignore
    WebUI.destroy window
    printfn "testDestroyWindow: Window destroyed"

let testWindowSetSize () =
    let window = WebUI.newWindow()
    WebUI.setSize window 800u 600u
    printfn "testWindowSetSize: Set window size to 800x600"

let testWindowSetPosition () =
    let window = WebUI.newWindow()
    WebUI.setPosition window 100u 100u
    printfn "testWindowSetPosition: Set window position to (100, 100)"

let testWindowSetCenter () =
    let window = WebUI.newWindow()
    WebUI.setCenter window
    printfn "testWindowSetCenter: Centered window"

let testSetResizable () =
    let window = WebUI.newWindow()
    WebUI.setResizable window false
    printfn "testSetResizable: Set window non-resizable"

let testSetFrameless () =
    let window = WebUI.newWindow()
    WebUI.setFrameless window true
    printfn "testSetFrameless: Set window frameless"

let testSetTransparent () =
    let window = WebUI.newWindow()
    WebUI.setTransparent window true
    printfn "testSetTransparent: Set window transparent"

let testSetKiosk () =
    let window = WebUI.newWindow()
    WebUI.setKiosk window true
    printfn "testSetKiosk: Set window in kiosk mode"

let testMinimize () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Test</body></html>" |> ignore
    WebUI.minimize window
    printfn "testMinimize: Minimized window"

let testMaximize () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Test</body></html>" |> ignore
    WebUI.maximize window
    printfn "testMaximize: Maximized window"

let testFocus () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Test</body></html>" |> ignore
    WebUI.focus window
    printfn "testFocus: Focused window"

let testIsShown () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Test</body></html>" |> ignore
    let shown = WebUI.isShown window
    printfn "testIsShown: Window is shown = %b" shown

let testGetUrl () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Test</body></html>" |> ignore
    let url = WebUI.getUrl window
    printfn "testGetUrl: URL = '%s'" url

let testSetRootFolder () =
    let window = WebUI.newWindow()
    let result = WebUI.setRootFolder window "www"
    printfn "testSetRootFolder: Set root folder result = %b" result

let testSetDefaultRootFolder () =
    let result = WebUI.setDefaultRootFolder "www"
    printfn "testSetDefaultRootFolder: Set default root folder result = %b" result

let testBrowserExist () =
    let exist = WebUI.browserExist (WebUI.browserToUInt WebUI.Browser.Chrome)
    printfn "testBrowserExist: Chrome exists = %b" exist

let testChromiumBrowser () =
    let window = WebUI.newWindow()
    let html = "<html><head><script src=\"webui.js\"></script></head><body><h1>Chromium Test</h1></body></html>"
    let result = WebUI.showBrowser window html WebUI.Browser.Chromium
    printfn "testChromiumBrowser: Show with Chromium = %b" result

let testEncode () =
    let encoded = WebUI.encode "test<string>"
    printfn "testEncode: Encoded = '%s'" encoded

let testDecode () =
    let decoded = WebUI.decode "test&lt;string&gt;"
    printfn "testDecode: Decoded = '%s'" decoded

let testGetMimeType () =
    let mime = WebUI.getMimeType "test.html"
    printfn "testGetMimeType: MIME type = '%s'" mime

let testSetTimeout () =
    WebUI.setTimeout 30u
    printfn "testSetTimeout: Set timeout to 30 seconds"

let testGetPort () =
    let window = WebUI.newWindow()
    WebUI.show window "<html><head><script src=\"webui.js\"></script></head><body>Test</body></html>" |> ignore
    let port = WebUI.getPort window
    printfn "testGetPort: Port = %u" port

let testGetFreePort () =
    let port = WebUI.getFreePort ()
    printfn "testGetFreePort: Free port = %u" port

let testClean () =
    WebUI.clean ()
    printfn "testClean: Cleaned up resources"

let testExit () =
    WebUI.exit ()
    printfn "testExit: Exited application"

let runAllTests () =
    printfn "=== Running WebUI Library Tests ==="
    
    testNewWindow() |> ignore
    testShowHtml() |> ignore
    testBindEvent() |> ignore
    testRunJavaScript() |> ignore
    testScriptWithResult() |> ignore
    testWindowNavigation() |> ignore
    testWindowClose() |> ignore
    testDestroyWindow() |> ignore
    testWindowSetSize() |> ignore
    testWindowSetPosition() |> ignore
    testWindowSetCenter() |> ignore
    testSetResizable() |> ignore
    testSetFrameless() |> ignore
    testSetTransparent() |> ignore
    testSetKiosk() |> ignore
    testMinimize() |> ignore
    testMaximize() |> ignore
    testFocus() |> ignore
    testIsShown() |> ignore
    testGetUrl() |> ignore
    testBrowserExist() |> ignore
    testChromiumBrowser() |> ignore
    testEncode() |> ignore
    testDecode() |> ignore
    testGetMimeType() |> ignore
    testSetTimeout() |> ignore
    testGetPort() |> ignore
    testGetFreePort() |> ignore
    testClean() |> ignore
    
    printfn "=== All Tests Completed ==="

[<EntryPoint>]
let main argv =
    runAllTests()
    0

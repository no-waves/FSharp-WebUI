module Prettier

open WebUI

let html = """<!DOCTYPE html>
<html>
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <script src="webui.js"></script>
    <title>Prettier</title>
    <style>
      :root { font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; }
      html, body { height: 100%; margin: 0; }
      body { display: flex; align-items: center; justify-content: center; transition: background 300ms ease; }
      .container {
        width: min(760px, 90vw);
        max-width: 760px;
      }
      .grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 18px;
      }
      .btn {
        padding: 26px 14px;
        border-radius: 14px;
        border: none;
        color: white;
        font-weight: 700;
        font-size: 16px;
        cursor: pointer;
        box-shadow: 0 6px 18px rgba(0,0,0,0.12), inset 0 -4px 6px rgba(0,0,0,0.06);
        transition: transform 160ms ease, box-shadow 160ms ease;
        display:flex;align-items:center;justify-content:center;
      }
      .btn:active { transform: translateY(2px) scale(0.998); }
      .label { text-shadow: 0 2px 8px rgba(0,0,0,0.18); }

      /* individual colors (fallback) */
      .c1 { background: linear-gradient(135deg,#FF6B6B,#FF8787); }
      .c2 { background: linear-gradient(135deg,#FFD93D,#FFC857); color:#222; }
      .c3 { background: linear-gradient(135deg,#6BCB77,#3DDC84); }
      .c4 { background: linear-gradient(135deg,#4D96FF,#7FB3FF); }
      .c5 { background: linear-gradient(135deg,#845EC2,#B388EB); }
      .c6 { background: linear-gradient(135deg,#FF9671,#FFB199); }
      .c7 { background: linear-gradient(135deg,#00C2A8,#3FE3BD); color:#022; }
      .c8 { background: linear-gradient(135deg,#FF6F91,#FF9CB2); }
      .c9 { background: linear-gradient(135deg,#2B2D42,#464754); }

      @media (max-width:520px) {
        .btn { padding: 18px 8px; font-size: 14px; }
      }
    </style>
  </head>
  <body>
    <div class="container">
      <div class="grid">
        <button id="btn1" class="btn c1"><span class="label">Coral</span></button>
        <button id="btn2" class="btn c2"><span class="label">Sun</span></button>
        <button id="btn3" class="btn c3"><span class="label">Mint</span></button>
        <button id="btn4" class="btn c4"><span class="label">Ocean</span></button>
        <button id="btn5" class="btn c5"><span class="label">Lavender</span></button>
        <button id="btn6" class="btn c6"><span class="label">Peach</span></button>
        <button id="btn7" class="btn c7"><span class="label">Teal</span></button>
        <button id="btn8" class="btn c8"><span class="label">Pink</span></button>
        <button id="btn9" class="btn c9"><span class="label">Charcoal</span></button>
      </div>
    </div>
  </body>
</html>"""

[<EntryPoint>]
let main argv =
    let win = WebUI.newWindow()

    let buttons = [
      ("btn1", "#FF6B6B")
      ("btn2", "#FFD93D")
      ("btn3", "#6BCB77")
      ("btn4", "#4D96FF")
      ("btn5", "#845EC2")
      ("btn6", "#FF9671")
      ("btn7", "#00C2A8")
      ("btn8", "#FF6F91")
      ("btn9", "#2B2D42")
    ]

    // Bind each button id to an event handler
    for (id, color) in buttons do
        let handler = WebUI.WebUIEventHandler(fun _ ->
            // print to the F# console
            printfn "Color selected: %s" color
            // change background in the webview
            let script = sprintf "document.body.style.background = '%s';" color
            WebUI.run win script
        )
        WebUI.bind win id handler

    WebUI.showBrowser win html WebUI.Browser.Chromium |> ignore
    WebUI.wait()
    0

open System
open WebUIBootstrap

let private parseBool (value: string) =
    if String.IsNullOrWhiteSpace(value) then false
    else
        match value.Trim().ToLowerInvariant() with
        | "1" | "true" | "yes" | "on" -> true
        | _ -> false

[<EntryPoint>]
let main argv =
    try
        let releaseTag =
            Environment.GetEnvironmentVariable("WEBUI_RELEASE_TAG")
            |> fun v -> if String.IsNullOrWhiteSpace(v) then "nightly" else v.Trim()

        let refresh =
            Environment.GetEnvironmentVariable("WEBUI_BOOTSTRAP_REFRESH")
            |> parseBool

        let nativePath = Environment.GetEnvironmentVariable("WEBUI_NATIVE_PATH")

        let resolvedPath = ensureNativeLibsFromInputs releaseTag refresh nativePath
        printfn "WebUI native libraries ready: %s" resolvedPath
        0
    with ex ->
        eprintfn "WebUI bootstrap failed: %s" ex.Message
        1

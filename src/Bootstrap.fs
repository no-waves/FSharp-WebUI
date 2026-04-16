module WebUIBootstrap

open System
open System.IO
open System.IO.Compression
open System.Net.Http
open System.Runtime.InteropServices
open System.Security.Cryptography
open System.Text.Json
open System.Threading

type EnsureOptions = {
    ReleaseTag: string
    Refresh: bool
    NativePathOverride: string option
}

type private ReleaseAsset = {
    Name: string
    DownloadUrl: string
    Digest: string
}

type private ReleaseInfo = {
    ReleaseId: string
    Assets: ReleaseAsset list
}

let private syncRoot = obj ()
let mutable private ensuredCache: Map<string, string> = Map.empty

let private knownLibraryNames =
    set [ "libwebui-2.so"; "webui-2.dll"; "libwebui-2.dylib" ]

let private normalizeTag (tag: string) =
    if String.IsNullOrWhiteSpace(tag) then "nightly" else tag.Trim()

let private normalizeSha256 (digest: string) =
    if String.IsNullOrWhiteSpace(digest) then
        ""
    elif digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase) then
        digest.Substring("sha256:".Length).Trim().ToLowerInvariant()
    else
        digest.Trim().ToLowerInvariant()

let private parseBool (value: string) =
    if String.IsNullOrWhiteSpace(value) then false
    else
        match value.Trim().ToLowerInvariant() with
        | "1" | "true" | "yes" | "on" -> true
        | _ -> false

let defaultOptions () =
    let releaseTag =
        Environment.GetEnvironmentVariable("WEBUI_RELEASE_TAG")
        |> normalizeTag

    let refresh =
        Environment.GetEnvironmentVariable("WEBUI_BOOTSTRAP_REFRESH")
        |> parseBool

    let nativePathOverride =
        Environment.GetEnvironmentVariable("WEBUI_NATIVE_PATH")
        |> Option.ofObj
        |> Option.map (fun p -> p.Trim())
        |> Option.filter (String.IsNullOrWhiteSpace >> not)

    {
        ReleaseTag = releaseTag
        Refresh = refresh
        NativePathOverride = nativePathOverride
    }

let private getPlatformAsset () =
    let arch = RuntimeInformation.ProcessArchitecture

    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        match arch with
        | Architecture.X64 -> "webui-windows-msvc-x64.zip", "webui-2.dll"
        | _ -> failwithf "Unsupported Windows architecture for WebUI bootstrap: %A" arch
    elif RuntimeInformation.IsOSPlatform(OSPlatform.Linux) then
        match arch with
        | Architecture.X64 -> "webui-linux-gcc-x64.zip", "libwebui-2.so"
        | Architecture.Arm64 -> "webui-linux-gcc-arm64.zip", "libwebui-2.so"
        | Architecture.Arm -> "webui-linux-gcc-arm.zip", "libwebui-2.so"
        | _ -> failwithf "Unsupported Linux architecture for WebUI bootstrap: %A" arch
    elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
        match arch with
        | Architecture.Arm64 -> "webui-macos-clang-arm64.zip", "libwebui-2.dylib"
        | Architecture.X64 -> "webui-macos-clang-x64.zip", "libwebui-2.dylib"
        | _ -> failwithf "Unsupported macOS architecture for WebUI bootstrap: %A" arch
    else
        failwith "Unsupported OS for WebUI bootstrap"

let getExpectedLibraryFileName () =
    snd (getPlatformAsset())

let private getCacheRoot () =
    if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "fsharp-webui")
    elif RuntimeInformation.IsOSPlatform(OSPlatform.OSX) then
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Caches", "fsharp-webui")
    else
        let xdg = Environment.GetEnvironmentVariable("XDG_CACHE_HOME")
        if String.IsNullOrWhiteSpace(xdg) then
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "fsharp-webui")
        else
            Path.Combine(xdg, "fsharp-webui")

let private tryFindLibraryInDirectory (directory: string) (fileName: string) =
    if not (Directory.Exists(directory)) then
        None
    else
        Directory.EnumerateFiles(directory, fileName, SearchOption.AllDirectories)
        |> Seq.tryHead
        |> Option.map Path.GetFullPath

let private resolveNativeOverridePath (overridePath: string) (expectedLibrary: string) =
    let fullPath = Path.GetFullPath(overridePath)

    if File.Exists(fullPath) then
        let fileName = Path.GetFileName(fullPath)
        if knownLibraryNames.Contains(fileName) then
            Path.GetDirectoryName(fullPath)
        else
            failwithf "WEBUI_NATIVE_PATH points to file '%s', expected one of %A" fullPath knownLibraryNames
    elif Directory.Exists(fullPath) then
        match tryFindLibraryInDirectory fullPath expectedLibrary with
        | Some libPath -> Path.GetDirectoryName(libPath)
        | None -> failwithf "WEBUI_NATIVE_PATH '%s' does not contain '%s'" fullPath expectedLibrary
    else
        failwithf "WEBUI_NATIVE_PATH '%s' does not exist" fullPath

let private createHttpClient () =
    let client = new HttpClient()
    client.DefaultRequestHeaders.UserAgent.ParseAdd("fsharp-webui-bootstrap/0.0.1")
    client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json")
    client

let private parseReleaseInfo (json: string) =
    use doc = JsonDocument.Parse(json)
    let root = doc.RootElement
    let releaseId = root.GetProperty("id").GetInt64().ToString()

    let assets =
        root.GetProperty("assets").EnumerateArray()
        |> Seq.map (fun asset ->
            let digest =
                match asset.TryGetProperty("digest") with
                | true, value -> value.GetString()
                | _ -> null

            {
                Name = asset.GetProperty("name").GetString()
                DownloadUrl = asset.GetProperty("browser_download_url").GetString()
                Digest = digest
            })
        |> Seq.toList

    { ReleaseId = releaseId; Assets = assets }

let private fetchReleaseInfo (releaseTag: string) =
    use http = createHttpClient ()
    let encodedTag = Uri.EscapeDataString(releaseTag)
    let url = $"https://api.github.com/repos/webui-dev/webui/releases/tags/{encodedTag}"
    let json = http.GetStringAsync(url).GetAwaiter().GetResult()
    parseReleaseInfo json

let private computeSha256Hex (filePath: string) =
    use stream = File.OpenRead(filePath)
    use sha = SHA256.Create()
    sha.ComputeHash(stream)
    |> Array.map (fun b -> b.ToString("x2"))
    |> String.concat ""

let private getVersionJsonPath (directory: string) =
    Path.Combine(directory, "version.json")

let private readVersionInfo (directory: string) =
    let versionPath = getVersionJsonPath directory
    if not (File.Exists(versionPath)) then
        None
    else
        try
            use doc = JsonDocument.Parse(File.ReadAllText(versionPath))
            let root = doc.RootElement

            let tryReadString (name: string) =
                match root.TryGetProperty(name) with
                | true, value when value.ValueKind = JsonValueKind.String -> Some(value.GetString())
                | _ -> None

            match tryReadString "tag", tryReadString "releaseId", tryReadString "asset", tryReadString "sha256" with
            | Some tag, Some releaseId, Some asset, Some sha256 ->
                Some(tag, releaseId, asset, normalizeSha256 sha256)
            | _ -> None
        with _ ->
            None

let private writeVersionInfo (directory: string) (tag: string) (releaseId: string) (asset: string) (sha256: string) =
    let payload =
        {| tag = tag
           releaseId = releaseId
           asset = asset
           sha256 = normalizeSha256 sha256
           downloadedAtUtc = DateTimeOffset.UtcNow.ToString("O") |}

    let json = JsonSerializer.Serialize(payload, JsonSerializerOptions(WriteIndented = true))
    File.WriteAllText(getVersionJsonPath directory, json)

let private withFileLock (lockFilePath: string) (action: unit -> 'T) =
    let lockDir = Path.GetDirectoryName(lockFilePath)
    Directory.CreateDirectory(lockDir) |> ignore

    let rec acquire attempt =
        try
            new FileStream(lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)
        with
        | :? IOException when attempt < 120 ->
            Thread.Sleep(100)
            acquire (attempt + 1)

    use fileLock = acquire 0
    action ()

let private deleteDirectoryIfExists (path: string) =
    if Directory.Exists(path) then
        Directory.Delete(path, true)

let private downloadFile (url: string) (destinationPath: string) =
    use http = createHttpClient ()
    use stream = http.GetStreamAsync(url).GetAwaiter().GetResult()
    use output = File.Create(destinationPath)
    stream.CopyTo(output)

let private ensureCacheArtifact (releaseTag: string) (refresh: bool) =
    let assetName, expectedLibrary = getPlatformAsset ()
    let release = fetchReleaseInfo releaseTag

    let asset =
        release.Assets
        |> List.tryFind (fun x -> String.Equals(x.Name, assetName, StringComparison.OrdinalIgnoreCase))
        |> Option.defaultWith (fun () ->
            let known = release.Assets |> List.map (fun a -> a.Name) |> String.concat ", "
            failwithf "Asset '%s' not found in release '%s'. Available assets: %s" assetName releaseTag known)

    let expectedSha256 =
        asset.Digest
        |> normalizeSha256

    if String.IsNullOrWhiteSpace(expectedSha256) then
        failwithf "Asset '%s' in release '%s' does not provide a SHA256 digest" asset.Name releaseTag

    let releaseDir = Path.Combine(getCacheRoot (), releaseTag)
    Directory.CreateDirectory(releaseDir) |> ignore

    let targetDir = Path.Combine(releaseDir, assetName)
    let lockFilePath = Path.Combine(releaseDir, $"{assetName}.lock")

    withFileLock lockFilePath (fun () ->
        let isValidCache =
            if refresh || not (Directory.Exists(targetDir)) then
                false
            else
                match readVersionInfo targetDir with
                | Some(tag, releaseId, storedAsset, storedSha) ->
                    String.Equals(tag, releaseTag, StringComparison.OrdinalIgnoreCase)
                    && String.Equals(releaseId, release.ReleaseId, StringComparison.Ordinal)
                    && String.Equals(storedAsset, asset.Name, StringComparison.OrdinalIgnoreCase)
                    && String.Equals(storedSha, expectedSha256, StringComparison.OrdinalIgnoreCase)
                    && tryFindLibraryInDirectory targetDir expectedLibrary |> Option.isSome
                | None -> false

        if isValidCache then
            let libPath =
                tryFindLibraryInDirectory targetDir expectedLibrary
                |> Option.defaultWith (fun () -> failwithf "Cached artifact '%s' does not contain '%s'" targetDir expectedLibrary)
            Path.GetDirectoryName(libPath)
        else
            let tempZipPath = Path.Combine(Path.GetTempPath(), $"fsharp-webui-{Guid.NewGuid():N}.zip")
            let stagingDir = Path.Combine(releaseDir, $".staging-{Guid.NewGuid():N}")

            try
                downloadFile asset.DownloadUrl tempZipPath

                let actualSha = computeSha256Hex tempZipPath
                if not (String.Equals(actualSha, expectedSha256, StringComparison.OrdinalIgnoreCase)) then
                    failwithf "SHA256 mismatch for '%s'. Expected %s, got %s" asset.Name expectedSha256 actualSha

                Directory.CreateDirectory(stagingDir) |> ignore
                ZipFile.ExtractToDirectory(tempZipPath, stagingDir)

                let nativeLibraryPath =
                    tryFindLibraryInDirectory stagingDir expectedLibrary
                    |> Option.defaultWith (fun () ->
                        failwithf "Extracted asset '%s' does not contain expected native library '%s'" asset.Name expectedLibrary)

                writeVersionInfo stagingDir releaseTag release.ReleaseId asset.Name expectedSha256

                deleteDirectoryIfExists targetDir
                Directory.Move(stagingDir, targetDir)

                let finalLibraryPath =
                    tryFindLibraryInDirectory targetDir expectedLibrary
                    |> Option.defaultWith (fun () ->
                        failwithf "Installed artifact '%s' does not contain expected native library '%s'" targetDir expectedLibrary)

                Path.GetDirectoryName(finalLibraryPath)
            finally
                if File.Exists(tempZipPath) then File.Delete(tempZipPath)
                if Directory.Exists(stagingDir) then deleteDirectoryIfExists stagingDir)

let ensureNativeLibsWithOptions (options: EnsureOptions) =
    let normalizedOptions =
        {
            ReleaseTag = normalizeTag options.ReleaseTag
            Refresh = options.Refresh
            NativePathOverride = options.NativePathOverride |> Option.map Path.GetFullPath
        }

    let nativeOverrideKey = defaultArg normalizedOptions.NativePathOverride ""
    let cacheKey = $"{normalizedOptions.ReleaseTag}|{nativeOverrideKey}"

    lock syncRoot (fun () ->
        if not normalizedOptions.Refresh then
            match ensuredCache.TryFind(cacheKey) with
            | Some path -> path
            | None ->
                let resolvedPath =
                    match normalizedOptions.NativePathOverride with
                    | Some nativePath ->
                        resolveNativeOverridePath nativePath (getExpectedLibraryFileName())
                    | None ->
                        ensureCacheArtifact normalizedOptions.ReleaseTag false

                ensuredCache <- ensuredCache.Add(cacheKey, resolvedPath)
                resolvedPath
        else
            let resolvedPath =
                match normalizedOptions.NativePathOverride with
                | Some nativePath ->
                    resolveNativeOverridePath nativePath (getExpectedLibraryFileName())
                | None ->
                    ensureCacheArtifact normalizedOptions.ReleaseTag true

            ensuredCache <- ensuredCache.Add(cacheKey, resolvedPath)
            resolvedPath)

let ensureNativeLibsFromInputs (releaseTag: string) (refresh: bool) (nativePathOverride: string) =
    ensureNativeLibsWithOptions {
        ReleaseTag = normalizeTag releaseTag
        Refresh = refresh
        NativePathOverride =
            nativePathOverride
            |> Option.ofObj
            |> Option.map (fun p -> p.Trim())
            |> Option.filter (String.IsNullOrWhiteSpace >> not)
    }

let ensureNativeLibs () =
    ensureNativeLibsWithOptions (defaultOptions ())

module WebUI

open System
open System.Reflection
open System.Runtime.InteropServices
open WebUIBootstrap

let private expectedLib = getExpectedLibraryFileName()

let private ensureBootstrapLoaded () =
    let nativeDir = ensureNativeLibs ()
    let sourcePath = System.IO.Path.Combine(nativeDir, expectedLib)

    if not (System.IO.File.Exists(sourcePath)) then
        failwithf "WebUI native library not found at '%s'" sourcePath

    let runtimePath = System.IO.Path.Combine(AppContext.BaseDirectory, expectedLib)

    let shouldCopy =
        if not (System.IO.File.Exists(runtimePath)) then true
        else
            let srcInfo = System.IO.FileInfo(sourcePath)
            let dstInfo = System.IO.FileInfo(runtimePath)
            srcInfo.Length <> dstInfo.Length || srcInfo.LastWriteTimeUtc > dstInfo.LastWriteTimeUtc

    if shouldCopy then
        System.IO.Directory.CreateDirectory(AppContext.BaseDirectory) |> ignore
        System.IO.File.Copy(sourcePath, runtimePath, true)

let inline private withBootstrap f =
    ensureBootstrapLoaded ()
    f ()

do ensureBootstrapLoaded ()

type Browser = Chrome | Firefox | Edge | Safari | AnyBrowser | Chromium

let browserToUInt = function
    | Chrome -> 2u
    | Firefox -> 3u
    | Edge -> 4u
    | Safari -> 5u
    | AnyBrowser -> 1u
    | Chromium -> 6u

type ShowWaitConnection = ShowWaitConnection
type UiEventBlocking = UiEventBlocking

module NativeMethods =
    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_new_window()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_new_window_id(uint window_number)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_new_window_id()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_destroy(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_show(uint window, string content)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_show_browser(uint window, string content, uint browser)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_wait()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_wait_async()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_exit()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_close(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_clean()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_hide(uint window, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_kiosk(uint window, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_size(uint window, uint width, uint height)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_position(uint window, uint x, uint y)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_center(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_resizable(uint window, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_minimum_size(uint window, uint width, uint height)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_frameless(uint window, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_transparent(uint window, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_run(uint window, string script)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_get_url(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_navigate(uint window, string url)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_is_shown(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_browser_exist(uint browser)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_best_browser(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_set_root_folder(uint window, string path)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_set_default_root_folder(string path)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_open_url(string url)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_focus(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_minimize(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_maximize(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_delete_profile(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_parent_process_id(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_child_process_id(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_get_hwnd(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_win32_get_hwnd(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_timeout(uint second)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_event_blocking(uint window, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_public(uint window, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_port(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_set_port(uint window, uint port)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_free_port()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_profile(uint window, string name, string path)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_proxy(uint window, string proxy_server)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_custom_parameters(uint window, string parameters)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_high_contrast(uint window, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_is_high_contrast()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_encode(string str)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_decode(string str)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_get_mime_type(string file)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_free(IntPtr ptr)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_icon(uint window, string icon, string icon_type)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_script(uint window, string script, uint timeout, System.Text.StringBuilder buffer, uint buffer_length)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_script_client(IntPtr e, string script, uint timeout, System.Text.StringBuilder buffer, uint buffer_length)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_run_client(IntPtr e, string script)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_send_raw(uint window, string functionName, IntPtr raw, uint size)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_send_raw_client(IntPtr e, string functionName, IntPtr raw, uint size)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_runtime(uint window, uint runtime)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_count(IntPtr e)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern int64 webui_get_int_at(IntPtr e, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern int64 webui_get_int(IntPtr e)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern double webui_get_float_at(IntPtr e, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern double webui_get_float(IntPtr e)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_get_string_at(IntPtr e, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_get_string(IntPtr e)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_get_bool_at(IntPtr e, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_get_bool(IntPtr e)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_size_at(IntPtr e, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_size(IntPtr e)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_return_int(IntPtr e, int64 n)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_return_float(IntPtr e, double f)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_return_string(IntPtr e, string s)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_return_bool(IntPtr e, bool b)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_bind(uint window, string element, IntPtr callback)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_context(uint window, string element, IntPtr context)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_get_context(IntPtr e)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_get_last_error_number()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_get_last_error_message()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_malloc(uint size)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_memcpy(IntPtr dest, IntPtr src, uint count)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_interface_bind(uint window, string element, IntPtr callback)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_interface_show_client(uint window, uint eventNumber, string content)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_interface_close_client(uint window, uint eventNumber)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_interface_navigate_client(uint window, uint eventNumber, string url)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_interface_is_app_running()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_interface_get_string_at(uint window, uint eventNumber, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern int64 webui_interface_get_int_at(uint window, uint eventNumber, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern double webui_interface_get_float_at(uint window, uint eventNumber, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_interface_get_bool_at(uint window, uint eventNumber, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_interface_get_size_at(uint window, uint eventNumber, uint index)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_interface_set_response(uint window, uint eventNumber, string response)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_interface_send_raw_client(uint window, uint eventNumber, string functionName, IntPtr raw, uint size)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_interface_run_client(uint window, uint eventNumber, string script)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_interface_script_client(uint window, uint eventNumber, string script, uint timeout, System.Text.StringBuilder buffer, uint buffer_length)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_navigate_client(IntPtr e, string url)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_show_client(IntPtr e, string content)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_close_client(IntPtr e)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern IntPtr webui_start_server(uint window, string content)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_show_wv(uint window, string content)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_browser_folder(string path)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_delete_all_profiles()

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_config(uint option, bool status)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_close_handler_wv(uint window, IntPtr handler)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_file_handler(uint window, IntPtr handler)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_file_handler_window(uint window, IntPtr handler)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_interface_set_response_file_handler(uint window, IntPtr response, int length)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern uint webui_interface_get_window_id(uint window)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern void webui_set_logger(IntPtr logger, IntPtr userData)

    [<DllImport("libwebui-2.so", CallingConvention = CallingConvention.Cdecl)>]
    extern bool webui_set_tls_certificate(string certificatePem, string privateKeyPem)

let newWindow () = withBootstrap (fun () -> NativeMethods.webui_new_window())
let newWindowId id = withBootstrap (fun () -> NativeMethods.webui_new_window_id id)
let getNewWindowId () = withBootstrap (fun () -> NativeMethods.webui_get_new_window_id())
let destroy w = withBootstrap (fun () -> NativeMethods.webui_destroy w)

let show (w: uint) (html: string) =
    withBootstrap (fun () ->
    NativeMethods.webui_show(w, html) |> ignore; true

    )
let showBrowser w c b = withBootstrap (fun () -> NativeMethods.webui_show_browser(w, c, browserToUInt b) |> ignore; true)
let wait () = withBootstrap (fun () -> NativeMethods.webui_wait())
let waitAsync () = withBootstrap (fun () -> NativeMethods.webui_wait_async())
let exit () = withBootstrap (fun () -> NativeMethods.webui_exit())
let close w = withBootstrap (fun () -> NativeMethods.webui_close w)
let clean () = withBootstrap (fun () -> NativeMethods.webui_clean())

let setHide w h = withBootstrap (fun () -> NativeMethods.webui_set_hide(w, h) |> ignore)
let setKiosk w k = withBootstrap (fun () -> NativeMethods.webui_set_kiosk(w, k) |> ignore)
let setSize w wdt hgt = withBootstrap (fun () -> NativeMethods.webui_set_size(w, wdt, hgt) |> ignore)
let setPosition w x y = withBootstrap (fun () -> NativeMethods.webui_set_position(w, x, y) |> ignore)
let setCenter w = withBootstrap (fun () -> NativeMethods.webui_set_center w)
let setResizable w r = withBootstrap (fun () -> NativeMethods.webui_set_resizable(w, r) |> ignore)
let setMinimumSize w wdt hgt = withBootstrap (fun () -> NativeMethods.webui_set_minimum_size(w, wdt, hgt) |> ignore)
let setFrameless w f = withBootstrap (fun () -> NativeMethods.webui_set_frameless(w, f) |> ignore)
let setTransparent w t = withBootstrap (fun () -> NativeMethods.webui_set_transparent(w, t) |> ignore)
let run w s = withBootstrap (fun () -> NativeMethods.webui_run(w, s) |> ignore)

let getUrl w = 
    ensureBootstrapLoaded ()
    let ptr = NativeMethods.webui_get_url w
    if ptr <> IntPtr.Zero then Marshal.PtrToStringAnsi(ptr) else ""
let navigate w u = withBootstrap (fun () -> NativeMethods.webui_navigate(w, u) |> ignore)
let isShown w = withBootstrap (fun () -> NativeMethods.webui_is_shown w)

let browserExist b = withBootstrap (fun () -> NativeMethods.webui_browser_exist (uint b))
let getBestBrowser w = withBootstrap (fun () -> NativeMethods.webui_get_best_browser w)

let setRootFolder w p = withBootstrap (fun () -> NativeMethods.webui_set_root_folder(w, p))
let setDefaultRootFolder p = withBootstrap (fun () -> NativeMethods.webui_set_default_root_folder p)
let openUrl u = withBootstrap (fun () -> NativeMethods.webui_open_url u |> ignore)
let focus w = withBootstrap (fun () -> NativeMethods.webui_focus w |> ignore)
let minimize w = withBootstrap (fun () -> NativeMethods.webui_minimize w |> ignore)
let maximize w = withBootstrap (fun () -> NativeMethods.webui_maximize w |> ignore)
let deleteProfile w = withBootstrap (fun () -> NativeMethods.webui_delete_profile w |> ignore)
let getParentProcessId w = withBootstrap (fun () -> NativeMethods.webui_get_parent_process_id w)
let getChildProcessId w = withBootstrap (fun () -> NativeMethods.webui_get_child_process_id w)

let setTimeout s = withBootstrap (fun () -> NativeMethods.webui_set_timeout s |> ignore)
let setEventBlocking w b = withBootstrap (fun () -> NativeMethods.webui_set_event_blocking(w, b) |> ignore)
let setPublic w p = withBootstrap (fun () -> NativeMethods.webui_set_public(w, p) |> ignore)
let getPort w = withBootstrap (fun () -> NativeMethods.webui_get_port w)
let setPort w p = withBootstrap (fun () -> NativeMethods.webui_set_port(w, p))
let getFreePort () = withBootstrap (fun () -> NativeMethods.webui_get_free_port())

let setProfile w n p = withBootstrap (fun () -> NativeMethods.webui_set_profile(w, n, p) |> ignore)
let setProxy w p = withBootstrap (fun () -> NativeMethods.webui_set_proxy(w, p) |> ignore)
let setCustomParameters w p = withBootstrap (fun () -> NativeMethods.webui_set_custom_parameters(w, p) |> ignore)
let setHighContrast w h = withBootstrap (fun () -> NativeMethods.webui_set_high_contrast(w, h) |> ignore)
let isHighContrast () = withBootstrap (fun () -> NativeMethods.webui_is_high_contrast())

let encode s =
    withBootstrap (fun () ->
    let ptr = NativeMethods.webui_encode s
    if ptr <> IntPtr.Zero then
        let r = Marshal.PtrToStringAnsi(ptr)
        NativeMethods.webui_free ptr
        r
    else ""
    )
let decode s =
    withBootstrap (fun () ->
    let ptr = NativeMethods.webui_decode s
    if ptr <> IntPtr.Zero then
        let r = Marshal.PtrToStringAnsi(ptr)
        NativeMethods.webui_free ptr
        r
    else ""
    )
let getMimeType f =
    withBootstrap (fun () ->
    let ptr = NativeMethods.webui_get_mime_type f
    if ptr <> IntPtr.Zero then Marshal.PtrToStringAnsi(ptr) else ""

    )
let setIcon w i t = withBootstrap (fun () -> NativeMethods.webui_set_icon(w, i, t) |> ignore)

let script (w: uint) (script: string) (timeout: uint) (bufferSize: uint) =
    withBootstrap (fun () ->
    if bufferSize = 0u then ""
    else
        let buffer = System.Text.StringBuilder(int bufferSize)
        let success = NativeMethods.webui_script(w, script, timeout, buffer, bufferSize)
        if success then buffer.ToString() else ""

    )
let scriptClient (e: IntPtr) (script: string) (timeout: uint) (bufferSize: uint) =
    withBootstrap (fun () ->
    if bufferSize = 0u then ""
    else
        let buffer = System.Text.StringBuilder(int bufferSize)
        let success = NativeMethods.webui_script_client(e, script, timeout, buffer, bufferSize)
        if success then buffer.ToString() else ""

    )
let runClient (e: IntPtr) (script: string) = withBootstrap (fun () -> NativeMethods.webui_run_client(e, script) |> ignore)

let sendRaw (w: uint) (functionName: string) (data: byte[]) =
    withBootstrap (fun () ->
    let ptr = Marshal.AllocHGlobal(data.Length)
    try
        Marshal.Copy(data, 0, ptr, data.Length)
        NativeMethods.webui_send_raw(w, functionName, ptr, uint data.Length)
    finally
        Marshal.FreeHGlobal(ptr)

    )
let sendRawClient (e: IntPtr) (functionName: string) (data: byte[]) =
    withBootstrap (fun () ->
    let ptr = Marshal.AllocHGlobal(data.Length)
    try
        Marshal.Copy(data, 0, ptr, data.Length)
        NativeMethods.webui_send_raw_client(e, functionName, ptr, uint data.Length)
    finally
        Marshal.FreeHGlobal(ptr)

    )
type Runtime = None | Deno | NodeJS | Bun

let setRuntime w r = 
    ensureBootstrapLoaded ()
    let runtimeVal = match r with None -> 0u | Deno -> 1u | NodeJS -> 2u | Bun -> 3u
    NativeMethods.webui_set_runtime(w, runtimeVal) |> ignore

let getCount (e: IntPtr) = withBootstrap (fun () -> NativeMethods.webui_get_count(e))
let getIntAt (e: IntPtr) (index: uint) = withBootstrap (fun () -> NativeMethods.webui_get_int_at(e, index))
let getInt (e: IntPtr) = withBootstrap (fun () -> NativeMethods.webui_get_int(e))
let getFloatAt (e: IntPtr) (index: uint) = withBootstrap (fun () -> NativeMethods.webui_get_float_at(e, index))
let getFloat (e: IntPtr) = withBootstrap (fun () -> NativeMethods.webui_get_float(e))
let getStringAt (e: IntPtr) (index: uint) = 
    ensureBootstrapLoaded ()
    let ptr = NativeMethods.webui_get_string_at(e, index)
    if ptr <> IntPtr.Zero then Marshal.PtrToStringAnsi(ptr) else ""
let getString (e: IntPtr) = 
    ensureBootstrapLoaded ()
    let ptr = NativeMethods.webui_get_string(e)
    if ptr <> IntPtr.Zero then Marshal.PtrToStringAnsi(ptr) else ""
let getBoolAt (e: IntPtr) (index: uint) = withBootstrap (fun () -> NativeMethods.webui_get_bool_at(e, index))
let getBool (e: IntPtr) = withBootstrap (fun () -> NativeMethods.webui_get_bool(e))
let getSizeAt (e: IntPtr) (index: uint) = withBootstrap (fun () -> NativeMethods.webui_get_size_at(e, index))
let getSize (e: IntPtr) = withBootstrap (fun () -> NativeMethods.webui_get_size(e))

let returnInt (e: IntPtr) (n: int64) = withBootstrap (fun () -> NativeMethods.webui_return_int(e, n) |> ignore)
let returnFloat (e: IntPtr) (f: double) = withBootstrap (fun () -> NativeMethods.webui_return_float(e, f) |> ignore)
let returnString (e: IntPtr) (s: string) = withBootstrap (fun () -> NativeMethods.webui_return_string(e, s) |> ignore)
let returnBool (e: IntPtr) (b: bool) = withBootstrap (fun () -> NativeMethods.webui_return_bool(e, b) |> ignore)

type WebUIEventHandler = delegate of (IntPtr) -> unit

let bind (w: uint) (element: string) (handler: WebUIEventHandler) =
    withBootstrap (fun () ->
    let ptr = Marshal.GetFunctionPointerForDelegate(handler)
    NativeMethods.webui_bind(w, element, ptr)

    )
let setContext (w: uint) (element: string) (context: IntPtr) = withBootstrap (fun () -> NativeMethods.webui_set_context(w, element, context) |> ignore)
let getContext (e: IntPtr) = withBootstrap (fun () -> NativeMethods.webui_get_context(e))

let getLastErrorNumber () = withBootstrap (fun () -> NativeMethods.webui_get_last_error_number())
let getLastErrorMessage () = 
    ensureBootstrapLoaded ()
    let ptr = NativeMethods.webui_get_last_error_message()
    if ptr <> IntPtr.Zero then Marshal.PtrToStringAnsi(ptr) else ""

let malloc (size: uint) = withBootstrap (fun () -> NativeMethods.webui_malloc(size))
let memcpy (dest: IntPtr) (src: IntPtr) (count: uint) = withBootstrap (fun () -> NativeMethods.webui_memcpy(dest, src, count) |> ignore)

let isAppRunning () = withBootstrap (fun () -> NativeMethods.webui_interface_is_app_running())

let interfaceGetStringAt (w: uint) (eventNumber: uint) (index: uint) =
    withBootstrap (fun () ->
    let ptr = NativeMethods.webui_interface_get_string_at(w, eventNumber, index)
    if ptr <> IntPtr.Zero then Marshal.PtrToStringAnsi(ptr) else ""
    )
let interfaceGetIntAt (w: uint) (eventNumber: uint) (index: uint) = withBootstrap (fun () -> NativeMethods.webui_interface_get_int_at(w, eventNumber, index))
let interfaceGetFloatAt (w: uint) (eventNumber: uint) (index: uint) = withBootstrap (fun () -> NativeMethods.webui_interface_get_float_at(w, eventNumber, index))
let interfaceGetBoolAt (w: uint) (eventNumber: uint) (index: uint) = withBootstrap (fun () -> NativeMethods.webui_interface_get_bool_at(w, eventNumber, index))
let interfaceGetSizeAt (w: uint) (eventNumber: uint) (index: uint) = withBootstrap (fun () -> NativeMethods.webui_interface_get_size_at(w, eventNumber, index))

let interfaceSetResponse (w: uint) (eventNumber: uint) (response: string) = withBootstrap (fun () -> NativeMethods.webui_interface_set_response(w, eventNumber, response) |> ignore)
let interfaceSendRawClient (w: uint) (eventNumber: uint) (functionName: string) (data: byte[]) =
    withBootstrap (fun () ->
    let ptr = Marshal.AllocHGlobal(data.Length)
    try
        Marshal.Copy(data, 0, ptr, data.Length)
        NativeMethods.webui_interface_send_raw_client(w, eventNumber, functionName, ptr, uint data.Length)
    finally
        Marshal.FreeHGlobal(ptr)
    )
let interfaceRunClient (w: uint) (eventNumber: uint) (script: string) = withBootstrap (fun () -> NativeMethods.webui_interface_run_client(w, eventNumber, script) |> ignore)

let interfaceScriptClient (w: uint) (eventNumber: uint) (script: string) (timeout: uint) (bufferSize: uint) =
    withBootstrap (fun () ->
    if bufferSize = 0u then ""
    else
        let buffer = System.Text.StringBuilder(int bufferSize)
        let success = NativeMethods.webui_interface_script_client(w, eventNumber, script, timeout, buffer, bufferSize)
        if success then buffer.ToString() else ""

    )
let navigateClient (e: IntPtr) (url: string) = withBootstrap (fun () -> NativeMethods.webui_navigate_client(e, url) |> ignore)
let showClient (e: IntPtr) (content: string) = withBootstrap (fun () -> NativeMethods.webui_show_client(e, content) |> ignore)
let closeClient (e: IntPtr) = withBootstrap (fun () -> NativeMethods.webui_close_client(e) |> ignore)

let startServer (w: uint) (content: string) =
    withBootstrap (fun () ->
    let ptr = NativeMethods.webui_start_server(w, content)
    if ptr <> IntPtr.Zero then Marshal.PtrToStringAnsi(ptr) else ""

    )
let showWv (w: uint) (content: string) = withBootstrap (fun () -> NativeMethods.webui_show_wv(w, content))

let setBrowserFolder (path: string) = withBootstrap (fun () -> NativeMethods.webui_set_browser_folder(path) |> ignore)
let deleteAllProfiles () = withBootstrap (fun () -> NativeMethods.webui_delete_all_profiles() |> ignore)

type Config = ShowWaitConnection | UiEventBlocking | FolderMonitor | MultiClient | UseCookies | AsynchronousResponse

let setConfig (config: Config) (enabled: bool) =
    withBootstrap (fun () ->
    let configVal = match config with ShowWaitConnection -> 0u | UiEventBlocking -> 1u | FolderMonitor -> 2u | MultiClient -> 3u | UseCookies -> 4u | AsynchronousResponse -> 5u
    NativeMethods.webui_set_config(configVal, enabled) |> ignore

    )
type CloseHandler = delegate of (uint) -> bool

let setCloseHandlerWv (w: uint) (handler: CloseHandler) =
    withBootstrap (fun () ->
    let ptr = Marshal.GetFunctionPointerForDelegate(handler)
    NativeMethods.webui_set_close_handler_wv(w, ptr)

    )
type FileHandler = delegate of (string * IntPtr) -> IntPtr
type FileHandlerWindow = delegate of (uint * string * IntPtr) -> IntPtr

let setFileHandler (w: uint) (handler: FileHandler) =
    withBootstrap (fun () ->
    let ptr = Marshal.GetFunctionPointerForDelegate(handler)
    NativeMethods.webui_set_file_handler(w, ptr)

    )
let setFileHandlerWindow (w: uint) (handler: FileHandlerWindow) =
    withBootstrap (fun () ->
    let ptr = Marshal.GetFunctionPointerForDelegate(handler)
    NativeMethods.webui_set_file_handler_window(w, ptr)

    )
let interfaceSetResponseFileHandler (w: uint) (response: IntPtr) (length: int) = withBootstrap (fun () -> NativeMethods.webui_interface_set_response_file_handler(w, response, length) |> ignore)

let interfaceGetWindowId (w: uint) = withBootstrap (fun () -> NativeMethods.webui_interface_get_window_id(w))

type LogLevel = Debug | Info | Error

type Logger = delegate of (LogLevel * string * IntPtr) -> unit

let setLogger (logger: Logger) (userData: IntPtr) =
    withBootstrap (fun () ->
    let ptr = Marshal.GetFunctionPointerForDelegate(logger)
    NativeMethods.webui_set_logger(ptr, userData)

    )
let setTlsCertificate (certificatePem: string) (privateKeyPem: string) = withBootstrap (fun () -> NativeMethods.webui_set_tls_certificate(certificatePem, privateKeyPem))

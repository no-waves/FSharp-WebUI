#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

BOOTSTRAPPER_PROJ="tools/WebUI.Bootstrapper/WebUI.Bootstrapper.fsproj"
BOOTSTRAPPER_DLL="tools/WebUI.Bootstrapper/bin/Debug/net10.0/WebUI.Bootstrapper.dll"

log() {
  printf "[smoke] %s\n" "$*"
}

fail() {
  printf "[smoke][FAIL] %s\n" "$*" >&2
  exit 1
}

expect_success() {
  local description="$1"
  shift
  if "$@"; then
    log "PASS: ${description}"
  else
    fail "${description}"
  fi
}

expect_failure() {
  local description="$1"
  shift
  if "$@"; then
    fail "${description} (expected failure, got success)"
  else
    log "PASS: ${description}"
  fi
}

native_lib_name() {
  local os
  os="$(uname -s | tr '[:upper:]' '[:lower:]')"

  case "$os" in
    linux*)
      echo "libwebui-2.so"
      ;;
    darwin*)
      echo "libwebui-2.dylib"
      ;;
    msys*|mingw*|cygwin*)
      echo "webui-2.dll"
      ;;
    *)
      fail "Unsupported OS in smoke script: $(uname -s)"
      ;;
  esac
}

build_bootstrapper() {
  dotnet build "$BOOTSTRAPPER_PROJ" -c Debug >/dev/null
  [[ -f "$BOOTSTRAPPER_DLL" ]] || fail "Bootstrapper DLL not found at $BOOTSTRAPPER_DLL"
}

test_override_success() {
  local tmpdir libname
  tmpdir="$(mktemp -d)"
  libname="$(native_lib_name)"

  trap "rm -rf '$tmpdir'" RETURN

  touch "$tmpdir/$libname"

  WEBUI_NATIVE_PATH="$tmpdir" \
  WEBUI_BOOTSTRAP_REFRESH="false" \
  dotnet exec "$BOOTSTRAPPER_DLL" >/dev/null
}

test_override_failure() {
  local tmpdir
  tmpdir="$(mktemp -d)"
  trap "rm -rf '$tmpdir'" RETURN

  WEBUI_NATIVE_PATH="$tmpdir" \
  WEBUI_BOOTSTRAP_REFRESH="false" \
  dotnet exec "$BOOTSTRAPPER_DLL" >/dev/null 2>&1
}

test_network_refresh() {
  WEBUI_BOOTSTRAP_REFRESH="true" \
  WEBUI_RELEASE_TAG="nightly" \
  dotnet exec "$BOOTSTRAPPER_DLL" >/dev/null
}

main() {
  log "Building bootstrapper"
  build_bootstrapper

  expect_success "WEBUI_NATIVE_PATH override succeeds when native file exists" test_override_success
  expect_failure "WEBUI_NATIVE_PATH override fails when native file is missing" test_override_failure

  if [[ "${SKIP_NETWORK_TEST:-}" == "1" ]]; then
    log "SKIP: network refresh test (SKIP_NETWORK_TEST=1)"
  else
    expect_success "nightly refresh downloads/verifies native artifact" test_network_refresh
  fi

  log "All smoke tests passed"
}

main "$@"

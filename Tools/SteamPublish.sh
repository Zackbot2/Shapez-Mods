#!/usr/bin/env bash

# the verified flag is used to determine if the build and manifest have already been verified.
# this is typically used by the publishing workflow
VERIFIED=false

if [[ "$1" == "--verified" ]]; then
    VERIFIED=true
    shift
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

MOD_NAME="$1"
VERSION="$2"

if [[ "$VERIFIED" == true && -z "$VERSION" ]]; then
    echo "ERROR: Version must be supplied when using --verified."
    exit 1
fi

MOD_DIR="$REPO_DIR/$MOD_NAME"
MANIFEST="$MOD_DIR/manifest.json"

# Find the project file and steam folder directory
PROJECT=$(find "$MOD_DIR" -maxdepth 1 -name '*.csproj' -print -quit)
STEAM_DIR="$MOD_DIR/Steam"

# If there's no project found, log it and exit
if [[ -z "$PROJECT" ]]; then
    echo "ERROR: No .csproj found."
    exit 1
fi

echo "PROJECT: $PROJECT"

if [[ "$VERIFIED" == false ]]; then
    # Check for jq - this is needed to parse the manifest to get the version
    if ! command -v jq >/dev/null 2>&1; then
        echo "ERROR: jq is required but was not found."
        echo "Install jq and try again."
        exit 1
    fi

    # Ensure the manifest exists
    if [[ ! -f "$MANIFEST" ]]; then
        echo "ERROR: No manifest.json found."
        exit 1
    fi

    # Validate that the manifest actually has json data
    if ! jq empty "$MANIFEST" >/dev/null 2>&1; then
        echo "ERROR: manifest.json contains invalid JSON."
        exit 1
    fi

    # Use jq to get the version from the manifest
    VERSION=$(jq -r '.Version' "$MANIFEST")

    # Make sure the version was successfully read
    if [[ -z "$VERSION" || "$VERSION" == "null" || "$VERSION" == "CHANGE_ME" ]]; then
        echo "ERROR: Could not read Version from $MANIFEST"
        exit 1
    fi
fi

echo "VERSION: $VERSION"

CONTENT_PATH=$(dotnet msbuild "$PROJECT" -getProperty:OutputPath -nologo)

# if unverified, make sure the otuput manifest version matches the input manifest version
# if verified, then that means these checks have already been done either manually or automatically
if [[ "$VERIFIED" == false ]]; then
    CONTENT_MANIFEST="$CONTENT_PATH/manifest.json"
    # Use jq to get the version from the manifest
    CONTENT_VERSION=$(jq -r '.Version' "$CONTENT_MANIFEST")

    # The versions should match. If they don't, you forgot to build!
    if [[ "$CONTENT_VERSION" != "$VERSION" ]]; then
        echo "ERROR: Build output version ($CONTENT_VERSION) does not match project version ($VERSION)!"
        exit 1
    fi
fi

# Convert the content path to Windows format
CONTENT_PATH=$(cygpath -w "$CONTENT_PATH")

# Composes the location of the preview image
PREVIEW_IMG=$STEAM_DIR\\preview.png

# Convert the preview image path to Windows format
PREVIEW_IMG=$(cygpath -w "$PREVIEW_IMG")

# Adjust paths to use double backslashes
CONTENT_PATH="${CONTENT_PATH//\\/\\\\}"
PREVIEW_IMG="${PREVIEW_IMG//\\/\\\\}"

echo "CONTENT_PATH: $CONTENT_PATH"
echo "PREVIEW_IMG: $PREVIEW_IMG"

export CONTENT_PATH
export PREVIEW_IMG
export VERSION

# Adjust temporary .vdf with absolute paths for the content and the preview image
envsubst < "$STEAM_DIR/base.vdf" > "$STEAM_DIR/base.tmp.vdf"

# Log the final version
cat "$STEAM_DIR/base.tmp.vdf"

TMP_VDF=$STEAM_DIR/base.tmp.vdf

STEAM_USER="${STEAM_USER:?STEAM_USER is not set}"

# Execute
steamcmd +login "$STEAM_USER" +workshop_build_item "$TMP_VDF" +quit

# If SteamCMD failed to build, exit.
if [[ $? -ne 0 ]]; then
    echo "ERROR: SteamCMD failed to publish the workshop item."
    rm "$TMP_VDF"
    exit 1
fi

# Copy published file id back
cat "$TMP_VDF"

# Grab published file id 
FILE_ID=$(grep '"publishedfileid"' "$TMP_VDF" | sed 's/.*"publishedfileid"[ \t]*"\([0-9]\+\)".*/\1/')

# Update original file with new published file ID
echo "New published file ID: $FILE_ID"

# Clean temporary files
rm "$TMP_VDF"
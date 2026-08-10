#!/usr/bin/env bash

# Directory containing SteamPublish.sh
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

MOD_NAME="$1"
MOD_DIR="$REPO_DIR/$MOD_NAME"

# Find the .csproj file
PROJECT=$(find "$MOD_DIR" -maxdepth 1 -name '*.csproj' -print -quit)
STEAM_DIR="$MOD_DIR/Steam"

# If there's no project found, log it and exit
if [[ -z "$PROJECT" ]]; then
    echo "ERROR: No .csproj found."
    exit 1
fi

echo "PROJECT: $PROJECT"

CONTENT_PATH=$(dotnet msbuild "$PROJECT" -getProperty:OutputPath -nologo)

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

# Adjust temporary .vdf with absolute paths for the content and the preview image
envsubst < "$STEAM_DIR/base.vdf" > "$STEAM_DIR/base.tmp.vdf"

# Log the final version
cat "$STEAM_DIR/base.tmp.vdf"

TMP_VDF=$STEAM_DIR/base.tmp.vdf

STEAMCMD_PATH="${STEAMCMD_PATH:-/d/steamcmd/steamcmd.exe}"
STEAM_USER="${STEAM_USER:?STEAM_USER is not set}"

# Execute
"$STEAMCMD_PATH" +login "$STEAM_USER" +workshop_build_item "$TMP_VDF" +quit;

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
sed -i 's/\("publishedfileid"[ \t]*"\)[0-9]\+"/\1'"$FILE_ID"'"/' "$STEAM_DIR/base.vdf"
echo "Published file ID added to $STEAM_DIR\\base.vdf"

# Clean temporary files
rm "$TMP_VDF"
#!/usr/bin/env sh
set -eu

# Force Avalonia/SDL to prefer Wayland and avoid X11.
export XDG_SESSION_TYPE=wayland
export GDK_BACKEND=wayland
export QT_QPA_PLATFORM=wayland
export SDL_VIDEODRIVER=wayland

# Disable X11 fallback paths.
unset DISPLAY
unset WAYLAND_DISPLAY || true

# If your compositor sets WAYLAND_DISPLAY, keep it. Otherwise you must run under a compositor (e.g., weston).
# Example: WAYLAND_DISPLAY=wayland-0

DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
exec "$DIR/BACApp.UI" "$@"
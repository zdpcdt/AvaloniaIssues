# FullScreen from Maximized shrinks an X11 CSD window

Minimal repro for: on X11, entering `WindowState.FullScreen` while the window is
`Maximized` leaves the window at its pre-maximize size instead of covering the
monitor. With drawn decorations the title bar is also hidden (fullscreen style),
so the app looks like a shrunken fullscreen window.

No third-party dependencies — plain Avalonia 12.1.6, two window-level settings
and one property assignment.

## Environment

- Avalonia 12.1.6 (also present on current `master`)
- Linux, GNOME on Wayland (window runs under XWayland), Mutter
- Screen 1920×1080

## Run

```bash
dotnet run -- --auto
```

The `--auto` flag drives the whole sequence after startup and prints the live
state to the console. Without it, click **Reproduce: Maximize → FullScreen**
in the window.

Sequence performed:

```csharp
WindowState = WindowState.Normal;
Width = 1440; Height = 733;
WindowState = WindowState.Maximized;
WindowState = WindowState.FullScreen;   // <-- what the fullscreen caption button does
```

### Expected

`WindowState=FullScreen  ClientSize=1920x1080`

### Actual

```
[repro] WindowState=FullScreen  ClientSize=1440x733  Width=1440 Height=733
```

The window keeps the `_NET_WM_STATE_FULLSCREEN` flag but sits at the restore
size (1440×733). Exiting fullscreen then also leaves a wrong floating size,
because the WM's saved rect was clobbered.

## Root cause

`X11Window.WindowState` (`src/Avalonia.X11/X11Window.cs`) transitions
Maximized → FullScreen with **two** client messages, in this order:

1. `_NET_WM_STATE_ADD _NET_WM_STATE_FULLSCREEN` (maximize atoms still present)
2. `_NET_WM_STATE_REMOVE _NET_WM_STATE_MAXIMIZED_VERT|HORZ`

`xprop -spy _NET_WM_STATE` while running `--auto`:

```
_NET_WM_STATE(ATOM) = _NET_WM_STATE_FOCUSED
_NET_WM_STATE(ATOM) = _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT, _NET_WM_STATE_FOCUSED
_NET_WM_STATE(ATOM) = _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT, _NET_WM_STATE_FULLSCREEN, _NET_WM_STATE_FOCUSED
_NET_WM_STATE(ATOM) = _NET_WM_STATE_FULLSCREEN, _NET_WM_STATE_FOCUSED          <-- window back at 1440x733
```

When Mutter processes message 2 it un-maximizes the (now fullscreen) window and
restores its saved pre-maximize rectangle, keeping the fullscreen flag. The
remove-maximize step was added in 3491315091 (`[X11] Fixed reverting back from
fullscreen`, 2020) which fixed the reverse transition.

Not WM-decorated-window related, and not the forced-CSD path either — see below.

## What it takes to reproduce

Only the *extended* CSD mode (window extends into decorations and manages
margins itself) is affected:

| Configuration | Result of Maximized → FullScreen |
|---|---|
| `ForceDrawnDecorations=true` + window `ExtendClientAreaToDecorationsHint=True` (this demo) | shrinks to 1440×733 |
| `ForceDrawnDecorations=true` only (forced mode, insets managed by Avalonia) | works, 1920×1080 |
| `ForceDrawnDecorations=false` (Mutter server-side decorations) | works, 1920×1080 |

So the minimal window setup is:

```xml
<Window ...
        ExtendClientAreaToDecorationsHint="True">
```

plus

```csharp
.With(new X11PlatformOptions { ForceDrawnDecorations = true })
```

(`EnableDrawnDecorations = true` instead of `ForceDrawnDecorations` should hit
the same path; it was not tested separately.)

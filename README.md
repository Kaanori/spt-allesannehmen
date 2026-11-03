# AllesAnnehmen (accept all) — SPT 4.1.6

Collects **all attachments from all traders at once** from the messenger
window and puts them **directly into the stash**. No hand-over window, no
dragging with the mouse, no clicking through trader after trader.

## How this mod was made (AI disclosure)

This mod was **programmed by Claude**, an AI assistant made by Anthropic.
**Kaan was the tester**: Kaan has no programming background, played with the mod
on SPT 4.1.6, reported what went wrong, and Claude fixed it. No human programmer
has reviewed the code. It is open source so anyone can check it — if you find a
bug, please open an [issue](https://github.com/Kaanori/spt-allesannehmen/issues).

## Requirements

- SPT 4.1.6 (EFT 0.16.9). BepInEx ships with SPT.

## Installation

1. Download `AllesAnnehmen-1.2.0.zip` from the [Releases](https://github.com/Kaanori/spt-allesannehmen/releases) page.
2. Extract it **into your SPT game folder** (the folder that contains `EscapeFromTarkov.exe`).
   The ZIP already has the right layout; the mod ends up in `BepInEx/plugins/`.
   On Windows, make sure "Extract All" does not add an extra subfolder.
3. Restart the game.

## Uninstall

Delete the mod's files from `BepInEx/plugins/` (settings: `BepInEx/config/` for client mods) and restart the game.

## What the mod does

1. For every conversation it asks the server for the attachments
   (the same call the game uses for the paperclip button).
2. Profile bonuses (money, experience, trader standing) are redeemed.
3. Every item is moved into the stash one at a time, using the same game
   code as the "Receive all" button in the hand-over window, but with
   `forcePutInStash: true` so that nothing ends up in pockets or backpack.
4. At the end the green "unread" counters are cleared.

If the stash fills up, the mod stops cleanly and reports how much it
managed. The rest stays in the mail.

## Usage — three ways

| Way | When |
|---|---|
| **Paperclip button** next to a trader (tooltip "Alles Annehmen") | Default. It now clears **all** traders instead of just one. |
| **"ALLE ANNEHMEN" button** (accept all) at the top of the messenger window | Added by the mod. |
| **F9** | While the messenger window is open. Always available, even if the button is not in place. |

Individual messages are still accepted the usual way, with the receive
button on the message itself. The mod does not change that.

## Settings (F12 in game, "Alles Annehmen")

- **Anhang-Knopf umbiegen** (redirect the attachment button) (on) — off:
  the paperclip button behaves like in the unmodified game again.
- **Knopf anzeigen** (show button) (on) / **Knopfbeschriftung** (button label)
- **Tastenkürzel** (shortcut) (F9)
- **Profilboni mit einlösen** (also redeem profile bonuses) (on)
- **Alles als gelesen markieren** (mark everything as read) (on)

## If something goes wrong

Log: `BepInEx/LogOutput.log`, lines with `Alles Annehmen`.
The mod also logs **where** it placed the button
("Knopf gesetzt an: …", i.e. "button placed at: …"). If it sits in an ugly
spot, that line helps with adjusting it.

## Why there is new mail right after collecting

If `TaskAutomation` is running: the 148 items land in the stash, which
suddenly satisfies "Hand over X" conditions. TaskAutomation completes the
tasks and sends new reward mail. This is not a bug, just a chain reaction.
Simply press again until nothing more arrives.

## Changes 1.1.0

- The button was invisible: the layout group in the header had squashed it
  to 0 pixels in height. It now gets a fixed height and re-measures itself
  after two frames. If it is still flat, it places itself by hand in the
  right spot.
- The green counters and the paperclip counters stayed in place: the server
  call alone does not change the numbers on the conversations. They are now
  reset and redrawn directly, as is the total at the bottom right of the
  menu bar.
- The pop-up message is now called "Alles Annehmen: …", stays longer and
  has an envelope icon.

## Changes 1.2.0 — messages are now really marked as read

The game function `AddDialogueToReadQueue` only accepts a conversation if
it has unread messages *and* new attachments **at the same time**. A
conversation like Mechanic (green 1, no paperclip) therefore failed
completely — it was never reported as read.

Now:

- All conversations go straight into the queue, bypassing that check, and
  are sent to the server via `client/mail/dialog/read`. The server clears
  both the unread and the attachment flag, so it stays gone after a restart.
- In addition the counters in the display are set to 0 immediately — on
  every conversation row and in the total at the bottom right.
- Even when there are no attachments at all, messages are now marked as
  read ("Keine Anhänge da, 8 Gespräche als gelesen markiert." — "No
  attachments there, 8 conversations marked as read.").

## Building from source

Needs the .NET 10 SDK and an SPT 4.1.6 installation to build against.

```bash
./build.sh             # Linux: build only
./build.sh --install   # build and copy into the game folder
```
```powershell
.\build.ps1            # Windows
.\build.ps1 -Install
```

The game folder is set in `Directory.Build.props` (Windows default `C:/SPT`)
or via the environment variable `SPT_DIR`.

## Support

Bugs and questions: [GitHub issues](https://github.com/Kaanori/spt-allesannehmen/issues).

## License

MIT — see [LICENSE](LICENSE).

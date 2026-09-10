# Changelog

All notable changes to Forgetti are recorded here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and versions follow
[Semantic Versioning](https://semver.org/).

## [1.0.0] - 2026-09-09

The first release. Forgetti is the open-source successor to the
[Recent File Reseterer for Revit](https://parallaxteam.onfastspring.com/recent-file-reseterer-for-revit)
from Parallax Team, which cleared one Revit version at a time and remains available.

### Added

- Lists every Revit version with a settings file under `%AppData%\Autodesk\Revit`, newest
  first, with a count of the recent files each holds. Any number can be forgotten in one go.
- A copy of each `Revit.ini` is saved beside the original before it changes, and **Undo** puts
  every one of them back.
- Notices when Revit is open and says so, because Revit writes the list back when it exits.
- Paper and ink themes, following [Interlude](https://github.com/johnpierson/Interlude)'s
  neubrutalist look.
- Development switches: `--dark`, `--revit-running`, `--sample`, `--screenshot <path>`.

### Changed, relative to the original

- Every Revit version on the machine is listed and can be cleared together.
- `Revit.ini` keeps its original encoding (UTF-16 LE with a byte-order mark) and line endings.
- Only the `File1=`, `File2=`, ... entries are removed. The section header and every other
  section are left exactly as they were.

[1.0.0]: https://github.com/johnpierson/Forgetti/releases/tag/v1.0.0

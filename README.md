# Duplication Detector
A command-line tool for detecting and managing duplicate files between folders.

## Features

- **Detects duplicate files** between a source folder and one or more target folders.
- **Supports recursive search** in both source and target folders.
- **Interactive duplicate management**: Move found duplicates to a folder of your choice.

## Usage

```sh
DuplicationDetector [-tr] [-sr] <source_folder> <target_folder1> [target_folder2] ...
```

### Options

| Option | Description                                |
|--------|--------------------------------------------|
| `-tr`  | Recursively check target folders           |
| `-sr`  | Recursively check the source folder        |

### Arguments

- `<source_folder>`: The folder to compare files against.
- `<target_folder1>` `[target_folder2] ...`: One or more folders to check for duplicates.

## How It Works

1. **Scan Files**: The tool scans all files in the specified target folder(s) and compares them with files in the source folder.
2. **Detect Duplicates**: If any files in the target folder(s) are found to be duplicates of files in the source folder, they are reported.
3. **Manage Duplicates**: When duplicates are detected, you'll be prompted to specify a folder. All duplicate files from the target folder(s) will be moved to this folder.

## Example

```sh
DuplicationDetector -sr -tr /path/to/source /path/to/target1 /path/to/target2
```

- This command will recursively check both the source and target folders for duplicate files.
- If duplicates are found, you will be asked where to move them.

## Notes

- Duplicates are detected based on file similarity (e.g., filename minus the extension name).
- The tool **does not delete files automatically**; it only moves duplicates upon your confirmation.

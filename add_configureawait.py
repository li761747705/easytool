#!/usr/bin/env python3
"""
Add .ConfigureAwait(false) to all await statements in library code.
Excludes test files and handles edge cases properly.
"""

import os
import sys
from pathlib import Path

# Directories to process
LIBRARIES = [
    "EasyTool.Core",
    "EasyTool.AI",
    "EasyTool.Web",
    "EasyTool.System",
    "EasyTool.Media",
    "EasyTool.NPOI",
    "EasyTool.Image",
    "EasyTool.EmitMapper"
]

def should_process_file(filepath):
    """Check if file should be processed."""
    path_str = str(filepath)

    # Skip obj and bin directories
    if '/obj/' in path_str or '/bin/' in path_str:
        return False

    # Check if file is in one of the library directories
    for lib in LIBRARIES:
        if lib in path_str:
            return True

    return False

def process_await_line(line):
    """
    Process a line to add ConfigureAwait(false) to await statements.
    Returns (new_line, number_of_changes)
    """
    # Skip lines with await using
    if 'await using' in line:
        return line, 0

    # Skip lines that already have ConfigureAwait
    if 'ConfigureAwait' in line:
        return line, 0

    # Skip lines without await
    if 'await ' not in line:
        return line, 0

    new_line = line
    changes = 0
    pos = 0

    while True:
        # Find next 'await '
        idx = new_line.find('await ', pos)
        if idx == -1:
            break

        # Check word boundary
        if idx > 0 and new_line[idx - 1].isalnum():
            pos = idx + 6
            continue

        # Find the end of the await expression
        start = idx + 6  # Skip "await "
        paren_count = 0
        bracket_count = 0
        end = -1

        for i in range(start, len(new_line)):
            c = new_line[i]

            if c == '(':
                paren_count += 1
            elif c == ')':
                if paren_count == 0:
                    end = i
                    break
                paren_count -= 1
                if paren_count == 0:
                    end = i + 1
                    break
            elif c == '[':
                bracket_count += 1
            elif c == ']':
                if bracket_count == 0 and paren_count == 0:
                    end = i + 1
                    break
                bracket_count -= 1
                if bracket_count == 0 and paren_count == 0:
                    end = i + 1
                    break
            elif c in (';', ',', '\r', '\n'):
                if paren_count == 0 and bracket_count == 0:
                    end = i
                    break

        if end > start:
            # Extract the expression
            expr = new_line[start:end].strip()

            # Build new line with ConfigureAwait
            before = new_line[:start]
            after = new_line[end:]
            new_line = before + expr + '.ConfigureAwait(false)' + after
            changes += 1

            # Move past this await
            pos = start + len(expr) + len('.ConfigureAwait(false)')
        else:
            break

    return new_line, changes

def process_file(filepath):
    """Process a single file."""
    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            lines = f.readlines()

        new_lines = []
        total_changes = 0

        for line in lines:
            new_line, changes = process_await_line(line)
            new_lines.append(new_line)
            total_changes += changes

        if total_changes > 0:
            with open(filepath, 'w', encoding='utf-8') as f:
                f.writelines(new_lines)
            return total_changes

        return 0

    except Exception as e:
        print(f"Error processing {filepath}: {e}", file=sys.stderr)
        return 0

def main():
    """Main function."""
    total_files = 0
    modified_files = 0
    total_changes = 0

    # Walk through all files
    for root, dirs, files in os.walk('.'):
        # Remove obj and bin from dirs
        dirs[:] = [d for d in dirs if d not in ('obj', 'bin')]

        for filename in files:
            if not filename.endswith('.cs'):
                continue

            filepath = Path(root) / filename

            if should_process_file(filepath):
                total_files += 1
                changes = process_file(filepath)

                if changes > 0:
                    modified_files += 1
                    total_changes += changes
                    rel_path = os.path.relpath(filepath, '.')
                    print(f"Modified: {rel_path} ({changes} changes)")

    print(f"\nSummary:")
    print(f"- Files scanned: {total_files}")
    print(f"- Files modified: {modified_files}")
    print(f"- Total changes: {total_changes}")

if __name__ == '__main__':
    main()

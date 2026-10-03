#!/usr/bin/env bash
# Run Unity batchmode -executeMethod calls against game/ on Linux, one
# Editor launch per method, and report pass/fail from the log rather than
# the exit code (see AGENT_HANDBOOK.md -- check a real artifact or the log).
#
# Usage:
#   tools/unity.sh <Class.Method> [<Class.Method> ...]
#   tools/unity.sh verify        # every *Verification.RunAll + BootstrapSmoke
#   tools/unity.sh webgl         # BuildScript.BuildWebGL
#
# The Editor is found at $UNITY_EDITOR, else at the Hub's default install
# path for the version pinned in game/ProjectSettings/ProjectVersion.txt.
# Logs land in game/Logs/<Method>.log (gitignored).

set -uo pipefail

repo="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
project="$repo/game"
version="$(sed -n 's/^m_EditorVersion: //p' "$project/ProjectSettings/ProjectVersion.txt")"
editor="${UNITY_EDITOR:-$HOME/Unity/Hub/Editor/$version/Editor/Unity}"

if [[ ! -x "$editor" ]]; then
    echo "Unity $version not found at $editor"
    echo "Install it via Unity Hub (with Web Build Support), or set UNITY_EDITOR."
    exit 1
fi
if [[ $# -eq 0 ]]; then
    sed -n '6,9p' "$0" | sed 's/^# \?//'
    exit 1
fi

methods=()
for arg in "$@"; do
    case "$arg" in
        verify)
            methods+=(BootstrapSmoke.RunAll)
            for f in "$project"/Assets/Editor/*Verification.cs; do
                cls="$(basename "$f" .cs)"
                grep -q 'public static void RunAll' "$f" && methods+=("$cls.RunAll")
            done ;;
        webgl) methods+=(BuildScript.BuildWebGL) ;;
        *) methods+=("$arg") ;;
    esac
done

mkdir -p "$project/Logs"
failed=0
for m in "${methods[@]}"; do
    log="$project/Logs/$m.log"
    printf '%-40s ' "$m"
    "$editor" -batchmode -quit -projectPath "$project" -executeMethod "$m" -logFile "$log" >/dev/null 2>&1
    if ! grep -q 'Exiting batchmode successfully' "$log"; then
        echo "CRASH  (see $log)"; failed=1
    elif grep -q '\] FAIL' "$log" || grep -q 'error CS[0-9]' "$log"; then
        echo "FAIL   (see $log)"; failed=1
        grep -E '\] FAIL|error CS[0-9]' "$log" | sed 's/^/    /' | head -20
    else
        echo "ok     ($(grep -c '\] PASS' "$log") checks passed)"
    fi
done
exit $failed

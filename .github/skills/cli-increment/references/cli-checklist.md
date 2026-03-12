# CLI Checklist

## Command Surface
- Prefer explicit commands such as `convert` and `doctor`.
- Keep the first implementation small.
- Avoid hidden coupling between unrelated options.

## Output Behavior
- stdout for conversion output
- stderr for diagnostics and failures
- Stable documented exit codes

## Option Design
- Define whether the option applies to files, directories, or both.
- Define defaults explicitly.
- Define conflict rules between options.

## Test Coverage
- Parse success cases
- Parse failure cases
- Output path behavior
- Overwrite handling
- Exit code behavior

## Documentation
- Reflect new commands and options in README.
- Keep README, planning, and instructions aligned.
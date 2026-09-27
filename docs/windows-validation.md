# Windows validation before release

The interface refresh was developed on Linux. The following runtime checks remain to be performed on a disposable Windows VM with a licensed test installation. Do not treat portable configuration checks as evidence of successful Office deployment.

## Build and presentation

- Build the nested project in Debug and Release with the .NET Framework 4.7.2 targeting pack.
- Check the layout at 100%, 125%, 150% and 200% scaling, including moving between monitors. Resize down to the minimum window size; all choices, status text and actions must remain reachable.
- Navigate with Tab and Shift+Tab. Toggle choices with Space and use the action mnemonics. Verify that selection and focus remain visible with Windows high contrast enabled.
- Check that long installation details, existing exclusions and error messages wrap or scroll without hiding the action row. Check accessible names with Narrator.

## Detection and selection

- With no Click-to-Run installation, Apply stays disabled and no setup process starts.
- Unknown product, ambiguous suites, unknown channel/architecture and unsupported existing exclusion IDs block Apply with a useful message.
- Exercise both 32-bit and 64-bit Office detection on 64-bit Windows.
- With ODT missing, verify automatic download and extraction, a responsive window and disabled Apply during preparation. On failure, verify Refresh detection retries. With ODT present, verify it is reused without a download.
- Confirm the displayed installation matches registry values. Verify existing `<ProductID>.ExcludedApps` values appear in the summary and generated configuration.
- Confirm nothing is checked initially. Change and clear selections, including all components; count and summary must agree.

## Applying changes

- Cancel the review dialog: no XML is generated and no ODT process starts.
- Decline UAC: show cancellation, retain the selection, and allow retry.
- During execution, selection, refresh and Apply are disabled. Closing the window must show a waiting message and leave the operation active.
- Confirm Office is not forcibly closed and no automatic Windows restart occurs.
- On setup success, show completion, clear the new selection and retain the effective exclusions in the summary. Verify actual Office behavior separately.
- On a nonzero exit code, show that code, retain selection and allow retry; do not display success or a restart action.
- Leave setup running for more than 20 minutes: the application must continue waiting rather than report a timeout as completion.
- Verify temporary XML is removed after setup exits, including nonzero results, and repeat the operation with another selection.

## Release packaging

- Use `M365Debloater.exe` from the new Release build.
- Test the helper with its default path, a custom executable path and a path containing spaces.
- Test helper failures: missing local executable, network failure, changed download page, invalid installer signature and extraction failure. Its console must remain visible and errors must prevent launch.
- Capture a real Windows screenshot and replace the labeled design preview only after the layout has been verified.

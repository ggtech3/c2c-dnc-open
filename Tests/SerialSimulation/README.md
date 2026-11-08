# Serial service simulation

Run from the repository root with the .NET 10 SDK:

~~~powershell
pwsh -File .\Tests\SerialSimulation\Run.ps1
~~~

The runner regenerates test copies from current SerialDncService.cs, DncTextProcessor.cs and CncProfile.cs, records hashes in ignored source-manifest.json and output in ignored results.txt. It prepends exactly one using alias and LF to SerialDncService.cs so the service uses FakeSerialPort. The remainder of the decoded source is checked for exact equality; the other two files are byte-identical copies. No production source or algorithms are changed, and no real port is opened.

The 21 scenarios cover repeated byte-exact sends, all 18 profile settings, percent framing and chunk boundaries, normalization/filtering, idle completion, cancellation, open/read/write failures, partial transfers, drain timeout, disposal, error counting and no automatic retry. The repeat case performs 20 independent sends. The actual ten-second drain timeout loop is exercised.

## Preserved baseline behavior, not safety guarantees

- Incomplete percent-framed receive data can complete on idle fallback without a closing percent marker.
- Each repeated SendAsync request transmits; service-level duplicate-request prevention is absent.
- Empty input sends a newline; fully filtered comment-only input rejects.
- A pre-cancelled send opens the fake endpoint before cancellation, then closes without writing.
- Non-ASCII characters become ASCII question marks.
- Failed transfers may be partial and are never automatically retried.

## Limits

Injected write timeouts validate propagation and the configured timeout, not real driver timing. Flow control and parity replacement are configuration assertions only. These checks do not validate electrical characteristics, baud timing, hardware handshakes, USB driver behavior, physical buffer loss, real-port arbitration, CNC compatibility, packaged UI or registration removal. Test those separately within an explicitly authorized plan. Never interpret a passing simulation as machine acceptance.

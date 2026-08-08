# Bug Reports

## B1 — High-priority tickets are buried at the bottom of the queue (20 points)

**Reported by:** Support team lead
**Severity:** High — urgent-looking work gets picked up, important work does not.

**Steps to reproduce**

1. `GET /api/tickets?page=1&pageSize=50`
2. Read the `reference` and `priority` of each row in order.

**Expected:** most severe first — the `Urgent` ticket, then `High`, then the
`Normal` ones, then `Low`. Within one priority, the oldest ticket first, because
it has been waiting longest.

**Actual:** the `Urgent` ticket is first, which is why this took a while to
notice. After that the order falls apart: the `Normal` tickets come next, then
`Low`, and the `High` ticket — `TKT-0002` — is **last in the queue**, below
everything. The `Normal` tickets are also in the wrong order among themselves:
newest first, so the ticket that has been waiting longest is the last one an agent
sees.

**Notes from the reporter:** "It is not random — it is the same wrong order every
time, and it is the same on every page. Sorting looks like it is working right up
until you notice where High ended up. Nobody has touched this query in months; it
has probably always been wrong and we only just got a High ticket that sat long
enough for someone to complain."

**Tests:** `dotnet test --filter Category=B1`

# User Stories

## S1 — Assign a ticket to an agent (30 points)

**As a** support lead
**I want** to assign an open ticket to a named agent
**So that** work is distributed deliberately instead of by whoever notices first.

There is no way to assign a ticket today. Add one.

**Acceptance criteria**

- `POST /api/tickets/{id}/assign` takes an agent id in the request body.
- On success the ticket becomes `Assigned`, records the agent, and the response is
  the ticket as `GET /api/tickets/{id}` would return it.
- An unknown ticket is a `404`. An unknown agent is also a `404`.
- Assigning to an **inactive** agent is a `409` — they have left the team.
- Assigning a ticket that is already `Resolved` or `Closed` is a `409`.
- Reassigning an already-assigned open ticket to a different agent is allowed.
- A request with no agent id is a `400`, rejected before anything is read from the
  database.
- The agent's open workload count reflects the new assignment immediately.

**Out of scope:** notifying the agent, assignment history, round-robin or
capacity-based auto-assignment.

**Tests:** `dotnet test --filter Category=S1`

---

## S2 — Ticket comment thread (30 points)

**As an** agent
**I want** to read and add comments on a ticket, with internal notes kept separate
**So that** the conversation with the requester and the notes between agents live
in one place without ever mixing.

The `Comment` table and its internal flag already exist and carry seeded data.
No endpoint reads or writes them.

**Acceptance criteria**

- `GET /api/tickets/{id}/comments` returns the thread, oldest first.
- **Internal comments are excluded by default.** This is the part that matters:
  the default response is what a requester is allowed to see.
- `GET /api/tickets/{id}/comments?includeInternal=true` returns everything.
- `POST /api/tickets/{id}/comments` adds a comment. The request carries an author
  name, a body, and whether the comment is internal. It responds `201`.
- Reading or posting against an unknown ticket is a `404`.
- An empty body is a `400`.
- Each returned comment exposes its id, ticket id, author name, body, internal
  flag, and creation time in UTC.

**Out of scope:** editing or deleting comments, attachments, mentions,
authentication or any notion of who the caller is.

**Tests:** `dotnet test --filter Category=S2`

// LeaveRequestStatus is a C# enum (Pending = 0, Approved = 1, Rejected = 2) and the API
// serializes it as that number, not as text - keep this in sync with
// server/Models/LeaveRequestStatus.cs if the enum there ever changes.
export const LEAVE_REQUEST_STATUS = {
  PENDING: 0,
  APPROVED: 1,
  REJECTED: 2,
}

const STATUS_LABELS = {
  [LEAVE_REQUEST_STATUS.PENDING]: 'Väntande',
  [LEAVE_REQUEST_STATUS.APPROVED]: 'Godkänd',
  [LEAVE_REQUEST_STATUS.REJECTED]: 'Avslagen',
}

export function leaveRequestStatusLabel(status) {
  return STATUS_LABELS[status] ?? 'Okänd'
}

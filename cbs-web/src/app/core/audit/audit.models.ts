export interface AuditLogEntry {
  id: string;
  actorUserId: string;
  actionName: string;
  entityType: string | null;
  entityId: string | null;
  occurredAtUtc: string;
}

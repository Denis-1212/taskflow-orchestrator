export interface AuditLog {
    id: string
    userId: string
    userEmail: string
    action: string
    entityType: string
    entityId: string
    oldValue: string | null
    newValue: string | null
    ipAddress: string
    userAgent: string
    timestamp: string
}

export interface AuditFilters {
    userId?: string
    action?: string
    entityType?: string
    entityId?: string
    from?: string
    to?: string
    page?: number
    pageSize?: number
}

export type AuditResponse = AuditLog[]
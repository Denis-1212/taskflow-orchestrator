import { apiClient } from '@/lib/api/client'
import { AuditLog, AuditFilters, AuditResponse } from '../types/audit.types'

export const auditService = {
    async getAuditLogs(filters: AuditFilters): Promise<AuditResponse> {
        const response = await apiClient.get<AuditLog[]>('/audit/api/audit', {
            params: filters
        })
        return response.data
    },

    async cleanup(retentionDays: number = 90): Promise<void> {
        await apiClient.post('/audit/api/audit/cleanup', null, {
            params: { retentionDays }
        })
    },
}
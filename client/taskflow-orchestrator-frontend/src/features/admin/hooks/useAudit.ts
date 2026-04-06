import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { auditService } from '../services/auditService'
import { AuditFilters } from '../types/audit.types'

export const auditKeys = {
    all: ['audit'] as const,
    list: (filters: AuditFilters) => [...auditKeys.all, 'list', filters] as const,
}

export function useAuditLogs(filters: AuditFilters) {
    return useQuery({
        queryKey: auditKeys.list(filters),
        queryFn: () => auditService.getAuditLogs(filters),
        enabled: true,
        staleTime: 1000 * 60, // 1 минута
    })
}

export function useCleanupAudit() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: (retentionDays: number = 90) => auditService.cleanup(retentionDays),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: auditKeys.all })
            toast.success('Audit logs cleaned up successfully')
        },
        onError: (error: any) => {
            toast.error(error.response?.data?.message || 'Failed to cleanup audit logs')
        },
    })
}
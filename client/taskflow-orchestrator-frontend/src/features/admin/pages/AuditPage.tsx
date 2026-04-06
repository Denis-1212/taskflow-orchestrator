import { useState } from 'react'
import { RefreshCw, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { useAuditLogs, useCleanupAudit } from '../hooks/useAudit'
import { AuditFilters } from '../components/AuditFilters'
import { AuditFilters as AuditFiltersType } from '../types/audit.types'
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from '@/components/ui/alert-dialog'

export default function AuditPage() {
    const [filters, setFilters] = useState<AuditFiltersType>({})
    const [cleanupDialogOpen, setCleanupDialogOpen] = useState(false)

    const { data: logs, isLoading, error, refetch } = useAuditLogs(filters)
    const cleanup = useCleanupAudit()

    const handleFilterChange = (newFilters: AuditFiltersType) => {
        setFilters(newFilters)
    }

    const handleCleanup = async () => {
        await cleanup.mutateAsync(90)
        setCleanupDialogOpen(false)
        refetch()
    }

    if (error) {
        return (
            <div className="space-y-6">
                <div className="flex justify-between items-center">
                    <h1 className="text-3xl font-bold">Audit Logs</h1>
                    <Button variant="outline" onClick={() => refetch()}>
                        <RefreshCw className="h-4 w-4 mr-2" />
                        Retry
                    </Button>
                </div>
                <Card className="p-12 text-center">
                    <p className="text-destructive">Failed to load audit logs</p>
                </Card>
            </div>
        )
    }

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <h1 className="text-3xl font-bold">Audit Logs</h1>
                <div className="flex gap-2">
                    <Button variant="outline" onClick={() => refetch()}>
                        <RefreshCw className="h-4 w-4 mr-2" />
                        Refresh
                    </Button>
                    <Button variant="destructive" onClick={() => setCleanupDialogOpen(true)}>
                        <Trash2 className="h-4 w-4 mr-2" />
                        Cleanup (90 days)
                    </Button>
                </div>
            </div>

            <AuditFilters filters={filters} onFilterChange={handleFilterChange} />

            {isLoading ? (
                <div className="space-y-3">
                    {[1, 2, 3, 4, 5].map((i) => (
                        <Card key={i} className="p-4 h-20 animate-pulse bg-muted" />
                    ))}
                </div>
            ) : !logs || logs.length === 0 ? (
                <Card className="p-12 text-center">
                    <p className="text-muted-foreground">No audit logs found</p>
                </Card>
            ) : (
                <div className="space-y-3">
                    {logs.map((log) => (
                        <Card key={log.id} className="p-4">
                            <div className="flex justify-between items-start">
                                <div className="space-y-1 flex-1">
                                    <div className="flex items-center gap-3 flex-wrap">
                                        <span className="font-medium">{log.userEmail || log.userId.slice(0, 8)}</span>
                                        <span className="text-xs px-2 py-0.5 rounded bg-primary/10 text-primary">
                                            {log.action}
                                        </span>
                                        <span className="text-xs text-muted-foreground">
                                            {log.entityType}
                                        </span>
                                    </div>
                                    <div className="text-sm">
                                        <span className="text-muted-foreground">Entity ID:</span>{' '}
                                        <span className="font-mono text-xs">{log.entityId}</span>
                                    </div>
                                    {log.newValue && (
                                        <div className="text-xs text-muted-foreground break-all">
                                            <span className="font-medium">Changes:</span>{' '}
                                            {log.newValue.length > 150 ? `${log.newValue.substring(0, 150)}...` : log.newValue}
                                        </div>
                                    )}
                                </div>
                                <div className="text-right text-xs text-muted-foreground ml-4">
                                    <div>{new Date(log.timestamp).toLocaleString()}</div>
                                    <div className="mt-1">{log.ipAddress}</div>
                                </div>
                            </div>
                        </Card>
                    ))}
                </div>
            )}

            <AlertDialog open={cleanupDialogOpen} onOpenChange={setCleanupDialogOpen}>
                <AlertDialogContent>
                    <AlertDialogHeader>
                        <AlertDialogTitle>Cleanup Audit Logs?</AlertDialogTitle>
                        <AlertDialogDescription>
                            This will permanently delete all audit logs older than 90 days.
                            This action cannot be undone.
                        </AlertDialogDescription>
                    </AlertDialogHeader>
                    <AlertDialogFooter>
                        <AlertDialogCancel>Cancel</AlertDialogCancel>
                        <AlertDialogAction onClick={handleCleanup} className="bg-destructive text-destructive-foreground">
                            Cleanup
                        </AlertDialogAction>
                    </AlertDialogFooter>
                </AlertDialogContent>
            </AlertDialog>
        </div>
    )
}
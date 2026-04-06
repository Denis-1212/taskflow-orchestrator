import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card } from '@/components/ui/card'
import { Search, X } from 'lucide-react'
import { AuditFilters as AuditFiltersType } from '../types/audit.types'

interface AuditFiltersProps {
    filters: AuditFiltersType
    onFilterChange: (filters: AuditFiltersType) => void
}

export function AuditFilters({ filters, onFilterChange }: AuditFiltersProps) {
    const [localFilters, setLocalFilters] = useState<AuditFiltersType>(filters)

    const handleChange = (key: keyof AuditFiltersType, value: string | number | undefined) => {
        const newFilters = { ...localFilters, [key]: value || undefined }
        setLocalFilters(newFilters)
    }

    const handleApply = () => {
        onFilterChange(localFilters)
    }

    const handleReset = () => {
        const emptyFilters: AuditFiltersType = {}
        setLocalFilters(emptyFilters)
        onFilterChange(emptyFilters)
    }

    const hasFilters = Object.values(localFilters).some(v => v)

    return (
        <Card className="p-4 space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-sm font-medium">Filters</h3>
                {hasFilters && (
                    <Button variant="ghost" size="sm" onClick={handleReset} className="h-8 px-2">
                        <X className="h-4 w-4 mr-1" />
                        Clear all
                    </Button>
                )}
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                <div>
                    <Label className="text-xs">User ID</Label>
                    <Input
                        placeholder="User ID"
                        value={localFilters.userId || ''}
                        onChange={(e) => handleChange('userId', e.target.value)}
                    />
                </div>

                <div>
                    <Label className="text-xs">Action</Label>
                    <Input
                        placeholder="CREATE, UPDATE, DELETE, LOGIN..."
                        value={localFilters.action || ''}
                        onChange={(e) => handleChange('action', e.target.value)}
                    />
                </div>

                <div>
                    <Label className="text-xs">Entity Type</Label>
                    <Input
                        placeholder="Project, Task, User..."
                        value={localFilters.entityType || ''}
                        onChange={(e) => handleChange('entityType', e.target.value)}
                    />
                </div>

                <div>
                    <Label className="text-xs">Entity ID</Label>
                    <Input
                        placeholder="Entity ID"
                        value={localFilters.entityId || ''}
                        onChange={(e) => handleChange('entityId', e.target.value)}
                    />
                </div>

                <div>
                    <Label className="text-xs">From Date</Label>
                    <Input
                        type="date"
                        value={localFilters.from?.split('T')[0] || ''}
                        onChange={(e) => handleChange('from', e.target.value ? `${e.target.value}T00:00:00Z` : undefined)}
                    />
                </div>

                <div>
                    <Label className="text-xs">To Date</Label>
                    <Input
                        type="date"
                        value={localFilters.to?.split('T')[0] || ''}
                        onChange={(e) => handleChange('to', e.target.value ? `${e.target.value}T23:59:59Z` : undefined)}
                    />
                </div>
            </div>

            <div className="flex justify-end">
                <Button onClick={handleApply} className="gap-2">
                    <Search className="h-4 w-4" />
                    Apply Filters
                </Button>
            </div>
        </Card>
    )
}
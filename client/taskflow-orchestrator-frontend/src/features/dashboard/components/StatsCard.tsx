import { Card } from '@/components/ui/card'
import { cn } from '@/lib/utils/cn'

interface StatsCardProps {
    title: string
    value: number
    icon: React.ReactNode
    description?: string
    className?: string
}

export function StatsCard({ title, value, icon, description, className }: StatsCardProps) {
    return (
        <Card className={cn("p-6", className)}>
            <div className="flex items-center justify-between">
                <div>
                    <p className="text-sm text-muted-foreground">{title}</p>
                    <p className="text-3xl font-bold mt-1">{value}</p>
                    {description && (
                        <p className="text-xs text-muted-foreground mt-2">{description}</p>
                    )}
                </div>
                <div className="h-12 w-12 rounded-full bg-primary/10 flex items-center justify-center">
                    {icon}
                </div>
            </div>
        </Card>
    )
}
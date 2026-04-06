import * as React from "react"
import { cn } from "@/lib/utils/cn"

interface AlertDialogProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    children: React.ReactNode
}

export function AlertDialog({ open, onOpenChange, children }: AlertDialogProps) {
    if (!open) return null

    return (
        <div
            className="fixed inset-0 z-50 bg-black/50 flex items-center justify-center"
            onClick={() => onOpenChange(false)}
        >
            <div onClick={(e) => e.stopPropagation()}>
                {children}
            </div>
        </div>
    )
}

export function AlertDialogContent({
    children,
    className
}: {
    children: React.ReactNode
    className?: string
}) {
    return (
        <div className={cn(
            "bg-background rounded-lg p-6 shadow-lg max-w-md w-full",
            className
        )}>
            {children}
        </div>
    )
}

export function AlertDialogHeader({ children }: { children: React.ReactNode }) {
    return <div className="mb-4">{children}</div>
}

export function AlertDialogFooter({
    children,
    className
}: {
    children: React.ReactNode
    className?: string
}) {
    return <div className={cn("flex justify-end gap-2 mt-6", className)}>{children}</div>
}

export function AlertDialogTitle({ children }: { children: React.ReactNode }) {
    return <h2 className="text-lg font-semibold">{children}</h2>
}

export function AlertDialogDescription({ children }: { children: React.ReactNode }) {
    return <p className="text-sm text-muted-foreground">{children}</p>
}

export function AlertDialogCancel({
    children,
    onClick
}: {
    children: React.ReactNode
    onClick?: () => void
}) {
    return (
        <button
            className="px-4 py-2 text-sm font-medium border rounded-md hover:bg-accent transition-colors"
            onClick={onClick}
        >
            {children}
        </button>
    )
}

export function AlertDialogAction({
    children,
    onClick,
    className
}: {
    children: React.ReactNode
    onClick?: () => void
    className?: string
}) {
    return (
        <button
            className={cn(
                "px-4 py-2 text-sm font-medium bg-primary text-primary-foreground rounded-md hover:bg-primary/90 transition-colors",
                className
            )}
            onClick={onClick}
        >
            {children}
        </button>
    )
}
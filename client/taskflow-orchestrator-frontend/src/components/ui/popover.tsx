import { useState, ReactNode } from 'react'
import { cn } from '@/lib/utils/cn'

interface PopoverProps {
    trigger: ReactNode
    children: ReactNode
    open?: boolean
    onOpenChange?: (open: boolean) => void
    className?: string
    align?: 'start' | 'center' | 'end'
}

export function Popover({
    trigger,
    children,
    open: controlledOpen,
    onOpenChange,
    className,
    align = 'center'
}: PopoverProps) {
    const [internalOpen, setInternalOpen] = useState(false)
    const isOpen = controlledOpen !== undefined ? controlledOpen : internalOpen

    const setIsOpen = (newOpen: boolean) => {
        if (onOpenChange) onOpenChange(newOpen)
        setInternalOpen(newOpen)
    }

    const alignClasses = {
        start: 'left-0',
        center: 'left-1/2 -translate-x-1/2',
        end: 'right-0',
    }

    return (
        <div className="relative inline-block">
            <div onClick={() => setIsOpen(!isOpen)} className="cursor-pointer">
                {trigger}
            </div>

            {isOpen && (
                <>
                    <div className="fixed inset-0 z-40" onClick={() => setIsOpen(false)} />
                    <div className={cn(
                        "absolute z-50 mt-2",
                        alignClasses[align],
                        className
                    )}>
                        <div className="rounded-md border bg-popover text-popover-foreground shadow-md">
                            {children}
                        </div>
                    </div>
                </>
            )}
        </div>
    )
}
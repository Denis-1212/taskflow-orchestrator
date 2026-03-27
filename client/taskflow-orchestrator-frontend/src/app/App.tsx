import { QueryProvider } from './providers/QueryProvider'
import { ThemeProvider } from './providers/ThemeProvider'
import { RouterProvider } from './providers/RouterProvider'
import { Toaster } from 'sonner'

function App() {
  return (
    <QueryProvider>
      <ThemeProvider>
        <RouterProvider />
        <Toaster position="top-right" richColors />
      </ThemeProvider>
    </QueryProvider>
  )
}

export default App

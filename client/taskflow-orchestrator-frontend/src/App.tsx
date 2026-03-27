import { QueryProvider } from './app/providers/QueryProvider'
import { ThemeProvider } from './app/providers/ThemeProvider'
import { RouterProvider } from './app/providers/RouterProvider'
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

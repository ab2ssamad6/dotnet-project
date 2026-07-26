import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import App from './App';
import { AuthProvider } from '@/features/auth/AuthContext';
import { NotificationsProvider } from '@/features/notifications/NotificationsContext';
import { ConfirmProvider } from '@/components/ui';
import { ErrorBoundary } from '@/components/common/ErrorBoundary';
import './styles/index.css';

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ErrorBoundary>
      <BrowserRouter>
        <AuthProvider>
          <NotificationsProvider>
            <ConfirmProvider>
              <App />
              <Toaster
                position="top-right"
                toastOptions={{
                  duration: 4000,
                  className: 'text-sm',
                  style: { borderRadius: '10px', background: '#1e293b', color: '#fff' },
                  success: { iconTheme: { primary: '#10b981', secondary: '#fff' } },
                  error: { iconTheme: { primary: '#f43f5e', secondary: '#fff' } },
                }}
              />
            </ConfirmProvider>
          </NotificationsProvider>
        </AuthProvider>
      </BrowserRouter>
    </ErrorBoundary>
  </StrictMode>,
);

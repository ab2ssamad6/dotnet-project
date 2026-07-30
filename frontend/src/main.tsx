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
                gutter={10}
                toastOptions={{
                  duration: 4000,
                  className: 'text-sm font-medium',
                  style: {
                    borderRadius: '14px',
                    background: '#1d1b18',
                    color: '#f7f7f5',
                    padding: '10px 14px',
                    boxShadow: '0 24px 48px -24px rgb(29 27 24 / 0.45)',
                  },
                  success: { iconTheme: { primary: '#3fada4', secondary: '#1d1b18' } },
                  error: { iconTheme: { primary: '#fb7185', secondary: '#1d1b18' } },
                  loading: { iconTheme: { primary: '#ebc468', secondary: '#1d1b18' } },
                }}
              />
            </ConfirmProvider>
          </NotificationsProvider>
        </AuthProvider>
      </BrowserRouter>
    </ErrorBoundary>
  </StrictMode>,
);

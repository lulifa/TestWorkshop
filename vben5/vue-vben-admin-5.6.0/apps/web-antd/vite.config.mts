import { defineConfig } from '@vben/vite-config';

export default defineConfig(async () => {
  return {
    application: {},
    vite: {
      server: {
        proxy: {
          '/.well-known': {
            changeOrigin: true,
            target: 'http://localhost:44382/',
          },
          '/api': {
            changeOrigin: true,
            target: 'http://localhost:44382/',
          },
          '/connect': {
            changeOrigin: true,
            target: 'http://localhost:44382/',
          },
          '/signalr-hubs': {
            changeOrigin: true,
            target: 'http://localhost:44382/',
            ws: true,
          },
        },
      },
    },
  };
});

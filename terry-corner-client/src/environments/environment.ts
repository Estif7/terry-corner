export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000/api',
  signalRHubUrl: 'http://localhost:5000/hubs',
  // Origin only (no /api) — used to resolve relative image URLs like "/uploads/images/x.jpg"
  // returned by the upload endpoint, since the API runs on a different port than Angular in dev.
  assetBaseUrl: 'http://localhost:5000',
  supabase: {
    url: '',
    anonKey: '',
  },
};

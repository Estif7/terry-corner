export const environment = {
  production: true,
  apiBaseUrl: '/api',
  signalRHubUrl: '/hubs',
  // Empty in prod: Angular and the API are served from the same origin, so relative
  // image paths already resolve correctly with no prefix needed.
  assetBaseUrl: '',
};

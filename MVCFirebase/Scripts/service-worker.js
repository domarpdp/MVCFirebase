////// Cache name for versioning
////const CACHE_NAME = 'my-app-cache-v1';

////// Files to cache
////const FILES_TO_CACHE = [
////    '/'

////];

////// Install event: Caches the specified files
////self.addEventListener('install', (event) => {
////    console.log('[Service Worker] Install');
////    event.waitUntil(
////        caches.open(CACHE_NAME).then((cache) => {
////            console.log('[Service Worker] Caching files');
////            return cache.addAll(FILES_TO_CACHE);
////        })
////    );
////    self.skipWaiting();
////});

////// Activate event: Cleans up old caches
////self.addEventListener('activate', (event) => {
////    console.log('[Service Worker] Activate');
////    event.waitUntil(
////        caches.keys().then((cacheNames) => {
////            return Promise.all(
////                cacheNames.map((cache) => {
////                    if (cache !== CACHE_NAME) {
////                        console.log('[Service Worker] Removing old cache', cache);
////                        return caches.delete(cache);
////                    }
////                })
////            );
////        })
////    );
////    self.clients.claim();
////});

////// Fetch event: Intercept requests and save the last visited path
////self.addEventListener('fetch', (event) => {
////    if (event.request.mode == 'navigate') {
////        // Save the last visited path
////        const url = new URL(event.request.url);
////        const lastPath = url.pathname;
////        console.log('lastPath', lastPath, event);

////        event.respondWith(
////            caches.match(event.request).then((cachedResponse) => {
////                // Save the path in localStorage
////                self.clients.matchAll().then((clients) => {
////                    clients.forEach((client) => {
////                        client.postMessage({ action: 'saveLastPath', path: lastPath });
////                    });
////                });

////                return (
////                    cachedResponse ||
////                    fetch(event.request).catch(() => caches.match('/Home/index.html'))
////                );
////            })
////        );
////    } else {
////        // Handle other requests (e.g., assets)
////        event.respondWith(
////            caches.match(event.request).then((cachedResponse) => {
////                return cachedResponse || fetch(event.request);
////            })
////        );
////    }
////});

const CACHE_NAME = "my-pwa-cache-v1";
const urlsToCache = [
    "/"
];

self.addEventListener("install", event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            return cache.addAll(urlsToCache);
        })
    );
});

self.addEventListener("fetch", event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});
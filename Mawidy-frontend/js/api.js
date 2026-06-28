/**
 * Mawidy Frontend integration & localization helper
 */
const CONFIG = {
    apiBaseUrl: 'http://localhost:5281', // Update to your backend's host and port
    mockDelay: 600
};

// State management
let translations = null;
let currentLanguage = localStorage.getItem('mw3dy-lang') || 'ar';
let jwtToken = localStorage.getItem('mw3dy-token') || '';

// Parse token from query parameter if present
const urlParams = new URLSearchParams(window.location.search);
if (urlParams.has('token')) {
    jwtToken = urlParams.get('token');
    localStorage.setItem('mw3dy-token', jwtToken);
    
    // Clean URL without reloading page
    urlParams.delete('token');
    const newRelativePathQuery = window.location.pathname + (urlParams.toString() ? '?' + urlParams.toString() : '');
    window.history.replaceState({}, document.title, newRelativePathQuery);
}

/**
 * Initialize dynamic translations on the page
 */
async function initLocalization() {
    try {
        const response = await fetch('/translations.json');
        translations = await response.json();
        applyTranslations();
    } catch (err) {
        console.error('Failed to load translations.json, trying fallback...', err);
        // Try relative path parent folder (if in admin directory)
        try {
            const fallbackResponse = await fetch('../translations.json');
            translations = await fallbackResponse.json();
            applyTranslations();
        } catch (fErr) {
            console.error('Localization fallback failed.', fErr);
        }
    }
}

/**
 * Apply translations to DOM elements with data-translate attributes
 */
function applyTranslations() {
    if (!translations) return;

    // Apply document direction and language
    document.documentElement.lang = currentLanguage;
    document.documentElement.dir = currentLanguage === 'ar' ? 'rtl' : 'ltr';

    // Translate elements
    document.querySelectorAll('[data-translate]').forEach(el => {
        const keyPath = el.getAttribute('data-translate');
        const translatedText = getTranslationByKey(keyPath);
        if (translatedText) {
            if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                el.placeholder = translatedText;
            } else {
                el.innerHTML = translatedText;
            }
        }
    });

    // Translate Lucide icons labels if necessary, and re-draw icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

/**
 * Get nested translation key (e.g. "common.appName")
 */
function getTranslationByKey(keyPath) {
    if (!translations || !translations[currentLanguage]) return null;
    
    const parts = keyPath.split('.');
    let current = translations[currentLanguage];
    
    for (let part of parts) {
        if (current[part] === undefined) return null;
        current = current[part];
    }
    return current;
}

/**
 * Switch language dynamically
 */
function setLanguage(lang) {
    currentLanguage = lang;
    localStorage.setItem('mw3dy-lang', lang);
    applyTranslations();
}

/**
 * Clear session token (logout) and redirect
 */
function logout(redirectUrl = 'index.html') {
    localStorage.removeItem('mw3dy-token');
    jwtToken = '';
    window.location.href = redirectUrl;
}

/**
 * Authorized HTTP request helper
 */
async function apiFetch(endpoint, options = {}) {
    const headers = new Headers(options.headers || {});
    if (jwtToken) {
        headers.append('Authorization', `Bearer ${jwtToken}`);
    }
    headers.append('Content-Type', 'application/json');

    const config = {
        ...options,
        headers
    };

    try {
        const response = await fetch(`${CONFIG.apiBaseUrl}${endpoint}`, config);
        if (response.status === 401) {
            console.warn('Unauthorized request. Redirecting to login...');
            logout('index.html');
            return null;
        }
        return await response.json();
    } catch (err) {
        console.error(`API fetch failed on ${endpoint}:`, err);
        throw err;
    }
}

// Auto init on page load
document.addEventListener('DOMContentLoaded', () => {
    initLocalization();
});

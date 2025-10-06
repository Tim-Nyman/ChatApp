function loadDOMPurify() {
    if (!window.DOMPurify) {
        console.error("DOMPurify not loaded!");
    }
}

window.sanitizeInput = (input) => {
    if (!window.DOMPurify) loadDOMPurify();
    return DOMPurify.sanitize(input);
};
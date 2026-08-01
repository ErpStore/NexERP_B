function isNumericWithDot(evt) {
    const char = String.fromCharCode(evt.which);
    const allowed = /^[0-9.]$/;
    const input = evt.target;

    // Prevent multiple dots
    if (char === '.' && input.value.includes('.')) {
        return false;
    }

    return allowed.test(char);
}
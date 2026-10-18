/**
 * Formats a CPF string (might include masking) and masks it for display.
 * @param cpf - A CPF string (11 digits without punctuation)
 * @returns Formatted CPF with masking or the original input if invalid
 */
export function maskCpf(cpf: string): string {
    // Validate if it's 11 digits
    if (cpf.length !== 11) {
        return cpf;
    }
    // Mask with standard punctuation: 123.456.789-10
    return `${cpf.slice(0, 3)}.${cpf.slice(3, 6)}.${cpf.slice(6, 9)}-${cpf.slice(9)}`;
}

/**
 * Formats a CPF string with proper punctuation (123.456.789-10)
 * @param cpf - A CPF string (11 digits without punctuation)
 * @returns Formatted CPF or the original input if invalid
 */
export function formatCpf(cpf: string): string {
    // Clean input, keeping only digits
    const cleanCpf = cpf.replace(/\D/g, '');
    
    // Validate if it's 11 digits
    if (cleanCpf.length !== 11) {
        return cpf;
    }
    
    // Format with standard punctuation: 123.456.789-10
    return `${cleanCpf.slice(0, 3)}.${cleanCpf.slice(3, 6)}.${cleanCpf.slice(6, 9)}-${cleanCpf.slice(9)}`;
}

/**
 * Formats a date to DD/MM/YYYY format.
 * Dates coming from the API are calendar dates stored as UTC midnight, so they are read in UTC
 * to avoid showing the previous day in timezones behind UTC (e.g. Brazil).
 * @param date - Date object or string/number that can be parsed into a Date
 * @returns Formatted date string in DD/MM/YYYY format or empty string if invalid
 */
export function formatDateToDDMMYYYY(date: Date | string | number | null | undefined): string {
    if (date === null || date === undefined || date === '') {
        return '';
    }

    // ISO strings: use the calendar part as-is, no timezone conversion.
    if (typeof date === 'string') {
        const match = /^(\d{4})-(\d{2})-(\d{2})/.exec(date);
        if (match) {
            return `${match[3]}/${match[2]}/${match[1]}`;
        }
    }

    const dateObj = date instanceof Date ? date : new Date(date);
    if (isNaN(dateObj.getTime())) {
        return '';
    }

    const day = String(dateObj.getUTCDate()).padStart(2, '0');
    const month = String(dateObj.getUTCMonth() + 1).padStart(2, '0'); // Months are 0-indexed
    const year = dateObj.getUTCFullYear();

    return `${day}/${month}/${year}`;
}

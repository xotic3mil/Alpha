function validateForm(formId, customValidators = {}) {
    const form = document.getElementById(formId);
    if (!form) return false;

    ensureQuillEditorsInitialized(form);

    clearValidationErrors(form);
    
    let isValid = true;

    const requiredFields = form.querySelectorAll('[required], [data-val-required]');
    requiredFields.forEach(field => {
        if (field.type === 'hidden') return;
        
        if (!field.value.trim()) {
            const errorMessage = field.getAttribute('data-val-required') || 'This field is required';
            showValidationError(field, errorMessage);
            isValid = false;
        }
    });
    
    const emailFields = form.querySelectorAll('input[type="email"], [data-val-email]');
    emailFields.forEach(field => {
        if (field.value.trim() && !isValidEmail(field.value)) {
            const errorMessage = field.getAttribute('data-val-email') || 'Please enter a valid email address';
            showValidationError(field, errorMessage);
            isValid = false;
        }
    });
    
    const startDateField = form.querySelector('[name$="StartDate"]');
    const endDateField = form.querySelector('[name$="EndDate"]');
    if (startDateField && endDateField && startDateField.value && endDateField.value) {
        const startDate = new Date(startDateField.value);
        const endDate = new Date(endDateField.value);
        
        if (endDate < startDate) {
            showValidationError(endDateField, 'End date must be after start date');
            isValid = false;
        }
    }
    
    const passwordField = form.querySelector('input[type="password"][name$="Password"]:not([name$="ConfirmPassword"])');
    const confirmPasswordField = form.querySelector('input[type="password"][name$="ConfirmPassword"]');
    if (passwordField && confirmPasswordField && 
        passwordField.value && confirmPasswordField.value && 
        passwordField.value !== confirmPasswordField.value) {
        showValidationError(confirmPasswordField, 'Passwords do not match');
        isValid = false;
    }
    
    const quillContainers = form.querySelectorAll('.ql-container');
    quillContainers.forEach(container => {
        const editorId = container.closest('[data-quill-editor]')?.id;
        if (editorId) {
            const hiddenInput = document.getElementById(editorId);
            const editor = document.querySelector(`#${editorId} .ql-editor`);
            
            if (hiddenInput && editor) {
                const plainText = editor.textContent.trim();
                
                if (hiddenInput.hasAttribute('required') && !plainText) {
                    showValidationError(container, 'This field is required');
                    isValid = false;
                }
                
                if (typeof sanitizeHtml === 'function') {
                    hiddenInput.value = sanitizeHtml(editor.innerHTML);
                } else {
                    hiddenInput.value = editor.innerHTML;
                }
            }
        }
    });
    
    if (customValidators) {
        Object.keys(customValidators).forEach(fieldId => {
            const field = document.getElementById(fieldId);
            if (field) {
                const validationResult = customValidators[fieldId](field);
                if (!validationResult.valid) {
                    showValidationError(field, validationResult.message);
                    isValid = false;
                }
            }
        });
    }
    
    if (!isValid) {
        showValidationSummary(form);
    }
    
    return isValid;
}


function showValidationError(field, errorMessage) {
    field.classList.add('is-invalid');
    
    let errorContainer = field.nextElementSibling;
    if (!errorContainer || !errorContainer.classList.contains('field-validation-error')) {
        errorContainer = Array.from(field.parentNode.children).find(el => 
            el.classList.contains('field-validation-error') || 
            el.classList.contains('text-danger')
        );
    }
    
    if (!errorContainer) {
        errorContainer = document.createElement('span');
        errorContainer.classList.add('field-validation-error', 'text-danger');
        field.insertAdjacentElement('afterend', errorContainer);
    }
    
    errorContainer.textContent = errorMessage;
    
    field.setAttribute('aria-invalid', 'true');
    field.setAttribute('aria-describedby', `${field.id}-error`);
    errorContainer.id = `${field.id}-error`;
}

function showValidationSummary(form) {
    const summaryContainer = form.querySelector('.validation-summary-errors, [data-valmsg-summary="true"]');
    
    if (summaryContainer) {
        const errorList = summaryContainer.querySelector('ul') || document.createElement('ul');
        errorList.innerHTML = '';
        
        const errorMessages = form.querySelectorAll('.field-validation-error, .text-danger');
        errorMessages.forEach(error => {
            if (error.textContent.trim()) {
                const li = document.createElement('li');
                li.textContent = error.textContent;
                errorList.appendChild(li);
            }
        });
        
        if (!summaryContainer.contains(errorList)) {
            summaryContainer.appendChild(errorList);
        }
        
        summaryContainer.classList.remove('d-none');
    }
}
function clearValidationErrors(form) {
    const errorSpans = form.querySelectorAll('.field-validation-error, .text-danger');
    errorSpans.forEach(span => {
        span.textContent = '';
    });
    
    const invalidInputs = form.querySelectorAll('.is-invalid');
    invalidInputs.forEach(input => {
        input.classList.remove('is-invalid');
        input.removeAttribute('aria-invalid');
        input.removeAttribute('aria-describedby');
    });
    
    const summaryContainer = form.querySelector('.validation-summary-errors, [data-valmsg-summary="true"]');
    if (summaryContainer) {
        summaryContainer.classList.add('d-none');
        const errorList = summaryContainer.querySelector('ul');
        if (errorList) {
            errorList.innerHTML = '';
        }
    }
}



function isValidEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
}


function sanitizeHtml(html) {
    if (!html) return '';
    
    const doc = new DOMParser().parseFromString(html, 'text/html');
    const scripts = doc.querySelectorAll('script');
    scripts.forEach(script => script.remove());
    
    const allElements = doc.querySelectorAll('*');
    allElements.forEach(el => {
        Array.from(el.attributes).forEach(attr => {
            if (attr.name.startsWith('on') || 
                (attr.name === 'href' && attr.value.toLowerCase().startsWith('javascript:'))) {
                el.removeAttribute(attr.name);
            }
        });
    });
    
    return doc.body.innerHTML;
}

document.addEventListener('DOMContentLoaded', function() {
    document.querySelectorAll('form[data-validate="true"]').forEach(form => {
        form.addEventListener('submit', function(e) {
            if (!validateForm(form.id)) {
                e.preventDefault();
                e.stopPropagation();
                return false;
            }
        });
    });
    
});
// ========================================
// VALIDACIONES UNIVERSALES PARA TODOS LOS CRUDS
// ========================================

// Patrones peligrosos de SQL Injection y XSS
const DANGEROUS_PATTERNS = [
    /(\bOR\b|\bAND\b).*=/i,
    /['";`]|--|\/\*|\*\//,  // Comillas, punto y coma, backticks, comentarios SQL
    /\b(EXEC|EXECUTE|DROP|DELETE|UPDATE|INSERT|SELECT.*FROM|UNION.*SELECT)\b/i,
    /<script/i,
    /javascript:/i,
    /onerror\s*=/i,
    /onload\s*=/i,
    /<iframe/i,
    /on\w+\s*=/i,
    /[$%^&*(){}[\]\\|]/  // Caracteres especiales peligrosos ($%^&*) pero NO incluye tildes ni ñ
];

// ========================================
// FUNCIONES DE VALIDACIÓN
// ========================================

/**
 * Verifica si un texto contiene patrones peligrosos
 */
function containsDangerousPattern(input) {
    if (!input) return false;
    return DANGEROUS_PATTERNS.some(pattern => pattern.test(input));
}

/**
 * Muestra un mensaje de error en un campo
 */
function showFieldError(field, message) {
 if (!field) return;
    
    const errorSpan = field.parentElement.querySelector('.text-danger');
    if (errorSpan) {
        errorSpan.textContent = message;
    errorSpan.style.display = 'block';
  }
    field.classList.add('is-invalid');
    field.style.borderColor = '#dc3545';
}

/**
 * Limpia el mensaje de error de un campo
 */
function clearFieldError(field) {
    if (!field) return;
    
    const errorSpan = field.parentElement.querySelector('.text-danger');
    if (errorSpan) {
        errorSpan.textContent = '';
        errorSpan.style.display = 'none';
    }
field.classList.remove('is-invalid');
    field.style.borderColor = '';
}

/**
 * Valida que un campo de texto no esté vacío
 */
function validateRequired(field, fieldName) {
    const value = field.value.trim();
    if (!value) {
        showFieldError(field, `${fieldName} es requerido.`);
        return false;
    }
    return true;
}

/**
 * Valida la longitud mínima de un texto
 */
function validateMinLength(field, fieldName, minLength) {
    const value = field.value.trim();
    if (value && value.length < minLength) {
        showFieldError(field, `${fieldName} debe tener al menos ${minLength} caracteres.`);
 return false;
    }
  return true;
}

/**
 * Valida la longitud máxima de un texto
 */
function validateMaxLength(field, fieldName, maxLength) {
    const value = field.value.trim();
    if (value && value.length > maxLength) {
        showFieldError(field, `${fieldName} no puede exceder ${maxLength} caracteres.`);
return false;
    }
    return true;
}

/**
 * Valida que no contenga patrones peligrosos
 */
function validateSecurity(field, fieldName) {
    const value = field.value.trim();
    if (value && containsDangerousPattern(value)) {
      showFieldError(field, `?? ${fieldName} contiene caracteres o patrones no permitidos (evite: <, >, ', ", --, SELECT, DROP, etc.)`);
      return false;
    }
    return true;
}

/**
 * Valida un campo de email
 */
function validateEmail(field) {
    const value = field.value.trim();
    if (value) {
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
   if (!emailRegex.test(value)) {
    showFieldError(field, '? Formato de email inválido.');
            return false;
        }
    }
    return true;
}

/**
 * Valida formato de fecha DD/MM/YYYY
 */
function validateDateFormat(fecha) {
    if (!fecha) return true;
    const regex = /^(\d{2})\/(\d{2})\/(\d{4})$/;
    if (!regex.test(fecha)) return false;
    
    const [, dia, mes, año] = regex.exec(fecha);
    const d = parseInt(dia, 10);
  const m = parseInt(mes, 10);
  const a = parseInt(año, 10);
    
    if (m < 1 || m > 12) return false;
    if (d < 1 || d > 31) return false;
    
  const diasPorMes = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    if (a % 4 === 0 && (a % 100 !== 0 || a % 400 === 0)) diasPorMes[1] = 29;
    
    return d <= diasPorMes[m - 1];
}

/**
 * Parsea una fecha en formato DD/MM/YYYY
 */
function parseDate(fechaStr) {
    const [dia, mes, año] = fechaStr.split('/').map(Number);
    return new Date(año, mes - 1, dia);
}

/**
 * Valida que una fecha no sea pasada
 */
function validateDateNotPast(fecha) {
    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);
    return fecha >= hoy;
}

// ========================================
// CONFIGURACIÓN DE VALIDACIONES EN TIEMPO REAL
// ========================================

/**
 * Configura validación en tiempo real para un campo de texto
 */
function setupTextFieldValidation(field, fieldName, options = {}) {
    if (!field) return;

    const {
        required = false,
        minLength = 0,
        maxLength = 0,
        checkSecurity = true,
        email = false
    } = options;

    // Validación mientras escribe (input)
    field.addEventListener('input', function() {
        clearFieldError(this);
        
  const value = this.value.trim();
        
        if (checkSecurity && value && containsDangerousPattern(value)) {
            showFieldError(this, `?? ${fieldName} contiene caracteres o patrones no permitidos.`);
        }
    });

    // Validación al perder foco (blur)
    field.addEventListener('blur', function() {
        const value = this.value.trim();
      
    if (required && !value) {
    showFieldError(this, `${fieldName} es requerido.`);
       return;
  }
        
        if (minLength > 0 && value && value.length < minLength) {
   showFieldError(this, `${fieldName} debe tener al menos ${minLength} caracteres.`);
      return;
        }
     
        if (maxLength > 0 && value && value.length > maxLength) {
            showFieldError(this, `${fieldName} no puede exceder ${maxLength} caracteres.`);
  return;
      }
        
        if (email && value && !validateEmail(this)) {
 return;
        }
    });
}

/**
 * Configura validación para un campo de fecha
 */
function setupDateFieldValidation(field, fieldName, options = {}) {
    if (!field || typeof flatpickr === 'undefined') return;

    const {
 required = false,
        allowPast = false,
      minDate = null
    } = options;

    const config = {
        dateFormat: "d/m/Y",
        locale: "es",
allowInput: true,
        altInput: false,
        disableMobile: false,
        defaultDate: new Date(),
        onChange: function() {
         clearFieldError(field);
        }
    };

    if (!allowPast) {
   config.minDate = minDate || new Date();
    }

    flatpickr(field, config);

    field.addEventListener('blur', function() {
  const value = this.value.trim();
        
        if (required && !value) {
      showFieldError(this, `${fieldName} es requerida.`);
            return;
        }
        
        if (value && !validateDateFormat(value)) {
            showFieldError(this, `? Formato de fecha inválido. Use DD/MM/AAAA`);
            return;
        }
      
        if (!allowPast && value) {
         const fecha = parseDate(value);
   if (!validateDateNotPast(fecha)) {
            showFieldError(this, `? ${fieldName} no puede ser anterior a hoy.`);
       }
        }
    });
}

/**
 * Configura validación para un select
 */
function setupSelectValidation(field, fieldName, required = true) {
    if (!field) return;

    field.addEventListener('change', function() {
     clearFieldError(this);
        
 if (required && (!this.value || this.value === '' || this.value === '0')) {
    showFieldError(this, `${fieldName} es requerido.`);
        }
    });
}

// ========================================
// VALIDACIÓN DE FORMULARIO COMPLETO
// ========================================

/**
 * Valida un formulario completo antes de enviarlo
 */
function validateForm(formId, validations) {
    const form = document.getElementById(formId);
    if (!form) return;

    form.addEventListener('submit', function(event) {
        let hasErrors = false;

        // Ejecutar cada validación
        for (const validation of validations) {
 const field = document.querySelector(validation.selector);
            if (!field) continue;

  clearFieldError(field);

            const value = field.value.trim();

            // Validar requerido
            if (validation.required && !value) {
                event.preventDefault();
         showFieldError(field, `${validation.name} es requerido.`);
  hasErrors = true;
        continue;
       }

          // Validar longitud mínima
            if (validation.minLength && value && value.length < validation.minLength) {
        event.preventDefault();
       showFieldError(field, `${validation.name} debe tener al menos ${validation.minLength} caracteres.`);
    hasErrors = true;
                continue;
  }

            // Validar longitud máxima
     if (validation.maxLength && value && value.length > validation.maxLength) {
         event.preventDefault();
          showFieldError(field, `${validation.name} no puede exceder ${validation.maxLength} caracteres.`);
             hasErrors = true;
  continue;
    }

      // Validar seguridad
  if (validation.checkSecurity !== false && value && containsDangerousPattern(value)) {
       event.preventDefault();
       showFieldError(field, `?? ${validation.name} contiene caracteres o patrones no permitidos.`);
            hasErrors = true;
        continue;
            }

     // Validar email
       if (validation.email && value && !validateEmail(field)) {
     event.preventDefault();
     hasErrors = true;
       continue;
            }

            // Validar fecha
            if (validation.isDate && value) {
           if (!validateDateFormat(value)) {
         event.preventDefault();
   showFieldError(field, `? Formato de fecha inválido. Use DD/MM/AAAA`);
  hasErrors = true;
      continue;
  }

     if (!validation.allowPast) {
       const fecha = parseDate(value);
         if (!validateDateNotPast(fecha)) {
    event.preventDefault();
    showFieldError(field, `? ${validation.name} no puede ser anterior a hoy.`);
    hasErrors = true;
}
                }
 }
        }

  if (hasErrors) {
            console.error('? Errores de validación encontrados en el formulario');
        }
 });
}

// ========================================
// FUNCIÓN DE INICIALIZACIÓN UNIVERSAL
// ========================================

/**
 * Inicializa validaciones para cualquier formulario
 * @param {string} formId - ID del formulario
 * @param {Array} fields - Array de configuración de campos
 */
function initUniversalValidation(formId, fields) {
    const form = document.getElementById(formId);
    if (!form) return;

    // Configurar validación en tiempo real para cada campo
  fields.forEach(fieldConfig => {
        const field = document.querySelector(fieldConfig.selector);
        if (!field) return;

        if (fieldConfig.type === 'text' || fieldConfig.type === 'textarea') {
            setupTextFieldValidation(field, fieldConfig.name, {
        required: fieldConfig.required,
        minLength: fieldConfig.minLength,
       maxLength: fieldConfig.maxLength,
            checkSecurity: fieldConfig.checkSecurity !== false,
         email: fieldConfig.email
            });
    } else if (fieldConfig.type === 'date') {
       setupDateFieldValidation(field, fieldConfig.name, {
             required: fieldConfig.required,
       allowPast: fieldConfig.allowPast,
       minDate: fieldConfig.minDate
            });
        } else if (fieldConfig.type === 'select') {
        setupSelectValidation(field, fieldConfig.name, fieldConfig.required);
    }
    });

  // Configurar validación al enviar
    validateForm(formId, fields);
}

// Exportar funciones para uso global
window.CrudValidation = {
    initUniversalValidation,
    validateForm,
    setupTextFieldValidation,
    setupDateFieldValidation,
    setupSelectValidation,
showFieldError,
    clearFieldError,
    containsDangerousPattern
};

console.log('? Sistema de validaciones universal cargado');

// ========================================
// VALIDACIONES DE SEGURIDAD PARA PROYECTOS
// ========================================

// Patrones peligrosos de SQL Injection y XSS
const dangerousPatterns = [
    /(\bOR\b|\bAND\b).*=/i,
    /['";`]|--|\/\*|\*\//,  // Comillas, punto y coma, backticks, comentarios SQL
    /\b(EXEC|EXECUTE|DROP|DELETE|UPDATE|INSERT|SELECT.*FROM|UNION.*SELECT)\b/i,
    /<script/i,
    /javascript:/i,
    /onerror\s*=/i,
    /onload\s*=/i,
    /<iframe/i,
    /on\w+\s*=/i,
    /[$%^&*(){}[\]\\|]/  // Caracteres especiales peligrosos pero NO incluye tildes
];

function containsDangerousPattern(input) {
  if (!input) return false;
    return dangerousPatterns.some(pattern => pattern.test(input));
}

function showFieldError(field, message) {
    const errorSpan = field.parentElement.querySelector('.text-danger');
    if (errorSpan) {
        errorSpan.textContent = message;
   errorSpan.style.display = 'block';
    }
    field.classList.add('is-invalid');
    field.style.borderColor = '#dc3545';
}

function clearFieldError(field) {
    const errorSpan = field.parentElement.querySelector('.text-danger');
    if (errorSpan) {
        errorSpan.textContent = '';
        errorSpan.style.display = 'none';
    }
    field.classList.remove('is-invalid');
    field.style.borderColor = '';
}

function validarFormatoFecha(fecha) {
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

function parsearFecha(fechaStr) {
    const [dia, mes, año] = fechaStr.split('/').map(Number);
    return new Date(año, mes - 1, dia);
}

function validarFechaNoEsPasado(fecha) {
    const hoy = new Date();
    hoy.setHours(0, 0, 0, 0);
    return fecha >= hoy;
}

// ========================================
// INICIALIZAR VALIDACIONES
// ========================================
function initProyectoValidation() {
    const form = document.querySelector('#createProyectoForm');
    if (!form) return;

    const nombreInput = document.querySelector('input[name="Proyecto.Nombre"]');
 const descripcionInput = document.querySelector('textarea[name="Proyecto.Descripcion"]');
    const fechaInicioInput = document.querySelector('#fechaInicioInput');
    const fechaFinInput = document.querySelector('#fechaFinInput');

    // ========================================
    // VALIDACIÓN DE NOMBRE EN TIEMPO REAL
    // ========================================
    if (nombreInput) {
     nombreInput.addEventListener('input', function() {
            clearFieldError(this);
     
        const value = this.value.trim();

         if (value && containsDangerousPattern(value)) {
       showFieldError(this, '?? El nombre contiene caracteres no permitidos (evite: $, %, ^, &, *, <, >, \', ", --, SELECT, DROP, etc.)');
          }
        });

  nombreInput.addEventListener('blur', function() {
       const value = this.value.trim();
   
  if (!value) {
    showFieldError(this, 'El nombre del proyecto es requerido.');
   } else if (value.length < 3) {
  showFieldError(this, 'El nombre debe tener al menos 3 caracteres.');
   } else if (value.length > 200) {
     showFieldError(this, 'El nombre no puede exceder 200 caracteres.');
            }
 });
    }

    // ========================================
    // VALIDACIÓN DE DESCRIPCIÓN EN TIEMPO REAL
    // ========================================
    if (descripcionInput) {
    descripcionInput.addEventListener('input', function() {
         clearFieldError(this);
         
      const value = this.value.trim();
            
     if (value && containsDangerousPattern(value)) {
       showFieldError(this, '?? La descripción contiene caracteres no permitidos (evite: $, %, ^, &, *, <script>, javascript:, etc.)');
      }
        });

      descripcionInput.addEventListener('blur', function() {
            const value = this.value.trim();
 
          if (value && value.length > 1000) {
    showFieldError(this, 'La descripción no puede exceder 1000 caracteres.');
        }
   });
    }

    // ========================================
    // CONFIGURACIÓN DE FLATPICKR PARA FECHAS
  // ========================================
    if (fechaInicioInput && typeof flatpickr !== 'undefined') {
        flatpickr(fechaInicioInput, {
   dateFormat: "d/m/Y",
            locale: "es",
       allowInput: true,
            altInput: false,
   disableMobile: false,
            defaultDate: new Date(),
     minDate: new Date(),
            onChange: function (selectedDates) {
    clearFieldError(fechaInicioInput);

                if (selectedDates.length > 0) {
           const fechaFinPicker = document.querySelector('#fechaFinInput')._flatpickr;
           if (fechaFinPicker) {
       fechaFinPicker.set('minDate', selectedDates[0]);
            if (!fechaFinInput.value) {
        fechaFinPicker.jumpToDate(selectedDates[0]);
          }
           }
       }
            }
        });
    }

    if (fechaFinInput && typeof flatpickr !== 'undefined') {
        flatpickr(fechaFinInput, {
            dateFormat: "d/m/Y",
       locale: "es",
            allowInput: true,
            altInput: false,
            disableMobile: false,
   defaultDate: new Date(),
            minDate: new Date(),
         onChange: function () {
    clearFieldError(fechaFinInput);
        }
        });
    }

    // ========================================
    // VALIDACIÓN COMPLETA AL ENVIAR FORMULARIO
    // ========================================
    form.addEventListener('submit', function (event) {
        let hasErrors = false;

        // Validar nombre
        if (nombreInput) {
      const nombreValue = nombreInput.value.trim();
        if (!nombreValue) {
                event.preventDefault();
          showFieldError(nombreInput, 'El nombre del proyecto es requerido.');
         hasErrors = true;
  } else if (nombreValue.length < 3) {
   event.preventDefault();
       showFieldError(nombreInput, 'El nombre debe tener al menos 3 caracteres.');
   hasErrors = true;
      } else if (containsDangerousPattern(nombreValue)) {
     event.preventDefault();
      showFieldError(nombreInput, '?? El nombre contiene caracteres o patrones no permitidos. Por favor, use solo letras, números y espacios.');
          hasErrors = true;
}
        }

        // Validar descripción
        if (descripcionInput) {
            const descripcionValue = descripcionInput.value.trim();
     if (descripcionValue && containsDangerousPattern(descripcionValue)) {
    event.preventDefault();
      showFieldError(descripcionInput, '?? La descripción contiene caracteres o patrones no permitidos. Por favor, use solo texto normal.');
hasErrors = true;
       }
      }

        // Validar fechas
      if (fechaInicioInput && fechaFinInput) {
 const inicioStr = fechaInicioInput.value;
 const finStr = fechaFinInput.value;

            if (inicioStr && !validarFormatoFecha(inicioStr)) {
     event.preventDefault();
             showFieldError(fechaInicioInput, '? Formato de fecha inválido. Use DD/MM/AAAA (ejemplo: 15/12/2024)');
                hasErrors = true;
        } else if (inicioStr) {
                const fechaInicio = parsearFecha(inicioStr);
         if (!validarFechaNoEsPasado(fechaInicio)) {
 event.preventDefault();
                showFieldError(fechaInicioInput, '? La fecha de inicio no puede ser anterior a hoy.');
  hasErrors = true;
          }
          }

         if (finStr && !validarFormatoFecha(finStr)) {
   event.preventDefault();
                showFieldError(fechaFinInput, '? Formato de fecha inválido. Use DD/MM/AAAA (ejemplo: 20/12/2024)');
     hasErrors = true;
     } else if (finStr) {
            const fechaFin = parsearFecha(finStr);
           if (!validarFechaNoEsPasado(fechaFin)) {
   event.preventDefault();
                showFieldError(fechaFinInput, '? La fecha de finalización no puede ser anterior a hoy.');
           hasErrors = true;
                }
       }

    if (inicioStr && finStr && validarFormatoFecha(inicioStr) && validarFormatoFecha(finStr)) {
     const inicio = parsearFecha(inicioStr);
     const fin = parsearFecha(finStr);
             
            if (fin < inicio) {
 event.preventDefault();
           showFieldError(fechaFinInput, '? La fecha de finalización no puede ser anterior a la fecha de inicio.');
            hasErrors = true;
      } else if (fin.getTime() === inicio.getTime()) {
           event.preventDefault();
         showFieldError(fechaFinInput, '? La fecha de finalización debe ser posterior a la fecha de inicio (no pueden ser iguales).');
                hasErrors = true;
                }
            }
  }

  if (hasErrors) {
        console.error('? Errores de validación encontrados');
        }
    });
}

// Inicializar cuando el DOM esté listo
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initProyectoValidation);
} else {
    initProyectoValidation();
}

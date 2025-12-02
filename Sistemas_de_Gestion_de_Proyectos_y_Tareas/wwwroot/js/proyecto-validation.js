// Client-side validation for Proyecto forms - ULTRA STRICT MODE
(function () {
    'use strict';

    // Dangerous characters pattern
    const dangerousCharactersPattern = /[$%^&*(){}[\]\\|<>'"`;@#!]/;

    function initProyectoValidation() {
        const createForm = document.querySelector('#createProyectoForm');
    const editForm = document.querySelector('#editProyectoForm');
        
      const form = createForm || editForm;
        if (!form) return;

        // Get input fields
        const nombreInput = form.querySelector('input[name="Proyecto.Nombre"]');
        const descripcionInput = form.querySelector('textarea[name="Proyecto.Descripcion"]');
        const fechaInicioInput = form.querySelector('#fechaInicioInput');
        const fechaFinInput = form.querySelector('#fechaFinInput');

        if (!nombreInput || !descripcionInput) {
       console.warn('Form inputs not found for proyecto validation');
          return;
    }

        // Remove HTML5 validation to use our custom validation
        form.setAttribute('novalidate', 'novalidate');

        // Validate field - STRICT
   function validateField(input, fieldName, isRequired = true) {
     const value = input.value;
    const trimmedValue = value.trim();
            let errorSpan = input.parentElement.querySelector('.text-danger');

  if (!errorSpan) {
      errorSpan = document.createElement('span');
      errorSpan.className = 'text-danger';
        input.parentElement.appendChild(errorSpan);
        }

 errorSpan.textContent = '';
      input.classList.remove('is-invalid');

     // Check required
 if (isRequired && !trimmedValue) {
     const message = `?? ${fieldName} es obligatorio.`;
    errorSpan.textContent = message;
    input.classList.add('is-invalid');
  return false;
    }

            // Check dangerous characters
            if (trimmedValue && dangerousCharactersPattern.test(trimmedValue)) {
   const message = `?? ${fieldName} contiene caracteres no permitidos: $ % ^ & * ( ) { } [ ] \\ | < > ' " ; \` @ # !`;
  errorSpan.textContent = message;
     input.classList.add('is-invalid');
  return false;
            }

       return true;
        }

        // Validate date field
    function validateDateField(input, fieldName) {
            const value = input.value.trim();
        let errorSpan = input.parentElement.querySelector('.text-danger');
 
            if (!errorSpan) {
       errorSpan = document.createElement('span');
       errorSpan.className = 'text-danger';
             input.parentElement.appendChild(errorSpan);
 }

            errorSpan.textContent = '';
         input.classList.remove('is-invalid');

   if (!value) {
      const message = `?? ${fieldName} es obligatoria.`;
   errorSpan.textContent = message;
   input.classList.add('is-invalid');
        return false;
            }

         return true;
        }

        // Block dangerous character input in real-time
 function blockDangerousInput(event) {
        const char = event.key;
   if (char && char.length === 1 && dangerousCharactersPattern.test(char)) {
      event.preventDefault();
            event.stopPropagation();
   
  const input = event.target;
      const fieldName = input === nombreInput ? 'El nombre del proyecto' : 'La descripción';
         let errorSpan = input.parentElement.querySelector('.text-danger');
           
       if (!errorSpan) {
         errorSpan = document.createElement('span');
      errorSpan.className = 'text-danger';
       input.parentElement.appendChild(errorSpan);
                }
                
         errorSpan.textContent = `?? El carácter "${char}" no está permitido.`;
 input.classList.add('is-invalid');
         
       // Clear error after 3 seconds
      setTimeout(() => {
        if (errorSpan.textContent.includes('carácter')) {
   errorSpan.textContent = '';
     input.classList.remove('is-invalid');
          }
                }, 3000);
    
     return false;
            }
        }

   // Real-time validation
        nombreInput.addEventListener('keydown', blockDangerousInput);
        descripcionInput.addEventListener('keydown', blockDangerousInput);

        nombreInput.addEventListener('input', function() {
         validateField(this, 'El nombre del proyecto', true);
        });

        descripcionInput.addEventListener('input', function() {
    validateField(this, 'La descripción', true);
    });

        nombreInput.addEventListener('blur', function() {
         validateField(this, 'El nombre del proyecto', true);
    });

 descripcionInput.addEventListener('blur', function() {
      validateField(this, 'La descripción', true);
    });

        // Prevent paste of dangerous characters
    [nombreInput, descripcionInput].forEach(input => {
            input.addEventListener('paste', function(event) {
                setTimeout(() => {
    const value = input.value;
    if (dangerousCharactersPattern.test(value)) {
   // Remove dangerous characters
    input.value = value.replace(dangerousCharactersPattern, '');
             const fieldName = input === nombreInput ? 'El nombre del proyecto' : 'La descripción';
        validateField(input, fieldName, true);
       }
           }, 10);
            });
        });

        // CRITICAL: Form submission validation with MODAL
 form.addEventListener('submit', function(event) {
            // ALWAYS prevent default first
      event.preventDefault();
            event.stopImmediatePropagation();

   let errors = [];
        let isValid = true;

     // Validate nombre
            if (!validateField(nombreInput, 'El nombre del proyecto', true)) {
          errors.push('• El nombre del proyecto es obligatorio y no debe contener caracteres especiales');
 isValid = false;
  }

        // Validate descripcion
          if (!validateField(descripcionInput, 'La descripción', true)) {
            errors.push('• La descripción es obligatoria y no debe contener caracteres especiales');
      isValid = false;
            }

 // Validate fechas
          if (fechaInicioInput && !validateDateField(fechaInicioInput, 'La fecha de inicio')) {
       errors.push('• La fecha de inicio es obligatoria');
      isValid = false;
 }

            if (fechaFinInput && !validateDateField(fechaFinInput, 'La fecha de fin')) {
          errors.push('• La fecha de fin es obligatoria');
        isValid = false;
            }

    // Additional safety checks
const nombreValue = nombreInput.value.trim();
     const descripcionValue = descripcionInput.value.trim();

       if (nombreValue && dangerousCharactersPattern.test(nombreValue)) {
        errors.push('• El nombre contiene caracteres no permitidos: $ % ^ & * ( ) { } [ ] \\ | < > \' " ; ` @ # !');
        isValid = false;
          }

    if (descripcionValue && dangerousCharactersPattern.test(descripcionValue)) {
         errors.push('• La descripción contiene caracteres no permitidos: $ % ^ & * ( ) { } [ ] \\ | < > \' " ; ` @ # !');
     isValid = false;
            }

            if (!isValid) {
    // Show modal with errors
      showValidationModal(errors);
         
    // Scroll to first error
                const firstError = form.querySelector('.is-invalid');
      if (firstError) {
         firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
      firstError.focus();
   }

         return false;
       }

       // If all validations pass, submit the form
form.submit();
     }, true);

        // Show validation modal
   function showValidationModal(errors) {
      const modalHtml = `
           <div class="modal fade" id="validationErrorModal" tabindex="-1" aria-labelledby="validationErrorModalLabel" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="false">
          <div class="modal-dialog modal-dialog-centered">
               <div class="modal-content" style="background-color: #2c2c31; color: #f5f5f7; border: 2px solid #dc3545;">
             <div class="modal-header" style="border-bottom: 2px solid #dc3545; background: linear-gradient(135deg, #dc3545, #c82333);">
   <h5 class="modal-title" id="validationErrorModalLabel" style="color: white; font-weight: bold;">
        <i class="bi bi-exclamation-triangle-fill"></i> Error de Validación
     </h5>
             </div>
          <div class="modal-body" style="padding: 2rem;">
              <p style="font-size: 1.1rem; margin-bottom: 1rem;"><strong>No se puede guardar el proyecto por los siguientes errores:</strong></p>
    <div style="background-color: #3a3a40; padding: 1rem; border-radius: 8px; border-left: 4px solid #dc3545;">
  ${errors.map(err => `<p style="margin: 0.5rem 0; color: #ff6b6b;">${err}</p>`).join('')}
  </div>
    <p style="margin-top: 1.5rem; color: #888; font-size: 0.9rem;">
         <strong>Caracteres no permitidos:</strong> $ % ^ & * ( ) { } [ ] \\ | < > ' " ; ` @ # !
     </p>
</div>
       <div class="modal-footer" style="border-top: 1px solid rgba(255,255,255,0.1);">
        <button type="button" class="btn btn-danger" data-bs-dismiss="modal" style="min-width: 120px;">
      <i class="bi bi-x-circle"></i> Cerrar
        </button>
           </div>
             </div>
   </div>
       </div>
          `;

      // Remove existing modal if any
   const existingModal = document.getElementById('validationErrorModal');
            if (existingModal) {
         existingModal.remove();
      }

    // Add modal to body
      document.body.insertAdjacentHTML('beforeend', modalHtml);

         // Show modal
            const modal = new bootstrap.Modal(document.getElementById('validationErrorModal'));
 modal.show();

            // Remove modal from DOM after it's hidden
            document.getElementById('validationErrorModal').addEventListener('hidden.bs.modal', function() {
                this.remove();
   });
        }
    }

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initProyectoValidation);
    } else {
        initProyectoValidation();
    }
})();

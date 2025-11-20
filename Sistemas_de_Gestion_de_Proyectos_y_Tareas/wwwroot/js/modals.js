// ========================================
// SISTEMA DE MODALES GLOBALES - VERSIÓN CORREGIDA
// ========================================

// Configuración de modales
const ModalConfig = {
    confirmEdit: {
    id: 'confirmEditModal',
        title: 'Confirmar Edición',
  message: '¿Estás seguro de que deseas guardar estos cambios?',
 confirmText: 'Sí, Guardar',
        cancelText: 'Cancelar'
    },
  confirmDelete: {
        id: 'confirmDeleteModal',
        title: 'Confirmar Eliminación',
    message: '¿Estás seguro de que deseas eliminar este elemento? Esta acción no se puede deshacer.',
        confirmText: 'Sí, Eliminar',
   cancelText: 'Cancelar'
    },
    confirmCreate: {
      id: 'confirmCreateModal',
        title: 'Confirmar Creación',
        message: '¿Estás seguro de que deseas crear este elemento?',
        confirmText: 'Sí, Crear',
        cancelText: 'Cancelar'
    },
    confirmLogout: {
        id: 'logoutModal',
        title: 'Confirmar Cierre de Sesión',
        message: '¿Estás seguro de que deseas cerrar sesión?',
    confirmText: 'Sí, Cerrar Sesión',
    cancelText: 'Cancelar'
    },
    success: {
   id: 'successModal',
     title: 'Operación Exitosa',
     confirmText: 'Aceptar'
    },
    error: {
    id: 'errorModal',
      title: 'Error',
        confirmText: 'Aceptar'
    }
};

// Clase para manejar modales
class ModalManager {
    constructor() {
        this.currentModal = null;
        this.currentCallback = null;
   this.itemIdToDelete = null;
    }

    showConfirmModal(type, message = null, callback = null) {
        const config = ModalConfig[type];
        if (!config) return;

   let modal = document.getElementById(config.id);
     if (!modal) {
    console.error(`Modal ${config.id} no encontrado`);
    return;
  }

        // Actualizar mensaje si se proporciona uno
        if (message) {
   const messageElement = modal.querySelector('.modal-body p');
  if (messageElement) {
        messageElement.textContent = message;
        }
      }

        this.currentCallback = callback;
        const bootstrapModal = new bootstrap.Modal(modal);
        bootstrapModal.show();
    }

    showNotification(type, message, autoClose = true) {
        const config = ModalConfig[type];
        if (!config) return;

      let modal = document.getElementById(config.id);
        if (!modal) {
            console.error(`Modal ${config.id} no encontrado`);
  return;
      }

        const messageElement = modal.querySelector('.modal-body p');
        if (messageElement) {
 messageElement.textContent = message;
        }

        const bootstrapModal = new bootstrap.Modal(modal);
bootstrapModal.show();

     if (autoClose) {
     setTimeout(() => {
  bootstrapModal.hide();
      }, 3000);
     }
    }
}

// Instancia global
const modalManager = new ModalManager();

// Interceptar envíos de formularios para mostrar confirmación
function setupFormConfirmation(formId, modalType = 'confirmEdit') {
    const form = document.getElementById(formId);
    if (!form) {
        console.warn(`Formulario ${formId} no encontrado`);
      return;
    }

let formSubmitted = false;

    form.addEventListener('submit', function(e) {
        if (formSubmitted) {
            return true; // Permitir el envío real
        }

        e.preventDefault();
    e.stopPropagation();

        // Mostrar modal de confirmación
        modalManager.showConfirmModal(modalType, null, () => {
 formSubmitted = true;

 // Deshabilitar botón de envío
    const submitBtn = form.querySelector('[type="submit"]');
        if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.textContent = 'Procesando...';
            }

            // Enviar formulario
     form.submit();
        });

   return false;
    });
}

// Inicialización automática
document.addEventListener('DOMContentLoaded', function() {
    console.log('?? Sistema de modales inicializado');

  // Configurar el botón de confirmación del modal de edición
    const confirmEditBtn = document.getElementById('confirmEditButton');
    if (confirmEditBtn) {
        confirmEditBtn.addEventListener('click', () => {
          if (modalManager.currentCallback) {
 modalManager.currentCallback();
   modalManager.currentCallback = null;
     }
            const modal = bootstrap.Modal.getInstance(document.getElementById('confirmEditModal'));
            if (modal) modal.hide();
        });
    }

    // Configurar el botón de confirmación del modal de creación
  const confirmCreateBtn = document.getElementById('confirmCreateButton');
    if (confirmCreateBtn) {
        confirmCreateBtn.addEventListener('click', () => {
            if (modalManager.currentCallback) {
       modalManager.currentCallback();
       modalManager.currentCallback = null;
      }
       const modal = bootstrap.Modal.getInstance(document.getElementById('confirmCreateModal'));
      if (modal) modal.hide();
 });
    }

    // Configurar el botón de confirmación del modal de eliminación
    const confirmDeleteBtn = document.getElementById('confirmDeleteButton');
    if (confirmDeleteBtn) {
        // IMPORTANTE: Limpiar event listeners anteriores
        const newDeleteBtn = confirmDeleteBtn.cloneNode(true);
        confirmDeleteBtn.parentNode.replaceChild(newDeleteBtn, confirmDeleteBtn);
        
        newDeleteBtn.addEventListener('click', () => {
        if (modalManager.itemIdToDelete) {
      // Buscar el formulario de eliminación
     let form = document.getElementById('deleteForm-' + modalManager.itemIdToDelete);
         
  if (!form) {
     // Crear formulario dinámicamente si no existe
          form = document.createElement('form');
         form.method = 'post';
   form.action = '?handler=Delete&id=' + modalManager.itemIdToDelete;

         const token = document.querySelector('input[name="__RequestVerificationToken"]');
           if (token) {
                const hiddenToken = document.createElement('input');
        hiddenToken.type = 'hidden';
hiddenToken.name = '__RequestVerificationToken';
   hiddenToken.value = token.value;
      form.appendChild(hiddenToken);
          }

     document.body.appendChild(form);
     }
                
       if (form) {
           console.log('Enviando formulario de eliminación para ID:', modalManager.itemIdToDelete);
       form.submit();
   }
      }

  const modal = bootstrap.Modal.getInstance(document.getElementById('confirmDeleteModal'));
            if (modal) modal.hide();
        });
    }

    // Configurar el botón de confirmación de logout
    const confirmLogoutBtn = document.getElementById('confirmLogoutButton');
    if (confirmLogoutBtn) {
     confirmLogoutBtn.addEventListener('click', () => {
const form = document.getElementById('logoutForm');
    if (form) form.submit();
        });
    }

    // Configurar botones de eliminación que usan Bootstrap Modal
    const confirmDeleteModal = document.getElementById('confirmDeleteModal');
    if (confirmDeleteModal) {
        confirmDeleteModal.addEventListener('show.bs.modal', function (event) {
       const button = event.relatedTarget;
       if (button) {
              modalManager.itemIdToDelete = button.getAttribute('data-item-id') || 
      button.getAttribute('data-project-id') || 
    button.getAttribute('data-delete-id');
     
          console.log('Modal de eliminación abierto para ID:', modalManager.itemIdToDelete);
     }
        });
    }

    // Mostrar notificaciones de TempData si existen
    const successMessage = document.querySelector('[data-success-message]');
  if (successMessage) {
        const message = successMessage.getAttribute('data-success-message');
        if (message) {
  modalManager.showNotification('success', message);
        }
    }

    const errorMessage = document.querySelector('[data-error-message]');
    if (errorMessage) {
const message = errorMessage.getAttribute('data-error-message');
        if (message) {
         modalManager.showNotification('error', message);
        }
    }
});

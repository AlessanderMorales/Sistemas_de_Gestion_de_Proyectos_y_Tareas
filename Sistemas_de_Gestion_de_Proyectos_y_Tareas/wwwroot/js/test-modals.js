// ================================================
// SCRIPT DE PRUEBA PARA EL SISTEMA DE MODALES
// ================================================
// Ejecutar este script en la consola del navegador para probar los modales

console.log('%c?? INICIANDO PRUEBAS DEL SISTEMA DE MODALES', 'background: #222; color: #bada55; font-size: 16px; font-weight: bold;');

// ===========================================
// PRUEBA 1: Verificar que modalManager existe
// ===========================================
function test1_ModalManagerExists() {
    console.log('\n%c?? Prueba 1: Verificar modalManager', 'color: #4CAF50; font-weight: bold;');
    
    if (typeof modalManager !== 'undefined') {
 console.log('? modalManager existe');
        console.log('   - currentModal:', modalManager.currentModal);
      console.log('   - currentCallback:', modalManager.currentCallback);
    console.log('   - itemIdToDelete:', modalManager.itemIdToDelete);
        return true;
    } else {
        console.error('? modalManager NO existe');
        return false;
    }
}

// ===========================================
// PRUEBA 2: Verificar modales en el DOM
// ===========================================
function test2_ModalsInDOM() {
    console.log('\n%c?? Prueba 2: Verificar modales en el DOM', 'color: #2196F3; font-weight: bold;');
  
    const modals = [
        'confirmEditModal',
        'confirmDeleteModal',
  'confirmCreateModal',
        'successModal',
        'errorModal',
  'logoutModal'
    ];
    
  let allFound = true;
    modals.forEach(modalId => {
   const modal = document.getElementById(modalId);
   if (modal) {
     console.log(`? ${modalId} encontrado`);
     } else {
        console.error(`? ${modalId} NO encontrado`);
        allFound = false;
  }
    });
    
    return allFound;
}

// ===========================================
// PRUEBA 3: Probar modal de éxito
// ===========================================
function test3_SuccessModal() {
    console.log('\n%c?? Prueba 3: Probar modal de éxito', 'color: #4CAF50; font-weight: bold;');
    
    try {
        modalManager.showNotification('success', '¡Esta es una prueba de notificación de éxito!');
        console.log('? Modal de éxito mostrado (se cerrará en 3 segundos)');
        return true;
    } catch (error) {
        console.error('? Error al mostrar modal de éxito:', error);
        return false;
    }
}

// ===========================================
// PRUEBA 4: Probar modal de error
// ===========================================
function test4_ErrorModal() {
  console.log('\n%c?? Prueba 4: Probar modal de error', 'color: #f44336; font-weight: bold;');
    
 try {
        setTimeout(() => {
            modalManager.showNotification('error', '¡Esta es una prueba de notificación de error!');
       console.log('? Modal de error mostrado (se cerrará en 3 segundos)');
        }, 3500); // Esperar a que se cierre el modal de éxito
        return true;
    } catch (error) {
        console.error('? Error al mostrar modal de error:', error);
        return false;
    }
}

// ===========================================
// PRUEBA 5: Probar modal de confirmación
// ===========================================
function test5_ConfirmModal() {
    console.log('\n%c?? Prueba 5: Probar modal de confirmación', 'color: #FF9800; font-weight: bold;');
    
  try {
   setTimeout(() => {
      modalManager.showConfirmModal('confirmDelete', '¿Estás seguro de que deseas eliminar este elemento de prueba?', () => {
   console.log('? Callback ejecutado - Usuario confirmó la eliminación');
      });
            console.log('? Modal de confirmación mostrado');
        }, 7000); // Esperar a que se cierren los anteriores
   return true;
    } catch (error) {
        console.error('? Error al mostrar modal de confirmación:', error);
        return false;
    }
}

// ===========================================
// PRUEBA 6: Verificar botones de eliminación
// ===========================================
function test6_DeleteButtons() {
 console.log('\n%c?? Prueba 6: Verificar botones de eliminación', 'color: #9C27B0; font-weight: bold;');
    
    const deleteButtons = document.querySelectorAll('[data-bs-target="#confirmDeleteModal"]');
    
    if (deleteButtons.length > 0) {
 console.log(`? Encontrados ${deleteButtons.length} botones de eliminación`);
        deleteButtons.forEach((btn, index) => {
          const itemId = btn.getAttribute('data-item-id') || 
                btn.getAttribute('data-project-id') || 
               btn.getAttribute('data-delete-id');
   console.log(`   ${index + 1}. Botón con ID: ${itemId}`);
   });
        return true;
    } else {
     console.log('?? No se encontraron botones de eliminación en esta página');
        return true; // No es un error, simplemente no hay botones
    }
}

// ===========================================
// PRUEBA 7: Verificar formularios de eliminación
// ===========================================
function test7_DeleteForms() {
console.log('\n%c?? Prueba 7: Verificar formularios de eliminación', 'color: #00BCD4; font-weight: bold;');
    
    const deleteForms = document.querySelectorAll('form[id^="deleteForm-"]');
    
    if (deleteForms.length > 0) {
    console.log(`? Encontrados ${deleteForms.length} formularios de eliminación`);
        deleteForms.forEach((form, index) => {
   console.log(`   ${index + 1}. Formulario ID: ${form.id}`);
        });
        return true;
    } else {
        console.log('?? No se encontraron formularios de eliminación en esta página');
        return true; // No es un error, simplemente no hay formularios
    }
}

// ===========================================
// PRUEBA 8: Verificar Bootstrap
// ===========================================
function test8_Bootstrap() {
    console.log('\n%c?? Prueba 8: Verificar Bootstrap', 'color: #673AB7; font-weight: bold;');
    
    if (typeof bootstrap !== 'undefined') {
    console.log('? Bootstrap está cargado');
   console.log('   - Versión:', bootstrap.Modal ? 'Modal disponible' : 'Modal NO disponible');
        return true;
    } else {
        console.error('? Bootstrap NO está cargado');
        return false;
    }
}

// ===========================================
// EJECUTAR TODAS LAS PRUEBAS
// ===========================================
function runAllTests() {
    console.log('%c\n?? EJECUTANDO TODAS LAS PRUEBAS...', 'background: #222; color: #FFD700; font-size: 14px; font-weight: bold;');
    
 const results = {
        test1: test1_ModalManagerExists(),
        test2: test2_ModalsInDOM(),
        test8: test8_Bootstrap(),
        test6: test6_DeleteButtons(),
   test7: test7_DeleteForms()
  };
    
    // Pruebas visuales (con delay)
    test3_SuccessModal();
    test4_ErrorModal();
    test5_ConfirmModal();
    
    // Resumen
    setTimeout(() => {
        console.log('\n%c?? RESUMEN DE PRUEBAS', 'background: #222; color: #FFD700; font-size: 16px; font-weight: bold;');
        const passed = Object.values(results).filter(r => r).length;
        const total = Object.keys(results).length;
        
        if (passed === total) {
            console.log(`%c? TODAS LAS PRUEBAS PASARON (${passed}/${total})`, 'color: #4CAF50; font-size: 14px; font-weight: bold;');
 } else {
            console.log(`%c?? ALGUNAS PRUEBAS FALLARON (${passed}/${total})`, 'color: #FF9800; font-size: 14px; font-weight: bold;');
   }
     
  console.log('\n%c?? PRUEBAS VISUALES', 'background: #222; color: #2196F3; font-size: 14px; font-weight: bold;');
console.log('   - Modal de éxito: Aparecerá en 0s y se cerrará en 3s');
        console.log('   - Modal de error: Aparecerá en 3.5s y se cerrará en 3s');
        console.log('   - Modal de confirmación: Aparecerá en 7s');
        console.log('\n%c?? TIP: Haz clic en un botón "Eliminar" para probar el modal de confirmación real', 'color: #FFD700; font-style: italic;');
    }, 500);
}

// ===========================================
// FUNCIONES AUXILIARES PARA EL DESARROLLADOR
// ===========================================

// Función para probar un modal específico
window.testModal = function(type, message) {
    console.log(`%c?? Probando modal: ${type}`, 'color: #2196F3; font-weight: bold;');
    if (type === 'success' || type === 'error') {
        modalManager.showNotification(type, message || `Prueba de modal ${type}`);
    } else {
    modalManager.showConfirmModal(type, message || `Prueba de modal ${type}`, () => {
            console.log('? Usuario confirmó');
        });
    }
};

// Función para listar todos los modales
window.listModals = function() {
    console.log('%c?? LISTA DE MODALES DISPONIBLES', 'color: #4CAF50; font-weight: bold;');
    const modals = document.querySelectorAll('.modal');
    modals.forEach((modal, index) => {
        console.log(`${index + 1}. ID: ${modal.id}`);
    });
};

// Función para simular eliminación
window.testDelete = function(itemId = '123') {
    console.log('%c??? SIMULANDO ELIMINACIÓN', 'color: #f44336; font-weight: bold;');
    modalManager.itemIdToDelete = itemId;
const modal = new bootstrap.Modal(document.getElementById('confirmDeleteModal'));
    modal.show();
};

// ===========================================
// EJECUTAR PRUEBAS AUTOMÁTICAMENTE
// ===========================================
runAllTests();

console.log('\n%c?? COMANDOS DISPONIBLES:', 'background: #222; color: #00BCD4; font-size: 14px; font-weight: bold;');
console.log('   - %crunAllTests()%c: Ejecutar todas las pruebas nuevamente', 'color: #FFD700', 'color: inherit');
console.log('   - %ctestModal(type, message)%c: Probar un modal específico', 'color: #FFD700', 'color: inherit');
console.log('     Ejemplos: testModal("success", "¡Éxito!")');
console.log('           testModal("error", "¡Error!")');
console.log('      testModal("confirmDelete", "¿Eliminar?")');
console.log('   - %clistModals()%c: Listar todos los modales', 'color: #FFD700', 'color: inherit');
console.log('   - %ctestDelete(itemId)%c: Simular modal de eliminación', 'color: #FFD700', 'color: inherit');

/**
 * Utilidades de validacion de formularios.
 *
 * Centraliza las reglas de validacion que se aplican antes de enviar
 * cualquier formulario al servidor. Cada funcion recibe el valor del campo
 * y devuelve un mensaje de error cuando la regla no se cumple, o null
 * cuando el valor es valido.
 *
 * Uso:
 *   import { validateEmail, validatePhone } from '@core/validators/form.validators';
 *   const error = validateEmail(this.form.email);
 *   if (error) { this.feedback.set({ type: 'error', message: error }); return; }
 */

/**
 * Verifica que el correo tenga un formato estandar (usuario@dominio.extension).
 * No acepta espacios ni caracteres especiales fuera del estandar RFC.
 */
export function validateEmail(value: string): string | null {
  const trimmed = value.trim();
  if (!trimmed) return 'El correo electronico es obligatorio.';
  const pattern = /^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$/;
  if (!pattern.test(trimmed)) return 'Ingresa un correo electronico valido (ejemplo: usuario@correo.com).';
  return null;
}

/**
 * Verifica que el numero de telefono sea un numero colombiano valido.
 * Acepta numeros de 10 digitos, con o sin prefijo internacional +57.
 * Ejemplos validos: 3001234567, +573001234567, 3101234567.
 */
export function validatePhone(value: string): string | null {
  const trimmed = value.trim().replace(/\s+/g, '');
  if (!trimmed) return 'El numero de telefono es obligatorio.';
  const pattern = /^(\+57)?[3][0-9]{9}$/;
  if (!pattern.test(trimmed)) {
    return 'Ingresa un numero de celular colombiano valido de 10 digitos (ejemplo: 3001234567).';
  }
  return null;
}

/**
 * Verifica que el nombre completo tenga al menos dos palabras y solo letras con acentos y espacios.
 * Evita nombres de una sola palabra o valores con numeros y caracteres especiales.
 */
export function validateFullName(value: string): string | null {
  const trimmed = value.trim();
  if (!trimmed) return 'El nombre completo es obligatorio.';
  if (trimmed.length < 4) return 'El nombre debe tener al menos 4 caracteres.';
  const pattern = /^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+([\s][a-zA-ZáéíóúÁÉÍÓÚñÑüÜ]+)+$/;
  if (!pattern.test(trimmed)) return 'Ingresa el nombre y apellido completos, solo letras.';
  return null;
}

/**
 * Verifica que la contraseña tenga al menos 8 caracteres, una letra mayuscula
 * y un numero. Este nivel de seguridad es el minimo aceptable para datos de usuario.
 */
export function validatePassword(value: string): string | null {
  if (!value) return 'La contrasena es obligatoria.';
  if (value.length < 8) return 'La contrasena debe tener al menos 8 caracteres.';
  if (!/[A-Z]/.test(value)) return 'La contrasena debe tener al menos una letra mayuscula.';
  if (!/[0-9]/.test(value)) return 'La contrasena debe tener al menos un numero.';
  return null;
}

/**
 * Verifica que la ciudad no este vacia y tenga un formato de texto simple.
 * Minimo 3 caracteres para evitar abreviaturas o codigos.
 */
export function validateCity(value: string): string | null {
  const trimmed = value.trim();
  if (!trimmed) return 'La ciudad de entrega es obligatoria.';
  if (trimmed.length < 3) return 'Escribe el nombre completo de la ciudad.';
  return null;
}

/**
 * Verifica que la direccion sea descriptiva: numero minimo de caracteres
 * y al menos un digito para que incluya el numero de la calle o carrera.
 */
export function validateAddress(value: string): string | null {
  const trimmed = value.trim();
  if (!trimmed) return 'La direccion de entrega es obligatoria.';
  if (trimmed.length < 8) return 'Escribe una direccion completa (calle, carrera, numero, etc.).';
  if (!/\d/.test(trimmed)) return 'La direccion debe incluir un numero (calle, carrera, casa, apto).';
  return null;
}

/**
 * Reune todas las validaciones del formulario de registro de cliente.
 * Devuelve el primer error encontrado o null si todos los campos son validos.
 */
export function validateRegistrationForm(fields: {
  fullName: string;
  email: string;
  password: string;
  phone: string;
  city: string;
  address: string;
}): string | null {
  return (
    validateFullName(fields.fullName) ??
    validateEmail(fields.email) ??
    validatePassword(fields.password) ??
    validatePhone(fields.phone) ??
    validateCity(fields.city) ??
    validateAddress(fields.address)
  );
}

/**
 * Reune las validaciones del formulario de edicion de perfil.
 * El correo electronico no se puede cambiar desde este formulario.
 */
export function validateProfileEditForm(fields: {
  fullName: string;
  phone: string;
  city: string;
  address: string;
}): string | null {
  return (
    validateFullName(fields.fullName) ??
    validatePhone(fields.phone) ??
    validateCity(fields.city) ??
    validateAddress(fields.address)
  );
}

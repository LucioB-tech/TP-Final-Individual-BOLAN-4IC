const formLogin = document.getElementById("formLogin");
if (formLogin) {
    formLogin.addEventListener("submit", function (event) {
        const usuario = document.getElementById("nombreUsuario");
        const contrasena = document.getElementById("contrasena");
        const errorUsuario = document.getElementById("errorNombreUsuario");
        const errorContrasena = document.getElementById("errorContrasena");
        let valido = true;

        errorUsuario.textContent = "";
        errorContrasena.textContent = "";
        if (!usuario.value.trim()) {
            errorUsuario.textContent = "Ingresá el nombre de usuario.";
            valido = false;
        }
        if (!contrasena.value.trim()) {
            errorContrasena.textContent = "Ingresá la contraseña.";
            valido = false;
        }
        if (!valido) event.preventDefault();
    });
}

const formCrearCuenta = document.getElementById("formCrearCuenta");
if (formCrearCuenta) {
    formCrearCuenta.addEventListener("submit", function (event) {
        const nombre = document.getElementById("nombre");
        const apellido = document.getElementById("apellido");
        const usuario = document.getElementById("nombreUsuario");
        const contrasena = document.getElementById("contrasena");
        let valido = true;

        document.querySelectorAll("small.text-danger").forEach(function (error) {
            error.textContent = "";
        });
        if (!nombre.value.trim()) {
            document.getElementById("errorNombre").textContent = "Ingresá el nombre.";
            valido = false;
        }
        if (!apellido.value.trim()) {
            document.getElementById("errorApellido").textContent = "Ingresá el apellido.";
            valido = false;
        }
        if (!usuario.value.trim()) {
            document.getElementById("errorNombreUsuario").textContent = "Ingresá el usuario.";
            valido = false;
        }
        if (contrasena.value.length < 6) {
            document.getElementById("errorContrasena").textContent = "La contraseña debe tener al menos 6 caracteres.";
            valido = false;
        }
        if (!valido) event.preventDefault();
    });
}

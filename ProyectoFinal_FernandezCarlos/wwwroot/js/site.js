function configurarTablaCursosAjax() {
    const inputBusqueda = document.getElementById("buscarCursos");
    const inputCreditos = document.getElementById("creditosCursos");
    const contenedorTabla = document.getElementById("tablaCursos");

    if (!inputBusqueda || !inputCreditos || !contenedorTabla) {
        return;
    }

    let temporizador = null;

    function cargarCursos(page = 1) {
        const parametros = new URLSearchParams();
        const busqueda = inputBusqueda.value.trim();
        const creditos = inputCreditos.value.trim();

        if (busqueda.length > 0) {
            parametros.append("search", busqueda);
        }

        if (creditos.length > 0) {
            parametros.append("creditos", creditos);
        }

        parametros.append("page", page);

        fetch(`/Cursos/TablaCursos?${parametros.toString()}`, {
            method: "GET",
            headers: {
                "X-Requested-With": "XMLHttpRequest"
            }
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error("No se pudo cargar la tabla de cursos.");
                }

                return response.text();
            })
            .then(html => {
                contenedorTabla.innerHTML = html;
            })
            .catch(() => {
                contenedorTabla.innerHTML = '<div class="alert alert-danger">Ocurrió un error al cargar los cursos.</div>';
            });
    }

    function cargarConEspera() {
        clearTimeout(temporizador);
        temporizador = setTimeout(() => cargarCursos(1), 300);
    }

    inputBusqueda.addEventListener("input", cargarConEspera);
    inputCreditos.addEventListener("input", cargarConEspera);

    contenedorTabla.addEventListener("click", event => {
        const botonPagina = event.target.closest("[data-page]");

        if (!botonPagina) {
            return;
        }

        event.preventDefault();
        cargarCursos(botonPagina.dataset.page);
    });
}